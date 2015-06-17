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

using FiftyOne.Foundation.Mobile.Detection.Entities;
using System;
using System.IO;

namespace FiftyOne.Foundation.Mobile.Detection.Factories
{
    /// <summary>
    /// Extension methods used to load data into the data set entity.
    /// </summary>
    internal static class CommonFactory
    {
        /// <summary>
        /// Loads the data set headers information.
        /// </summary>
        /// <param name="dataSet">The data set to be loaded</param>
        /// <param name="reader">Reader positioned at the beginning of the data source</param>
        internal static void LoadHeader(DataSet dataSet, BinaryReader reader)
        {
            // Check for an exception which would indicate the file is the 
            // wrong type for the API.
            try
            {
                dataSet.Version = new Version(
                    reader.ReadInt32(),
                    reader.ReadInt32(),
                    reader.ReadInt32(),
                    reader.ReadInt32());
            }
            catch (ArgumentOutOfRangeException ex)
            {
                throw new MobileException(String.Format(
                    "Data file is invalid. Check that the data file is " +
                    "decompressed and is the latest version '{0}' format.",
                    BinaryConstants.FormatVersion), ex);
            }

            // Throw exception if the data file does not have the correct
            // version in formation.
            if (dataSet.Version.Major != BinaryConstants.FormatVersion.Major ||
                dataSet.Version.Minor != BinaryConstants.FormatVersion.Minor)
                throw new MobileException(String.Format(
                    "Version mismatch. Data is version '{0}' for '{1}' reader",
                    dataSet.Version,
                    BinaryConstants.FormatVersion));

            dataSet.Tag = new Guid(reader.ReadBytes(16));
            dataSet.CopyrightOffset = reader.ReadInt32();
            dataSet.AgeAtPublication = new TimeSpan(reader.ReadInt16() * TimeSpan.TicksPerDay * 30);
            dataSet.MinUserAgentCount = reader.ReadInt32();
            dataSet.NameOffset = reader.ReadInt32();
            dataSet.FormatOffset = reader.ReadInt32();
            dataSet.Published = ReadDate(reader);
            dataSet.NextUpdate = ReadDate(reader);
            dataSet.DeviceCombinations = reader.ReadInt32();
            dataSet.MaxUserAgentLength = reader.ReadInt16();
            dataSet.MinUserAgentLength = reader.ReadInt16();
            dataSet.LowestCharacter = reader.ReadByte();
            dataSet.HighestCharacter = reader.ReadByte();
            dataSet.MaxSignatures = reader.ReadInt32();
            dataSet.SignatureProfilesCount = reader.ReadInt32();
            dataSet.SignatureNodesCount = reader.ReadInt32();
            dataSet.MaxValues = reader.ReadInt16();
            dataSet.CsvBufferLength = reader.ReadInt32();
            dataSet.JsonBufferLength = reader.ReadInt32();
            dataSet.XmlBufferLength = reader.ReadInt32();
            dataSet.MaxSignaturesClosest = reader.ReadInt32();
        }

        /// <summary>
        /// Reads a date in year, month and day order from the reader.
        /// </summary>
        /// <param name="reader">Reader positioned at the start of the date</param>
        /// <returns>A date time with the year, month and day set from the reader</returns>
        private static DateTime ReadDate(BinaryReader reader)
        {
            return new DateTime(reader.ReadInt16(), reader.ReadByte(), reader.ReadByte());
        }
    }
}
