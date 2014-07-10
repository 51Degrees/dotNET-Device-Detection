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

using System.Web;
using System.Web.Configuration;
using System;

// The following assembly attribute make the static method run on app start.
[assembly: PreApplicationStartMethod(typeof(FiftyOne.Foundation.PreApplicationStart), "Start")]

namespace FiftyOne.Foundation
{
    /// <summary>
    /// Classed used by ASP.NET v4 to activate 51Degrees.mobi Foundation removing
    /// the need to activate from the web.config file.
    /// </summary>
    public static class PreApplicationStart
    {
        /// <summary>
        /// Flag to indicate if the method has already been called.
        /// </summary>
        private static bool _initialised = false;

        /// <summary>
        /// Method called with the worker process starts to activate the
        /// mobile capabilities of the DLL without requiring web.config
        /// entries.
        /// </summary>
        public static void Start()
        {
            if (_initialised == false && 
                FiftyOne.Foundation.Mobile.Detection.Configuration.Manager.Enabled)
            {
                // Replace the browser capabilities provider with one that is 51Degrees
                // enabled if not done so already and detection is enabled.
                if (HttpCapabilitiesBase.BrowserCapabilitiesProvider is
                    FiftyOne.Foundation.Mobile.Detection.MobileCapabilitiesProvider == false &&
                    FiftyOne.Foundation.Mobile.Detection.Configuration.Manager.Enabled)
                {
                    HttpCapabilitiesBase.BrowserCapabilitiesProvider =
                        new FiftyOne.Foundation.Mobile.Detection.MobileCapabilitiesProvider();
                }

                // Include the detection module if the Microsoft.Web.Infrastructure assembly is
                // available. This is needed to perform background actions such as automatic
                // updates.
                try
                {
                    RegisterModule();
                }
                catch (Exception ex)
                {
                    EventLog.Warn("Detection module could not automatically be registered. " +
                        "Some detection and redirection services will not be available unless the " +
                        "HttpModule is included explicitly in the web.config file or " +
                        "Microsoft.Web.Infrastructure is installed.");
                    if (EventLog.IsDebug)
                        EventLog.Debug(ex);
                }
                _initialised = true;
            }
        }

        /// <summary>
        /// Registers the HttpModule for detection and redirection.
        /// </summary>
        /// <remarks>
        /// The detection module inherits from the redirection module due to maintaining
        /// compaitability with legacy code. The functionality of the modules is controlled
        /// via the 51Degrees.config file.
        /// </remarks>
        private static void RegisterModule()
        {
            Microsoft.Web.Infrastructure.DynamicModuleHelper.DynamicModuleUtility.RegisterModule(
                typeof(FiftyOne.Foundation.Mobile.Detection.DetectorModule));
        }
    }
}
