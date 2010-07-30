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
 * Mobile Experts Limited are Copyright (C) 2009 - 2010. All Rights Reserved.
 * 
 * Contributor(s):
 *     James Rosewell <james@51degrees.mobi>
 * 
 * ********************************************************************* */

using System.Web;
using System;

namespace FiftyOne.Foundation.Mobile.Detection
{
    internal class Devices
    {
        /// <summary>
        /// Cache for mobile capabilities. Items are removed approx. 60 minutes after the last
        /// time they were used.
        /// </summary>
        private static Cache<MobileCapabilities> _cache = new Cache<MobileCapabilities>(60);

        /// <summary>
        /// Checks to determine if the device is mobile by making a direct
        /// call to the device detection layer.
        /// </summary>
        /// <param name="context">The context of the request.</param>
        /// <returns>True if the device is a mobile device.</returns>
        internal static bool IsMobileDevice(HttpContext context)
        {
            if (Wurfl.Configuration.Manager.Enabled == true)
                return Wurfl.Provider.IsMobileDevice(context);
            else
                return context.Request.Browser.IsMobileDevice;
        }

        /// <summary>
        /// Creates a new <see cref="MobileCapabilities"/> class based on the useragent
        /// string provided.
        /// </summary>
        /// <param name="userAgent">The useragent for the device.</param>
        /// <returns></returns>
        internal static MobileCapabilities Create(string userAgent)
        {
            MobileCapabilities caps = null;

            if (_cache.GetTryParse(userAgent, out caps) == false &&
                Wurfl.Configuration.Manager.Enabled == true)
            {
                caps = new Wurfl.MobileCapabilities(userAgent);
                _cache[userAgent] = caps;
            }

            return caps;
        }

        /// <summary>
        /// Creates a new <see cref="MobileCapabilities"/> class based on the context
        /// of the requesting device.
        /// </summary>
        /// <param name="context"><see cref="HttpContext"/> of the requesting device.</param>
        /// <returns></returns>
        internal static MobileCapabilities Create(HttpContext context)
        {
            MobileCapabilities caps = null;

            if (String.IsNullOrEmpty(context.Request.UserAgent) == false &&
                _cache.GetTryParse(context.Request.UserAgent, out caps) == false &&
                Wurfl.Configuration.Manager.Enabled == true)
            {
                caps = new Wurfl.MobileCapabilities(context);
                _cache[context.Request.UserAgent] = caps;
            }

            return caps;
        }
    }
}
