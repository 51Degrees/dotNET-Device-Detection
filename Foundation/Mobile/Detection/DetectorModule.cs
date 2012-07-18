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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Security;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;

#endregion

namespace FiftyOne.Foundation.Mobile.Detection
{
    /// <summary>
    /// Module used to enhance mobile browser information and detect and redirect
    /// mobile devices accessing non-mobile web pages.
    /// </summary>
    public class DetectorModule : Redirection.RedirectModule
    {
        #region Fields

        /// <summary>
        /// Used to lock the initialisation of static fields.
        /// </summary>
        private static readonly object _lock = new object();

        #if !VER4

        /// <summary>
        /// Indicates if static initialisation has been completed.
        /// </summary>
        private static bool _initialised = false;

        /// <summary>
        /// Collection of client targets used by the application with the key
        /// field representing the alias and the value the useragent.
        /// </summary>
        private static SortedList<string, string> _clientTargets;

        #endif

        #endregion

        #region Initialisers

        /// <summary>
        /// Initialises the device detection module.
        /// </summary>
        /// <param name="application">HttpApplication object for the web application.</param>
        public override void Init(HttpApplication application)
        {
            Initialise(application);
            base.Init(application);
        }

        /// <summary>
        /// Initiliases the HttpModule registering this modules interest in
        /// all new requests and handler mappings.
        /// </summary>
        /// <param name="application">HttpApplication object for the web application.</param>
        protected void Initialise(HttpApplication application)
        {
#if VER4
                // Replace the .NET v4 BrowserCapabilitiesProvider if it's not ours.
                if (HttpCapabilitiesBase.BrowserCapabilitiesProvider is MobileCapabilitiesProvider == false)
                {
                    lock (_lock)
                    {
                        if (HttpCapabilitiesBase.BrowserCapabilitiesProvider is MobileCapabilitiesProvider == false)
                        {
                            // Use the existing provider as the parent if it's not null. 
                            if (HttpCapabilitiesBase.BrowserCapabilitiesProvider != null)
                                HttpCapabilitiesBase.BrowserCapabilitiesProvider = 
                                    new MobileCapabilitiesProvider((HttpCapabilitiesDefaultProvider)HttpCapabilitiesBase.BrowserCapabilitiesProvider);
                            else
                                HttpCapabilitiesBase.BrowserCapabilitiesProvider = 
                                    new MobileCapabilitiesProvider();
                        }
                    }
                }

                // The new BrowserCapabilitiesProvider functionality in .NET v4 negates the need to 
                // override browser properties in the following way. The rest of the module is redundent
                // in .NET v4.
#else
            EventLog.Debug("Initialising Detector Module");

            StaticFieldInit();

            // Intercept the beginning of the request to override the capabilities.
            application.BeginRequest += OnBeginRequest;

            // Check for a MobilePage handler being used.
            application.PreRequestHandlerExecute += SetPreferredRenderingType;

            // If client targets are specified then check to see if one is being used
            // and override the requesting device information.
            if (_clientTargets != null)
                application.PreRequestHandlerExecute += SetPagePreIntClientTargets;
#endif
        }

#if !VER4

        /// <summary>
        /// Initialises the static fields.
        /// </summary>
        private static void StaticFieldInit()
        {
            if (_initialised == false)
            {
                lock (_lock)
                {
                    if (_initialised == false)
                    {
                        // Get a list of the client target names.
                        _clientTargets = GetClientTargets();

                        // Indicate initialisation is complete.
                        _initialised = true;
                    }
                }
            }
        }

#endif

        #endregion

#if !VER4

        #region Events


        /// <summary>
        /// If the handler is a MobilePage then ensure a compaitable textwriter will be used.
        /// </summary>
        /// <param name="sender">HttpApplication related to the request.</param>
        /// <param name="e">EventArgs related to the event. Not used.</param>
        private static void SetPreferredRenderingType(object sender, EventArgs e)
        {
            if (sender is HttpApplication)
            {
                HttpContext context = ((HttpApplication)sender).Context;

                // Check to see if the handler is a mobile page. If so then the preferred markup
                // needs to change as the legacy mobile controls do not work with html4 specified.
                // If these lines are removed a "No mobile controls device configuration was registered 
                // for the requesting device" exception is likely to occur.
                if (context.Handler != null &&
                    IsMobileType(context.Handler.GetType()) &&
                    "html4".Equals(context.Request.Browser.PreferredRenderingType, 
                        StringComparison.InvariantCultureIgnoreCase))
                {
                    context.Request.Browser.Capabilities["preferredRenderingType"] = "html32";
                }
            }
        }

        /// <summary>
        /// Ensures the PreInit event of the page is processed by the module
        /// to override capabilities associated with a client target specification.
        /// </summary>
        /// <param name="sender">HttpApplication related to the request.</param>
        /// <param name="e">EventArgs related to the event. Not used.</param>
        private void SetPagePreIntClientTargets(object sender, EventArgs e)
        {
            if (sender is HttpApplication)
            {
                HttpContext context = ((HttpApplication)sender).Context;

                // If this handler relates to a page use the preinit event of the page
                // to set the capabilities providing time for the page to set the 
                // clienttarget property if required.
                if (context != null &&
                    context.Handler is Page)
                    ((Page)context.Handler).PreInit += OnPreInitPage;
            }
        }

