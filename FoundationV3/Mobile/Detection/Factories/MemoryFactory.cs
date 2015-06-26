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

using System;
using FiftyOne.Foundation.Mobile.Detection.Entities;
using FiftyOne.Foundation.Mobile.Detection.Readers;
using FiftyOne.Foundation.Mobile.Detection.Entities.Memory;
using System.IO;
using System.Reflection;

namespace FiftyOne.Foundation.Mobile.Detection.Factories
{
    /// <summary>
    /// Factory class used to create a <see cref="DataSet"/> from a source data
    /// structure. All the entities are held in memory and the source data structure not
    /// referenced once the data set is created.
    /// </summary>
    /// <para>
    /// The memory usage of the resulting data set following initialisation will be consistent. 
    /// The performance of the data set will be very fast compared to the stream based
    /// implementation as all required data is loaded into memory and references between
    /// related objects set at initialisation. However overall memory usage will be higher
    /// than the stream based implementation on lightly loaded environments.
    /// </para>
    /// <remarks>
    /// Initialisation may take several seconds depending on system performance. Initialisation
    /// calculates all the references between entities. If initialisation is not performed
    /// then references will be calculated when needed. As such avoiding initialisation
    /// improves the time taken to create the data set, at the expense of performance for
    /// the initial detections. The default setting is to initialise the data set.
    /// </remarks>
    /// <para>
    /// For more information see https://51degrees.com/Support/Documentation/Net
    /// </para>
    public static class MemoryFactory
    {
        #region Public Create Methods

        /// <summary>
        /// Creates a new <see cref="DataSet"/> from the byte array.
        /// </summary>
        /// <param name="array">Array of bytes to build the data set from</param>
        /// <returns>A <see cref="DataSet"/> filled with data from the array</returns>
        public static DataSet Create(byte[] array)
        {
            return Create(array, false);
        }

        /// <summary>
        /// Creates a new <see cref="DataSet"/> from the byte array.
        /// </summary>
        /// <param name="array">Array of bytes to build the data set from</param>
        /// <param name="init">True to indicate that the data set should be fulling initialised</param>
        /// <returns>A <see cref="DataSet"/> filled with data from the array</returns>
        public static DataSet Create(byte[] array, bool init)
        {
            DataSet dataSet = new DataSet(DateTime.MinValue, DataSet.Modes.Memory);
            using (var reader = new Reader(new MemoryStream(array)))
            {
                 Load(dataSet, reader, init);
            }
            return dataSet;
        }
        
        /// <summary>
        /// Creates a new <see cref="DataSet"/> from the file provided. The last modified
        /// date of the data set is the last write time of the data file provided.
        /// </summary>
        /// <param name="filePath">
        /// Uncompressed file containing the data for the data set
        /// </param>
        /// <returns>A <see cref="DataSet"/> filled with data from the array</returns>
        public static DataSet Create(string filePath)
        {
            return Create(filePath, false, File.GetLastWriteTimeUtc(filePath));
        }

