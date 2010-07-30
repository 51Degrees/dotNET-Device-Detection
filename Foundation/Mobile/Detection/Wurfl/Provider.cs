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
 *     Andy Allan <andy.allan@mobgets.com>
 * 
 * ********************************************************************* */

using System;
using System.Collections.Generic;
using System.Collections;
using System.Web;
using FiftyOne.Foundation.Mobile.Detection.Wurfl.Handlers;
using FiftyOne.Foundation.Mobile.Detection.Wurfl.Matchers;

namespace FiftyOne.Foundation.Mobile.Detection.Wurfl
{
    /// <summary>
    /// Represents all device data and capabilities.
    /// </summary>
    internal class Provider
    {
        #region Fields

        /// <summary>
        /// The singleton instance of the Devices class.
        /// </summary>
        private static Provider _instance;
        /// <summary>
        /// True if all data has been loaded and handlers initialised.
        /// </summary>
        private bool _isLoaded = false;
        /// <summary>
        /// Lock used to ensure only one thread can load the data.
        /// </summary>
        private static object _staticLock = new object();
        /// <summary>
        /// An array of handlers used to match devices.
        /// </summary>
        private Handler[] _handlers = null;
        /// <summary>
        /// Hashtable of all devices.
        /// </summary>
        private Hashtable _deviceIDs = new Hashtable(Constants.EstimateNumberOfDevices);
        /// <summary>
        /// Cache for devices found via this class. Devices not used within 60 minutes will be
        /// removed from the cache.
        /// </summary>
        private Cache<DeviceInfo> _cache = new Cache<DeviceInfo>(60);
        /// <summary>
        /// Stores the default device for the WURFL provider.
        /// </summary>
        private static DeviceInfo _defaultDevice = null;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates an instance of Devices.
        /// </summary>
        internal Provider()
        {
            InitHandlers();
        }

        #endregion

        #region Singleton