        /// <summary>
        /// Override the capabilities assigned to the request.
        /// </summary>
        /// <param name="sender">HttpApplication related to the request.</param>
        /// <param name="e">EventArgs related to the event. Not used.</param>
        private void OnBeginRequest(object sender, EventArgs e)
        {
            if (sender is HttpApplication)
            {
                HttpContext context = ((HttpApplication)sender).Context;
                if (context != null)
                {
                    try
                    {
                        // Override the capabilities incase the developers page needs them in the 
                        // preinit event of the page.                
                        OverrideCapabilities(context);
                    }

                    // Some issues of null exceptions have been reported and this is temporary code to trap 
                    // them recording headers to help further diagnosis.
                    catch (NullReferenceException ex)
                    {
                        StringBuilder builder = new StringBuilder("A Null Exception occured which has the following header info:");
                        foreach (string key in context.Request.Headers.Keys)
                            builder.Append(Environment.NewLine).Append(key).Append("=").Append(
                                context.Request.Headers[key]);

                        // Create new exception with the additional information and record to the log file.
                        EventLog.Fatal(new MobileException(builder.ToString(), ex));
                    }
                }
            }
        }

        /// <summary>
        /// Before the page initialises make sure the latest browser capabilities
        /// are available if a client target has been specified.
        /// </summary>
        /// <param name="sender">Page associated with the request.</param>
        /// <param name="e">Event arguements.</param>
        private void OnPreInitPage(object sender, EventArgs e)
        {
            Page page = sender as Page;

            // Check to see if a client target has been specified and if it has
            // use the associated useragent string to assign the capabilities.
            string userAgent = null;
            if (page != null && _clientTargets.TryGetValue(page.ClientTarget, out userAgent))
                OverrideCapabilities(page.Request, userAgent);
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Gets the client target section from the configuration if the security level
        /// allows this method to be used. Will fail in medium trust environments.
        /// </summary>
        /// <returns></returns>
        private static ClientTargetSection GetClientTargetsSection()
        {
            return WebConfigurationManager.GetWebApplicationSection(
                       "system.web/clientTarget") as ClientTargetSection;
        }

        /// <summary>
        /// Gets a list of the client target names configured for the application. Will return null
        /// if either target names are not defined or unrestricted security access is not available.
        /// </summary>
        /// <returns>A list of client target names.</returns>
        private static SortedList<string, string> GetClientTargets()
        {
            try
            {
                ClientTargetSection targets = GetClientTargetsSection();
                if (targets != null)
                {
                    // Client targets have been defined so set the sorted list to include
                    // these details.
                    SortedList<string, string> clientNames = new SortedList<string, string>();
                    for (int index = 0; index < targets.ClientTargets.Count; index++)
                    {
                        clientNames.Add(
                            targets.ClientTargets[index].Alias,
                            targets.ClientTargets[index].UserAgent);
                    }
                    return clientNames;
                }
            }
            catch (SecurityException)
            {
                // There is nothing we can do so return null.
                return null;
            }
            return null;
        }

        /// <summary>
        /// Adds the capabilities provided into the existing dictionary of capabilities
        /// already assigned by Microsoft.
        /// </summary>
        /// <param name="currentCapabilities"></param>
        /// <param name="overrideCapabilities"></param>
        protected static HttpBrowserCapabilities AddNewCapabilities(HttpBrowserCapabilities currentCapabilities, IDictionary overrideCapabilities)
        {
            // We can't do anything with null capabilities. Return the current ones.
            if (overrideCapabilities == null)
                return currentCapabilities;

            // Use our own capabilities object.
            return new FiftyOneBrowserCapabilities(currentCapabilities, overrideCapabilities);
        }

        #endregion

        #region Virtual Methods

        /// <summary>
        /// If this device is not already using our mobile capabilities
        /// then check to see if it's a mobile. If we think it's a mobile
        /// or .NET thinks it's not the same type of device then override
        /// the current capabilities.
        /// </summary>
        /// <param name="context"><see cref="HttpContext"/> to be tested and overridden.</param>
        protected virtual void OverrideCapabilities(HttpContext context)
        {
            context.Request.Browser = AddNewCapabilities(context.Request.Browser,
                Factory.Create(context.Request, context.Request.Browser.Capabilities));
        }

        /// <summary>
        /// Adds new capabilities for the useragent provided rather than the request details
        /// provided in the request paramters.
        /// </summary>
        /// <param name="request">The request who's capabilities collection should be updated.</param>
        /// <param name="userAgent">The useragent string of the device requiring capabilities to be added.</param>
        protected virtual void OverrideCapabilities(HttpRequest request, string userAgent)
        {
            request.Browser = AddNewCapabilities(request.Browser, Factory.Create(userAgent));
        }

        #endregion

#endif

        #region Methods

        /// <summary>
        /// Records the module being disposed if debug enabled.
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            EventLog.Debug("Disposing Detector Module");
        }

        #endregion
    }
}