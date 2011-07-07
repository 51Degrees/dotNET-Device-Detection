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
 *     Andy Allan <andy.allan@mobgets.com>
 * 
 * ********************************************************************* */

#if VER4
using System.Linq;
#endif

#region Usings

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using System.Xml.Schema;
using FiftyOne.Foundation.Mobile.Detection.Wurfl.Configuration;
using FiftyOne.Foundation.Mobile.Detection.Handlers;
using FiftyOne.Foundation.Mobile.Detection.Matchers;
using FiftyOne.Foundation.Mobile.Detection.Wurfl.Handlers;
using Matcher=FiftyOne.Foundation.Mobile.Detection.Matchers.Final.Matcher;
using RegexSegmentHandler = FiftyOne.Foundation.Mobile.Detection.Handlers.RegexSegmentHandler;

#endregion

namespace FiftyOne.Foundation.Mobile.Detection.Wurfl
{
    /// <summary>
    /// Represents all device data and capabilities.
    /// </summary>
    public class Provider : BaseProvider
    {
        #region Fields

        /// <summary>
        /// If set to true only the device roots will be returned.
        /// </summary>
        private readonly bool _useActualDeviceRoot;

        #endregion

        #region Properties

        /// <summary>
        /// The index of the "is_wireless_device" string in the Strings static collection.
        /// </summary>
        internal readonly int IsWirelessDeviceIndex ;

        /// <summary>
        /// The index of the "is_tablet" string in the Strings static collection.
        /// </summary>
        internal readonly int IsTabletDeviceIndex;

        #endregion
        
        #region Public Constructors

        /// <summary>
        /// Creates an instance of the WURFL provider class.
        /// </summary>
        public Provider(string[] wurflFilePaths, string[] capabilitiesWhiteList, bool useActualDeviceRoot)
            : this(useActualDeviceRoot)
        {
            InitHandlers();
            InitWurflFiles(wurflFilePaths, capabilitiesWhiteList);
        }

        #endregion

        #region Internal Constructors

        /// <summary>
        /// Constructs a new instance of <see cref="BaseProvider"/>.
        /// </summary>
        internal Provider()
        {
            // Set common string index values.
            IsWirelessDeviceIndex = Strings.Add("is_wireless_device");
            IsTabletDeviceIndex = Strings.Add("is_tablet");
        }

        /// <summary>
        /// Constructs a new instance of <see cref="BaseProvider"/>.
        /// </summary>
        /// <param name="useActualDeviceRoot">True if only root devices should be returned.</param>
        internal Provider(bool useActualDeviceRoot)
            : this()
        {
            _useActualDeviceRoot = useActualDeviceRoot;
        }

        #endregion

        #region Initialise Methods

        /// <summary>
        /// Creates a single instance of this class to be used by all
        /// clients within the AppDomain.
        /// </summary>
        private void InitWurflFiles(string[] wurflFilePaths, string[] capabilitiesWhiteList)
        {
            try
            {
                // Record the start time for the log file.
                long startTicks = DateTime.Now.Ticks;

                // Load data from all available sources.
                Processor.ParseWurflFiles(this, wurflFilePaths, capabilitiesWhiteList);

                long duration = (long) TimeSpan.FromTicks(DateTime.Now.Ticks - startTicks + 1).TotalMilliseconds;

                // Log the length of time taken to load the device data.
                EventLog.Info(String.Format("Loaded {0} devices using {1} strings in {2}ms",
                                            AllDevices.Count,
                                            Strings.Count,
                                            duration));
            }
            catch (WurflException ex)
            {
                // Record the exception.
                EventLog.Fatal(ex);
            }
        }

