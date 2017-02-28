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
    /// Extension of general cache contract to provide for getting a value with
    /// a particular value loaded. Primarily used to allow the value loader to 
    /// be an already instantiated value of the type V to avoid construction
    /// costs of that value.
    /// (In other words the loader has the signature. " where V : IValueLoader").
    /// Used only in UA Matching.
    /// </summary>
    /// <typeparam name="K">
    /// The type of the key for the data being loaded
    /// </typeparam>
    /// <typeparam name="V">
    /// The type of the data being loaded
    /// </typeparam>
    public interface ILoadingCache<K, V> : ICache<K, V>
    {
        /// <summary>
        /// Get the value using the specified key and calling the specified loader 
        /// if needed.
        /// </summary>
        /// <param name="key">The key of the value to load</param>
        /// <param name="loader">
        /// The loader to use when getting the value
        /// </param>
        /// <returns>
        /// The value from the cache, or loader if not available.
        /// </returns>
        V this[K key, IValueLoader<K, V> loader] { get; }
    }
}
