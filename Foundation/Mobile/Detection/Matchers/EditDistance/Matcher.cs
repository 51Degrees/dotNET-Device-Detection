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

using System;
using System.Threading;
using FiftyOne.Foundation.Mobile.Detection.Handlers;

#endregion

namespace FiftyOne.Foundation.Mobile.Detection.Matchers.EditDistance
{
    internal class Matcher : Matchers.Matcher
    {
        /// <summary>
        /// Returns the closest match to the userAgent string from the queue of
        /// possible values.
        /// </summary>
        /// <param name="userAgent">userAgent being matched</param>
        /// <param name="handler">the handler associated with the matching request</param>
        /// <returns>best match userAgent</returns>
        internal static Results Match(string userAgent, Handler handler)
        {
#pragma warning disable 162
            if (Environment.ProcessorCount > 1 &&
                Detection.Constants.ForceSingleProcessor == false)
            {
                return MatchMultiProcessor(userAgent, handler);
            }
            else
            {
                return MatchSingleProcessor(userAgent, handler);
            }
#pragma warning restore 162
        }

        private static Results MatchSingleProcessor(string userAgent, Handler handler)
        {
            // Create a single matcher and start it.
            Request request = new Request(userAgent, handler);
            // Process the request.
            ServiceRequest(request);
            // Return the results.
            return request.Results;
        }

        private static Results MatchMultiProcessor(string userAgent, Handler handler)
        {
            // Create the request.
            Request request = new Request(userAgent, handler);
            if (request.Count > 0)
            {
                // For each thread add this to the queue.
                for (int i = 0; i < request.ThreadCount; i++)
                    ThreadPool.QueueUserWorkItem(ServiceRequest, request);

                // Wait until a signal is received. Keeping coming back to
                // this thread so that a request to close the request
                // can be processed.
                while (request.Wait(1) == false)
                {
                    // Do nothing 
                }
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
            string userAgent = request.UserAgent;
            int[][] rows = new int[][] { new int[userAgent.Length + 1], new int[userAgent.Length + 1] };
            BaseDeviceInfo current = request.Next();
            while (current != null)
            {
                // Perform the edit distance check.
                int distance = Algorithms.EditDistance(rows, userAgent, current.UserAgent, request.Results.MinDistance);
                if (distance <= request.Results.MinDistance)
                {
                    lock (request.Results)
                    {
                        if (distance < request.Results.MinDistance)
                        {
                            request.Results.MinDistance = distance;
                            request.Results.Clear();
                            request.Results.Add(current, request.Handler, (uint)distance, userAgent);
                        }
                        else if (distance == request.Results.MinDistance)
                        {
                            request.Results.Add(current, request.Handler, (uint)distance, userAgent);
                        }
                    }
                }
                current = request.Next();
            }
            request.Complete();
        }
    }
}