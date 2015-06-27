/* *********************************************************************
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited. 
 * Copyright © 2014 51Degrees Mobile Experts Limited, 5 Charlotte Close,
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

namespace FiftyOne.Foundation.Mobile.Detection.Entities.Stream
{
    /// <summary>
    /// Lists that use a cache can return information about the cache
    /// misses for performance analysis. They implement this interface
    /// to provide this data to the <see cref="DataSet"/>.
    /// </summary>
    internal interface ICacheList
    {
        /// <summary>
        /// Returns the percentage of cache misses.
        /// </summary>
        double PercentageMisses { get; }

        /// <summary>
        /// The number of times the lists have been switched.
        /// </summary>
        long Switches { get; }

        /// <summary>
        /// Resets the cache used by the list.
        /// </summary>
        void ResetCache();
    }
}
