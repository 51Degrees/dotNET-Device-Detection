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

using System.Collections.Generic;

#if VER4 || VER35

using System.Linq;

#endif

#endregion

namespace FiftyOne.Foundation.Mobile.Detection.Matchers.Final
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
        /// <returns>The closest matching result</returns>
        internal static Result Match(string userAgent, Results results)
        {
            int pos = 0;
            int highestPosition = 0;
            List<Result> subset = new List<Result>();
            foreach (Result result in results)
            {
                // Find the shortest length and compare characters
                // upto this point.
                int length = result.Device.UserAgent.Length > userAgent.Length
                                 ? userAgent.Length
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
                    subset.Add(result);
                }
                // If the position is the same as the best one found so far
                // then add it to the results.
                else if (pos == highestPosition)
                {
                    subset.Add(result);
                }
            }

            // If only one is found return it.
            if (subset.Count == 1)
                return subset[0];

            // If there is an exact match return it.
            if (highestPosition == userAgent.Length)
            {
#if VER35 || VER4
                var res = subset.FirstOrDefault(i => i.Device.UserAgent == userAgent);
                if (res != null)
                {
                    return res;
                }
            }

#else
                foreach (Result result in subset)
                    if (result.Device.UserAgent == userAgent)
                        return result;
            }
#endif

            // If there are more than 1 find the best one based on the end
            // of the useragent strings.
            if (subset.Count > 1)
                return MatchTails(userAgent, highestPosition, subset);
            return null;
        }

        private static Result MatchTails(string userAgent, int pos, List<Result> results)
        {
            int longestSubset = 0;
            Queue<string> tails = new Queue<string>();

            // Get the tails of all the strings and add them to the queue.
#if VER4 || VER35
            foreach (string tail in
                results.Select(i => 
                    i.Device.UserAgent.Substring(pos, i.Device.UserAgent.Length - pos)))
            {
                tails.Enqueue(tail);
                if (tail.Length > longestSubset)
                    longestSubset = tail.Length;
            }
#else
            foreach (Result res in results)
            {
                string tail = res.Device.UserAgent.Substring(pos, res.Device.UserAgent.Length - pos);
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
            int[][] rows = new int[][] { new int[userAgentTail.Length + 1], new int[userAgentTail.Length + 1] };
            while (tails.Count > 0)
            {
                string current = tails.Dequeue();
                int currentDistance = Algorithms.EditDistance(rows, userAgentTail, current, minDistance);
                if (currentDistance < minDistance)
                {
                    minDistance = currentDistance;
                    closestTail = current;
                }
            }

            // Find the 1st matching useragent and return.
            Result result = null;
#if VER35 || VER4
            result = results.Find(i => 
                i.Device.UserAgent.EndsWith(closestTail));
#else
            foreach (Result res in results)
                if (res.Device.UserAgent.EndsWith(closestTail))
                    result = res;
#endif
            if (result != null)
                return result;

            // Give up and return the 1st element!
            return results[0];
        }
    }
}