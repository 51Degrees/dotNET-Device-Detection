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

namespace FiftyOne.Foundation.Mobile.Detection.Matchers
{
    /// <summary>
    /// Contains a device matched via a handler.
    /// </summary>
    public class Result : IComparable<Result>
    {
        private readonly BaseDeviceInfo _device;

        internal Result(BaseDeviceInfo device)
        {
            _device = device;
        }

        internal BaseDeviceInfo Device
        {
            get { return _device; }
        }

        #region IComparable<Result> Members

        /// <summary>
        /// Compare this instance to another.
        /// </summary>
        /// <param name="other">Instance for comparison.</param>
        /// <returns>Zero if equal. 1 if higher or -1 if lower.</returns>
        public int CompareTo(Result other)
        {
            if (Device == null && other.Device == null)
                return 0;
            return Device.DeviceId.CompareTo(other.Device.DeviceId);
        }

        #endregion
    }
}