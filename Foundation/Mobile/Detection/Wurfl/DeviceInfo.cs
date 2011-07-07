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

namespace FiftyOne.Foundation.Mobile.Detection.Wurfl
{
    /// <summary>
    /// Represents a WURFL device and holds all its settings.
    /// </summary>
    public class DeviceInfo : BaseDeviceInfo
    {
        #region Fields
               
        /// <summary>
        /// The fallback device.
        /// </summary>
        private DeviceInfo _fallbackDevice;

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
                    _provider.Strings.Get(GetCapability(((Provider)_provider).IsTabletDeviceIndex)),
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
                    _provider.Strings.Get(GetCapability(((Provider)_provider).IsWirelessDeviceIndex)),
                    StringComparison.InvariantCultureIgnoreCase);
            }
        }

        /// <summary>
        /// Returns true if the device is a root device.
        /// </summary>
        internal bool IsActualDeviceRoot { get; set; }

        /// <summary>
        /// Returns the fallback device.
        /// </summary>
        internal DeviceInfo FallbackDevice
        {
            set { _fallbackDevice = value; }
            get { return _fallbackDevice; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates an instance of <cref see="DeviceInfo"/>.
        /// </summary>
        /// <param name="userAgent">User agent string used to identify this device.</param>
        /// <param name="deviceId">A unique Identifier of the device.</param>
        /// <param name="provider">A reference to the complete index of devices.</param>
        internal DeviceInfo(
            BaseProvider provider,
            string deviceId,
            string userAgent)
            : base(provider, deviceId, userAgent)
        {
        }

        /// <summary>
        /// Creates an instance of <cref see="DeviceInfo"/>.
        /// </summary>
        /// <param name="deviceId">A unique Identifier of the device.</param>
        /// <param name="provider">A reference to the complete index of devices.</param>
        internal DeviceInfo(
            BaseProvider provider,
            string deviceId)
            : base(provider, deviceId)
        {
        }

        /// <summary>
        /// Creates an instance of DeviceInfo.
        /// </summary>
        /// <param name="userAgent">User agent string used to identify this device.</param>
        /// <param name="deviceId">A unique Identifier of the device.</param>
        /// <param name="provider">A reference to the base provider.</param>
        /// <param name="fallbackDevice">The fallback device to use for this device if any.</param>
        internal DeviceInfo(
            BaseProvider provider,
            string deviceId,
            string userAgent,
            DeviceInfo fallbackDevice)
            : base(provider, deviceId, userAgent)
        {
            if (fallbackDevice == null)
                throw new ArgumentNullException("fallbackDevice");
            _fallbackDevice = fallbackDevice;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns the capability value.
        /// </summary>
        /// <param name="capabilityName">Name of the capability required.</param>
        /// <returns>The value associated with the capability.</returns>
        public string GetCapability(string capabilityName)
        {
            int index = GetCapability(_provider.Strings.Add(capabilityName));
            if (index >= 0)
                return _provider.Strings.Get(index);
            return null;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns true if the deviceId is in the parent hierarchy of the 
        /// device.
        /// </summary>
        /// <param name="deviceId">DeviceId being sought.</param>
        /// <returns>True if the device is a parent.</returns>
        internal bool GetIsParent(string deviceId)
        {
            if (DeviceId.Equals(deviceId))
                return true;
            if (FallbackDevice != null)
                return FallbackDevice.GetIsParent(deviceId);
            return false;
        }

        /// <summary>
        /// Gets the capability value index in the static Strings collection for this device
        /// based on the index of the capability name.
        /// </summary>
        /// <param name="index">Capability name index.</param>
        /// <returns>Capability index value in the String collection, or -1 if the capability does not exist.</returns>
        internal protected override int GetCapability(int index)
        {
            int value = base.GetCapability(index);
            if (value >= 0)
                return value;

            if (_fallbackDevice != null)
                return _fallbackDevice.GetCapability(index);
            return -1;
        }

        /// <summary>
        /// Checks if another DeviceInfo is equal to this one.
        /// </summary>
        /// <param name="other">Other DeviceInfo.</param>
        /// <returns>True if the object instances are the same.</returns>
        internal bool Equals(DeviceInfo other)
        {
            return base.Equals(other) &&
                   FallbackDevice.DeviceId.Equals(other.FallbackDevice.DeviceId) &&
                   IsActualDeviceRoot == other.IsActualDeviceRoot;
        }

        #endregion
    }
}
