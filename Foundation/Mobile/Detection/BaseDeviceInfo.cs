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
 *     Andy Allan <andy.allan@mobgets.com>
 * 
 * ********************************************************************* */

#region Usings

using System;
using FiftyOne.Foundation.Mobile.Detection.Matchers;
using System.Collections.Generic;
using System.Collections.Specialized;

#endregion

namespace FiftyOne.Foundation.Mobile.Detection
{
    /// <summary>
    /// Represents a device and holds all its settings.
    /// </summary>
    public class BaseDeviceInfo
    {
        #region Fields

        /// <summary>
        /// The parent device.
        /// </summary>
        internal BaseDeviceInfo _parent;

        /// <summary>
        /// Holds all properties from the current device
        /// </summary>
        private Collection _deviceProperties;

        /// <summary>
        /// The Id of the device.
        /// </summary>
        private string _deviceId;

        /// <summary>
        /// A reference to the provider associated with this device.
        /// </summary>
        protected BaseProvider _provider;

        /// <summary>
        /// The useragent string of the device.
        /// </summary>
        private string _userAgent;

        #endregion

        #region Public Properties

        /// <summary>
        /// Contains the device user agent string.
        /// </summary>
        public string UserAgent
        {
            get { return _userAgent; }
        }

        /// <summary>
        /// The parent of the device, or null if this is a root device.
        /// </summary>
        public BaseDeviceInfo Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }

        #endregion

        #region Internal Properties

        /// <summary>
        /// Returns the provider associated with the device.
        /// </summary>
        internal BaseProvider Provider
        {
            get { return _provider; }
        }

        /// <summary>
        /// Internal accessor for user agent string.
        /// </summary>
        internal string InternalUserAgent
        {
            set { _userAgent = value; }
        }

        /// <summary>
        /// The list of device properties.
        /// </summary>
        internal Collection Properties
        {
            get { return _deviceProperties; }
        }

        /// <summary>
        /// Gets the internal identifier of the device.
        /// </summary>
        internal string DeviceId
        {
            get { return _deviceId; }
        }
        
        #endregion

        #region Constructors

        /// <summary>
        /// Hide the default constructor.
        /// </summary>
        private BaseDeviceInfo()
        {
        }

        /// <summary>
        /// Creates an instance of <cref see="BaseDeviceInfo"/>.
        /// </summary>
        /// <param name="userAgent">User agent string used to identify this device.</param>
        /// <param name="deviceId">A unique Identifier of the device.</param>
        /// <param name="devices">A reference to the complete index of devices.</param>
        /// <param name="parent">The parent device if one exists.</param>
        internal BaseDeviceInfo(
            BaseProvider devices,
            string deviceId,
            string userAgent,
            BaseDeviceInfo parent)
        {
            Init(devices, deviceId, userAgent, parent);
        }

        /// <summary>
        /// Creates an instance of <cref see="BaseDeviceInfo"/>.
        /// </summary>
        /// <param name="userAgent">User agent string used to identify this device.</param>
        /// <param name="deviceId">A unique Identifier of the device.</param>
        /// <param name="devices">A reference to the complete index of devices.</param>
        internal BaseDeviceInfo(
            BaseProvider devices,
            string deviceId,
            string userAgent)
        {
            Init(devices, deviceId, userAgent);
        }

        /// <summary>
        /// Creates an instance of DeviceInfo.
        /// </summary>
        /// <param name="deviceId">A unique Identifier of the device.</param>
        /// <param name="devices">A reference to the complete index of devices.</param>
        internal BaseDeviceInfo(
            BaseProvider devices,
            string deviceId)
        {
            Init(devices, deviceId);
        }

        private void Init(
            BaseProvider devices,
            string deviceId)
        {
            if (string.IsNullOrEmpty(deviceId))
                throw new ArgumentNullException("deviceId");

            if (devices == null)
                throw new ArgumentNullException("devices");

            _provider = devices;
            _deviceId = deviceId;
            _deviceProperties = new Collection(devices.Strings);
        }

        private void Init(
            BaseProvider devices,
            string deviceId,
            string userAgent,
            BaseDeviceInfo parent)
        {
            _parent = parent;
            Init(devices, deviceId, userAgent);
        }
        
        private void Init(
            BaseProvider devices,
            string deviceId,
            string userAgent)
        {
            _userAgent = userAgent;
            Init(devices, deviceId);
        }

        #endregion

        #region Parent Methods

        /// <summary>
        /// Returns true if the deviceId is in the parent hierarchy of the 
        /// device.
        /// </summary>
        /// <param name="device">The device being checked.</param>
        /// <returns>True if the device is within the parent hierarchy.</returns>
        internal bool GetIsParent(BaseDeviceInfo device)
        {
            if (this == device)
                return true;
            if (Parent != null)
                return Parent.GetIsParent(device);
            return false;
        }

        #endregion

        #region Property Methods

