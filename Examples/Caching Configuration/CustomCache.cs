/**
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
using System;
using System.Runtime.Caching;
using System.Threading;

namespace Caching_Configuration
{
    /// <summary>
    /// Class that adapts the .NET <see cref="MemoryCache"/> to the 51Degrees
    /// <see cref="IPutCache{K, V}"/> interface.
    /// A sliding expiry is used to evict items that are not accessed 
    /// for 10 minutes.
    /// </summary>
    /// <typeparam name="K">
    /// The type of the key used in the cache
    /// </typeparam>
    /// <typeparam name="V">
    /// The type of the objects stored in the cache
    /// </typeparam>
    public class CustomCache<K, V> : IPutCache<K, V>
    {
        /// <summary>
        /// The cache
        /// </summary>
        private MemoryCache _cache;
        /// <summary>
        /// The cache eviction policy
        /// </summary>
        private CacheItemPolicy _policy;

        /// <summary>
        /// Number of requests to the cache
        /// </summary>
        private long _requestCount;
        /// <summary>
        /// Number of cache misses
        /// </summary>
        private long _missCount;

        /// <summary>
        /// Constructor
        /// </summary>
        public CustomCache()
        {
            // initialise the cache
            _cache = new MemoryCache("CustomCache");

            // Create the eviction policy object
            _policy = new CacheItemPolicy();
            _policy.SlidingExpiration = new TimeSpan(0, 10, 0);
        }

        /// <summary>
        /// Get the percentage of cache misses
        /// </summary>
        public double PercentageMisses
        {
            get
            {
                return _missCount / _requestCount;
            }
        }

        /// <summary>
        /// Get the specified data item from the cache
        /// </summary>
        /// <param name="key">
        /// The key of the data item
        /// </param>
        /// <returns>
        /// The data item
        /// </returns>
        public V this[K key]
        {
            get
            {
                Interlocked.Increment(ref _requestCount);
                return (V)_cache[key.ToString()];
            }
        }

        /// <summary>
        /// Put the specified data item into the cache using the specified key.
        /// This is called by the 51Degrees API when a cache miss occurs
        /// </summary>
        /// <param name="key">
        /// The key
        /// </param>
        /// <param name="value">
        /// The data item
        /// </param>
        public void Put(K key, V value)
        {
            Interlocked.Increment(ref _missCount);
            _cache.Set(key.ToString(), value, _policy);
        }

        /// <summary>
        /// Clear all items from the cache
        /// </summary>
        public void ResetCache()
        {
            _cache.Dispose();
            _cache = new MemoryCache("CustomCache");
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            _cache.Dispose();
        }

    }
}
