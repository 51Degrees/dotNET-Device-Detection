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
    /// A cache that supports a thread safe put method for inserting to cache.
    /// By contrast, for example, <see cref="LruCache{K, V}"/> is a loading 
    /// cache, it automaticlly updates itelf by being provided with a data 
    /// loader.
    /// </summary>
    /// <typeparam name="K">
    /// The type of the key to use for the data in the cache
    /// </typeparam>
    /// <typeparam name="V">The type of the data in teh cache</typeparam>
    public interface IPutCache<K, V> : ICache<K, V>
    {
        /// <summary>
        /// Add an item to the cache
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="value">The data</param>
        void Put(K key, V value);
    }
}
