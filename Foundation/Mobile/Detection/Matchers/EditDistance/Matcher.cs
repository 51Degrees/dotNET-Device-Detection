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
            string userAgent = request.UserAgent;
            BaseDeviceInfo current = request.Next();
            while (current != null)
            {
                // Perform the edit distance check.
                int distance = Algorithms.EditDistance(current.UserAgent, userAgent, request.Results.MinDistance);
                if (distance <= request.Results.MinDistance)
                {
                    lock (request.Results)
                    {
                        if (distance < request.Results.MinDistance)
                        {
                            request.Results.MinDistance = distance;
                            request.Results.Clear();
                            request.Results.Add(current);
                        }
                        else if (distance == request.Results.MinDistance)
                        {
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