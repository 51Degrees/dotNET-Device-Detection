/* *********************************************************************
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited. 
 * Copyright © 2015 51Degrees Mobile Experts Limited, 5 Charlotte Close,
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
using System.Linq;
using System.Text;
using System.IO;
using FiftyOne.Foundation.Mobile.Detection.Entities.Stream;
using FiftyOne.Foundation.Mobile.Detection.Readers;

namespace FiftyOne.Foundation.Mobile.Detection.Factories
{
    /// <summary>
    /// Reader used to create a provider from data structured in a decision
    /// tree format.
    /// </summary>
    public static class TrieFactory
    {
        /// <summary>
        /// Creates a new provider from the byte array supplied.
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static TrieProvider Create(byte[] array)
        {
            return Create(new Pool(new SourceMemory(array)));
        }

        /// <summary>
        /// Creates a new provider from the binary file supplied.
        /// </summary>
        /// <param name="file">Binary file to use to create the provider.</param>
        /// <returns>A new provider initialised with data from the file provided.</returns>
        public static TrieProvider Create(string file)
        {
            return Create(file, false);
        }

        /// <summary>
        /// Creates a new provider from the binary file supplied.
        /// </summary>
        /// <param name="file">Binary file to use to create the provider.</param>
        /// <param name="isTempFile">True if the file should be deleted when the source is disposed</param>
        /// <returns>A new provider initialised with data from the file provided.</returns>
        public static TrieProvider Create(string file, bool isTempFile)
        {
            FileInfo fileInfo = new FileInfo(file);
            if (fileInfo.Exists)
            {
                return Create(new Pool(new SourceFile(file, isTempFile)));
            }
            return null;
        }

        private static TrieProvider Create(Pool pool)
        {
            var reader = pool.GetReader();
            try
            {
                // Check the version number is correct for this API.
                var version = reader.ReadUInt16();

                // Construct the right provider.
                switch(version)
                {
                    case 3:
                        return new TrieProviderV3(
                            Encoding.ASCII.GetString(reader.ReadBytes((int)reader.ReadUInt32())),
                            ReadStrings(reader),
                            ReadProperties(reader),
                            ReadDevices(reader),
                            ReadLookupList(reader),
                            reader.ReadInt64(),
                            reader.BaseStream.Position,
                            pool);
                    case 32:
                        return new TrieProviderV32(
                            Encoding.ASCII.GetString(reader.ReadBytes((int)reader.ReadUInt32())),
                            ReadStrings(reader),
                            ReadHeaders(reader),
                            ReadProperties(reader),
                            ReadDevices(reader),
                            ReadLookupList(reader),
                            reader.ReadInt64(),
                            reader.BaseStream.Position,
                            pool);
                    default:
                        throw new MobileException(String.Format(
                            "Version mismatch. Data is version '{0}' for '{1}' reader",
                            version,
                            String.Join(",", BinaryConstants.SupportedTrieFormatVersions.Select(i => i.Value.ToString()))));
                }
            }
            finally
            {
                // Return the reader back to the pool.
                pool.Release(reader);
            }
        }

        private static byte[] ReadLookupList(BinaryReader reader)
        {
            return reader.ReadBytes((int)reader.ReadUInt32());
        }

        private static byte[] ReadStrings(BinaryReader reader)
        {
            return reader.ReadBytes((int)reader.ReadUInt32());
        }

        private static byte[] ReadProperties(BinaryReader reader)
        {
            return reader.ReadBytes((int)reader.ReadUInt32());
        }

        private static byte[] ReadHeaders(BinaryReader reader)
        {
            return reader.ReadBytes((int)reader.ReadUInt32());
        }

        private static byte[] ReadDevices(BinaryReader reader)
        {
            return reader.ReadBytes((int)reader.ReadUInt32());
        }
    }
}
