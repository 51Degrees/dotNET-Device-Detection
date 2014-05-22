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
using FiftyOne.Foundation.Mobile.Detection.Readers;

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
    /// For more information see http://51degrees.com/Support/Documentation/Net.aspx
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
            using (var ms = new MemoryStream(array))
            {
                using (var reader = new Reader(ms))
                {
                    return Read(reader, new Source(array));
                }
            }
        }

        /// <summary>
        /// Creates a new <see cref="DataSet"/> from the file provided.
        /// </summary>
        /// <param name="filePath">Uncompressed file containing the data for the data set</param>
        /// <returns>
        /// A <see cref="DataSet"/> configured to read entities from the file path when required
        /// </returns>
        public static DataSet Create(string filePath)
        {
            using (var stream = File.OpenRead(filePath))
            {
                using (var reader = new Reader(stream))
                {
                    return Read(reader, new Source(filePath));
                }
            }
        }
              
        #endregion

        #region Private Methods

        /// <summary>
        /// Creates a new <see cref="DataSet"/> from the binary reader provided.
        /// </summary>
        /// <param name="reader">
        /// Reader connected to the source data structure and positioned to start reading
        /// </param>
        /// <param name="source">
        /// The source of the data used by the created detector.
        /// </param>
        /// <para>
        /// A <see cref="DataSet"/> is constructed using the reader to retrieve 
        /// the header information. The data is only loaded when required by the detection
        /// process.
        /// </para>
        /// <returns>
        /// A <see cref="DataSet"/> configured to read entities from the array when required
        /// </returns>
        internal static DataSet Read(Reader reader, Source source)
        {
            var dataSet = new DataSet(reader);

            dataSet.Strings = new VariableList<AsciiString>(dataSet, reader, source, new AsciiStringFactory());
            var components = new MemoryFixedList<Component>(dataSet, reader, new ComponentFactory());
            dataSet._components = components;
            var maps = new MemoryFixedList<Map>(dataSet, reader, new MapFactory());
            dataSet._maps = maps;
            var properties = new MemoryFixedList<Property>(dataSet, reader, new PropertyFactory());
            dataSet._properties = properties;
            dataSet._values = new FixedList<Value>(dataSet, reader, source, new ValueFactory());
            dataSet.Profiles = new VariableList<Profile>(dataSet, reader, source, new ProfileFactory());
            dataSet._signatures = new FixedList<Signature>(dataSet, reader, source, new SignatureFactory(dataSet));
            var rankedSignatureIndexes = new MemoryFixedList<RankedSignatureIndex>(
                dataSet, reader, new RankedSignatureIndexFactory());
            dataSet._rankedSignatureIndexes = rankedSignatureIndexes;
            dataSet.Nodes = new VariableList<Node>(dataSet, reader, source, new NodeFactory());
            var rootNodes = new MemoryFixedList<Node>(dataSet, reader, new RootNodeFactory());
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
            reader.BaseStream.Position = rankedSignatureIndexes.Header.StartPosition;
            rankedSignatureIndexes.Read(reader);
            reader.BaseStream.Position = rootNodes.Header.StartPosition;
            rootNodes.Read(reader);
            reader.BaseStream.Position = profileOffsets.Header.StartPosition;
            profileOffsets.Read(reader);

            return dataSet;
        }

        #endregion
    }
}
