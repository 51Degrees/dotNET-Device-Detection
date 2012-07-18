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

#if VER4

using System.Linq;

#endif

#endregion

namespace FiftyOne.Foundation.Mobile.Detection
{
    /// <summary>
    /// Represents all device data and capabilities.
    /// </summary>
    public class Provider : BaseProvider
    {
        #region Fields

        /// <summary>
        /// A list of properties and associated values which can be 
        /// returned by the provider and it's data source.
        /// </summary>
        private readonly List<Property> _properties = new List<Property>();

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
        /// returned by the provider and it's data source.
        /// </summary>
        public List<Property> Properties
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

        #endregion

        #region Internal & Private Methods

        /// <summary>
        /// Returns the closest matching device from the result set to the target userAgent.
        /// </summary>
        /// <param name="results">The result set to find a device from.</param>
        /// <param name="userAgent">Target useragent.</param>
        /// <returns>The closest matching device.</returns>
        private BaseDeviceInfo GetDeviceInfoClosestMatch(Results results, string userAgent)
        {
            if (results == null || results.Count == 0)
                return null;

            if (results.Count == 1)
                return results[0].Device as BaseDeviceInfo;
            
            results.Sort();
            BaseDeviceInfo device = Matcher.Match(userAgent, results);
            if (device != null)
                return device;

            return null;
        }

        #endregion

        #region Public Methods
        
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
        /// Gets the closest matching device based on the HTTP headers.
        /// </summary>
        /// <param name="headers">Collection of Http headers associated with the request.</param>
        /// <returns>The closest matching device.</returns>
        public BaseDeviceInfo GetDeviceInfo(NameValueCollection headers)
        {
            return GetDeviceInfoClosestMatch(
                GetMatches(headers), GetUserAgent(headers));
        }

        /// <summary>
        /// Gets the single most likely device to match the useragent provided.
        /// </summary>
        /// <param name="userAgent">Useragent string associated with the device to be found.</param>
        /// <returns>The closest matching device.</returns>
        public BaseDeviceInfo GetDeviceInfo(string userAgent)
        {
            return GetDeviceInfoClosestMatch(GetMatches(userAgent), userAgent);
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