        /// <summary>
        /// Loads the handlers based on the configuration resource included in the
        /// Foundation project.
        /// </summary>
        private void InitHandlers()
        {
            using (StreamReader streamReader = new StreamReader(GetType().Assembly.GetManifestResourceStream(Constants.HandlerResourceName)))
            {
                string xml = Regex.Replace(streamReader.ReadToEnd(), "<!DOCTYPE.+>", "");
                using (StringReader textReader = new StringReader(xml))
                {
                    using (XmlReader reader = XmlReader.Create(textReader))
                    {
                        while (reader.Read())
                        {
                            if (reader.IsStartElement("handler"))
                            {
                                ProcessHandler(CreateHandler(reader, null, this), reader.ReadSubtree());
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Processes the current handler and adds it to the list of handlers.
        /// </summary>
        /// <param name="handler">The current handler object.</param>
        /// <param name="reader">The XML stream reader.</param>
        private void ProcessHandler(Handler handler, XmlReader reader)
        {
            while (reader.Read())
            {
                if (reader.Depth > 0 && reader.IsStartElement())
                {
                    switch (reader.Name)
                    {
                        case "supportedRootDevices":
                            ((IHandler)handler).SupportedRootDeviceIds.AddRange(ProcessDevices(reader.ReadSubtree()));
                            break;
                        case "unSupportedRootDevices":
                            ((IHandler)handler).UnSupportedRootDeviceIds.AddRange(ProcessDevices(reader.ReadSubtree()));
                            break;
                        case "canHandle":
                            handler.CanHandleRegex.AddRange(ProcessRegex(reader.ReadSubtree()));
                            break;
                        case "cantHandle":
                            handler.CantHandleRegex.AddRange(ProcessRegex(reader.ReadSubtree()));
                            break;
                        case "regexSegments":
                            if (handler is RegexSegmentHandler)
                                ProcessRegexSegments((RegexSegmentHandler)handler, reader.ReadSubtree());
                            break;
                    }
                }
            }
            Handlers.Add(handler);
        }

        /// <summary>
        /// Adds the segments to the regular expression handler.
        /// </summary>
        /// <param name="handler">Regular expression handler.</param>
        /// <param name="reader">The XML stream reader.</param>
        private void ProcessRegexSegments(RegexSegmentHandler handler, XmlReader reader)
        {
            while (reader.Read())
            {
                if (reader.Depth > 0)
                {
                    string pattern = reader.GetAttribute("pattern");
                    int weight = 0;
                    if (String.IsNullOrEmpty(pattern) == false &&
                        int.TryParse(reader.GetAttribute("weight"), out weight))
                        handler.AddSegment(pattern, weight);
                }
            }
        }

        /// <summary>
        /// Returns a list of regexs used to evaluate the handler to see if it can
        /// be used to handle the requested useragent.
        /// </summary>
        /// <param name="reader">The XML stream reader.</param>
        /// <returns>A list of regexes.</returns>
        private List<HandleRegex> ProcessRegex(XmlReader reader)
        {
            List<HandleRegex> regexs = new List<HandleRegex>();
            while (reader.Read())
            {
                if (reader.Depth > 0 && reader.IsStartElement("regex"))
                {
                    HandleRegex regex = new HandleRegex(reader.GetAttribute("pattern"));
                    regex.Children.AddRange(ProcessRegex(reader.ReadSubtree()));
                    regexs.Add(regex);
                }
            }
            return regexs;
        }

        /// <summary>
        /// Returns a list of device IDs.
        /// </summary>
        /// <param name="reader">The XML stream reader.</param>
        /// <returns>A list of device IDs.</returns>
        private List<string> ProcessDevices(XmlReader reader)
        {
            List<string> devices = new List<string>();
            while (reader.Read())
            {
                if (reader.IsStartElement("device"))
                    devices.Add(reader.GetAttribute("id"));
            }
            return devices;
        }

        /// <summary>
        /// Creates a new handler based on the attributes of the current element.
        /// </summary>
        /// <param name="reader">The XML stream reader.</param>
        /// <param name="parent">The parent handler, or null if it does not exist.</param>
        /// <param name="provider">The provider the handler will be associated with.</param>
        /// <returns>A new handler object.</returns>
        private Handler CreateHandler(XmlReader reader, Handler parent, Provider provider)
        {
            bool checkUAProf;
            byte confidence;
            string name = reader.GetAttribute("name");
            string defaultDeviceId = reader.GetAttribute("defaultDevice");
            string type = reader.GetAttribute("type");
            bool.TryParse(reader.GetAttribute("checkUAProf"), out checkUAProf);
            byte.TryParse(reader.GetAttribute("confidence"), out confidence);

            switch (type)
            {
                case "editDistance":
                    return new Handlers.EditDistanceHandler(provider, name, defaultDeviceId, confidence, checkUAProf);
                case "reducedInitialString":
                    return new Handlers.ReducedInitialStringHandler(provider, name, defaultDeviceId, confidence, checkUAProf, reader.GetAttribute("tolerance"));
                case "regexSegment":
                    return new Handlers.RegexSegmentHandler(provider, name, defaultDeviceId, confidence, checkUAProf);
            }

            throw new WurflException(String.Format("Type '{0}' is invalid.", type));
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns the closest matching device from the result set to the target userAgent.
        /// </summary>
        /// <param name="results">The result set to find a device from.</param>
        /// <param name="userAgent">Target useragent.</param>
        /// <returns>The closest matching device.</returns>
        private DeviceInfo GetDeviceInfoClosestMatch(Results results, string userAgent)
        {
            if (results.Count == 1)
                return results[0].Device as DeviceInfo;
            
            results.Sort();
            DeviceInfo device = Matcher.Match(userAgent, results) as DeviceInfo;
            if (device != null)
                return device;

            foreach (string deviceId in Constants.DefaultDeviceId)
            {
                DeviceInfo defaultDevice = GetDeviceInfoFromID(deviceId) as DeviceInfo;
                if (defaultDevice != null)
                    return defaultDevice;
            }

            return null;
        }

        /// <summary>
        /// Returns the most common shared device across the results set.
        /// If only one result is available this is returned.
        /// </summary>
        /// <param name="results">The result set to find a device from.</param>
        /// <returns>The most likely single device.</returns>
        private DeviceInfo GetDeviceInfoSharedParent(Results results)
        {
            if (results.Count == 1)
                return results[0].Device as DeviceInfo;

            foreach(Result result in results)
            {
                DeviceInfo sharedDevice = FindSharedParent(results, (DeviceInfo)result.Device);
                if (sharedDevice != null)
                    return sharedDevice;
            }

            foreach (string deviceId in Constants.DefaultDeviceId)
            {
                DeviceInfo defaultDevice = GetDeviceInfoFromID(deviceId) as DeviceInfo;
                if (defaultDevice != null)
                    return defaultDevice;
            }

            return null;
        }

        /// <summary>
        /// Returns the shared parent if one exists.
        /// </summary>
        /// <param name="results"></param>
        /// <param name="device"></param>
        /// <returns></returns>
        private static DeviceInfo FindSharedParent(Results results, DeviceInfo device)
        {
            if (IsShared(results, device))
                return device;
            if (device.FallbackDevice != null)
                return FindSharedParent(results, device.FallbackDevice);
            return null;
        }

        /// <summary>
        /// Returns true if the device is shared in the parent hierarchy of 
        /// all the others devices in the resultset. 
        /// </summary>
        /// <param name="results"></param>
        /// <param name="device"></param>
        /// <returns></returns>
        private static bool IsShared(Results results, DeviceInfo device)
        {
            foreach(Result result in results)
            {
                if (((DeviceInfo)result.Device).GetIsParent(device.DeviceId) == false)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Enhances the base implementation to check for devices marked with 
        /// "actual_device_root" and only return these.
        /// </summary>
        /// <param name="request">HttpRequest associated with the requesting device.</param>
        /// <returns>The closest matching device.</returns>
        internal BaseDeviceInfo GetDeviceInfo(HttpRequest request)
        {
            DeviceInfo device = GetDeviceInfoClosestMatch(GetMatches(request), GetUserAgent(request));

            if (device != null)
            {
                // If we're only looking for devices marked with 
                // "actual_device_root" then look back throught the
                // fallback devices until one is found.
                if (_useActualDeviceRoot &&
                    device.IsActualDeviceRoot == false)
                    device = GetActualDeviceRootDeviceInfo(device);
            }

            return device;
        }

        /// <summary>
        /// Enhances the base implementation to check for devices marked with 
        /// "actual_device_root" and only return these.
        /// </summary>
        /// <param name="userAgent">Useragent string associated with the mobile device.</param>
        /// <returns>The closest matching device.</returns>
        public DeviceInfo GetDeviceInfo(string userAgent)
        {
            DeviceInfo device = GetDeviceInfoClosestMatch(GetMatches(userAgent), userAgent);

            if (device != null)
            {
                // If we're only looking for devices marked with 
                // "actual_device_root" then look back throught the
                // fallback devices until one is found.
                if (_useActualDeviceRoot &&
                    device.IsActualDeviceRoot == false)
                    device = GetActualDeviceRootDeviceInfo(device);
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
                if (fallback.IsActualDeviceRoot)
                    return fallback;
                else
                    return GetActualDeviceRootDeviceInfo(fallback);
            }
            return null;
        }

        #endregion
    }
}