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
 * Mobile Experts Limited are Copyright (C) 2009 - 2012. All Rights Reserved.
 * 
 * Contributor(s):
 *     James Rosewell <james@51degrees.mobi>
 * 
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
        internal DeviceInfo(BaseProvider provider, string uniqueDeviceID, int userAgentStringIndex, DeviceInfo parent) : 
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
        internal DeviceInfo(BaseProvider provider, string uniqueDeviceID, DeviceInfo parent) :
            base(provider,
                uniqueDeviceID,
                String.Empty,
                parent)
        {
        }

        #endregion
    }
}
