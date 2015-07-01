/* *********************************************************************
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited. 
 * Copyright © 2014 51Degrees Mobile Experts Limited, 5 Charlotte Close,
 * Caversham, Reading, Berkshire, United Kingdom RG4 7BY
 * 
 * This Source Code Form is the subject of the following patent 
 * applications, owned by 51Degrees Mobile Experts Limited of 5 Charlotte
 * Close, Caversham, Reading, Berkshire, United Kingdom RG4 7BY: 
 * European Patent Application No. 13192291.6; and 
 * United States Patent Application Nos. 14/085,223 and 14/085,301.
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0.
 * 
 * If a copy of the MPL was not distributed with this file, You can obtain
 * one at http://mozilla.org/MPL/2.0/.
 * 
 * This Source Code Form is “Incompatible With Secondary Licenses”, as
 * defined by the Mozilla Public License, v. 2.0.
 * ********************************************************************* */

using System.IO;
using FiftyOne.Foundation.Mobile.Detection.Entities;
using FiftyOne.Foundation.Mobile.Detection.Entities.Memory;
using FiftyOne.Foundation.Mobile.Detection.Entities.Stream;
using DataSet = FiftyOne.Foundation.Mobile.Detection.Entities.Stream.DataSet;
using FiftyOne.Foundation.Mobile.Detection.Readers;
using System;

namespace FiftyOne.Foundation.Mobile.Detection.Factories
{
    /// <summary>
    /// Factory class used to create a <see cref="DataSet"/> from a source data
    /// structure. All the entities are held in the persistent store and only loads into
    /// memory when required. A cache mechanisim is used to improve efficiency as many
    /// entities are frequently used in a high volume environment.
    /// </summary>
    /// <para>
    /// The data set will be initialised very quickly as only the header information is read.
    /// Entities are then created when requested by the detection process and stored in a 
    /// cache to avoid being recreated if their requested again after a short period of time.
    /// </para>
    /// <remarks>
    /// The very small data structures RootNodes, Properties and Components are always
    /// stored in memory as there is no benefit retrieving them every time they're needed.
    /// </remarks>
    /// <para>
    /// For more information see https://51degrees.com/Support/Documentation/Net
    /// </para>
    public static class StreamFactory
    {
        #region Public Methods

        /// <summary>
        /// Creates a new <see cref="DataSet"/> from the byte array.
        /// </summary>
        /// <param name="array">Array of bytes to build the data set from</param>
        /// <returns>
        /// A <see cref="DataSet"/> configured to read entities from the array when required
        /// </returns>
        public static DataSet Create(byte[] array)
        {
            var dataSet = new DataSet(array, DataSet.Modes.MemoryMapped);
            Load(dataSet);
            return dataSet;
        }

        /// <summary>
        /// Creates a new <see cref="DataSet"/> from the file provided. The last modified
        /// date of the data set is the last write time of the data file provided.
        /// </summary>
        /// <param name="filePath">Uncompressed file containing the data for the data set</param>
        /// <returns>
        /// A <see cref="DataSet"/>configured to read entities from the file path when required
        /// </returns>
        public static DataSet Create(string filePath)
        {
            return Create(filePath, File.GetLastWriteTimeUtc(filePath));
        }

        /// <summary>
        /// Creates a new <see cref="DataSet"/> from the file provided.
        /// </summary>
        /// <param name="filePath">Uncompressed file containing the data for the data set</param>
        /// <param name="lastModified">Date and time the source data was last modified.</param>
        /// <returns>
        /// A <see cref="DataSet"/>configured to read entities from the file path when required
        /// </returns>
        public static DataSet Create(string filePath, DateTime lastModified)
        {
            var dataSet = new DataSet(filePath, lastModified, DataSet.Modes.File);
            Load(dataSet);
            return dataSet;
        }
              
        #endregion

        #region Private Methods