        /// <summary>
        /// Gets the capability value index in the static Strings collection for this device
        /// based on the index of the capability name. If this device does not have the 
        /// value then checks the parent if one exists.
        /// </summary>
        /// <param name="index">Capability name index.</param>
        /// <returns>Capability index value in the String collection, or -1 if the capability does not exist.</returns>
        internal protected virtual IList<int> GetPropertyValueStringIndexes(int index)
        {
            IList<int> value;
            if (_deviceProperties.TryGetValue(index, out value))
                return value;
            if (Parent != null)
                return Parent.GetPropertyValueStringIndexes(index);
            return null;
        }

        /// <summary>
        /// Returns the string index of the first element in the collection.
        /// </summary>
        /// <param name="index">Capability name index.</param>
        /// <returns>Capability index value in the String collection, or -1 if the capability does not exist.</returns>
        internal protected virtual int GetFirstPropertyValueStringIndex(int index)
        {
            IList<int> value = GetPropertyValueStringIndexes(index);
            if (value != null && value.Count > 0)
                return value[0];
            return -1;
        }

        /// <summary>
        /// Returns a list of the string values for the property index string provided.
        /// </summary>
        /// <param name="propertyStringIndex">The string index of the property name.</param>
        /// <returns>A list of string values.</returns>
        internal protected List<string> GetPropertyValues(int propertyStringIndex)
        {
            var values = new List<string>();
            foreach (int index in GetPropertyValueStringIndexes(propertyStringIndex))
                if (index >= 0)
                    values.Add(Provider.Strings.Get(index));
            return values;
        }

        /// <summary>
        /// Adds the device properties to the collection.
        /// </summary>
        /// <param name="collection">Collection to have properties added to.</param>
        internal protected void AddProperties(SortedList<string, List<string>> collection)
        {
            foreach (var propertyStringIndex in Properties.Keys)
            {
                var property = _provider.Strings.Get(propertyStringIndex);
                if (Constants.ExcludePropertiesFromAllProperties.Contains(property) == false &&
                    collection.ContainsKey(property) == false)
                    collection.Add(
                        property,
                        GetPropertyValues(propertyStringIndex));
            }
            if (Parent != null)
                Parent.AddProperties(collection);
        }

        /// <summary>
        /// Gets the value of the first value for the property.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public string GetFirstPropertyValue(string property)
        {
            int index = GetFirstPropertyValueStringIndex(Provider.Strings.Add(property));
            if (index >= 0)
                return Provider.Strings.Get(index);
            return null;
        }

        /// <summary>
        /// Returns a list of the string values for the property.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public List<string> GetPropertyValues(string property)
        {
            var values = new List<string>();
            var indexes = GetPropertyValueStringIndexes(Provider.Strings.Add(property));
            if (indexes != null)
            {
                foreach (int index in indexes)
                {
                    if (index >= 0)
                        values.Add(Provider.Strings.Get(index));
                }
            }
            return values;
        }

        /// <summary>
        /// Returns a sorted list containing all the property values for the
        /// the device.
        /// </summary>
        public SortedList<string, List<string>> GetAllProperties()
        {
            var collection = new SortedList<string, List<string>>();
#if DEBUG
            var handlerNames = new List<string>();
            foreach (var handler in _provider.GetHandlers(UserAgent))
                handlerNames.Add(handler.Name);
            collection.Add("Handlers", new List<string>(new[] { String.Join(", ", handlerNames.ToArray()) }));
            collection.Add("UserAgent", new List<string>(new[] { UserAgent }));
            collection.Add("DeviceID", new List<string>(new[] { DeviceId }));
#endif
            AddProperties(collection);
            return collection;
        }

        #endregion

        #region Equal Members

        /// <summary>
        /// Checks if another BaseDeviceInfo is equal to this one.
        /// </summary>
        /// <param name="other">Other BaseDeviceInfo.</param>
        /// <returns>True if the object instances are the same.</returns>
        internal bool Equals(BaseDeviceInfo other)
        {
            return DeviceId.Equals(other.DeviceId) &&
                   UserAgent.Equals(other.UserAgent) &&
                   Properties.Equals(other.Properties) &&
                   CapabilitiesEquals(other);
        }

        /// <summary>
        /// Check the strings are all equal.
        /// </summary>
        /// <param name="other">Other BaseDeviceInfo.</param>
        /// <returns>True if the object capability strings are the same.</returns>
        private bool CapabilitiesEquals(BaseDeviceInfo other)
        {
            foreach(var key in Properties.Keys)
            {
                if (_provider.Strings.Get(key).Equals(other.Provider.Strings.Get(key)) == false)
                    return false;
                foreach(var value in GetPropertyValueStringIndexes(key))
                    if (_provider.Strings.Get(value).Equals(other.Provider.Strings.Get(value)) == false)
                    return false;
            }
            return true;
        }

        #endregion
    }
}