/* *********************************************************************
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited.
 * Copyright (c) 2017 51Degrees Mobile Experts Limited, 5 Charlotte Close,
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
 * This Source Code Form is "Incompatible With Secondary Licenses", as
 * defined by the Mozilla Public License, v. 2.0.
 */

using FiftyOne.Foundation.Mobile.Detection.Caching;
using FiftyOne.Foundation.Mobile.Detection.Entities.Stream;
using FiftyOne.Foundation.Mobile.Detection.Readers;
using System;

namespace FiftyOne.Foundation.Mobile.Detection
{
    /// <summary>
    /// A data set which includes a pool of data readers that are used to fetch
    /// data from the source when the data set is used to retrieve data not 
    /// already in memory. It also provides for caching of values.
    /// </summary>
    /// <para>
    /// Class provides extra methods to assess status of readers and to
    /// evaluate the caches.
    /// </para>
    /// <para>
    /// This class superseeds <see cref="Entities.Stream.DataSet"/> 
    /// </para>
    public class IndirectDataSet : Entities.DataSet, IStreamDataSet
    {        
        #region Fields

        /// <summary>
        /// The source of readers used by the pool.
        /// </summary>
        internal readonly SourceBase Source;

        /// <summary>
        /// Pool of readers connected the underlying data file.
        /// </summary>
        public Pool Pool { get; internal set; }

        #endregion 

        #region Internal Properties

        /// <summary>
        /// Advises FindProfiles methods that the profiles associated with 
        /// a value should be referenced explicitly.
        /// </summary>
        internal override bool FindProfilesInitialiseValueProfiles
        {
            get
            {
                return false;
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The number of readers that have been created in the pool
        /// that connects the data set to the data source.
        /// </summary>
        public int ReadersCreated
        {
            get { return Pool.ReadersCreated; }
        }

        /// <summary>
        /// The number of readers in the queue ready to be used.
        /// </summary>
        public int ReadersQueued
        {
            get { return Pool.ReadersQueued; }
        }

        /// <summary>
        /// The caches used by this dataset
        /// </summary>
        internal CacheMap CacheMap { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new stream <see cref="Entities.DataSet"/> connected to the data
        /// file provided.
        /// </summary>
        /// <param name="fileName">
        /// Valid path to the uncompressed data set file.
        /// </param>
        /// <param name="lastModified">
        /// Date and time the source data was last modified.
        /// </param>
        /// <param name="mode">
        /// The mode of operation the data set will be using.
        /// </param>
        /// <param name="isTempFile">
        /// True if the file should be deleted when the source is disposed.
        /// </param>
        internal IndirectDataSet(
            string fileName,
            DateTime lastModified,
            Modes mode, 
            bool isTempFile)
            : base(lastModified, mode)
        {
            Source = new SourceFile(fileName, isTempFile);
            Pool = new Pool(Source);
        }

        /// <summary>
        /// Creates a new stream data set connected to the byte array
        /// data source provided.
        /// </summary>
        /// <param name="data">
        /// Byte array containing uncompressed data set.
        /// </param>
        /// <param name="mode">
        /// The mode of operation the data set will be using.
        /// </param>
        internal IndirectDataSet(byte[] data, Modes mode)
            : base(DateTime.MinValue, mode)
        {
            Source = new SourceMemory(data);
            Pool = new Pool(Source);
        }

        #endregion

        #region Destructor

        /// <summary>
        /// Disposes of the data set closing all readers and streams in 
        /// the pool. If a temporary data file is used then the file
        /// is also deleted if it's not being used by other processes.
        /// </summary>
        /// <param name="disposing">
        /// True if the calling method is Dispose, false for the finaliser.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            Source.Dispose();
            base.Dispose(disposing);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Resets the caches for the data set.This is a "best efforts"
        /// operation that may not be supported by all underlying cache
        /// implementations.
        /// </summary>
        public override void ResetCache()
        {
            base.ResetCache();
            CacheMap.NodeCache.ResetCache();
            CacheMap.ProfileCache.ResetCache();
            CacheMap.SignatureCache.ResetCache();
            CacheMap.StringCache.ResetCache();
            CacheMap.ValueCache.ResetCache();
        }

        #endregion
    }

}
