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
using System.IO;
using System.Collections.Generic;
using System.Threading;
using FiftyOne.Foundation.Mobile.Detection.Entities.Headers;
using FiftyOne.Foundation.Mobile.Detection.Factories;
using FiftyOne.Foundation.Mobile.Detection.Readers;

namespace FiftyOne.Foundation.Mobile.Detection.Entities.Stream
{
    /// <summary>
    /// <para>
    /// Lists can be stored as a set of related objects entirely within memory, or 
    /// the relevent objects loaded as required from a file or other permanent store
    /// as required.
    /// </para>
    /// </summary>
    /// <para>
    /// This class provides core functions needed for lists which load objects
    /// as required. It implements the <see cref="Cache{T}"/> to store frequently requested
    /// objects and improve memory usage and performance.
    /// </para>
    /// <remarks>
    /// Delegate methods are used to create new instances of items to add to the list
    /// in order to avoid creating many inherited list classes for each 
    /// <see cref="BaseEntity"/> type.
    /// </remarks>
    /// <remarks>
    /// Should not be referenced directly.
    /// </remarks>
    /// <typeparam name="T">The type of <see cref="BaseEntity"/> the list will contain</typeparam>
    public abstract class CacheList<T> : BaseList<T>, ICacheList where T : BaseEntity
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
        /// Constructs a new instance of <see cref="BaseList{T}"/> ready to read entities from 
        /// the source.
        /// </summary>
        /// <param name="dataSet">Dataset being created</param>
        /// <param name="reader">Reader used to initialise the header only</param>
        /// <param name="entityFactory">Used to create new instances of the entity</param>
        /// <param name="cacheSize">Number of items in list to have capacity to cache</param>
        internal CacheList(
            DataSet dataSet, 
            Reader reader,
            BaseEntityFactory<T> entityFactory,
            int cacheSize) : base (dataSet, reader, entityFactory)
        {
            _cache = new Cache<T>(cacheSize);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Resets the cache.
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
