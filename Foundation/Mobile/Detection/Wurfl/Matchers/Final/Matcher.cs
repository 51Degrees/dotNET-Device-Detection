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

#region

using System.Collections.Generic;

#endregion

#if VER4
using System.Linq;
#endif

namespace FiftyOne.Foundation.Mobile.Detection.Wurfl.Matchers.Final
{
    internal class Matcher
    {
        /// <summary>
        /// Examines each string in the list to find the one that has the highest number of
        /// initial matching characters. If only one is found this is returned. If more than
        /// one have the same number of matches are found further analysis is performed
        /// on the non matching parts or string tails.
        /// </summary>
        /// <param name="userAgent">userAgent to be found</param>
        /// <param name="results">list of possible devices to match against</param>
        /// <returns>The closest matching device</returns>
        internal static DeviceInfo Match(string userAgent, Results results)
        {
            int pos = 0;
            int highestPosition = 0;
            List<DeviceInfo> subset = new List<DeviceInfo>();
            foreach (Result result in results)
            {
                // Find the shortest length and compare characters
                // upto this point.
                int length = result.Device.UserAgent.Length > userAgent.Length
                                 ?
                                     userAgent.Length
                                 : result.Device.UserAgent.Length;
                // For each character check equality. If the characters
                // aren't equal record this position.
                for (pos = 0; pos < length; pos++)
                {
                    if (userAgent[pos] != result.Device.UserAgent[pos])
                        break;
                }
                // If this position is greater than the highest position so
                // far than empty the list of subsets found and record this
                // result in addition to the new highest position.
                if (pos > highestPosition)
                {
                    highestPosition = pos;
                    subset.Clear();
                    subset.Add(result.Device);
                }
                    // If the position is the same as the best one found so far
                    // then add it to the results.
                else if (pos == highestPosition)
                {
                    subset.Add(result.Device);
                }
            }
            if (subset.Count == 1)
                return subset[0];
            else if (subset.Count > 1)
            {
                return MatchTails(userAgent, highestPosition, subset);
            }
            return null;
        }

        private static DeviceInfo MatchTails(string userAgent, int pos, List<DeviceInfo> devices)
        {
            int longestSubset = 0;
            Queue<string> tails = new Queue<string>();

            // Get the tails of all the strings and add them to the queue.
#if VER4
            foreach (string tail in
                devices.Select(device => device.UserAgent.Substring(pos, device.UserAgent.Length - pos)))
            {
                tails.Enqueue(tail);
                if (tail.Length > longestSubset)
                    longestSubset = tail.Length;
            }
#elif VER2
            foreach (DeviceInfo device in devices)
            {
                string tail = device.UserAgent.Substring(pos, device.UserAgent.Length - pos);
                tails.Enqueue(tail);
                if (tail.Length > longestSubset)
                    longestSubset = tail.Length;
            }
#endif
            // Get the longest part of the tail needed.
            string userAgentTail = userAgent.Substring(pos,
                                                       longestSubset + pos < userAgent.Length
                                                           ? longestSubset
                                                           : userAgent.Length - pos);

            // Find the tail with the closest edit distance match.
            string closestTail = null;
            int minDistance = int.MaxValue;
            while (tails.Count > 0)
            {
                string current = tails.Dequeue();
                int currentDistance = Algorithms.EditDistance(userAgentTail, current, minDistance);
                if (currentDistance < minDistance)
                {
                    minDistance = currentDistance;
                    closestTail = current;
                }
            }

            // Find the matching useragent and return.
#if VER4
            foreach (DeviceInfo device in devices.Where(device => device.UserAgent.EndsWith(closestTail)))
            {
                return device;
            }
#elif VER2
            foreach (DeviceInfo device in devices)
            {
                if (device.UserAgent.EndsWith(closestTail))
                    return device;
            }
#endif
            // Give up and return the 1st element!
            return devices[0];
        }
    }
}