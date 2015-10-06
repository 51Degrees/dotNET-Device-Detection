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

using System.Collections.Generic;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace FiftyOne.Foundation.Mobile.Detection
{
    /// <summary>
    /// Many of the entities used by the detector data set are requested repeatably. 
    /// The cache improves memory usage and reduces strain on the garbage collector
    /// by storing previously requested entities for a short period of time to avoid the
    /// need to refetch them from the underlying storage mechanisim.
    /// </summary>
    /// <para>
    /// The cache works by maintaining two dictionaries of entities keyed on their offset
    /// or index. The inactive list contains all items requested since the cache was
    /// created or last serviced. The active list contains all the items currently in 
    /// the cache. The inactive list is always updated when an item is requested.
    /// </para>
    /// <para>
    /// When the cache is serviced the active list is destroyed and the inactive list
    /// becomes the active list. i.e. all the items that were requested since the cache
    /// was last serviced are now in the cache. A new inactive list is created to store
    /// all items being requested since the cache was last serviced.
    /// </para>
    /// <typeparam name="K">Key for the cache items</typeparam>
    /// <typeparam name="V">Value for the cache items</typeparam>
    internal class Cache<K, V>
    {
        #region Fields

        /// <summary>
        /// The active list of cached items.
        /// </summary>
        internal ConcurrentDictionary<K, V> _itemsActive;

        /// <summary>
        /// The list of inactive cached items.
        /// </summary>
        internal ConcurrentDictionary<K, V> _itemsInactive;

        /// <summary>
        /// When this number of items are in the cache the lists should
        /// be switched.
        /// </summary>
        private readonly int _cacheServiceSize;

        /// <summary>
        /// The number of items the cache lists should have capacity for.
        /// </summary>
        private readonly int _cacheSize;

        /// <summary>
        /// The number of requests made to the cache.
        /// </summary>
        internal long Requests;

        /// <summary>
        /// The number of times an item was not available.
        /// </summary>
        internal long Misses;

        /// <summary>
        /// The number of times the cache was switched.
        /// </summary>
        internal long Switches;

        /// <summary>
        /// Indicates a switch operation is in progress.
        /// </summary>
        private bool _swtiching = false;

        #endregion

        #region Properties

        /// <summary>
        /// Percentage of cache misses.
        /// </summary>
        internal double PercentageMisses
        {
            get
            {
                return (double)Misses / (double)Requests;
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Constucts a new instance of the cache.
        /// </summary>
        /// <param name="cacheSize">The number of items to store in the cache</param>
        internal Cache(int cacheSize)
        {
            _cacheSize = cacheSize;
            _cacheServiceSize = cacheSize / 2;
            _itemsInactive = new ConcurrentDictionary<K, V>(Environment.ProcessorCount, cacheSize);
            _itemsActive = new ConcurrentDictionary<K, V>(Environment.ProcessorCount, cacheSize);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Indicates an item has been retrieved from the data set and should
        /// be reset in the cache so it's not removed at the next service.
        /// If the inactive cache is now more than 1/2 the total cache size
        /// the the lists should be switched.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        internal void AddRecent(K key, V value)
        {
            _itemsInactive[key] = value;
            if (_itemsInactive.Count > _cacheServiceSize &&
                _swtiching == false)
            {
                lock (this)
                {
                    if (_itemsInactive.Count > _cacheServiceSize &&
                        _swtiching == false)
                    {
                        ThreadPool.QueueUserWorkItem(ServiceCache, this);
                        _swtiching = true;
                    }
                }
            }
        }

        /// <summary>
        /// Locks the cache before removing old items from the cache.
        /// Call when the cache becomes full and needs to be switched.
        /// </summary>
        /// <param name="state">Reference to the cache being serviced.</param>
        private static void ServiceCache(object state)
        {
            var cache = (Cache<K, V>)state;

            // Create a temporary copy of the new active list.
            var temp = cache._itemsInactive;

            // Clear the inactive list.
            cache._itemsInactive.Clear();

            // Switch over the cached items dictionaries.
            cache._itemsActive = temp;

            // Increase the switch count for the cache.
            cache.Switches++;

            // Allow other switch operations to proceed.
            cache._swtiching = false;
        }

        #endregion
    }
}
