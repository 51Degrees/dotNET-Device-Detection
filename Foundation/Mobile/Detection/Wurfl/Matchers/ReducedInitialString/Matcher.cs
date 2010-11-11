/* *********************************************************************
 * The contents of this file are subject to the Mozilla Public License 
 * Version 1.1 (the "License"); you may not use this file except in 
 * compliance with the License. You may obtain a copy of the License at 
 * http://www.mozilla.org/MPL/
 * 
 * Software distributed under the License is distributed on an "AS IS" 
 * basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 * See the License for the specific language governing rights and 
 * limitations under the License.
 *
 * The Original Code is named .NET Mobile API, first released under 
 * this licence on 11th March 2009.
 * 
 * The Initial Developer of the Original Code is owned by 
 * 51 Degrees Mobile Experts Limited. Portions created by 51 Degrees 
 * Mobile Experts Limited are Copyright (C) 2009 - 2010. All Rights Reserved.
 * 
 * Contributor(s):
 *     James Rosewell <james@51degrees.mobi>
 * 
 * ********************************************************************* */

#region Usings

using FiftyOne.Foundation.Mobile.Detection.Wurfl.Handlers;

#endregion

#if VER4
using System.Linq;
#endif

namespace FiftyOne.Foundation.Mobile.Detection.Wurfl.Matchers.ReducedInitialString
{
    internal class Matcher
    {
        /// <summary>
        /// Uses a reduced initial string matching routine to determine the results.
        /// </summary>
        /// <param name="userAgent">The useragent to be matched.</param>
        /// <param name="handler">The handler performing the match.</param>
        /// <param name="tolerance">The number of characters that need to be the same at the begining of the string for a match to have occurred.</param>
        /// <returns>All the devices that matched.</returns>
        internal static Results Match(string userAgent, Handler handler, int tolerance)
        {
            DeviceInfo bestMatch = null;
            int maxInitialString = 0;
            lock (handler.UserAgents)
            {
#if VER4
                foreach (DeviceInfo device in handler.UserAgents.SelectMany(devices => devices))
                {
                    Check(userAgent, ref bestMatch, ref maxInitialString, device);
                }
#elif VER2
                foreach (var devices in handler.UserAgents)
                {
                    foreach (DeviceInfo device in devices)
                    {
                        Check(userAgent, ref bestMatch, ref maxInitialString, device);
                    }
                }
#endif
            }

            return maxInitialString >= tolerance ? new Results(bestMatch) : null;
        }

        /// <summary>
        /// Checks to see if the current device or the useragent string contain each
        /// other at the start.
        /// </summary>
        /// <param name="userAgent">The useragent being searched for.</param>
        /// <param name="bestMatch">Reference to the best matching device so far.</param>
        /// <param name="maxInitialString">The maximum number of characters that have matched in the search so far.</param>
        /// <param name="current">The current device being checked for a match.</param>
        private static void Check(string userAgent, ref DeviceInfo bestMatch, ref int maxInitialString,
                                  DeviceInfo current)
        {
            if ((userAgent.StartsWith(current.UserAgent) ||
                 current.UserAgent.StartsWith(userAgent)) &&
                maxInitialString < current.UserAgent.Length)
            {
                maxInitialString = current.UserAgent.Length;
                bestMatch = current;
            }
        }
    }
}