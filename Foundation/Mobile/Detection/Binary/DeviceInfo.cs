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

namespace FiftyOne.Foundation.Mobile.Detection.Binary
{
    /// <summary>
    /// Device info for the binary data type where the parent information
    /// is held as a direct reference and not as a fallback device id.
    /// </summary>
    public class DeviceInfo : BaseDeviceInfo
    {
        #region Constructor

        /// <summary>
        /// Constructs a new instance of the device information. Device and useragent are provided
        /// uses indexes in the providers strings collection.
        /// </summary>
        /// <param name="provider">The provider the device will be assigned to.</param>
        /// <param name="uniqueDeviceID">The unique device name.</param>
        /// <param name="userAgentStringIndex">The string index of the user agent string.</param>
        /// <param name="parent">The parent device, or null if no parent.</param>
        internal DeviceInfo(Provider provider, string uniqueDeviceID, int userAgentStringIndex, DeviceInfo parent) : 
            base(provider,
                uniqueDeviceID,
                userAgentStringIndex >= 0 ? provider.Strings.Get(userAgentStringIndex) : String.Empty,
                parent) 
        {
        }

        /// <summary>
        /// Constructs a new instance of the device information. Device and useragent are provided
        /// uses indexes in the providers strings collection.
        /// </summary>
        /// <param name="provider">The provider the device will be assigned to.</param>
        /// <param name="uniqueDeviceID">The unique device name.</param>
        /// <param name="parent">The parent device, or null if no parent.</param>
        internal DeviceInfo(Provider provider, string uniqueDeviceID, DeviceInfo parent) :
            base(provider,
                uniqueDeviceID,
                String.Empty,
                parent)
        {
        }

        #endregion
    }
}
