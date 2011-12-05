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