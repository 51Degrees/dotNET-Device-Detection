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
 * 
 * ********************************************************************* */

using System;
using System.Collections.Generic;

namespace FiftyOne.Foundation.Mobile.Detection.Xml
{
    /// <summary>
    /// Represents a device and holds all its settings.
    /// </summary>
    public class DeviceInfo : BaseDeviceInfo
    {

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
            _parent = fallbackDevice;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the capability values index list for the static Strings collection for this device
        /// based on the index of the capability name.
        /// </summary>
        /// <param name="index">Capability name index.</param>
        /// <returns>Capability index value in the String collection, or null if the capability does not exist.</returns>
        internal protected override IList<int> GetPropertyValueStringIndexes(int index)
        {
            var value = base.GetPropertyValueStringIndexes(index);
            if (value != null)
                return value;

            if (_parent != null)
                return _parent.GetPropertyValueStringIndexes(index);

            return null;
        }

        /// <summary>
        /// Gets the list of capabilities as a pipe seperated string.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal protected string GetCapabilityAsString(int index)
        {
            // Get the string indexes for the capabilities values.
            var valueIndexes = GetPropertyValueStringIndexes(index);
            if (valueIndexes == null)
                return null;

            // Convert the values list into strings.
            var values = new string[valueIndexes.Count];
            for (int i = 0; i < valueIndexes.Count; i++)
                values[i] = _provider.Strings.Get(valueIndexes[i]);

            // Return a seperated list of strings.
            return String.Join(Detection.Constants.ValueSeperator, values);
        }

        /// <summary>
        /// Checks if another DeviceInfo is equal to this one.
        /// </summary>
        /// <param name="other">Other DeviceInfo.</param>
        /// <returns>True if the object instances are the same.</returns>
        internal bool Equals(DeviceInfo other)
        {
            return base.Equals(other) &&
                   Parent.DeviceId.Equals(other.Parent.DeviceId);
        }

        #endregion
    }
}
