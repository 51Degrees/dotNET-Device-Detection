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
 * Mobile Experts Limited are Copyright (C) 2009 - 2012. All Rights Reserved.
 * 
 * Contributor(s):
 *     James Rosewell <james@51degrees.mobi>
 * 
 * ********************************************************************* */

#region Usings

using System;
using System.Threading;
using FiftyOne.Foundation.Mobile.Detection.Handlers;

#endregion

namespace FiftyOne.Foundation.Mobile.Detection.Matchers.Segment
{
    internal class Matcher : Matchers.Matcher
    {
        internal static Results Match(string userAgent,
                                      SegmentHandler handler)
        {
            if (handler.Devices.Count > 0)
            {
                if (Environment.ProcessorCount > 1 &&
                    Detection.Constants.ForceSingleProcessor == false)
                {
                    return MatchMultiProcessor(userAgent, handler);
                }
                else
                {
                    return MatchSingleProcessor(userAgent, handler);
                }
            }
            return null;
        }

        private static Results MatchSingleProcessor(string userAgent, SegmentHandler handler)
        {
            // Create a segment matcher request.
            Request request = new Request(userAgent, handler);
            // Process the request.
            ServiceRequest(request);
            // Return the results.
            return request.Results;
        }

        private static Results MatchMultiProcessor(string userAgent, SegmentHandler handler)
        {
            // Provide an object to signal when the request has completed.
            AutoResetEvent waiter = new AutoResetEvent(false);

            // Create the request.
            Request request = new Request(userAgent, handler, waiter);
            if (request.Count > 0)
            {
                // For each thread add this to the queue.
                for (int i = 0; i < Environment.ProcessorCount; i++)
                    ThreadPool.QueueUserWorkItem(ServiceRequest, request);

                // Wait until a signal is received. Keeping coming back to
                // this thread so that a request to close the request
                // can be processed.
                while (waiter.WaitOne(1, false) == false)
                {
                    // Do nothing 
                }
                ;
            }
            // Return the results.
            return request.Results;
        }

        private static void ServiceRequest(object sender)
        {
            ServiceRequest((Request) sender);
        }

        private static void ServiceRequest(Request request)
        {
            int index;
            uint runningScore, score;
            BaseDeviceInfo current = request.Next();
            while (current != null)
            {
                // Reset the counters.
                runningScore = 0;
                index = 0;

                while (index < request.Target.Count &&
                    runningScore <= request.Results.LowestScore)
                {
                    // Get the next segment for the comparision.
                    var compare = request.Handler.CreateSegments(
                        current.UserAgent, index);

                    // If the two results are not equal in length so do not consider
                    // this useragent as a possible match.
                    if (request.Target[index].Count != compare.Count)
                    {
                        runningScore = uint.MaxValue;
                        break;
                    }

                    // Work out the score for each of the returned segments.
                    for (int segmentIndex = 0; 
                        segmentIndex < request.Target[index].Count; 
                        segmentIndex++)
                    {
                        // If the two are equal then set to zero.
                        if (request.Target[index][segmentIndex].Value == compare[segmentIndex].Value)
                            score = 0;
                        else
                            score = (uint)Algorithms.EditDistance(
                                request.Target[index][segmentIndex].Value,
                                compare[segmentIndex].Value,
                                int.MaxValue) *
                                (uint)request.Target[index][segmentIndex].Weight;

                        // Update the counters.
                        compare[segmentIndex].Score = score;
                        runningScore += score;
                    }
                    
                    index++;
                }

                if (runningScore <= request.Results.LowestScore)
                {
                    lock (request.Results)
                    {
                        if (runningScore == request.Results.LowestScore)
                        {
                            request.Results.Add(current);
                        }
                        else if (runningScore < request.Results.LowestScore)
                        {
                            request.Results.LowestScore = runningScore;
                            request.Results.Clear();
                            request.Results.Add(current);
                        }
                    }
                }

                current = request.Next();
                request.Complete();
            }
        }
    }
}