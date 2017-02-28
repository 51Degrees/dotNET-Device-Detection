/* *********************************************************************
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited.
 * Copyright (c) 2015 51Degrees Mobile Experts Limited, 5 Charlotte Close,
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

namespace FiftyOne.Foundation.Mobile.Detection.Caching
{
    /// <summary>
    /// Implementation of <see cref="ICacheBuilder"/> for 
    /// <see cref="LruCache{K, V}"/> caches.
    /// </summary>
    public class LruCacheBuilder : ILoadingCacheBuilder
    {
        /// <summary>
        /// Build and return an <see cref="LruCache{K, V}"/> 
        /// </summary>
        /// <typeparam name="K">
        /// The type to use as the key for the cache
        /// </typeparam>
        /// <typeparam name="V">
        /// The type of data that will be stored in the cache
        /// </typeparam>
        /// <param name="cacheSize">
        /// The maximum number of entries in the cache before the least 
        /// recently used will be evicted.
        /// </param>
        /// <returns>
        /// A new <see cref="LruCache{K, V}"/> 
        /// </returns>
        public ILoadingCache<K, V> Build<K, V>(int cacheSize)
        {
            LruCache<K, V> cache;
            cache = new LruCache<K, V>(cacheSize);
            return cache;
        }

        /// <summary>
        /// Build and return an <see cref="LruCache{K, V}"/> 
        /// </summary>
        /// <typeparam name="K">
        /// The type to use as the key for the cache
        /// </typeparam>
        /// <typeparam name="V">
        /// The type of data that will be stored in the cache
        /// </typeparam>
        /// <param name="cacheSize">
        /// The maximum number of entries in the cache before the least 
        /// recently used will be evicted.
        /// </param>
        /// <returns>
        /// A new <see cref="LruCache{K, V}"/> 
        /// </returns>
        ICache<K, V> ICacheBuilder.Build<K, V>(int cacheSize)
        {
            return Build<K, V>(cacheSize);
        }
    }
}
