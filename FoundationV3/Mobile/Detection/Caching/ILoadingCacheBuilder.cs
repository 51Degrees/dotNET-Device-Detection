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

namespace FiftyOne.Foundation.Mobile.Detection.Caching
{
    /// <summary>
    /// Provides a method to build caches that implement 
    /// <see cref="ILoadingCache{K, V}"/> 
    /// </summary>
    public interface ILoadingCacheBuilder : ICacheBuilder
    {
        /// <summary>
        /// Build and return an <see cref="ILoadingCache{K, V}"/>  
        /// </summary>
        /// <typeparam name="K">
        /// The type to use as the key for the cache
        /// </typeparam>
        /// <typeparam name="V">
        /// The type of data that will be stored in the cache
        /// </typeparam>
        /// <param name="cacheSize">
        /// The maximum number of entries that will be stored in the cache.
        /// </param>
        /// <returns>
        /// A cache imlpementing <see cref="ILoadingCache{K, V}"/> 
        /// </returns>
        new ILoadingCache<K, V> Build<K, V>(int cacheSize);
    }
}
