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

namespace FiftyOne.Foundation.Mobile.Detection
{
    /// <summary>
    /// Enumeration containing the different levels of request
    /// details that can be sent to 51Degrees.mobi. Cookie headers
    /// are never sent to 51Degrees.mobi. 
    /// </summary>
    internal enum NewDeviceDetails
    {
        /// <summary>
        /// Sends only UserAgent and UAProf header fields.
        /// </summary>
        Minimum = 0,
        /// <summary>
        /// Sends all headers except cookies.
        /// </summary>
        Maximum = 1
    }
}