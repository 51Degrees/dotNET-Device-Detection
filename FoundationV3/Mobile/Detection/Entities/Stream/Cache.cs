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
 * This Source Code Form is "Incompatible With Secondary Licenses", as
 * defined by the Mozilla Public License, v. 2.0.
 * ********************************************************************* */

using System;
using System.Collections.Generic;
using System.Threading;
using System.Collections.Concurrent;

namespace FiftyOne.Foundation.Mobile.Detection.Entities.Stream
{
    /// <summary>
    /// Cache class used by <see cref="FixedCacheList{T,D}"/> and 
    /// <see cref="CacheList{T,D}"/> to cache frequently accessed items.
    /// </summary>
    /// <typeparam name="T">
    /// The type of item the cache will contain.
    /// </typeparam>
    internal class Cache<T> : Cache<int, T>
    {
        /// <summary>
        /// Constructs a new instance of <see cref="Cache{T}"/> for
        /// use with entities.
        /// </summary>
        /// <param name="cacheSize">
        /// The number of items to store in the cache.
        /// </param>
        /// <param name="loader">
        /// Loader used to fetch items not in the cache.
        /// </param>
        internal Cache(int cacheSize, ICacheLoader<int, T> loader) : base(cacheSize, loader)
        {
        }
    }
}
