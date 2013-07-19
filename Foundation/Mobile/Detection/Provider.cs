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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using FiftyOne.Foundation.Mobile.Detection.Matchers;
using Matcher = FiftyOne.Foundation.Mobile.Detection.Matchers.Final.Matcher;
using System.IO;
using FiftyOne.Foundation.Mobile.Detection.Handlers;

#if VER4 || VER35

using System.Linq;

#endif

#endregion

namespace FiftyOne.Foundation.Mobile.Detection
{
    /// <summary>
    /// Represents all device data and capabilities.
    /// </summary>
    public class Provider
    {
        #region Enumerations

        /// <summary>
        /// Possible components properties can be related to.
        /// </summary>
        internal enum Components
        {
            Unknown = 0,
            Hardware = 1,
            Software = 2,
            Browser = 3,
            Crawler = 4
        }

        #endregion

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

        /// <summary>
        /// Used to lock the list of devices during load.
        /// </summary>
        private object _lockDevices = new object();

        /// <summary>
        /// A list of properties and associated values which can be 
        /// returned by the provider and it's data source. Keyed on 
        /// the string index of the property name.
        /// </summary>
        private readonly SortedList<int, Property> _properties = new SortedList<int, Property>();

        /// <summary>
        /// The date and time the 1st data source used to create the
        /// provider was created.
        /// </summary>
        internal DateTime _publishedDate = DateTime.MinValue;
        
        /// <summary>
        /// Lock object used to create the embedded provider.
        /// </summary>
        internal static object _lock = new object();

        /// <summary>
        /// A static reference to a provider that contains the embedded 
        /// device data.
        /// </summary>
        internal static Provider _embeddedProvider = null;

        /// <summary>
        /// The name of the data set the provider is using.
        /// </summary>
        internal string _dataSetName = "Unknown";

        #endregion

        #region Internal Constructors

