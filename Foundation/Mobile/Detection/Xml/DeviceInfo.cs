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
            Provider provider,
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
            Provider provider,
            string deviceId)
            : base(provider, deviceId)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the capability values index list for the static Strings collection for this device
        /// based on the index of the capability name.
        /// </summary>
        /// <param name="index">Capability name index.</param>
        /// <returns>Capability index value in the String collection, or null if the capability does not exist.</returns>
        internal protected override List<int> GetPropertyValueStringIndexes(int index)
        {
            List<int> value = base.GetPropertyValueStringIndexes(index);
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
            List<int> valueIndexes = GetPropertyValueStringIndexes(index);
            if (valueIndexes == null)
                return null;

            // Convert the values list into strings.
            string[] values = new string[valueIndexes.Count];
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
