/* *********************************************************************
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited. 
 * Copyright © 2017 51Degrees Mobile Experts Limited, 5 Charlotte Close,
 * Caversham, Reading, Berkshire, United Kingdom RG4 7BY
 * 
 * This Source Code Form is the subject of the following patents and patent 
 * applications, owned by 51Degrees Mobile Experts Limited of 5 Charlotte
 * Close, Caversham, Reading, Berkshire, United Kingdom RG4 7BY: 
 * European Patent No. 2871816;
 * European Patent Application No. 17184134.9;
 * United States Patent Nos. 9,332,086 and 9,350,823; and
 * United States Patent Application No. 15/686,066.
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
using System;

namespace FiftyOne.Foundation.Mobile.Detection.Factories
{
    /// <summary>
    /// Factory class used to create an <see cref="IndirectDataSet"/> from a source data
    /// structure. All the entities are held in the persistent store and only
    /// loads into memory when required. A cache mechanisim is used to improve
    /// efficiency as many entities are frequently used in a high volume
    /// environment.
    /// <para>
    /// The data set will be initialised very quickly as only the header
    /// information is read. Entities are then created when requested by the
    /// detection process and stored in a cache to avoid being recreated if
    /// their requested again after a short period of time.
    /// </para>
    /// A dataset can be created in several ways:
    /// <list>
    /// <item>Using a data file:
    /// <code>
    /// DataSet dataSet = StreamFactory.Create("path_to_file", false);
    /// </code>
    /// <para>
    /// Where the boolean flag indicates if the data file should or should not 
    /// be deleted when close() is invoked.
    /// </para>
    /// </item>
    /// <item>Using a byte array:
    /// <code>
    /// DataSet dataSet = StreamFactory.Create(dataFileAsByteArray);
    /// </code>
    /// <para>Where the byte array is the 51Degrees device data file read 
    /// into a byte array.
    /// </para>
    /// </item>
    /// </list>
    /// <para>
    /// For more information see https://51degrees.com/Support/Documentation/Net
    /// </para>
    /// </summary>
    /// <remarks>
    /// The very small data structures , Properties and Components are always
    /// stored in memory as there is no benefit retrieving them every time they're 
    /// needed.
    /// </remarks>
    public static class StreamFactory
    {
        #region Public Methods

        /// <summary>
        /// Creates a new <see cref="IndirectDataSet"/> from the byte array.
        /// </summary>
        /// <param name="array">Array of bytes to build the data set from
        /// </param>
        /// <returns>
        /// A <see cref="IndirectDataSet"/> configured to read entities from the array 
        /// when required
        /// </returns>
        public static IndirectDataSet Create(byte[] array)
        {
            return DataSetBuilder.Buffer()
                .ConfigureDefaultCaches()
                .Build(array);
        }

        /// <summary>
        /// Creates a new <see cref="IndirectDataSet"/> from the file provided. The
        /// last modified date of the data set is the last write time of the
        /// data file provided.
        /// </summary>
        /// <param name="filePath">Uncompressed file containing the data for
        /// the data set</param>
        /// <returns>
        /// A <see cref="IndirectDataSet"/>configured to read entities from the file
        /// path when required
        /// </returns>
        public static IndirectDataSet Create(string filePath)
        {
            return Create(filePath, File.GetLastWriteTimeUtc(filePath), false);
        }

        /// <summary>
        /// Creates a new <see cref="IndirectDataSet"/> from the file provided. The
        /// last modified date of the data set is the last write time of th
        /// data file provided.
        /// </summary>
        /// <param name="filePath">Uncompressed file containing the data for
        /// the data set</param>
        /// <param name="isTempFile">True if the file should be deleted when
        /// the source is disposed</param>
        /// <returns>
        /// A <see cref="IndirectDataSet"/>configured to read entities from the file
        /// path when required
        /// </returns>
        public static IndirectDataSet Create(string filePath, bool isTempFile)
        {
            return Create(filePath, File.GetLastWriteTimeUtc(filePath), isTempFile);
        }

        /// <summary>
        /// Creates a new <see cref="IndirectDataSet"/> from the file provided.
        /// </summary>
        /// <param name="filePath">Uncompressed file containing the data for
        /// the data set</param>
        /// <param name="lastModified">Date and time the source data was
        /// last modified.</param>
        /// <returns>
        /// A <see cref="IndirectDataSet"/>configured to read entities from the file
        /// path when required
        /// </returns>
        public static IndirectDataSet Create(string filePath, DateTime lastModified)
        {
            return Create(filePath, File.GetLastWriteTimeUtc(filePath), false);
        }

        /// <summary>
        /// Creates a new <see cref="IndirectDataSet"/> from the file provided.
        /// </summary>
        /// <param name="filePath">Uncompressed file containing the data for
        /// the data set</param>
        /// <param name="lastModified">Date and time the source data was last
        /// modified.</param>
        /// <param name="isTempFile">True if the file should be deleted when
        /// the source is disposed</param>
        /// <returns>
        /// A <see cref="IndirectDataSet"/>configured to read entities from the file
        /// path when required
        /// </returns>
        public static IndirectDataSet Create(string filePath, DateTime lastModified, bool isTempFile)
        {
            return DataSetBuilder.File()
                .ConfigureDefaultCaches()
                .SetTempFile(isTempFile)
                .LastModified(lastModified)
                .Build(filePath);
        }
              
        #endregion
        
        
        
    }
}
