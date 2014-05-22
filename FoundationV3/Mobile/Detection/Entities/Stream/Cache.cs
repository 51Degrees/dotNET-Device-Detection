/* *********************************************************************
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited. 
 * Copyright 2014 51Degrees Mobile Experts Limited, 5 Charlotte Close,
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
 * ********************************************************************* */

using System;
using System.Collections.Generic;
using System.Threading;

#if VER4
using System.Collections.Concurrent;
#endif

namespace FiftyOne.Foundation.Mobile.Detection.Entities.Stream
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
    /// <typeparam name="T">The type of <see cref="BaseEntity"/> the cache will contain</typeparam>
    internal class Cache<T> : IDisposable where T : BaseEntity 
    {
        #region Fields

        /// <summary>
        /// The active list of cached items.
        /// </summary>
#if VER4
        internal ConcurrentDictionary<int, T> _itemsActive = new ConcurrentDictionary<int, T>();
#else
        internal Dictionary<int, T> _itemsActive = new Dictionary<int, T>();
#endif

        /// <summary>
        /// The list of inactive cached items.
        /// </summary>
#if VER4
        internal ConcurrentDictionary<int, T> _itemsInactive = new ConcurrentDictionary<int, T>();
#else
        internal Dictionary<int, T> _itemsInactive = new Dictionary<int, T>();
#endif

        /// <summary>
        /// Timer thread used to service the cache.
        /// </summary>
        private readonly Timer _cacheServiceTimer;

        /// <summary>
        /// The number of requests made to the cache.
        /// </summary>
        internal long Requests;

        /// <summary>
        /// The number of times an item was not available.
        /// </summary>
        internal long Misses;

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
        /// <param name="serviceCacheInternal">Time between cache services</param>
        internal Cache(TimeSpan serviceCacheInternal)
        {
            _cacheServiceTimer = new Timer(ServiceCache, this, serviceCacheInternal, serviceCacheInternal);
        }

        /// <summary>
        /// Disposes of the timer thread.
        /// </summary>
        public void Dispose()
        {
            _cacheServiceTimer.Dispose();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Indicates an item has been retrieved from the data set and should
        /// be reset in the cache so it's not removed at the next service.
        /// </summary>
        /// <param name="item">The item retrieved</param>
        internal void AddRecent(T item)
        {
            _itemsInactive[item.Index] = item;
        }

        /// <summary>
        /// Locks the cache before removing old items from the cache.
        /// Call by the timer thread at the intervals requested by the
        /// related list or dictionary.
        /// </summary>
        /// <param name="state">Reference to the cache</param>
        private static void ServiceCache(object state)
        {
            var cache = (Cache<T>)state;

            // Create a temporary copy of the new active list.
            var temp = cache._itemsInactive;

            // Create a new inactive items dictionary.
#if VER4
            cache._itemsInactive = new ConcurrentDictionary<int, T>();
#else
            cache._itemsInactive = new Dictionary<int, T>();
#endif

            // Switch over the cached items dictionaries.
            cache._itemsActive = temp;
        }

        #endregion
    }
}
