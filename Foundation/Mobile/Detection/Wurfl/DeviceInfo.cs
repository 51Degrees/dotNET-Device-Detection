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
using FiftyOne.Foundation.Mobile.Detection.Wurfl.Matchers;

#endregion

namespace FiftyOne.Foundation.Mobile.Detection.Wurfl
{
    /// <summary>
    /// Represents a device and holds all its settings.
    /// </summary>
    public class DeviceInfo
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
        /// A reference to the collection of all devices.
        /// </summary>
        private Provider _devices;

        /// <summary>
        /// The fallback device.
        /// </summary>
        private DeviceInfo _fallbackDevice;

        /// <summary>
        /// The useragent string of the device.
        /// </summary>
        private string _userAgent;

        /// <summary>
        /// The static index of the "is_wireless_device" string in the Strings static collection.
        /// </summary>
        private static readonly int IsWirelessDeviceIndex = Strings.Add("is_wireless_device");

        /// <summary>
        /// The static index of the "is_tablet" string in the Strings static collection.
        /// </summary>
        private static readonly int IsTabletDeviceIndex = Strings.Add("is_tablet");
        
        #endregion

        #region Constructors

        /// <summary>
        /// Hide the default constructor.
        /// </summary>
        private DeviceInfo()
        {
        }

        /// <summary>
        /// Creates a new device using the source device as the fallback.
        /// </summary>
        /// <param name="source">Source device for the new device.</param>
        /// <param name="deviceId">Id of the new device.</param>
        internal DeviceInfo(DeviceInfo source, string deviceId)
            : this(source._devices, deviceId, source._userAgent, source)
        {
        }

        /// <summary>
        /// Creates an instance of DeviceInfo.
        /// </summary>
        /// <param name="userAgent">User agent string used to identify this device.</param>
        /// <param name="deviceId">A unique Identifier of the device.</param>
        /// <param name="devices">A reference to the complete index of devices.</param>
        internal DeviceInfo(
            Provider devices,
            string deviceId,
            string userAgent)
        {
            Init(devices, deviceId, userAgent);
        }

        /// <summary>
        /// Creates an instance of DeviceInfo.
        /// </summary>
        /// <param name="userAgent">User agent string used to identify this device.</param>
        /// <param name="deviceId">A unique Identifier of the device.</param>
        /// <param name="devices">A reference to the complete index of devices.</param>
        /// <param name="fallbackDevice">The fallback device to use for this device if any.</param>
        internal DeviceInfo(
            Provider devices,
            string deviceId,
            string userAgent,
            DeviceInfo fallbackDevice)
        {
            if (fallbackDevice == null)
                throw new ArgumentNullException("fallbackDevice");
            Init(devices, deviceId, userAgent);
            _fallbackDevice = fallbackDevice;
        }

        /// <summary>
        /// Creates an instance of DeviceInfo.
        /// </summary>
        /// <param name="deviceId">A unique Identifier of the device.</param>
        /// <param name="devices">A reference to the complete index of devices.</param>
        internal DeviceInfo(
            Provider devices,
            string deviceId)
        {
            Init(devices, deviceId);
        }

        private void Init(
            Provider devices,
            string deviceId)
        {
            if (string.IsNullOrEmpty(deviceId))
                throw new ArgumentNullException("deviceId");

            if (devices == null)
                throw new ArgumentNullException("devices");

            _devices = devices;
            _deviceId = deviceId;
            _deviceCapabilities = new Collection();
        }

        private void Init(
            Provider devices,
            string deviceId,
            string userAgent)
        {
            if (userAgent == null)
                throw new ArgumentNullException("userAgent");

            _userAgent = UserAgentParser.Parse(userAgent);

            Init(devices, deviceId);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Returns true if the device is a tablet device.
        /// </summary>
        internal bool IsTabletDevice
        {
            get
            {
                return bool.TrueString.Equals(
                    Strings.Get(GetCapability(IsTabletDeviceIndex)),
                    StringComparison.InvariantCultureIgnoreCase);
            }
        }

        /// <summary>
        /// Returns true if the device is a wireless device.
        /// </summary>
        internal bool IsMobileDevice
        {
            get
            {
                return bool.TrueString.Equals(
                    Strings.Get(GetCapability(IsWirelessDeviceIndex)),
                    StringComparison.InvariantCultureIgnoreCase);
            }
        }

        /// <summary>
        /// Returns true if the device is a root device.
        /// </summary>
        internal bool IsActualDeviceRoot { get; set; }

        /// <summary>
        /// Gets the internal identifier of the device as specified in the WURFL.
        /// </summary>
        internal string DeviceId
        {
            get { return _deviceId; }
        }

        /// <summary>
        /// Contains the device user agent string.
        /// </summary>
        internal string UserAgent
        {
            get { return _userAgent; }
        }

        internal DeviceInfo FallbackDevice
        {
            set { _fallbackDevice = value; }
            get { return _fallbackDevice; }
        }

        /// <summary>
        /// The list of device capabilities.
        /// </summary>
        internal Collection Capabilities
        {
            get { return _deviceCapabilities; }
        }

        /// <summary>
        /// Set the userAgent string for this device to the value specified.
        /// </summary>
        /// <param name="value"></param>
        internal void SetUserAgent(string value)
        {
            if (_userAgent != value)
            {
                _userAgent = value;
                // Ensure the indexes in the collection are updated to reflect
                // the change in UserAgent string for this device.
                _devices.Set(this);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the capability value index in the static Strings collection for this device
        /// based on the index of the capability name.
        /// </summary>
        /// <param name="index">Capability name index.</param>
        /// <returns>Capability index value in the String collection, or -1 if the capability does not exist.</returns>
        internal int GetCapability(int index)
        {
            if (_deviceCapabilities.ContainsKey(index))
                return _deviceCapabilities[index];
            if (_fallbackDevice != null)
                return _fallbackDevice.GetCapability(index);
            return -1;
        }

        /// <summary>
        /// Gets the capability value index in the static Strings collection for this device
        /// based on the index of the capability name. The fallback device will not be checked.
        /// </summary>
        /// <param name="index">Capability name index.</param>
        /// <returns>Capability index value in the String collection, or -1 if the capability does not exist.</returns>

        internal int GetCapabilityNoFallback(int index)
        {
            if (_deviceCapabilities.ContainsKey(index))
                return _deviceCapabilities[index];
            return -1;
        }

        #endregion
    }
}