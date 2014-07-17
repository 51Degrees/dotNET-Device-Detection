/* *********************************************************************
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited. 
 * Copyright © 2014 51Degrees Mobile Experts Limited, 5 Charlotte Close,
 * Caversham, Reading, Berkshire, United Kingdom RG4 7BY
 * 
 * This Source Code Form is the subject of the following patent 
 * applications, owned by 51Degrees Mobile Experts Limited of 5 Charlotte
 * Close, Caversham, Reading, Berkshire, United Kingdom RG4 7BY: 
 * European Patent Application No. 13192291.6; and
 * United States Patent Application Nos. 14/085,223 and 14/085,301.
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0.
 * 
 * If a copy of the MPL was not distributed with this file, You can obtain
 * one at http://mozilla.org/MPL/2.0/.
 * 
 * This Source Code Form is “Incompatible With Secondary Licenses”, as
 * defined by the Mozilla Public License, v. 2.0.
 * ********************************************************************* */

#if VER4

using System;
using System.Collections;
using System.Web;
using System.Web.Configuration;
using System.Diagnostics;
using System.Text;
using System.Collections.Generic;

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
        public MobileCapabilitiesProvider()
            : base()
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

        // If in debug compilation record the stack trace as this class is constructed
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
            System.Web.HttpBrowserCapabilities baseCapabilities = GetBaseCapabilities(request);

            // Get the new and overridden capabilities.
            var results = WebProvider.GetResults(HttpContext.Current, request);
            IDictionary enhancedCapabilities = MobileCapabilitiesOverride.EnhancedCapabilities(
                results, baseCapabilities);

            if (enhancedCapabilities != null)
                // Create a new browser capabilities instance combining the two.
                return new FiftyOneBrowserCapabilities(results, baseCapabilities, enhancedCapabilities);

            // We couldn't get any new values so return the current ones unaltered.
            return baseCapabilities;
        }

        /// <summary>
        /// Returns the .NET base browser capabilities.
        /// </summary>
        /// <param name="request">An HttpRequest that provides information about the source device.</param>
        /// <returns>.NET base browser capabilities</returns>
        protected virtual System.Web.HttpBrowserCapabilities GetBaseCapabilities(HttpRequest request)
        {
            return
                _parent == null
                    ? base.GetBrowserCapabilities(request)
                    : _parent.GetBrowserCapabilities(request);
        }
    }
}
#endif
