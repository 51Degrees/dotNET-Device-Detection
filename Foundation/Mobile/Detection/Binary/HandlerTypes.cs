/* *********************************************************************
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0.
 * 
 * If a copy of the MPL was not distributed with this file, You can obtain
 * one at http://mozilla.org/MPL/2.0/.
 * 
 * This Source Code Form is “Incompatible With Secondary Licenses”, as
 * defined by the Mozilla Public License, v. 2.0.
 * ********************************************************************* */

namespace FiftyOne.Foundation.Mobile.Detection.Binary
{
    /// <summary>
    /// An enumeration of the different handler types available.
    /// </summary>
    public enum HandlerTypes
    {
        /// <summary>
        /// The edit distance handler.
        /// </summary>
        EditDistance = 1,

        /// <summary>
        /// The regular expression handler.
        /// </summary>
        RegexSegment = 2,

        /// <summary>
        /// The reduced initial string handler.
        /// </summary>
        ReducedInitialString = 3
    }
}