        /// <summary>
        /// Constructs a new instance of <see cref="Provider"/>.
        /// </summary>
        internal Provider() : base()
        {
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
                            lock (AllDevices)
                            {
                                _actualDevices = new List<BaseDeviceInfo>();
                                string[] seperator = new string[] { Constants.ProfileSeperator };
                                foreach (int key in AllDevices.Keys)
                                    foreach (BaseDeviceInfo device in AllDevices[key])
                                        if (device.DeviceId.Split(
                                            seperator,
                                            StringSplitOptions.RemoveEmptyEntries).Length == 4)
                                            _actualDevices.Add(device);
                            }
                        }
                    }
                }
                return _actualDevices;
            }
        }
        
        /// <summary>
        /// Returns the a provider initialised with the data contained in the
        /// device data file embedded in the assembly.
        /// </summary>
        public static Provider EmbeddedProvider
        {
            get
            {
                if (_embeddedProvider == null)
                {
                    lock (_lock)
                    {
                        if (_embeddedProvider == null)
                        {
                            EventLog.Debug(String.Format("Creating provider from embedded device data '{0}'.",
                                Binary.BinaryConstants.EmbeddedDataResourceName));

                            using (Stream stream = Assembly.GetExecutingAssembly()
                                .GetManifestResourceStream(
                                Binary.BinaryConstants.EmbeddedDataResourceName))
                                _embeddedProvider = Binary.Reader.Create(stream);

                            EventLog.Info(String.Format("Created provider from embedded device data '{0}'.",
                                Binary.BinaryConstants.EmbeddedDataResourceName));
                        }
                    }
                }
                return _embeddedProvider;
            }
        }

        /// <summary>
        /// A list of properties and associated values which can be 
        /// returned by the provider and it's data source. Keyed on the 
        /// string index of the property name.
        /// </summary>
        public SortedList<int, Property> Properties
        {
            get { return _properties; }
        }

        /// <summary>
        /// Returns the date and time the data used to create the provider
        /// was published.
        /// </summary>
        public DateTime PublishedDate
        {
            get { return _publishedDate; }
        }

        /// <summary>
        /// Returns the name of the data set used to create the provider.
        /// </summary>
        public string DataSetName
        {
            get { return _dataSetName; }
        }

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

        #region Internal & Private Methods

        /// <summary>
        /// Sets the property whose name is provided to relate to the component
        /// provided.
        /// </summary>
        /// <param name="propertyName">Name of the property to set.</param>
        /// <param name="component">The type of component to relate the property to.</param>
        private void SetComponent(string propertyName, Components component)
        {
            Property property = null;
            if (Properties.TryGetValue(Strings.Add(propertyName), out property))
                property.SetComponent(component);
        }

        /// <summary>
        /// Sets the list of properties provided to the component.
        /// </summary>
        /// <param name="properties">List of properties to be set</param>
        /// <param name="component">Component to set them to</param>
        private void SetDefaultComponents(string[] properties, Components component)
        {
            foreach (string propertyName in properties)
                SetComponent(propertyName, component);
        }

        /// <summary>
        /// Sets the component properties relate to to their default values.
        /// Used for data formats that don't include this information in the data file.
        /// </summary>
        internal void SetDefaultComponents()
        {
            SetDefaultComponents(UI.Constants.Hardware, Components.Hardware);
            SetDefaultComponents(UI.Constants.Software, Components.Software);
            SetDefaultComponents(UI.Constants.Browser, Components.Browser);
        }

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
            // Does the device already exist?
            lock (AllDevices)
            {
                // Reset the list of devices.
                _actualDevices = null;

                // Add the device to the list using the ID hashcode as key.
                int hashCode = device.DeviceId.GetHashCode();
                if (AllDevices.ContainsKey(hashCode))
                {
                    // Yes. Add this device to the list.
                    AllDevices[hashCode].Add(device);
                }
                else
                {
                    // No. So add the new device.
                    AllDevices.Add(
                        hashCode,
                        new List<BaseDeviceInfo>(new BaseDeviceInfo[] { device }));
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
        /// Returns the main user agent string for the request.
        /// </summary>
        /// <param name="headers">Collection of Http headers associated with the request.</param>
        /// <returns>The main user agent string for the request</returns>
        internal static string GetUserAgent(NameValueCollection headers)
        {
            return headers[Detection.Constants.UserAgentHeader];
        }

        /// <summary>
        /// Used to check other header fields in case a device user agent is being used
        /// and returns the devices useragent string.
        /// </summary>
        /// <param name="headers">Collection of Http headers associated with the request.</param>
        /// <returns>The useragent string of the device.</returns>
        internal static string GetDeviceUserAgent(NameValueCollection headers)
        {
            foreach (string current in Detection.Constants.DeviceUserAgentHeaders)
                if (headers[current] != null)
                    return headers[current];
            return null;
        }

        /// <summary>
        /// Returns the closest matching device from the result set to the target userAgent.
        /// </summary>
        /// <param name="results">The result set to find a device from.</param>
        /// <param name="userAgent">Target useragent.</param>
        /// <returns>The closest matching result.</returns>
        private Result GetRequestClosestMatch(Results results, string userAgent)
        {
            if (results == null || results.Count == 0)
                return null;

            if (results.Count == 1)
                return results[0];
            
            results.Sort();
            Result result = Matcher.Match(userAgent, results);
            if (result != null)
                return result;

            return null;
        }

        #endregion

        #region Public Methods
        
        /// <summary>
        /// Using the unique device id returns the device. Very quick return
        /// when the device id is known.
        /// </summary>
        /// <param name="deviceID">Unique internal ID of the device.</param>
        /// <returns>BaseDeviceInfo object.</returns>
        public BaseDeviceInfo GetDeviceInfoByID(string deviceID)
        {
            List<BaseDeviceInfo> list;
            if (AllDevices.TryGetValue(deviceID.GetHashCode(), out list) == true)
            {
                // Return the first matching element.
#if VER35 || VER4
                return list.Find(i => i.DeviceId == deviceID);
#else
                foreach (BaseDeviceInfo device in list)
                    if (device.DeviceId == deviceID)
                        return device;
#endif
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
        /// Gets an array of  devices that match this useragent string.
        /// </summary>
        /// <param name="userAgent">Useragent string associated with the device to be found.</param>
        /// <returns>An array of matching devices.</returns>
        public List<BaseDeviceInfo> GetMatchingDeviceInfo(string userAgent)
        {
            List<BaseDeviceInfo> list = new List<BaseDeviceInfo>();
            Results results = GetMatches(userAgent);
            if (results != null)
            {
                foreach (Result result in results)
                    list.Add(result.Device);
            }
            return list;
        }

        /// <summary>
        /// Gets the closest matching result for the headers.
        /// </summary>
        /// <param name="headers">Collection of HTTP headers associated with the request.</param>
        /// <returns>The closest matching result.</returns>
        public Result GetResult(NameValueCollection headers)
        {
            Result result = GetRequestClosestMatch(
                GetMatches(headers), GetUserAgent(headers));
            string secondaryUserAgent = GetDeviceUserAgent(headers);
            if (result != null &&
                String.IsNullOrEmpty(secondaryUserAgent) == false)
            {
                Result secondaryResult = GetResult(secondaryUserAgent);
                if (secondaryResult != null)
                    result.SetSecondaryDevice(secondaryResult.DevicePrimary);
            }
            return result;
        }

        /// <summary>
        /// Gets the closest matching device based on the HTTP headers.
        /// </summary>
        /// <param name="headers">Collection of Http headers associated with the request.</param>
        /// <returns>The closest matching device.</returns>
        public BaseDeviceInfo GetDeviceInfo(NameValueCollection headers)
        {
            Result result = GetResult(headers);
            if (result != null)
                return result.Device;
            return null;
        }

        /// <summary>
        /// Returns the closest matching result for the user agent provided.
        /// </summary>
        /// <param name="userAgent">Useragent string associated with the device to be found.</param>
        /// <returns>The closest matching result.</returns>
        public Result GetResult(string userAgent)
        {
            return GetRequestClosestMatch(
                GetMatches(userAgent), userAgent);
        }

        /// <summary>
        /// Gets the single most likely device to match the useragent provided.
        /// </summary>
        /// <param name="userAgent">Useragent string associated with the device to be found.</param>
        /// <returns>The closest matching device.</returns>
        public BaseDeviceInfo GetDeviceInfo(string userAgent)
        {
            Result result = GetResult(userAgent);
            if (result != null)
                return result.Device;
            return null;
        }

        /// <summary>
        /// Returns all the devices that match the property and value passed 
        /// into the method.
        /// </summary>
        /// <param name="property">The property required.</param>
        /// <param name="value">The value the property must contain to be matched.</param>
        /// <returns>A list of matching devices. An empty list will be returned if no matching devices are found.</returns>
        public List<BaseDeviceInfo> FindDevices(string property, string value)
        {
            List<BaseDeviceInfo> list = new List<BaseDeviceInfo>();
            int propertyIndex = Strings.Add(property);
            int requiredValueIndex = Strings.Add(value);
            foreach (BaseDeviceInfo device in Devices)
            {
                foreach (int valueIndex in device.GetPropertyValueStringIndexes(propertyIndex))
                    if (requiredValueIndex == valueIndex)
                        list.Add(device);
            }
            return list;
        }

        /// <summary>
        /// Returns a list of devices based on the profile id provided.
        /// </summary>
        /// <param name="profileID">The profile id of the devices required.</param>
        /// <returns>A list of matching devices. An empty list will be returned if no matching devices are found.</returns>
        public List<BaseDeviceInfo> FindDevices(string profileID)
        {
            List<BaseDeviceInfo> list = new List<BaseDeviceInfo>();
            foreach (BaseDeviceInfo device in Devices)
            {
                foreach (string id in device.ProfileIDs)
                {
                    if (profileID == id)
                    {
                        list.Add(device);
                        break;
                    }
                }
            }
            return list;
        }

        #endregion
    }
}