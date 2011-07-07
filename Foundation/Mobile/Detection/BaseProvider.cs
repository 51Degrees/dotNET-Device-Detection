/* *********************************************************************
 * The contents of this file are subject to the Mozilla internal License 
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using FiftyOne.Foundation.Mobile.Detection.Handlers;

#if VER4
using System.Linq;
#endif

namespace FiftyOne.Foundation.Mobile.Detection
{
    /// <summary>
    /// The base provider class used by all detection providers.
    /// </summary>
    public class BaseProvider
    {
        #region Fields

        /// <summary>
        /// Lock used to ensure only one thread can load the data.
        /// </summary>
        protected static readonly object StaticLock = new object();
        
        /// <summary>
        /// Collection of all strings used by the provider.
        /// </summary>
        internal readonly Strings Strings = new Strings();

        /// <summary>
        /// A list of handlers used to match devices.
        /// </summary>
        internal readonly List<Handlers.Handler> Handlers = new List<Handlers.Handler>();

        /// <summary>
        /// Hashtable of all devices keyed on device id.
        /// </summary>
        internal readonly SortedDictionary<int, BaseDeviceInfo> AllDevices = 
            new SortedDictionary<int, BaseDeviceInfo>();

        #endregion

        #region Methods

        /// <summary>
        /// Find all the devices that match the request.
        /// </summary>
        /// <param name="request">HttpRequest associated with the requesting device.</param>
        /// <returns>The closest matching device.</returns>
        internal Matchers.Results GetMatches(HttpRequest request)
        {
            // Get a user agent string with common issues removed.
            string userAgent = GetUserAgent(request);
            if (String.IsNullOrEmpty(userAgent) == false)
            {
                // Using the handler for this userAgent find the device.
                return GetMatches(request, GetHandlers(request));
            }
            return null;
        }

        /// <summary>
        /// Find all the devices that match the useragent string.
        /// </summary>
        /// <param name="userAgent">Useragent string associated with the mobile device.</param>
        /// <returns>The closest matching device.</returns>
        internal Matchers.Results GetMatches(string userAgent)
        {
            if (String.IsNullOrEmpty(userAgent) == false)
            {
                // Using the handler for this userAgent find the device.
                return GetMatches(userAgent, GetHandlers(userAgent));
            }
            return null;
        }

        /// <summary>
        /// Use the HttpRequest fields to determine devices that match.
        /// </summary>
        /// <param name="request">HttpRequest object.</param>
        /// <param name="handlers">Handlers capable of finding devices for the request.</param>
        /// <returns>The closest matching device or null if one can't be found.</returns>
        private static Matchers.Results GetMatches(HttpRequest request, IEnumerable<Handler> handlers)
        {
            Matchers.Results results = new Matchers.Results();

            foreach (Handler handler in handlers)
            {
                // Find the closest matching devices.
                Matchers.Results temp = handler.Match(request);
                // If some results have been found.
                if (temp != null)
                    // Combine the results with results from previous
                    // handlers.
                    results.AddRange(temp);
            }

            // If no results were found add the default handlers devices.
            if (results.Count == 0)
            {
                foreach (Handler handler in handlers)
                    results.Add(handler.DefaultDevice);
            }

            return results;
        }

        /// <summary>
        /// Use the HttpRequest fields to determine devices that match.
        /// </summary>
        /// <param name="useragent">The useragent string of the device being sought.</param>
        /// <param name="handlers">Handlers capable of finding devices for the request.</param>
        /// <returns>The closest matching device or null if one can't be found.</returns>
        private static Matchers.Results GetMatches(string useragent, IEnumerable<Handler> handlers)
        {
            Matchers.Results results = new Matchers.Results();

            foreach (Handler handler in handlers)
            {
                // Find the closest matching devices.
                Matchers.Results temp = handler.Match(useragent);
                // If some results have been found.
                if (temp != null)
                    // Combine the results with results from previous
                    // handlers.
                    results.AddRange(temp);
            }

            // If no results were found add the default handlers devices.
            if (results.Count == 0)
            {
                foreach (Handler handler in handlers)
                    if (handler.DefaultDevice != null)
                        results.Add(handler.DefaultDevice);
            }

            return results;
        }

        /// <summary>
        /// Using the unique device id returns the device. Very quick return
        /// when the device id is known.
        /// </summary>
        /// <param name="deviceID">Unique internal ID of the device.</param>
        /// <returns>BaseDeviceInfo object.</returns>
        internal BaseDeviceInfo GetDeviceInfoFromID(string deviceID)
        {
            BaseDeviceInfo device;
            if(AllDevices.TryGetValue(deviceID.GetHashCode(), out device))
                return device;
            return null;
        }

        /// <summary>
        /// Gets an array of handlers that will support the device information.
        /// Device ids are used to determine if the handler supports the device
        /// tree the requested device is within.
        /// </summary>
        /// <param name="device">Device information to find supporting handlers.</param>
        /// <returns>A list of all handlers that can handle this device.</returns>
        private Handler[] GetHandlers(BaseDeviceInfo device)
        {
            byte highestConfidence = 0;
            List<Handler> handlers = new List<Handler>();

#if VER4
            foreach (Handler handler in Handlers.Where(handler => handler.CanHandle(device)))
            {
                GetHandlers(ref highestConfidence, handlers, handler);
            }
#elif VER2
            foreach (Handler handler in Handlers)
            {
                if (handler.CanHandle(device))
                    GetHandlers(ref highestConfidence, handlers, handler);
            }
#endif
            return handlers.ToArray();
        }

        /// <summary>
        /// Gets an array of handlers that will support the useragent string.
        /// </summary>
        /// <param name="userAgent">Useragent string associated with the HTTP request.</param>
        /// <returns>A list of all handlers that can handle this device.</returns>
        private Handler[] GetHandlers(string userAgent)
        {
            byte highestConfidence = 0;
            List<Handler> handlers = new List<Handler>();
#if VER4

            foreach (Handler handler in Handlers.Where(handler => handler.CanHandle(userAgent)))
            {
                GetHandlers(ref highestConfidence, handlers, handler);
            }
#elif VER2
            foreach (Handler handler in Handlers)
            {
                if (handler.CanHandle(userAgent))
                    GetHandlers(ref highestConfidence, handlers, handler);
            }
#endif
            return handlers.ToArray();
        }

        /// <summary>
        /// Returns an array of handlers that will be supported by the request.
        /// Is used when a request is available so that header fields other
        /// than Useragent can also be used in matching. For example; the
        /// Useragent Profile fields.
        /// </summary>
        /// <param name="request">HttpRequest object.</param>
        /// <returns>An array of handlers able to match the request.</returns>
        private Handler[] GetHandlers(HttpRequest request)
        {
            byte highestConfidence = 0;
            List<Handler> handlers = new List<Handler>();

#if VER4
            foreach (Handler handler in Handlers.Where(handler => handler.CanHandle(request)))
            {
                GetHandlers(ref highestConfidence, handlers, handler);
            }
#elif VER2
            foreach (Handler handler in Handlers)
            {
                // If the handler can support the request and it's not the
                // catch all handler add it to the list we'll use for matching.
                if (handler.CanHandle(request))
                    GetHandlers(ref highestConfidence, handlers, handler);
            }
#endif

            return handlers.ToArray();
        }

        /// <summary>
        /// Adds the handler to the list of handlers if the handler's confidence
        /// is higher than or equal to the current highest handler confidence.
        /// </summary>
        /// <param name="highestConfidence">Highest confidence value to far.</param>
        /// <param name="handlers">List of handlers.</param>
        /// <param name="handler">Handler to be considered for adding.</param>
        /// <returns>The new highest confidence value.</returns>
        private static void GetHandlers(ref byte highestConfidence, List<Handler> handlers, Handler handler)
        {
            if (handler.Confidence > highestConfidence)
            {
                handlers.Clear();
                handlers.Add(handler);
                highestConfidence = handler.Confidence;
            }
            else if (handler.Confidence == highestConfidence)
                handlers.Add(handler);
        }

        /// <summary>
        /// Records the device in the indexes used by the API. If a device
        /// with the same ID already exists the previous one is overwritten.
        /// The device is also assigned to a handler based on the useragent,
        /// and supported root devices of the handler.
        /// </summary>
        /// <param name="device">The new device being added.</param>
        internal virtual void Set(BaseDeviceInfo device)
        {
            // Does the device already exist?
            lock (AllDevices)
            {
                int hashCode = device.DeviceId.GetHashCode();
                if (AllDevices.ContainsKey(hashCode))
                {
                    // Yes. Replace the previous device as it's likely this new
                    // one is coming from a more current source.
                    AllDevices[hashCode] = device;
                }
                else
                {
                    // No. So add the new device.
                    AllDevices.Add(hashCode, device);
                }
            }

            // Add the new device to handlers that can support it.
            if (String.IsNullOrEmpty(device.UserAgent) == false)
            {
#if VER4
            foreach (Handler handler in GetHandlers(device).Where(handler => handler != null))
            {
                handler.Set(device);
            }
#elif VER2
                foreach (Handler handler in GetHandlers(device))
                    if (handler != null)
                        handler.Set(device);
#endif
            }
        }

        /// <summary>
        /// Used to check other header fields in case a transcoder is being used
        /// and returns the true useragent string.
        /// </summary>
        /// <param name="request">Contains details of the request.</param>
        /// <returns>The useragent string to use for matching purposes.</returns>
        internal static string GetUserAgent(HttpRequest request)
        {
            string userAgent = request.UserAgent;
#if VER4
            string transcodeUserAgent =
                Detection.Constants.TRANSCODER_USERAGENT_HEADERS.FirstOrDefault(e => request.Headers[e] != null);
            if (transcodeUserAgent != null)
                userAgent = transcodeUserAgent;
#elif VER2
            foreach (string current in Detection.Constants.TRANSCODER_USERAGENT_HEADERS)
            {
                if (request.Headers[current] != null)
                {
                    userAgent = request.Headers[current];
                    break;
                }
            }
#endif
            if (userAgent == null)
                userAgent = string.Empty;
            else
                userAgent = userAgent.Trim();
            return userAgent;
        }

        #endregion
    }
}