        /// <summary>
        /// Creates a new <see cref="DataSet"/> from the file provided.
        /// </summary>
        /// <param name="filePath">
        /// Uncompressed file containing the data for the data set
        /// </param>
        /// <param name="init">
        /// True to indicate that the data set should be fulling initialised
        /// </param>
        /// <param name="lastModified">Date and time the source data was last modified.</param>
        /// <returns>A <see cref="DataSet"/> filled with data from the array</returns>
        public static DataSet Create(string filePath, bool init, DateTime lastModified)
        {
            DataSet dataSet = new DataSet(lastModified, DataSet.Modes.Memory);
            using (var reader = new Reader(
                File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                Load(dataSet, reader, init);
            }
            return dataSet;
        }
       
        #endregion

        #region Private Methods

        /// <summary>
        /// Creates a new <see cref="DataSet"/> from the binary reader provided.
        /// </summary>
        /// <param name="dataSet">The data set to be loaded with data from the reader</param>
        /// <param name="reader">
        /// Reader connected to the source data structure and positioned to start reading
        /// </param>
        /// <param name="init">
        /// True to indicate that the data set should be fulling initialised
        /// </param>
        /// <para>
        /// A <see cref="DataSet"/> is constructed using the reader to retrieve 
        /// the header information. This is then passed to the Read methods to create the
        /// lists before reading the data into memory. Finally it initialise is required
        /// references between entities are worked out and stored.
        /// </para>
        internal static void Load(DataSet dataSet, Reader reader, bool init)
        {
            CommonFactory.LoadHeader(dataSet, reader);

            var strings = new MemoryVariableList<AsciiString>(dataSet, reader, new AsciiStringFactory());
            MemoryFixedList<Component> components = null;
            switch(dataSet.VersionEnum)
            {
                case BinaryConstants.FormatVersions.PatternV31:
                    components = new MemoryFixedList<Component>(dataSet, reader, new ComponentFactoryV31());
                    break;
                case BinaryConstants.FormatVersions.PatternV32:
                    components = new MemoryFixedList<Component>(dataSet, reader, new ComponentFactoryV32());
                    break;
            }
            var maps = new MemoryFixedList<Map>(dataSet, reader, new MapFactory());
            var properties = new PropertiesList(dataSet, reader, new PropertyFactory());
            var values = new MemoryFixedList<Value>(dataSet, reader, new ValueFactory());
            var profiles = new MemoryVariableList<Entities.Profile>(dataSet, reader, new ProfileMemoryFactory());
            MemoryFixedList<Signature> signatures = null;
            MemoryFixedList<Integer> signatureNodeOffsets = null;
            MemoryFixedList<Integer> nodeRankedSignatureIndexes = null;
            switch(dataSet.VersionEnum)
            {
                case BinaryConstants.FormatVersions.PatternV31:
                    signatures = new MemoryFixedList<Signature>(dataSet, reader, new SignatureFactoryV31(dataSet));
                    break;
                case BinaryConstants.FormatVersions.PatternV32:
                    signatures = new MemoryFixedList<Signature>(dataSet, reader, new SignatureFactoryV32(dataSet));
                    signatureNodeOffsets = new MemoryFixedList<Integer>(dataSet, reader, new IntegerFactory());
                    nodeRankedSignatureIndexes = new MemoryFixedList<Integer>(dataSet, reader, new IntegerFactory());
                    break;
            }
            var rankedSignatureIndexes = new MemoryFixedList<Integer>(
                dataSet, reader, new IntegerFactory());
            MemoryVariableList<Entities.Node> nodes = null;
            switch (dataSet.VersionEnum)
            {
                case BinaryConstants.FormatVersions.PatternV31:
                    nodes = new MemoryVariableList<Entities.Node>(dataSet, reader, new NodeMemoryFactoryV31());
                    break;
                case BinaryConstants.FormatVersions.PatternV32:
                    nodes = new MemoryVariableList<Entities.Node>(dataSet, reader, new NodeMemoryFactoryV32());
                    break;
            }
            var rootNodes = new MemoryFixedList<Entities.Node>(dataSet, reader, new RootNodeFactory());
            var profileOffsets = new MemoryFixedList<ProfileOffset>(dataSet, reader, new ProfileOffsetFactory());

            dataSet.Strings = strings;
            dataSet._components = components;
            dataSet._maps = maps;
            dataSet._properties = properties;
            dataSet._values = values;
            dataSet.Profiles = profiles;
            dataSet._signatures = signatures;
            dataSet._rankedSignatureIndexes = rankedSignatureIndexes;
            switch (dataSet.VersionEnum)
            {
                case BinaryConstants.FormatVersions.PatternV32:
                    dataSet._signatureNodeOffsets = signatureNodeOffsets;
                    dataSet._nodeRankedSignatureIndexes = nodeRankedSignatureIndexes;
                    break;
            }
            dataSet.Nodes = nodes;
            dataSet.RootNodes = rootNodes;
            dataSet._profileOffsets = profileOffsets;

            strings.Read(reader);
            components.Read(reader);
            maps.Read(reader);
            properties.Read(reader);
            values.Read(reader);
            profiles.Read(reader);
            signatures.Read(reader);
            switch (dataSet.VersionEnum)
            {
                case BinaryConstants.FormatVersions.PatternV32:
                    signatureNodeOffsets.Read(reader);
                    nodeRankedSignatureIndexes.Read(reader);
                    break;
            }
            rankedSignatureIndexes.Read(reader);
            nodes.Read(reader);
            rootNodes.Read(reader);
            profileOffsets.Read(reader);

            if (init)
            {
                // Set references between objects.
                dataSet.Init();

                // Force garbage collection as a lot of memory has been freed.
                GC.Collect();  
            }
        }
        
        #endregion
    }
}
