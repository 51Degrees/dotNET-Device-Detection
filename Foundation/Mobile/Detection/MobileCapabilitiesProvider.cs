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
 *     Tom Holmes <tom@51degrees.mobi>
 * 
 * ********************************************************************* */

#if VER4

using System;
using System.Collections;
using System.Web;
using System.Web.Configuration;
using System.Diagnostics;
using System.Text;

namespace FiftyOne.Foundation.Mobile.Detection
{

    /// <summary>
    /// Used to add additional 51Degrees.mobi based properties to the browser capabilities.
    /// </summary>
    public class MobileCapabilitiesProvider : HttpCapabilitiesDefaultProvider
    {
        private HttpCapabilitiesDefaultProvider _parent = null;

        /// <summary>
        /// Constructs an instance of <cref see="MobileCapabilitiesProvider"/>.
        /// Sets the cache key length to a value of 256 to allow for mobile
        /// useragents that can often be longer than the default 64 characters.
        /// </summary>
        public MobileCapabilitiesProvider() : base()
        {
            EventLog.Debug("Constructing MobileCapabilitiesProvider - Default");
            #if DEBUG
                LogStackTrace();
            #endif
            base.UserAgentCacheKeyLength = 256;    
        }

        /// <summary>
        /// Constructs an instance of <cref see="MobileCapabilitiesProvider"/>.
        /// Sets the cache key length to a value of 256 to allow for mobile
        /// useragents that can often be longer than the default 64 characters.
        /// </summary>
        public MobileCapabilitiesProvider(HttpCapabilitiesDefaultProvider parent)
            : base(parent)
        {
            EventLog.Debug("Constructing MobileCapabilitiesProvider - HttpCapabilitiesDefaultProvider");
            #if DEBUG
                LogStackTrace();
            #endif
            _parent = parent;
            base.UserAgentCacheKeyLength = 256; 
        }

        // If in debug compilation record the stake trace as this class is constructed
        // before the debugger can become active.
        #if DEBUG
        private void LogStackTrace()
        {
            StringBuilder trace = new StringBuilder().AppendLine("Constructor Stack Trace:");
            foreach (StackFrame frame in new StackTrace().GetFrames())
                trace.AppendLine(frame.ToString());
            EventLog.Debug(trace.ToString());
        }
        #endif

        /// <summary>
        /// Provides information to the web server about the requesting device.
        /// </summary>
        /// <param name="request">An HttpRequest that provides information about the source device.</param>
        /// <returns>A HttpBrowserCapabilities object containing information relevent to the device 
        /// sources from 51Degrees.mobi.</returns>
        public override HttpBrowserCapabilities GetBrowserCapabilities(HttpRequest request)
        {
            // Get the base capabilities.
            System.Web.HttpBrowserCapabilities baseCapabilities = 
                _parent == null
                    ? base.GetBrowserCapabilities(request)
                    : _parent.GetBrowserCapabilities(request);

            // Get the new and overridden capabilities.
            IDictionary overrideCapabilities = Create(request, baseCapabilities.Capabilities);

            if (overrideCapabilities != null)
                // Create a new browser capabilities instance combining the two.
                return new FiftyOneBrowserCapabilities(baseCapabilities, overrideCapabilities);

            // We couldn't get any new values so return the current ones unaltered.
            return baseCapabilities;
        }

        /// <summary>
        /// Create the new capabilities for the request.
        /// </summary>
        /// <param name="request">An HttpRequest that provides information about the source device.</param>
        /// <param name="capabilities">Current capabilities for the request.</param>
        /// <returns>A new list of capabilities.</returns>
        protected virtual IDictionary Create(HttpRequest request, IDictionary capabilities)
        {
            return Factory.Create(request, capabilities);
        }
    }
}
#endif
