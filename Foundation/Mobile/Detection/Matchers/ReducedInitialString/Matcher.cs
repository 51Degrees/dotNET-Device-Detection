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

#region Usings

using FiftyOne.Foundation.Mobile.Detection.Handlers;

#endregion

#if VER4
using System.Linq;
#endif

namespace FiftyOne.Foundation.Mobile.Detection.Matchers.ReducedInitialString
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
            BaseDeviceInfo bestMatch = null;
            int maxInitialString = 0;
            lock (handler.Devices)
            {
                foreach (BaseDeviceInfo[] devices in handler.Devices.Values)
                {
                    foreach (BaseDeviceInfo device in devices)
                    {
                        Check(userAgent, ref bestMatch, ref maxInitialString, device);
                    }
                }
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
        private static void Check(string userAgent, ref BaseDeviceInfo bestMatch, ref int maxInitialString,
                                  BaseDeviceInfo current)
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