        /// <summary>
        /// Initialises the dataset <see cref="DataSet"/> using the source of the data set.
        /// </summary>
        /// <para>
        /// A <see cref="DataSet"/> is initialised using the reader to retrieve 
        /// entity information. The data is only loaded when required by the detection
        /// process.
        /// </para>
        /// <param name="dataSet">A data set to be initialised ready for detection</param>
        private static void Load(DataSet dataSet)
        {
            var reader = dataSet.Pool.GetReader();
            try
            {
                reader.BaseStream.Position = 0;
                CommonFactory.LoadHeader(dataSet, reader);
                dataSet.Strings = new VariableList<AsciiString>(dataSet, reader, new AsciiStringFactory(), Constants.StringsCacheSize);
                MemoryFixedList<Component> components = null;
                switch (dataSet.VersionEnum)
                {
                    case BinaryConstants.FormatVersions.PatternV31:
                        components = new MemoryFixedList<Component>(dataSet, reader, new ComponentFactoryV31());
                        break;
                    case BinaryConstants.FormatVersions.PatternV32:
                        components = new MemoryFixedList<Component>(dataSet, reader, new ComponentFactoryV32());
                        break;
                }
                dataSet._components = components;
                var maps = new MemoryFixedList<Map>(dataSet, reader, new MapFactory());
                dataSet._maps = maps;
                var properties = new PropertiesList(dataSet, reader, new PropertyFactory());
                dataSet._properties = properties;
                dataSet._values = new FixedCacheList<Value>(dataSet, reader, new ValueFactory(), Constants.ValuesCacheSize);
                dataSet.Profiles = new VariableList<Entities.Profile>(dataSet, reader, new ProfileStreamFactory(dataSet.Pool), Constants.ProfilesCacheSize);
                switch (dataSet.VersionEnum)
                {
                    case BinaryConstants.FormatVersions.PatternV31:
                        dataSet._signatures = new FixedCacheList<Signature>(dataSet, reader, new SignatureFactoryV31(dataSet), Constants.SignaturesCacheSize);
                        break;
                    case BinaryConstants.FormatVersions.PatternV32:
                        dataSet._signatures = new FixedCacheList<Signature>(dataSet, reader, new SignatureFactoryV32(dataSet), Constants.SignaturesCacheSize);
                        dataSet._signatureNodeOffsets = new FixedList<Integer>(dataSet, reader, new IntegerFactory());
                        dataSet._nodeRankedSignatureIndexes = new FixedList<Integer>(dataSet, reader, new IntegerFactory());
                        break;
                }
                dataSet._rankedSignatureIndexes = new FixedCacheList<Integer>(
                    dataSet, reader, new IntegerFactory(), Constants.RankedSignaturesCacheSize);
                switch (dataSet.VersionEnum)
                {
                    case BinaryConstants.FormatVersions.PatternV31:
                        dataSet.Nodes = new VariableList<Entities.Node>(dataSet, reader, new NodeStreamFactoryV31(dataSet.Pool), Constants.NodesCacheSize);
                        break;
                    case BinaryConstants.FormatVersions.PatternV32:
                        dataSet.Nodes = new VariableList<Entities.Node>(dataSet, reader, new NodeStreamFactoryV32(dataSet.Pool), Constants.NodesCacheSize);
                        break;
                }
                var rootNodes = new MemoryFixedList<Entities.Node>(dataSet, reader, new RootNodeFactory());
                dataSet.RootNodes = rootNodes;
                var profileOffsets = new MemoryFixedList<ProfileOffset>(dataSet, reader, new ProfileOffsetFactory());
                dataSet._profileOffsets = profileOffsets;

                // Read into memory all the small lists which are frequently accessed.
                reader.BaseStream.Position = components.Header.StartPosition;
                components.Read(reader);
                reader.BaseStream.Position = maps.Header.StartPosition;
                maps.Read(reader);
                reader.BaseStream.Position = properties.Header.StartPosition;
                properties.Read(reader);
                reader.BaseStream.Position = rootNodes.Header.StartPosition;
                rootNodes.Read(reader);
                reader.BaseStream.Position = profileOffsets.Header.StartPosition;
                profileOffsets.Read(reader);
            }
            finally
            {
                dataSet.Pool.Release(reader);
            }
        }

        #endregion
    }
}
