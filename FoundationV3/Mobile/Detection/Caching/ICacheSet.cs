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

using System.Collections.Generic;

namespace FiftyOne.Foundation.Mobile.Detection.Caching
{
    /// <summary>
    /// Provides a method that allows the caller to obtain a 
    /// dictionary providing cache configruation for the 
    /// 51Degrees API.
    /// </summary>
    public interface ICacheSet
    {
        /// <summary>
        /// Get cache configuration that can be used by the 51 Degrees
        /// API to construct it's internal caches.
        /// </summary>
        /// <returns>
        /// A dictionary mapping <see cref="CacheType"/> to 
        /// <see cref="ICacheOptions"/>
        /// </returns>
        Dictionary<CacheType, ICacheOptions> GetCacheConfiguration();
    }
}
