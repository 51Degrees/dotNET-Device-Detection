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

namespace FiftyOne.Foundation.Mobile.Detection
{
    /// <summary>
    /// Enumerator of possible methods used to obtain the match.
    /// </summary>
    public enum MatchMethods
    {
        /// <summary>
        /// No match could be determined between the target User-Agent
        /// and the list of signatures. Defaults were used.
        /// </summary>
        None = 0,
        /// <summary>
        /// The signature returned matches precisely with the target user
        /// agent and was the only signature evaluated.
        /// </summary>
        Exact = 1,
        /// <summary>
        /// The signature returned matches the target User-Agent with only
        /// minor differences between numeric numbers.
        /// </summary>
        Numeric = 2,
        /// <summary>
        /// The signature returned contains all the same sub strings as the
        /// target User-Agent, but there are minor differences in position.
        /// </summary>
        Nearest = 3,
        /// <summary>
        /// No signature matched precisely and some relevant characters 
        /// may be different between the returned signature and the 
        /// target User-Agent. The <see cref="Match.Difference"/>
        /// property should be used to determine the degree of difference.
        /// </summary>
        Closest = 4
    }
}
