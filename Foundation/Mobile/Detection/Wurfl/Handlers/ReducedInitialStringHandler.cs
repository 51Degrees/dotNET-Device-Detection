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

using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace FiftyOne.Foundation.Mobile.Detection.Wurfl.Handlers
{
    /// <summary>
    /// Device detection handler using the reduced initial string method. The first
    /// part of the strings are checked to determine a match.
    /// </summary>
    internal sealed class ReducedInitialStringHandler : Detection.Handlers.ReducedInitialStringHandler, IHandler
    {
        #region Fields

        /// <summary>
        /// A list of device ids that must be in the device hierarchy
        /// to enable the handler to support the device.
        /// </summary>
        private List<string> _supportedRootDeviceIds = new List<string>();

        /// <summary>
        /// A list of device ids that must NOT be in the device hierarchy
        /// to enable the handler to support the device.
        /// </summary>
        private List<string> _unSupportedRootDeviceIds = new List<string>();

        #endregion

        #region Properties

        /// <summary>
        /// A list of device ids that must be in the device hierarchy
        /// to enable the handler to support the device.
        /// </summary>
        List<string> IHandler.SupportedRootDeviceIds
        {
            get { return _supportedRootDeviceIds; }
        }

        /// <summary>
        /// A list of device ids that must NOT be in the device hierarchy
        /// to enable the handler to support the device.
        /// </summary>
        List<string> IHandler.UnSupportedRootDeviceIds
        {
            get { return _unSupportedRootDeviceIds; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Constucts an instance of <see cref="ReducedInitialStringHandler"/>.
        /// </summary>
        /// <param name="provider">Reference to the provider instance the handler will be associated with.</param>
        /// <param name="name">Name of the handler for debugging purposes.</param>
        /// <param name="defaultDeviceId">The default device ID to return if no match is possible.</param>
        /// <param name="confidence">The confidence this handler should be given compared to others.</param>
        /// <param name="checkUAProfs">True if UAProfs should be checked.</param>
        /// <param name="tolerance">Regex used to calculate how many characters should be matched at the beginning of the useragent.</param>
        internal ReducedInitialStringHandler(BaseProvider provider, string name, string defaultDeviceId, byte confidence, bool checkUAProfs, string tolerance)
            : base(provider, name, defaultDeviceId ?? Constants.DefaultDeviceId[0], confidence, checkUAProfs, tolerance)
        {
        }

        #endregion

        #region Overriden Methods

        /// <summary>
        /// Checks to see if the handler can support this device. The 
        /// supported and unsupported device lists are checked along
        /// with the devices hierarchy to ensure the handler supports 
        /// the device.
        /// </summary>
        /// <param name="device">Device to be checked.</param>
        /// <returns>True if the device is supported, other false.</returns>
        protected internal override bool CanHandle(BaseDeviceInfo device)
        {
            return Support.CanHandle(this, (DeviceInfo)device) && base.CanHandle(device);
        }

        #endregion
    }
}
