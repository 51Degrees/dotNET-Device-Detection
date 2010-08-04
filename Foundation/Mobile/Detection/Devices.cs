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
using System.Collections;
using System.Web.UI.Adapters;

namespace FiftyOne.Foundation.Mobile.Detection
{
    internal class Devices
    {
        /// <summary>
        /// Cache for mobile capabilities. Items are removed approx. 60 minutes after the last
        /// time they were used.
        /// </summary>
        private static Cache<IDictionary> _cache = new Cache<IDictionary>(60);

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
            IDictionary caps = null;
            MobileCapabilities mobCaps = null;

            if (Wurfl.Configuration.Manager.Enabled == true)
            {
                if (_cache.GetTryParse(userAgent, out caps) == true)
                {
                    // Create a new mobile capabilities instance using the capabilities
                    // found previously.
                    mobCaps = new Wurfl.MobileCapabilities(caps);
                }
                else
                {
                    // Create the new mobile capabilities and record the collection of
                    // capabilities for quick creation in future requests.
                    mobCaps = new Wurfl.MobileCapabilities(userAgent);
                    _cache[userAgent] = mobCaps.Capabilities;
                }
            }

            return mobCaps;
        }

        /// <summary>
        /// Creates a new <see cref="MobileCapabilities"/> class based on the context
        /// of the requesting device.
        /// </summary>
        /// <param name="context"><see cref="HttpContext"/> of the requesting device.</param>
        /// <returns>A new mobile capabilities</returns>
        internal static MobileCapabilities Create(HttpContext context)
        {
            IDictionary caps = null;
            MobileCapabilities mobCaps = null;

            if (String.IsNullOrEmpty(context.Request.UserAgent) == false)
            {
                if (Wurfl.Configuration.Manager.Enabled == true)
                {
                    if (_cache.GetTryParse(context.Request.UserAgent, out caps) == true)
                    {
                        // Create a new mobile capabilities instance using the capabilities
                        // found previously.
                        mobCaps = new Wurfl.MobileCapabilities(caps);
                    }
                    else
                    {
                        // Create the new mobile capabilities and record the collection of
                        // capabilities for quick creation in future requests.
                        mobCaps = new Wurfl.MobileCapabilities(context);
                        _cache[context.Request.UserAgent] = mobCaps.Capabilities;
                    }
                }
            }

            // Copy over the adapters collections from the base capabilities.
            foreach (object key in context.Request.Browser.Adapters.Keys)
                mobCaps.Adapters.Add(key, context.Request.Browser.Adapters[key]);

            // Check to ensure the tagWriter.
            return mobCaps;
        }
    }
}
