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
            DataSet dataSet = null;

                using (var reader = new Reader(new MemoryStream(array)))
                {
                    dataSet = new DataSet(reader, array);
                    Read(reader, dataSet);
                }

            return dataSet;
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
            DataSet dataSet = null;

            using (var reader = new Reader(File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                dataSet = new DataSet(reader, filePath, lastModified);
                Read(reader, dataSet);
            }

            return dataSet;
        }
              
        #endregion

        #region Private Methods

        /// <summary>
        /// Initialises the dataset <see cref="DataSet"/> using the binary reader provided.
        /// </summary>
        /// <para>
        /// A <see cref="DataSet"/> is initialised using the reader to retrieve 
        /// entity information. The data is only loaded when required by the detection
        /// process.
        /// </para>
        /// <param name="dataSet">A data set to be initialised ready for detection</param>
        /// <param name="reader">A binary reader connected to the underlying data source 
        /// and positioned after the header.</param>
        internal static void Read(Reader reader, DataSet dataSet)
        {
            dataSet.Strings = new VariableList<AsciiString>(dataSet, reader, new AsciiStringFactory(), Constants.StringsCacheSize);
            var components = new MemoryFixedList<Component>(dataSet, reader, new ComponentFactory());
            dataSet._components = components;
            var maps = new MemoryFixedList<Map>(dataSet, reader, new MapFactory());
            dataSet._maps = maps;
            var properties = new PropertiesList(dataSet, reader, new PropertyFactory());
            dataSet._properties = properties;
            dataSet._values = new FixedList<Value>(dataSet, reader, new ValueFactory(), Constants.ValuesCacheSize);
            dataSet.Profiles = new VariableList<Entities.Profile>(dataSet, reader, new ProfileStreamFactory(dataSet.Pool), Constants.ProfilesCacheSize);
            dataSet._signatures = new FixedList<Signature>(dataSet, reader, new SignatureFactory(dataSet), Constants.SignaturesCacheSize);
            dataSet._rankedSignatureIndexes = new FixedList<RankedSignatureIndex>(dataSet, reader, new RankedSignatureIndexFactory(), Constants.RankedSignaturesCacheSize);
            dataSet.Nodes = new VariableList<Entities.Node>(dataSet, reader, new NodeStreamFactory(dataSet.Pool), Constants.NodesCacheSize);
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

        #endregion
    }
}
