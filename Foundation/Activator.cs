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
            if (_initialised == false)
            {
                // Replace the browser capabilities provider with one that is 51Degrees.mobi
                // enabled.
                if (HttpCapabilitiesBase.BrowserCapabilitiesProvider is FiftyOne.Foundation.Mobile.Detection.MobileCapabilitiesProvider == false)
                    HttpCapabilitiesBase.BrowserCapabilitiesProvider = new FiftyOne.Foundation.Mobile.Detection.MobileCapabilitiesProvider();

                // Include the redirection module if the Microsoft.Web.Infrastructure assembly is
                // available.
                try
                {
                    RegisterModule();
                }
                catch (Exception ex)
                {
                    EventLog.Warn("Redirection module could not automatically be registered. " +
                        "Redirection services will not be available unless the HttpModule is " +
                        "included explicitly in the web.config file or Microsoft.Web.Infrastructure " +
                        "is installed.");
                    if (EventLog.IsDebug)
                        EventLog.Debug(ex);
                }
                _initialised = true;
            }
        }

        /// <summary>
        /// Registers the HttpModule for redirection.
        /// </summary>
        private static void RegisterModule()
        {
            Microsoft.Web.Infrastructure.DynamicModuleHelper.DynamicModuleUtility.RegisterModule(
                typeof(FiftyOne.Foundation.Mobile.Redirection.RedirectModule));
        }
    }
}