        /// <summary>
        /// Gets an instance of Provider.
        /// </summary>
        /// <remarks>
        /// If none instance was yet created, tries to create a new one based on
        /// the fiftyone.wurfl section on <c>web.config</c>.
        /// </remarks>
        internal static Provider Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_staticLock)
                    {
                        // Check the instance is still null now we have a lock.
                        if (_instance == null)
                            LoadSingleton();
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Creates a single instance of this class to be used by all
        /// clients within the AppDomain.
        /// </summary>
        private static void LoadSingleton()
        {
            Provider newInstance = new Provider();
            try
            {
                long startTicks = DateTime.Now.Ticks;

                // Load data from all available sources.
                WurflProcessor.ParseWurflFiles(newInstance);

                long duration = TimeSpan.FromTicks(DateTime.Now.Ticks - startTicks + 1).Milliseconds;

                // Log the length of time taken to load the device data.
                EventLog.Info(String.Format("Loaded {0} devices using {1} strings in {2}ms",
                    newInstance._deviceIDs.Count,
                    Strings.Count,
                    duration));

                // Log the number of devices assigned to each handler if debugging is enabled.
                #if DEBUG
                // Display the handler results.
                if (newInstance._handlers != null && EventLog.IsDebug == true)
                {
                    for (int i = 0; i < newInstance._handlers.Length; i++)
                    {
                        EventLog.Debug(String.Format("Handler '{0}' loaded with {1} devices.",
                            newInstance._handlers[i].GetType().Name,
                            newInstance._handlers[i].UserAgents.Count));
                    }
                }
                #endif

                // Store the single instance and change the status to show the
                // data has finished loading.
                newInstance._isLoaded = true;
                _instance = newInstance;
            }
            catch (Wurfl.WurflException ex)
            {
                // Record the exception.
                EventLog.Fatal(ex);
                // Set an empty Devices instance as it's not possible
                // to reliably load the data files specified.
                _instance = new Provider();
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Returns the default device for the API. Used if all other options
        /// to identify the device has failed.
        /// </summary>
        internal static DeviceInfo DefaultDevice
        {
            get
            {
                if (_defaultDevice == null)
                {
                    lock (_staticLock)
                    {
                        if (_defaultDevice == null)
                        {
                            foreach (string current in Constants.DefaultDeviceId)
                            {
                                _defaultDevice = Instance.GetDeviceInfoFromID(current);
                                if (_defaultDevice != null) { break; }
                            }
                        }
                    }
                }
                return _defaultDevice;
            }
        }

        /// <summary>
        /// Indicates if this instance of <seealso cref="Devices"/> 
        /// has finished loading data.
        /// </summary>
        internal bool IsLoaded
        {
            get { return _isLoaded; }
        }

        /// <summary>
        /// The number of unique devices available following the loading of the
        /// device data.
        /// </summary>
        internal int Count
        {
            get { return _deviceIDs.Count; }
        }

        #endregion

        #region Methods

        private static bool IsHandler(Type type)
        {
            if (type == typeof(Handler))
                return true;
            if (type.BaseType != null)
                return IsHandler(type.BaseType);
            return false;
        }

        /// <summary>
        /// Loads all the handlers ready for future reference when
        /// matching requests.
        /// </summary>
        private void InitHandlers()
        {
            List<Handler> handlers = new List<Handler>();

            // Add the handler first to ensure both activity gets handled
            // with a generic browser device.
            handlers.Add(new Handlers.AlcatelHandler());
            handlers.Add(new Handlers.AlphaHandlerAtoF());
            handlers.Add(new Handlers.AlphaHandlerGtoN());
            handlers.Add(new Handlers.AlphaHandlerOtoS());
            handlers.Add(new Handlers.AlphaHandlerTtoZ());
            handlers.Add(new Handlers.AmoiHandler());
            handlers.Add(new Handlers.AndriodHandler());
            handlers.Add(new Handlers.AOLHandler());
            handlers.Add(new Handlers.AppleHandler());
            handlers.Add(new Handlers.AppleCoreMediaHandler());
            handlers.Add(new Handlers.AvantHandler());
            handlers.Add(new Handlers.BenQHandler());
            handlers.Add(new Handlers.BlackBerryHandler());
            handlers.Add(new Handlers.BirdHandler());
            handlers.Add(new Handlers.BoltHandler());
            handlers.Add(new Handlers.BrewHandler());
            handlers.Add(new Handlers.CatchAllHandler());
            handlers.Add(new Handlers.DoCoMoHandler());
            handlers.Add(new Handlers.FirefoxHandler());
            handlers.Add(new Handlers.GrundigHandler());
            handlers.Add(new Handlers.HTCHandler());
            handlers.Add(new Handlers.iTunesHandler());
            handlers.Add(new Handlers.KDDIHandler());
            handlers.Add(new Handlers.KyoceraHandler());
            handlers.Add(new Handlers.LCTHandler());
            handlers.Add(new Handlers.LGHandler());
            handlers.Add(new Handlers.MaxonHandler());
            handlers.Add(new Handlers.MitsubishiHandler());
            handlers.Add(new Handlers.MobileCatchAllHandler());
            handlers.Add(new Handlers.MobileSafariHandler());
            handlers.Add(new Handlers.MotorolaHandler());
            handlers.Add(new Handlers.MSIEHandler());
            handlers.Add(new Handlers.NecHandler());
            handlers.Add(new Handlers.NokiaHandler());
            handlers.Add(new Handlers.NumericHandler());
            handlers.Add(new Handlers.OperaHandler());
            handlers.Add(new Handlers.OperaMiniHandler());
            handlers.Add(new Handlers.OperaMobiHandler());
            handlers.Add(new Handlers.PalmHandler());
            handlers.Add(new Handlers.PanasonicHandler());
            handlers.Add(new Handlers.PantechHandler());
            handlers.Add(new Handlers.PhilipsHandler());
            handlers.Add(new Handlers.PortalmmmHandler());
            handlers.Add(new Handlers.QtekHandler());
            handlers.Add(new Handlers.SafariHandler());
            handlers.Add(new Handlers.SagemHandler());
            handlers.Add(new Handlers.SamsungHandler());
            handlers.Add(new Handlers.SanyoHandler());
            handlers.Add(new Handlers.SendoHandler());
            handlers.Add(new Handlers.SharpHandler());
            handlers.Add(new Handlers.SiemensHandler());
            handlers.Add(new Handlers.SoftBankHandler());
            handlers.Add(new Handlers.SonyEricssonHandler());
            handlers.Add(new Handlers.SPVHandler());
            handlers.Add(new Handlers.TianyuHandler());
            handlers.Add(new Handlers.ToshibaHandler());
            handlers.Add(new Handlers.VodafoneHandler());
            handlers.Add(new Handlers.WindowsCEHandler());
            handlers.Add(new Handlers.ZuneHandler());

            // Add handlers for desktop browsers
            handlers.Add(new Handlers.ChromeHandler());
            handlers.Add(new Handlers.FirefoxDesktopHandler());
            handlers.Add(new Handlers.MSIEDesktopHandler());
            handlers.Add(new Handlers.OperaDesktopHandler());
            handlers.Add(new Handlers.SafariDesktopHandler());


            _handlers = handlers.ToArray();
        }

        /// <summary>
        /// Gets an array of handlers that will support the device information.
        /// Device ids are used to determine if the handler supports the device
        /// tree the requested device is within.
        /// </summary>
        /// <param name="device">Device information to find supporting handlers.</param>
        /// <returns>A list of all handlers that can handle this device.</returns>
        private Handler[] GetHandlers(DeviceInfo device)
        {
            byte highestConfidence = 0;
            List<Handler> handlers = new List<Handler>();

            #if DEBUG
            EventLog.Debug(String.Format("Getting handlers for DeviceId '{0}'.", device.DeviceId));
            #endif

            foreach (Handler handler in _handlers)
            {
                if (handler.CanHandle(device) == true)
                    GetHandlers(ref highestConfidence, handlers, handler);
            }
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

            if (EventLog.IsDebug == true)
                EventLog.Debug(String.Format("Getting handlers for useragent '{0}'.", userAgent));

            foreach (Handler handler in _handlers)
            {
                if (handler.CanHandle(userAgent) == true)
                    GetHandlers(ref highestConfidence, handlers, handler);
            }
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

            if (EventLog.IsDebug == true)
                EventLog.Debug(String.Format("Getting handlers for HttpRequest '{0}'.", GetUserAgent(request)));

            foreach (Handler handler in _handlers)
            {
                // If the handler can support the request and it's not the
                // catch all handler add it to the list we'll use for matching.
                if (handler.CanHandle(request) == true)
                    GetHandlers(ref highestConfidence, handlers, handler);
            }

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
        internal void Set(DeviceInfo device)
        {
            // Does the device already exist?
            lock (_deviceIDs)
            {
                if (_deviceIDs.ContainsKey(device.DeviceId) == true)
                {
                    // Yes. Replace the previous device as it's likely this new
                    // one is coming from a more current source.
                    _deviceIDs[device.DeviceId] = device;
                }
                else
                {
                    // No. So add the new device.
                    _deviceIDs.Add(device.DeviceId, device);
                }
            }

            // Add the new device to handlers that can support it.
            if (String.IsNullOrEmpty(device.UserAgent) == false)
            {
                foreach (Handler handler in GetHandlers(device))
                    if (handler != null)
                        handler.Set(device);
            }
        }

        /// <summary>
        /// Using the unique device id returns the device. Very quick return
        /// when the device id is known.
        /// </summary>
        /// <param name="deviceID">Unique internal ID of the device.</param>
        /// <returns>DeviceInfo object.</returns>
        internal protected DeviceInfo GetDeviceInfoFromID(string deviceID)
        {
            if (_deviceIDs.ContainsKey(deviceID))
            {
                return (DeviceInfo)_deviceIDs[deviceID];
            }
            return null;
        }

        /// <summary>
        /// Used to check other header fields in case a transcoder is being used
        /// and returns the true useragent string.
        /// </summary>
        /// <param name="request">Contains details of the request.</param>
        /// <returns>The useragent string to use for matching purposes.</returns>
        internal protected static string GetUserAgent(HttpRequest request)
        {
            string userAgent = request.UserAgent;
            foreach (string current in Detection.Constants.TRANSCODER_USERAGENT_HEADERS)
            {
                if (request.Headers[current] != null)
                {
                    userAgent = request.Headers[current];
                    break;
                }
            }
            if (userAgent == null)
                userAgent = string.Empty;
            else
                userAgent = userAgent.Trim();
            return userAgent;
        }

        /// <summary>
        /// Based on all the useragent string provided finds the
        /// closed device if an exact match is not available.
        /// </summary>
        /// <param name="userAgent">Useragent string associated with the mobile device.</param>
        /// <returns>The closest matching device.</returns>
        internal DeviceInfo GetDeviceInfo(string userAgent)
        {
            long startTicks = EventLog.IsDebug ? DateTime.Now.Ticks : 0;
            DeviceInfo device = null;
            if (String.IsNullOrEmpty(userAgent) == false && 
                _cache.GetTryParse(userAgent, out device) == false)
            {
                // Using the handler for this userAgent find the device.
                Handler[] handlers = GetHandlers(userAgent);
                if (handlers != null && handlers.Length > 0)
                {
                    // Get the device from the handlers found.
                    device = GetDeviceInfo(userAgent, handlers);

                    // If we're only looking for devices marked with 
                    // "actual_device_root" then look back throught the
                    // fallback devices until one is found.
                    if (Wurfl.Configuration.Manager.UseActualDeviceRoot == true && device.IsActualDeviceRoot == false)
                        device = GetActualDeviceRootDeviceInfo(device);

                    // Add to the cache to improve performance.
                    if (device != null)
                        _cache[userAgent] = device;
                }
            }
            if (device == null)
            {
                device = DefaultDevice;
            }
            return device;
        }

        /// <summary>
        /// Loops through the fallback devices until an actual device root is found.
        /// </summary>
        /// <param name="device">The device to be checked.</param>
        /// <returns>A device that is a root.</returns>
        private DeviceInfo GetActualDeviceRootDeviceInfo(DeviceInfo device)
        {
            DeviceInfo fallback = device.FallbackDevice;
            if (fallback != null)
            {
                if (fallback.IsActualDeviceRoot == true)
                    return fallback;
                else
                    return GetActualDeviceRootDeviceInfo(fallback);
            }
            return null;
        }

        /// <summary>
        /// Based on all the HTTP request information available finds the
        /// closed device if an exact match is not available. Uses fields
        /// other than than useragent string to perform the match.
        /// </summary>
        /// <param name="context">Context associated with the requesting device.</param>
        /// <returns>The closest matching device.</returns>
        internal protected DeviceInfo GetDeviceInfo(HttpContext context)
        {
            long startTicks = EventLog.IsDebug ? DateTime.Now.Ticks : 0;
            string userAgent = GetUserAgent(context.Request);
            DeviceInfo device = null;
            if (String.IsNullOrEmpty(userAgent) == false && 
                _cache.GetTryParse(userAgent, out device) == false)
            {
                // Using the handler for this userAgent find the device.
                Handler[] handlers = GetHandlers(context.Request);
                if (handlers != null && handlers.Length > 0)
                {
                    // Get the device from the handlers found.
                    device = GetDeviceInfo(context.Request, handlers);

                    // If we're only looking for devices marked with 
                    // "actual_device_root" then look back throught the
                    // fallback devices until one is found.
                    if (Wurfl.Configuration.Manager.UseActualDeviceRoot == true && 
                        device.IsActualDeviceRoot == false)
                        device = GetActualDeviceRootDeviceInfo(device);

                    if (device != null)
                    {
                        RecordNewDevice(context.Request, device);
                        _cache[userAgent] = device;
                    }
                }
            }
            if (device == null)
            {
                device = DefaultDevice;
            }
            return device;
        }

        /// <summary>
        /// Use the useragent to determine the closest matching device
        /// from the handlers provided.
        /// </summary>
        /// <param name="userAgent">Useragent to be found.</param>
        /// <param name="handlers">Handlers capable of finding devices for the request.</param>
        /// <returns>The closest matching device or null if one can't be found.</returns>
        private static DeviceInfo GetDeviceInfo(string userAgent, Handler[] handlers)
        {
            DeviceInfo device = null;
            Results results = new Results();

            if (EventLog.IsDebug == true)
                EventLog.Debug(String.Format("Getting device info for useragent '{0}'.", userAgent));

            for (int i = 0; i < handlers.Length; i++)
            {
                // Find the closest matching devices.
                Results temp = handlers[i].Match(userAgent);
                // If some results have been found.
                if (temp != null)
                    // Combine the results with results from previous
                    // handlers.
                    results.AddRange(temp);
            }
            if (results.Count == 1)
            {
                // Use the only result provided.
                device = results[0].Device;
            }
            else if (results.Count > 1)
            {
                // Uses the matcher to narrow down the results.
                device = Matchers.Final.Matcher.Match(userAgent, results);
            }
            if (device == null)
                // No device was found so use the default device for the first 
                // handler provided.
                device = handlers[0].DefaultDevice;
            return device;
        }

        /// <summary>
        /// Use the HttpRequest fields to determine the closest matching device
        /// from the handlers provided.
        /// </summary>
        /// <param name="request">HttpRequest object.</param>
        /// <param name="handlers">Handlers capable of finding devices for the request.</param>
        /// <returns>The closest matching device or null if one can't be found.</returns>
        private static DeviceInfo GetDeviceInfo(HttpRequest request, Handler[] handlers)
        {
            DeviceInfo device = null;
            Results results = new Results();

            if (EventLog.IsDebug == true)
                EventLog.Debug(String.Format("Getting device info for HttpRequest '{0}'.", GetUserAgent(request)));

            for (int i = 0; i < handlers.Length; i++)
            {
                // Find the closest matching devices.
                Results temp = handlers[i].Match(request);
                // If some results have been found.
                if (temp != null)
                    // Combine the results with results from previous
                    // handlers.
                    results.AddRange(temp);
            }
            if (results.Count == 1)
            {
                // Use the only result provided.
                device = results[0].Device;
            }
            else if (results.Count > 1)
            {
                // Uses the matcher to narrow down the results.
                device = Matchers.Final.Matcher.Match(GetUserAgent(request), results);
            }
            if (device == null)
                // No device was found so use the default device for the first 
                // handler provided.
                device = handlers[0].DefaultDevice;
            return device;
        }

        /// <summary>
        /// If the device is new record it in the local new device log and 
        /// remote server depending on the configuration in the web.config
        /// file.
        /// Also places the newly found device into the cache so future page
        /// requests are serviced more rapidly.
        /// </summary>
        /// <param name="request">Details about the request that was used to create the new device.</param>
        /// <param name="device">Device found with the closest match to the useragent string.</param>
        private void RecordNewDevice(HttpRequest request, DeviceInfo device)
        {
            if (device.UserAgent != GetUserAgent(request) && device != DefaultDevice && WurflNewDevice.Enabled == true)
            {
                // Now that we've found a device add it to the list using
                // the useragent string as the device identifier.
                DeviceInfo newDevice = new DeviceInfo(this, GetUserAgent(request), GetUserAgent(request), device);
                WurflNewDevice.RecordNewDevice(request);
            }
        }

        /// <summary>
        /// Returns true if the requesting device appears to WURFL as a
        /// mobile device.
        /// </summary>
        /// <param name="context">Context associated with the requesting device.</param>
        /// <returns>True if the requesting device is a mobile device.</returns>
        internal protected static bool IsMobileDevice(HttpContext context)
        {
            DeviceInfo device = Instance.GetDeviceInfo(context);
            if (device != null)
                return device.IsMobileDevice;
            return false;
        }

        #endregion
    }
}
