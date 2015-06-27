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
using System.Collections.Generic;
using System.IO;
using FiftyOne.Foundation.Mobile.Detection.Factories;
using FiftyOne.Foundation.Mobile.Detection.Readers;

namespace FiftyOne.Foundation.Mobile.Detection.Entities.Stream
{
    /// <summary>
    /// A readonly list of fixed length entity types held on persistent storage rather
    /// than in memory.
    /// </summary>
    /// <para>
    /// Entities in the underlying data structure are either fixed length where the 
    /// data that represents them always contains the same number of bytes, or variable
    /// length where the number of bytes to represent the entity varies.
    /// </para>
    /// <para>
    /// This class uses the index of the entity in the accessor. The list is typically
    /// used by entities that need to be found quickly using a divide and conquer 
    /// algorithm.
    /// </para>
    /// <remarks>
    /// The constructor will read the header information about the underlying data structure.
    /// The data for each entity is only loaded when requested via the accessor. A cache is used
    /// to avoid creating duplicate objects when requested multiple times.
    /// </remarks>
    /// <remarks>
    /// Data sources which don't support seeking can not be used. Specifically compressed data 
    /// structures can not be used with these lists.
    /// </remarks>
    /// <remarks>
    /// Should not be referenced directly.
    /// </remarks>
    /// <typeparam name="T">The type of <see cref="BaseEntity"/> the list will contain</typeparam>
    public class FixedCacheList<T> : FixedList<T>, ICacheList where T : BaseEntity
    {
        #region Fields

        /// <summary>
        /// Used to store previously accessed items to improve performance and
        /// reduce memory consumption associated with creating new instances of 
        /// entities already in use.
        /// </summary>
        internal readonly Cache<T> _cache;

        #endregion

        #region Properties

        /// <summary>
        /// Percentage of request that were not already held in the cache.
        /// </summary>
        double ICacheList.PercentageMisses
        {
            get { return _cache != null ? _cache.PercentageMisses : 0; }
        }

        /// <summary>
        /// The number of times the cache has been switched.
        /// </summary>
        long ICacheList.Switches
        {
            get { return _cache != null ? _cache.Switches : 0; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a new instance of <see cref="FixedCacheList{T}"/>
        /// </summary>
        /// <param name="dataSet">The <see cref="DataSet"/> being created</param>
        /// <param name="reader">Reader connected to the source data structure and positioned to start reading</param>
        /// <param name="entityFactory">Used to create new instances of the entity</param>
        /// <param name="cacheSize">Number of items in list to have capacity to cache</param>
        internal FixedCacheList(
            DataSet dataSet, 
            Reader reader, 
            BaseEntityFactory<T> entityFactory,
            int cacheSize)
            : base(dataSet, reader, entityFactory)
        {
            _cache = new Cache<T>(cacheSize);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Resets the cache list stats for the list.
        /// </summary>
        public void ResetCache()
        {
            _cache.ResetCache();
        }

        /// <summary>
        /// Retrieves the entity at the offset or index requested.
        /// </summary>
        /// <param name="key">Index or offset of the entity required</param>
        /// <returns>A new instance of the entity at the offset or index</returns>
        public override T this[int key]
        {
            get
            {
                T item;
                // No need to lock the dictionaries as they support concurrency.
                if (_cache._itemsActive.TryGetValue(key, out item) == false)
                {
                    item = base[key];
                    _cache._itemsActive[key] = item;
                    _cache.Misses++;
                }
                _cache.AddRecent(item);
                _cache.Requests++;
                return item;
            }
        }

        #endregion



    }
}
