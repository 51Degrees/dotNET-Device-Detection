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

using System;

namespace FiftyOne.Foundation.Mobile.Detection
{
    /// <summary>
    /// Class used to manipulate a user agent profile url.
    /// </summary>
    public class UserAgentProfileUrlParser
    {
        /// <summary>
        /// Removes any speech marks from the user agent profile url string.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string CleanUserAgentProfileUrl(string value)
        {
            if (String.IsNullOrEmpty(value) == false)
                return value.Split(',')[0].Replace("\"", "");
            return value;
        }
    }
}
