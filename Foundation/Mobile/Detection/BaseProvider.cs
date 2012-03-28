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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using FiftyOne.Foundation.Mobile.Detection.Handlers;
using System.Collections.Specialized;

#if VER4 || VER35
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
        /// A list of handlers used to match devices.
        /// </summary>
        private readonly List<Handler> _handlers = new List<Handler>();

        /// <summary>
        /// Collection of all strings used by the provider.
        /// </summary>
        internal readonly Strings Strings = new Strings();
        
        /// <summary>
        /// Hashtable of all devices keyed on device hash code.
        /// </summary>
        internal readonly SortedDictionary<int, List<BaseDeviceInfo>> AllDevices =
            new SortedDictionary<int, List<BaseDeviceInfo>>();

        /// <summary>
        /// A list of string indexes used for user agent profile properties.
        /// </summary>
        private List<int> _userAgentProfileStringIndexes;

        /// <summary>
        /// List of all the actual devices ignoring parents, or those
        /// children added to handle useragent strings.
        /// </summary>
        private List<BaseDeviceInfo> _actualDevices = null;

        private object _lockDevices = new object();

        #endregion

        #region Internal Properties
                
        /// <summary>
        /// Returns a list of the string indexes for user agent profile properties.
        /// </summary>
        internal List<int> UserAgentProfileStringIndexes
        {
            get
            {
                if (_userAgentProfileStringIndexes == null)
                {
                    _userAgentProfileStringIndexes = new List<int>();
                    foreach (string value in Constants.UserAgentProfiles)
                        _userAgentProfileStringIndexes.Add(Strings.Add(value));
                }
                return _userAgentProfileStringIndexes;
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// A list of handlers being used by the provider.
        /// </summary>
        public List<Handler> Handlers
        {
            get { return _handlers; }
        }

        /// <summary>
        /// Returns a list of all devices available to the provider which
        /// have properties assigned to them. This is needed to ignore devices
        /// which are only present to represent a useragent and do not have
        /// any properties assigned to them.
        /// </summary>
        public List<BaseDeviceInfo> Devices
        {
            get
            {
                if (_actualDevices == null)
                {
                    lock (_lockDevices)
                    {
                        if (_actualDevices == null)
                        {
                            _actualDevices = new List<BaseDeviceInfo>();
                            foreach (int key in AllDevices.Keys)
                                foreach (var device in AllDevices[key])
                                    if (device.DeviceId.Split(new string[] { Constants.ProfileSeperator }, StringSplitOptions.RemoveEmptyEntries).Length == 4)
                                        _actualDevices.Add(device);
                        }
                    }
                }
                return _actualDevices;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Find all the devices that match the request.
        /// </summary>
        /// <param name="headers">List of http headers associated with the request.</param>
        /// <returns>The closest matching device.</returns>
        internal Matchers.Results GetMatches(NameValueCollection headers)
        {
            // Get a user agent string with common issues removed.
            string userAgent = GetUserAgent(headers);
            if (String.IsNullOrEmpty(userAgent) == false)
            {
                // Using the handler for this userAgent find the device.
                return GetMatches(headers, GetHandlers(headers));
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
        /// <param name="headers">Collection of Http headers associated with the request.</param>
        /// <param name="handlers">Handlers capable of finding devices for the request.</param>
        /// <returns>The closest matching device or null if one can't be found.</returns>
        private static Matchers.Results GetMatches(NameValueCollection headers, IEnumerable<Handler> handlers)
        {
            Matchers.Results results = new Matchers.Results();

            foreach (Handler handler in handlers)
            {
                // Find the closest matching devices.
                Matchers.Results temp = handler.Match(headers);
                // If some results have been found.
                if (temp != null)
                    // Combine the results with results from previous
                    // handlers.
                    results.AddRange(temp);
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

            return results;
        }

        /// <summary>
        /// Using the unique device id returns the device. Very quick return
        /// when the device id is known.
        /// </summary>
        /// <param name="deviceID">Unique internal ID of the device.</param>
        /// <returns>BaseDeviceInfo object.</returns>
        public BaseDeviceInfo GetDeviceInfoByID(string deviceID)
        {
            List<BaseDeviceInfo> list;
            if (AllDevices.TryGetValue(deviceID.GetHashCode(), out list))
            {
                // If only 1 element return this one.
                if (list.Count == 1)
                    return list[0];
                // Return the first matching element.
                return list.Find(i => i.DeviceId == deviceID);
            }
            return null;
        }

        /// <summary>
        /// Gets an array of handlers that will support the useragent string.
        /// </summary>
        /// <param name="userAgent">Useragent string associated with the HTTP request.</param>
        /// <returns>A list of all handlers that can handle this device.</returns>
        public Handler[] GetHandlers(string userAgent)
        {
            byte highestConfidence = 0;
            List<Handler> handlers = new List<Handler>();
#if VER4 || VER35

            foreach (Handler handler in Handlers.Where(handler => handler.CanHandle(userAgent)))
            {
                GetHandlers(ref highestConfidence, handlers, handler);
            }
#else
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
        /// <param name="headers">Collection of Http headers associated with the request.</param>
        /// <returns>An array of handlers able to match the request.</returns>
        private Handler[] GetHandlers(NameValueCollection headers)
        {
            byte highestConfidence = 0;
            List<Handler> handlers = new List<Handler>();
#if VER4 || VER35
            foreach (Handler handler in Handlers.Where(handler => handler.CanHandle(headers)))
            {
                GetHandlers(ref highestConfidence, handlers, handler);
            }
#else
            foreach (Handler handler in Handlers)
            {
                // If the handler can support the request and it's not the
                // catch all handler add it to the list we'll use for matching.
                if (handler.CanHandle(headers))
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
            // Reset the list of devices.
            _actualDevices = null;

            // Does the device already exist?
            lock (AllDevices)
            {
                int hashCode = device.DeviceId.GetHashCode();
                if (AllDevices.ContainsKey(hashCode))
                {
                    // Yes. Add this device to the list.
                    AllDevices[hashCode].Add(device);
                }
                else
                {
                    // No. So add the new device.
                    AllDevices.Add(hashCode, new List<BaseDeviceInfo>(new[] { device }));
                }
            }

            // Add the new device to handlers that can support it.
            if (String.IsNullOrEmpty(device.UserAgent) == false)
            {
#if VER4 || VER35
                foreach (Handler handler in GetHandlers(device.UserAgent).Where(handler => handler != null))
            {
                handler.Set(device);
            }
#else
                foreach (Handler handler in GetHandlers(device.UserAgent))
                    if (handler != null)
                        handler.Set(device);
#endif
            }
        }

        /// <summary>
        /// Used to check other header fields in case a transcoder is being used
        /// and returns the true useragent string.
        /// </summary>
        /// <param name="headers">Collection of Http headers associated with the request.</param>
        /// <returns>The useragent string to use for matching purposes.</returns>
        internal static string GetUserAgent(NameValueCollection headers)
        {
            string userAgent = headers[Detection.Constants.UserAgentHeader];
#if VER4 || VER35
            string transcodeUserAgent =
                Detection.Constants.TranscoderUserAgentHeaders.FirstOrDefault(e => headers[e] != null);
            if (transcodeUserAgent != null)
                userAgent = transcodeUserAgent;
#else
            foreach (string current in Detection.Constants.TranscoderUserAgentHeaders)
            {
                if (headers[current] != null)
                {
                    userAgent = headers[current];
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
