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

using System.Collections.Generic;
using FiftyOne.Foundation.Mobile.Detection.Entities;

namespace FiftyOne.Foundation.Mobile.Detection
{
    /// <summary>
    /// Provides the ability to efficiently retrieve the items from the list
    /// using a ranged enumerable.
    /// </summary>
    /// <typeparam name="T">Type of entity the list contains.</typeparam>
    public interface IFixedList<T> : IReadonlyList<T> where T : BaseEntity
    {
        /// <summary>
        /// Returns an enumerable starting at the index provided until
        /// count number of iterations have been performed.
        /// </summary>
        /// <param name="index">Start index in the fixed list.</param>
        /// <param name="count">Number of iterations to perform.</param>
        /// <returns>An enumerable to iterate over the range specified.</returns>
        IEnumerable<T> GetRange(int index, int count);
    }
}
