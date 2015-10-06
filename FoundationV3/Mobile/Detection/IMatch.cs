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
 * This Source Code Form is “Incompatible With Secondary Licenses”, as
 * defined by the Mozilla Public License, v. 2.0.
 * ********************************************************************* */

using System.Collections.Generic;

namespace FiftyOne.Foundation.Mobile.Detection
{
    /// <summary>
    /// Used internally to support match results from other platforms
    /// like Java, C and PHP.
    /// </summary>
    public interface IMatch
    {
        /// <summary>
        /// Gets MatchMethod used to obtain the match.
        /// </summary>
        MatchMethods Method { get; }

        /// <summary>
        /// Gets the number of signatures that were checked against the target
        /// user agent if the Closest Match Method was used.
        /// </summary>
        int SignaturesCompared { get; }

        /// <summary>
        /// Gets the user agent of the matching device with irrelevant 
        /// characters removed.
        /// </summary>
        string UserAgent { get; }

        /// <summary>
        /// Gets the matched profile ids, indexed by their component id.
        /// </summary>
        Dictionary<int, int> ProfileIds { get; }

        /// <summary>
        /// Gets the confidence of the match where 0 is 100% confident, and higher values 
        /// indicate less confidence. The value returned represents the amount of 
        /// difference between the target user agent and the one returned.
        /// </summary>
        int Difference { get; }
    }
}
