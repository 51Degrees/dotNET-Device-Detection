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

#region Usings

using System;
using FiftyOne.Foundation.Mobile.Detection.Matchers;

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
        /// Holds all capabilities from the current device
        /// </summary>
        private Collection _deviceCapabilities;

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
        /// Gets the internal identifier of the device as specified in the WURFL.
        /// </summary>
        public string DeviceId
        {
            get { return _deviceId; }
        }

        /// <summary>
        /// Contains the device user agent string.
        /// </summary>
        public string UserAgent
        {
            get { return _userAgent; }
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
        /// The list of device capabilities.
        /// </summary>
        internal Collection Capabilities
        {
            get { return _deviceCapabilities; }
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
            _deviceCapabilities = new Collection(devices.Strings);
        }

        private void Init(
            BaseProvider devices,
            string deviceId,
            string userAgent)
        {
            if (userAgent == null)
                throw new ArgumentNullException("userAgent");

            _userAgent = UserAgentParser.Parse(userAgent);

            Init(devices, deviceId);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the capability value index in the static Strings collection for this device
        /// based on the index of the capability name.
        /// </summary>
        /// <param name="index">Capability name index.</param>
        /// <returns>Capability index value in the String collection, or -1 if the capability does not exist.</returns>
        internal protected virtual int GetCapability(int index)
        {
            if (_deviceCapabilities.ContainsKey(index))
                return _deviceCapabilities[index];
            return -1;
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
                   Capabilities.Equals(other.Capabilities) &&
                   CapabilitiesEquals(other);
        }

        /// <summary>
        /// Check the strings are all equal.
        /// </summary>
        /// <param name="other">Other BaseDeviceInfo.</param>
        /// <returns>True if the object capability strings are the same.</returns>
        private bool CapabilitiesEquals(BaseDeviceInfo other)
        {
            foreach(int key in Capabilities.Keys)
                if (_provider.Strings.Get(key).Equals(other.Provider.Strings.Get(key)) == false ||
                    _provider.Strings.Get(GetCapability(key)).Equals(other.Provider.Strings.Get(other.GetCapability(key))) == false)
                    return false;
            return true;
        }

        #endregion
    }
}