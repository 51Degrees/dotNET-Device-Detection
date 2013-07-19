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

#region Usings

using System.Collections.Generic;
using FiftyOne.Foundation.Mobile.Detection.Handlers;

#endregion

namespace FiftyOne.Foundation.Mobile.Detection.Matchers
{
    /// <summary>
    /// Represents all the results found for the detection.
    /// </summary>
    internal class Results : List<Result>
    {
        /// <summary>
        /// Constructs an instance of the result class.
        /// </summary>
        internal Results()
        {
        }

        /// <summary>
        /// Constructs an instance of the result class.
        /// </summary>
        /// <param name="device">Initial device to be added.</param>
        /// <param name="handler">Handler to be associated with the device.</param>
        /// <param name="score">The score associated with the result.</param>
        /// <param name="userAgent">The target user agent.</param>
        internal Results(BaseDeviceInfo device, Handler handler, uint score, string userAgent)
        {
            Add(device, handler, score, userAgent);
        }

        /// <summary>
        /// Adds a result to the result set.
        /// </summary>
        /// <param name="device">Initial device to be added.</param>
        /// <param name="handler">Handler to be associated with the device.</param>
        /// <param name="score">The score associated with the result.</param>
        /// <param name="userAgent">The target user agent.</param>
        internal void Add(BaseDeviceInfo device, Handler handler, uint score, string userAgent)
        {
            Add(new Result(device.Provider, device, handler, score, userAgent));
        }

        /// <summary>
        /// Adds a range of devices to the results, all associated
        /// with the handler provided.
        /// </summary>
        /// <param name="devices">Array of all devices to be added.</param>
        /// <param name="handler">Handler to be associated with the devices.</param>
        /// <param name="score">The score associated with the result.</param>
        /// <param name="userAgent">The target user agent.</param>
        internal void AddRange(BaseDeviceInfo[] devices, Handler handler, uint score, string userAgent)
        {
            foreach (BaseDeviceInfo device in devices)
                Add(device, handler, score, userAgent);
        }
    }
}