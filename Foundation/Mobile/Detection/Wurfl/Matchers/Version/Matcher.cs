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
 * Mobile Experts Limited are Copyright (C) 2009 - 2011. All Rights Reserved.
 * 
 * Contributor(s):
 *     James Rosewell <james@51degrees.mobi>
 * 
 * ********************************************************************* */

#region Usings

using System;
using System.Collections.Generic;
using FiftyOne.Foundation.Mobile.Detection.Wurfl.Handlers;

#endregion

namespace FiftyOne.Foundation.Mobile.Detection.Wurfl.Matchers.Version
{
    /// <summary>
    /// Finds the devices that's version segments are closest to the string being sought.
    /// </summary>
    internal static class Matcher
    {
        #region Nested type: DeviceResult

        /// <summary>
        /// The score for each segment associated with the device being matched.
        /// </summary>
        private class DeviceResult
        {
            internal readonly DeviceInfo Device;
            internal readonly SegmentScore[] Scores;

            internal DeviceResult(SegmentScore[] scores, DeviceInfo device)
            {
                Scores = scores;
                Device = device;
            }
        }

        #endregion

        #region Nested type: SegmentScore

        /// <summary>
        /// Stores the score for each segment found.
        /// </summary>
        private struct SegmentScore
        {
            internal int CharactersMatched;
            internal int Difference;
        }

        #endregion

        #region Static Methods

        internal static Results Match(string userAgent, VersionHandler handler)
        {
            int[] maxCharacters = new int[handler.VersionRegexes.Length];
            List<DeviceResult> initialResults = new List<DeviceResult>(handler.UserAgents.Count);
            Results results = new Results();
            
            // The 1st pass calculates the scores for every segment of every device
            // available against the target useragent.
            FirstPass(handler, userAgent, maxCharacters, initialResults);

            // The 2nd pass returns the devices with the lowest difference across all the
            // versions available.
            SecondPass(handler, maxCharacters, initialResults, results);

            // Return the best device matches.
            return results;
        }

        private static void SecondPass(VersionHandler handler, int[] maxCharacters, List<DeviceResult> initialResults, Results results)
        {
            int lowestScore = int.MaxValue;
            foreach (DeviceResult current in initialResults)
            {
                // Calculate the score for this device.
                int deviceScore = 0;
                for (int segment = 0; segment < handler.VersionRegexes.Length; segment++)
                {
                    deviceScore += (maxCharacters[segment] - current.Scores[segment].CharactersMatched + 1)*
                                   (current.Scores[segment].Difference + maxCharacters[segment] -
                                    current.Scores[segment].CharactersMatched);
                }
                // If the score is lower than the lowest so far then reset the list
                // of best matching devices.
                if (deviceScore < lowestScore)
                {
                    results.Clear();
                    lowestScore = deviceScore;
                }
                // If the device score is the same as the lowest score so far then add this
                // device to the list.
                if (deviceScore == lowestScore)
                {
                    results.Add(current.Device);
                }
            }
        }

        private static void FirstPass(VersionHandler handler, string userAgent, int[] maxCharacters, List<DeviceResult> initialResults)
        {
            string compare, target;
            foreach (var devices in handler.UserAgents)
            {
                foreach (DeviceInfo device in devices)
                {
                    SegmentScore[] scores = new SegmentScore[handler.VersionRegexes.Length];
                    for (int segment = 0; segment < handler.VersionRegexes.Length; segment++)
                    {
                        target = handler.VersionRegexes[segment].Match(userAgent).Value;
                        compare = handler.VersionRegexes[segment].Match(device.UserAgent).Value;
                        for (int i = 0; i < target.Length && i < compare.Length; i++)
                        {
                            scores[segment].Difference += Math.Abs((target[i] - compare[i]));
                            scores[segment].CharactersMatched++;
                        }
                        if (scores[segment].CharactersMatched > maxCharacters[segment])
                            maxCharacters[segment] = scores[segment].CharactersMatched;
                    }
                    initialResults.Add(new DeviceResult(scores, device));
                }
            }
        }

        #endregion
    }
}