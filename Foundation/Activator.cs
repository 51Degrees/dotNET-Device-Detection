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
 * Mobile Experts Limited are Copyright (C) 2009 - 2012. All Rights Reserved.
 * 
 * Contributor(s):
 *     James Rosewell <james@51degrees.mobi>
 *     Steve Sanderson <Steve.Sanderson@microsoft.com>
 *     
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
                        "included explicitly in the web.config file.");
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
            Microsoft.Web.Infrastructure.DynamicModuleHelper.DynamicModuleUtility.RegisterModule(typeof(FiftyOne.Foundation.Mobile.Redirection.RedirectModule));
        }
    }
}
