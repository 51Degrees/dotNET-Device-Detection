/* *********************************************************************
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited.
 * Copyright (c) 2017 51Degrees Mobile Experts Limited, 5 Charlotte Close,
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
 * This Source Code Form is "Incompatible With Secondary Licenses", as
 * defined by the Mozilla Public License, v. 2.0.
 */

using System;

namespace FiftyOne.Foundation.Mobile.Detection.Caching
{
    /// <summary>
    /// Represents a cache that can be used by device detection
    /// </summary>
    /// <typeparam name="K">Key for the cache items</typeparam>
    /// <typeparam name="V">Value for the cache items</typeparam>
    public interface ICache<K, V> : IDisposable
    {
        /// <summary>
        /// Get an item from the cache.
        /// </summary>
        /// <param name="key">Key of the item to return</param>
        /// <returns>
        /// Value associated with the key, or null if not available.
        /// </returns>
        V this[K key] { get; }

        /// <summary>
        /// Reset the cache (i.e. clear it and reset all associated stats, etc.)
        /// </summary>
        void ResetCache();

        /// <summary>
        /// Percentage of cache misses.
        /// </summary>
        double PercentageMisses { get; }
    }

}
