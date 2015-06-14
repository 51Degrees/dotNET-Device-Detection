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
        /// <summary>
        /// Constructs an instance of <cref see="MobileCapabilitiesProvider"/>.
        /// All default values remain the same and are unaltered.
        /// </summary>
        public MobileCapabilitiesProvider()
            : base()
        {
            EventLog.Debug("Constructing MobileCapabilitiesProvider");
#if DEBUG
            LogStackTrace();
#endif
        }

#if DEBUG
        /// <summary>
        /// If in debug compilation record the stack trace as this class is constructed
        /// before the debugger can become active.
        /// </summary>
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
            var match = WebProvider.GetMatch(request);
            if (match != null)
            {
                // A provider is present so 51Degrees can be used to override
                // some of the returned values.
                return new FiftyOneBrowserCapabilities(
                    base.GetBrowserCapabilities(request), 
                    request,
                    match);
            }
            else
            {
                // No 51Degrees active provider is present so we have to use
                // the base capabilities only.
                return base.GetBrowserCapabilities(request);
            }
        }
    }
}