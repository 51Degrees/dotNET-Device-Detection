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
            Segments compare;
            BaseDeviceInfo current = request.Next();
            while (current != null)
            {
                // Reset the counters.
                runningScore = 0;
                index = 0;
                
                // Get matching number of segments.
                compare = request.Handler.CreateSegments(current.UserAgent);
                for (int i = compare.Count; i < request.Target.Count; i++)
                    compare.Add(new Segment(
                        String.Empty,
                        request.Target[i].Weight));
                
                while(index < request.Target.Count &&
                    runningScore <= request.Results.LowestScore)
                {
                    // If the two segments are exactly equal the score will be zero.
                    if (request.Target[index].Value == compare[index].Value)
                        score = 0;

                    // Otherwise assign the EditDistance value multiplied by the
                    // segment weight or if one is not available the index of the segment.
                    else
                        score = (uint)Algorithms.EditDistance(
                            request.Target[index].Value,
                            compare[index].Value,
                            int.MaxValue) *
                            (uint)request.Target[index].Weight;

                    // Update the counters.
                    compare[index].Score = score;
                    runningScore += score;
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