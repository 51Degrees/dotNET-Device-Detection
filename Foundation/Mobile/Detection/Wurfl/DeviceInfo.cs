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
using System.Drawing.Imaging;
using System.Web;
using System.Security.Permissions;

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
        
        private string _deviceId;
        private string _userAgent;
        private bool _isActualDeviceRoot;
        private DeviceInfo _fallbackDevice;
        private Provider _devices;
        
        #endregion

        #region Constructors

        /// <summary>
        /// Hide the default constructor.
        /// </summary>
        private DeviceInfo() {}

        /// <summary>
        /// Creates a new device using the source device as the fallback.
        /// </summary>
        /// <param name="source">Source device for the new device.</param>
        /// <param name="deviceId">Id of the new device.</param>
        internal DeviceInfo(DeviceInfo source, string deviceId) 
            : this(source._devices, deviceId, source._userAgent, source) {}

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
            if (string.IsNullOrEmpty(deviceId) == true)
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

            _userAgent = FiftyOne.Foundation.Mobile.Detection.Wurfl.Matchers.UserAgentParser.Parse(userAgent);

            Init(devices, deviceId);
        }
        
        #endregion

        #region Properties

        /// <summary>
        /// Returns true if the device is a wireless device.
        /// </summary>
        internal bool IsMobileDevice
        {
            get { return bool.TrueString.Equals(
                GetCapability("is_wireless_device"), 
                StringComparison.InvariantCultureIgnoreCase); }
        }

        /// <summary>
        /// Returns true if the device is a root device.
        /// </summary>
        internal bool IsActualDeviceRoot
        {
            get { return _isActualDeviceRoot; }
            set { _isActualDeviceRoot = value; }
        }
        
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

        #endregion

        #region Methods
        
        /// <summary>
        /// Gets the capability value for this device
        /// </summary>
        /// <param name="capabilityName">Capability name.</param>
        /// <returns>Capability value in string format.</returns>
        public string GetCapability(string capabilityName)
        {
            string value = _deviceCapabilities.Get(capabilityName);
            if (value == null && _fallbackDevice != null)
            {
                value = _fallbackDevice.GetCapability(capabilityName);
                if (value != null)
                {
                    _deviceCapabilities.Set(capabilityName, value);
                }
            }
            return value;
        }

        /// <summary>
        /// Returns the value of the capability without refering to a
        /// fall back device. Used for capabilities that the fallback
        /// device does not matter for. For example; uaprof information.
        /// </summary>
        /// <param name="capabilityName">Capability name.</param>
        /// <returns>Capability value in string format.</returns>
        internal string GetCapabilityNoFallback(string capabilityName)
        {
            return _deviceCapabilities.Get(capabilityName);
        }
        
        /// <summary>
        /// Gets the capability value for this device and convert it into a bool.
        /// </summary>
        /// <param name="capabilityName">Capability name.</param>
        /// <param name="capabilityValue">Capability value.</param>
        /// <returns>Indicates if the capability was successfully converted.</returns>
        public bool GetCapability(string capabilityName, out bool capabilityValue)
        {
            return bool.TryParse(GetCapability(capabilityName), out capabilityValue);
        }

        /// <summary>
        /// Gets the capability value for this device and convert it into an integer.
        /// </summary>
        /// <param name="capabilityName">Capability name.</param>
        /// <param name="capabilityValue">Capability value.</param>
        /// <returns>Indicates if the capability was successfully converted.</returns>
        public bool GetCapability(string capabilityName, out int capabilityValue)
        {
            return int.TryParse(GetCapability(capabilityName), out capabilityValue);
        }

        /// <summary>
        /// Gets the capability value for this device and convert it into a long.
        /// </summary>
        /// <param name="capabilityName">Capability name.</param>
        /// <param name="capabilityValue">Capability value.</param>
        /// <returns>Indicates if the capability was successfully converted.</returns>
        public bool GetCapability(string capabilityName, out long capabilityValue)
        {
            return long.TryParse(GetCapability(capabilityName), out capabilityValue);
        }

        /// <summary>
        /// Checks the device data to determine if the image format is supported.
        /// </summary>
        /// <param name="format">Image format being requested.</param>
        /// <returns>True if the format is supported. Otherwise false.</returns>
        public bool SupportsImageFormat(ImageFormat format)
        {
            string capabilityName = null;
            if (format.Guid == ImageFormat.Gif.Guid) capabilityName = "gif";
            else if (format.Guid == ImageFormat.Png.Guid) capabilityName = "png";
            else if (format.Guid == ImageFormat.Jpeg.Guid) capabilityName = "gif";
            if (capabilityName != null && bool.TrueString.Equals(GetCapability(capabilityName)) == true)
            {
                return true;
            }
            return false;
        }

        #endregion
    }
}
