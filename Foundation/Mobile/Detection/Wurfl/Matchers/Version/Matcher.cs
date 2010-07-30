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

using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using FiftyOne.Foundation.Mobile.Detection.Wurfl.Handlers;

namespace FiftyOne.Foundation.Mobile.Detection.Wurfl.Matchers.Version
{
    internal class Matcher
    {
        private struct SegmentScore
        {
            internal int CharactersMatched;
            internal int Difference;
        }

        private class DeviceResult
        {
            internal SegmentScore[] Scores = null;
            internal DeviceInfo Device = null;

            internal DeviceResult(SegmentScore[] scores, DeviceInfo device)
            {
                Scores = scores;
                Device = device;
            }
        }

        internal static Results Match(string userAgent, VersionHandler handler)
        {
            string compare, target = null;
            int[] maxCharacters = new int[handler.VersionRegexes.Length];
            List<DeviceResult> initialResults = new List<DeviceResult>(handler.UserAgents.Count);
            Results results = new Results();
            int lowestScore = int.MaxValue;
            
            // The 1st pass calculates the scores for every segment of every device
            // available against the target useragent.
            foreach (DeviceInfo[] devices in handler.UserAgents)
            {
                foreach (DeviceInfo device in devices)
                {
                    SegmentScore[] scores = new SegmentScore[handler.VersionRegexes.Length];
                    for(int segment = 0; segment < handler.VersionRegexes.Length; segment++)
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

            // The 2nd pass returns the devices with the lowest difference across all the
            // versions available.
            foreach (DeviceResult current in initialResults)
            {
                // Calculate the score for this device.
                int deviceScore = 0;
                for (int segment = 0; segment < handler.VersionRegexes.Length; segment++)
                {
                    deviceScore += (maxCharacters[segment] - current.Scores[segment].CharactersMatched + 1) *
                                   (current.Scores[segment].Difference + maxCharacters[segment] - current.Scores[segment].CharactersMatched);
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

            // Return the best device matches.
            return results;
        }
    }
}
