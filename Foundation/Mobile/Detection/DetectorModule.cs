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

using System;
using System.Web;
using System.Diagnostics;
using System.Text.RegularExpressions;
using FiftyOne.Foundation.Mobile.Configuration;
using FiftyOne.Foundation.Mobile.Detection.Wurfl;
using System.Drawing;
using System.Web.Configuration;
using System.Security.Permissions;
using System.Security;
using System.Collections.Generic;
using System.Web.Security;
using System.Text;

namespace FiftyOne.Foundation.Mobile.Detection
{
    /// <summary>
    /// Module used to enhance mobile browser information and detect and redirect
    /// mobile devices accessing non-mobile web pages.
    /// </summary>
    public class DetectorModule : IHttpModule
    {
        #region Constants

        /// <summary>
        /// Full type names of classes representing standard
        /// page handlers.
        /// </summary>
        private readonly static string[] PAGES = new string[] {
            "System.Web.UI.MobileControls.MobilePage",
            "System.Web.UI.Page" };

        /// <summary>
        /// Full type names of classes representing mobile
        /// page handlers.
        /// </summary>
        private readonly static string[] MOBILE_PAGES = {
            "System.Web.UI.MobileControls.MobilePage"};

        #endregion

        #region Fields

        /// <summary>
        /// The URL to use to redirect a mobile device accessing
        /// a non mobile web page to. Initialised from the <c>web.config</c> file
        /// when the module is created.
        /// </summary>
        private string _mobileHomePageUrl = null;

        /// <summary>
        /// If set to true only the first eligable request received by the web
        /// site will be redirected to the mobile landing page contained in
        /// _mobileRedirectUrl.
        /// </summary>
        private bool _firstRequestOnly = true;

        /// <summary>
        /// A regular expression that when applied to the current request Path
        /// (context.Request.AppRelativeCurrentExecutionFilePath) will return true
        /// if it should be considered a mobile page. Use this attribute to tell
        /// redirection about mobile pages derived from base classes such as 
        /// System.Web.UI.Page. Redirection needs to be aware of mobile pages so
        /// that requests to these pages can be ignored. Any page that derives from 
        /// System.Web.UI.MobileControls.MobilePage will automatically be treated 
        /// as a mobile page irrespective of this attribute. (Optional)
        /// </summary>
        private Regex _mobilePageRegex = null;

        /// <summary>
        /// Collection of client targets used by the application with the key
        /// field representing the alias and the value the useragent.
        /// </summary>
        private SortedList<string, string> _clientTargets = null;

        /// <summary>
        /// The login url for forms authentication.
        /// </summary>
        private string _formsLoginUrl = null;
        
        #endregion

        #region Initialisers

        /// <summary>
        /// Initiliases the HttpMobile registering this modules interest in
        /// all new requests and handler mappings.
        /// </summary>
        /// <param name="application">HttpApplication object for the web application.</param>
        public void Init(HttpApplication application)
        {
            EventLog.Debug("Initialising Detector Module");

            // Fetch the redirect url, first time redirect indicator and wire up the
            // events if a url has been provided.
            if (Manager.Redirect != null && Manager.Redirect.Enabled == true)
            {
                _mobileHomePageUrl = Manager.Redirect.MobileHomePageUrl;
                _firstRequestOnly = Manager.Redirect.FirstRequestOnly;
                if (String.IsNullOrEmpty(Manager.Redirect.MobilePagesRegex) == false)
                    _mobilePageRegex = new Regex(Manager.Redirect.MobilePagesRegex, RegexOptions.Compiled|RegexOptions.IgnoreCase);
                _formsLoginUrl = FormsAuthentication.LoginUrl;
            }

            // Get a list of the client target names.
            _clientTargets = GetClientTargets();

            // Intercept the beginning of the request to override the capabilities.
            application.BeginRequest += new EventHandler(OnBeginRequest);

            // Intercept request event after the hander and the state have been assigned
            // to redirect the page.
            application.PostAcquireRequestState += new EventHandler(OnPostAcquireRequestState);

            // Check for a MobilePage handler being used.
            application.PreRequestHandlerExecute += new EventHandler(OnPreRequestHandlerExecuteMobilePage);

            // If client targets are specified then check to see if one is being used
            // and override the requesting device information.
            if (_clientTargets != null)
                application.PreRequestHandlerExecute +=new EventHandler(OnPreRequestHandlerExecuteSetClientTargets);
        }
        
        #endregion

        #region Events

        /// <summary>
        /// If the handler is a MobilePage then ensure a compaitable textwriter will be used.
        /// </summary>
        /// <param name="sender">HttpApplication related to the request.</param>
        /// <param name="e">EventArgs related to the event. Not used.</param>
        public void OnPreRequestHandlerExecuteMobilePage(object sender, EventArgs e)
        {
            if (sender is HttpApplication)
            {
                HttpContext context = ((HttpApplication)sender).Context;

                // Check to see if the handler is a mobile page. If so then the preferred markup
                // needs to change as the legacy mobile controls do not work with html4 specified.
                // If these lines are removed a "No mobile controls device configuration was registered 
                // for the requesting device" exception is likely to occur.
                if (context.Handler is System.Web.UI.MobileControls.MobilePage &&
                    "html4" == context.Request.Browser.Capabilities["preferredRenderingType"] as string)
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
        public void OnPreRequestHandlerExecuteSetClientTargets(object sender, EventArgs e)
        {
            if (sender is HttpApplication)
            {
                HttpContext context = ((HttpApplication)sender).Context;
                                
                // If this handler relates to a page use the preinit event of the page
                // to set the capabilities providing time for the page to set the 
                // clienttarget property if required.
                if (context != null &&
                    context.Handler is System.Web.UI.Page)
                    ((System.Web.UI.Page)context.Handler).PreInit += new EventHandler(OnPreInitPage);
            }
        }

        /// <summary>
        /// Override the capabilities assigned to the request.
        /// </summary>
        /// <param name="sender">HttpApplication related to the request.</param>
        /// <param name="e">EventArgs related to the event. Not used.</param>
        public void OnBeginRequest(object sender, EventArgs e)
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
                            builder.Append(Environment.NewLine).Append(key).Append("=").Append(context.Request.Headers[key]);

                        // Create new exception with the additional information and record to the log file.
                        EventLog.Fatal(new MobileException(builder.ToString(), ex));
                    }
                }
            }
        }

        /// <summary>
        /// If the handler assigned to the request isn't a mobile one and the browser that
        /// is accessing the page is a wireless device then redirect the browser to the 
        /// mobile home page for the site. The redirect is done through the resposne to
        /// ensure a fresh request is made to the server.
        /// </summary>
        /// <param name="sender">HttpApplication related to the request.</param>
        /// <param name="e">EventArgs related to the event. Not used.</param>
        public void OnPostAcquireRequestState(object sender, EventArgs e)
        {
            if (sender is HttpApplication)
            {
                HttpContext context = ((HttpApplication)sender).Context;

                if (context != null)
                {
                    // Check to see if the request should be redirected.
                    if (context.Handler != null &&
                        String.IsNullOrEmpty(_mobileHomePageUrl) == false &&
                        IsRedirectPage(context) == false &&
                        IsPage(context) == true &&
                        IsMobilePage(context) == false &&
                        context.Request.Browser.IsMobileDevice == true &&
                        IsFirstTime(context, _firstRequestOnly) == true &&
                        IsRestrictedPageForRedirect(context) == false)
                    {
                        string newUrl = null;
                        if (String.IsNullOrEmpty(context.Request.QueryString.ToString()) == false)
                            newUrl = String.Format("{0}?{1}",
                                _mobileHomePageUrl,
                                context.Request.QueryString.ToString());
                        else
                            newUrl = _mobileHomePageUrl;
                        context.Response.Redirect(newUrl, true);
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
        public void OnPreInitPage(object sender, EventArgs e)
        {
            System.Web.UI.Page page = sender as System.Web.UI.Page;

            // Check to see if a client target has been specified and if it has
            // use the associated useragent string to assign the capabilities.
            string userAgent = null;
            if (page != null && _clientTargets.TryGetValue(page.ClientTarget, out userAgent) == true)
                OverrideCapabilities(page.Request, userAgent);
        }

        #endregion

        #region Methods
        
        /// <summary>
        /// Gets the client target section from the configuration if the security level
        /// allows this method to be used. Will fail in medium trust environments.
        /// </summary>
        /// <returns></returns>
        private ClientTargetSection GetClientTargetsSection()
        {
            return WebConfigurationManager.GetWebApplicationSection(
                "system.web/clientTarget") as ClientTargetSection;
        }

        /// <summary>
        /// Gets a list of the client target names configured for the application. Will return null
        /// if either target names are not defined or unrestricted security access is not available.
        /// </summary>
        /// <returns>A list of client target names.</returns>
        private SortedList<string, string> GetClientTargets()
        {
            try
            {
                ClientTargetSection targets = GetClientTargetsSection();
                if (targets != null)
                {
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
                return null;
            }
            return null;
        }

        /// <summary>
        /// If this device is not already using our mobile capabilities
        /// then check to see if it's a mobile. If we think it's a mobile
        /// or .NET thinks it's not the same type of device then override
        /// the current capabilities.
        /// </summary>
        /// <param name="context"><see cref="HttpContext"/> to be tested and overridden.</param>
        private static void OverrideCapabilities(HttpContext context)
        {
            if (context.Request.Browser is MobileCapabilities == false)
            {
                bool newIsMobile = Devices.IsMobileDevice(context);
                newIsMobile = true;
                if (newIsMobile == true || newIsMobile != context.Request.Browser.IsMobileDevice)
                {
                    MobileCapabilities newCap = Devices.Create(context);
                    if (newCap != null)
                        context.Request.Browser = newCap;
                }
            }
        }

        private static void OverrideCapabilities(HttpRequest request, string userAgent)
        {
            MobileCapabilities newCap = Devices.Create(userAgent);
            if (newCap != null)
                request.Browser = newCap;
        }

        /// <summary>
        /// Checks the page being requested to determine if it is eligable
        /// for redirection. The mobile home page and the forms authentication
        /// login page are restricted from redirection.
        /// </summary>
        /// <param name="context">The context of the request.</param>
        /// <returns>True if the page is restricted from being redirected.</returns>
        private bool IsRestrictedPageForRedirect(HttpContext context)
        {
            System.Web.UI.Page page = context.Handler as System.Web.UI.Page;
            if (page != null)
            {
                string currentPage = page.ResolveUrl(context.Request.AppRelativeCurrentExecutionFilePath);
                return
                    page.ResolveUrl(_mobileHomePageUrl) == currentPage ||
                    page.ResolveUrl(_formsLoginUrl) == currentPage;
            }
            return false;
        }

        internal static bool IsFirstTime(HttpContext context, bool firstRequestOnly)
        {
            // If the parameter indicating only the first request should be redirect
            // is false then return true as the implication is all requests should
            // be redirected.
            if (firstRequestOnly == false)
                return true;

            // Check to see if the Referrer URL contains the same host name
            // as the current page request. If there is a match this request has
            // come from another web page on the same host and is not the 1st request.
            if (context.Request.UrlReferrer != null &&
                context.Request.UrlReferrer.Host == context.Request.Url.Host)
                return false;

            // Check to see if we have a session parameter indicating a request has 
            // already been processed.
            if (context.Session != null &&
                context.Session[Constants.IsFirstRequest] != null)
            {
                // Update the parameter to indicate this is nolonger
                // the first request.
                context.Session[Constants.IsFirstRequest] = false;
                // Remove our own cookie from the response as it's not 
                // needed because the session is working.
                if (context.Request.Cookies[Constants.AlreadyAccessedCookieName] != null)
                    context.Response.Cookies.Remove(Constants.AlreadyAccessedCookieName);
                return false;
            }

            // Check to see if our cookie is present from a previous request.
            if (context.Request.Cookies[Constants.AlreadyAccessedCookieName] != null)
                return false;

            // Check to see if the requested IP address and HTTP headers hashcode is
            // on record as having been seen before.
            if (RequestHistory.IsPresent(context.Request) == true)
                return false;

            // The 4 checks have all failed so we're now certain this is the 1st request
            // to the web site. Record this information using the session if available
            // a cookie and the DevicesFile.
            if (context.Session != null)
                // Add a parameter to the session indicating this is the first
                // request. It will be updated later if available during subsequent
                // requests.
                context.Session[Constants.IsFirstRequest] = true;

            // Add a cookie to the response setting the expiry time to the session timeout
            // or if not available the redirection timeout.
            // Modified to check for existance of cookie to avoid recreating.
            if (new List<string>(context.Response.Cookies.AllKeys).Contains(Constants.AlreadyAccessedCookieName) == false)
            {
                HttpCookie alreadyAccessed = new HttpCookie(Constants.AlreadyAccessedCookieName);
                if (context.Session != null)
                    alreadyAccessed.Expires = DateTime.UtcNow.AddMinutes(context.Session.Timeout);
                else if (Manager.Redirect != null)
                    alreadyAccessed.Expires = DateTime.UtcNow.AddMinutes(Manager.Redirect.Timeout);
                context.Response.Cookies.Add(alreadyAccessed);
            }
            return true;
        }

        /// <summary>
        /// Checks the value is contained in the array.
        /// </summary>
        /// <param name="value">Value string.</param>
        /// <param name="array">Array of strings to be checked against.</param>
        /// <returns>True if the string is present. False if not.</returns>
        private static bool IsInArray(string value, string[] array)
        {
            foreach (string current in array)
            {
                if (value == current)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Checks the array of page types to determine if the type past into the method
        /// is in the array. Also checks basetypes where available.
        /// </summary>
        /// <param name="type">The type to be checked.</param>
        /// <returns>True is the request relates to a page.</returns>
        private static bool IsPageType(Type type)
        {
            if (type != null)
            {
                if (IsInArray(type.FullName, PAGES) == true)
                    return true;
                else if (type.BaseType != null)
                    return IsPageType(type.BaseType);
            }
            return false;
        }

        /// <summary>
        /// Returns true if the request relates to a handler that relates to web page.
        /// </summary>
        /// <param name="context">The context associated with the Http request.</param>
        private static bool IsPage(HttpContext context)
        {
            if (context.Handler != null)
                return IsPageType(context.Handler.GetType());
            return false;
        }

        /// <summary>
        /// Checks the type to see if it's one that matches a mobile web page.
        /// </summary>
        /// <param name="type">Type of class to check.</param>
        /// <returns>True if the type is a mobile web page.</returns>
        private static bool IsMobileType(Type type)
        {
            if (type != null)
            {
                if (IsInArray(type.FullName, MOBILE_PAGES) == true)
                    return true;
                else if (type.BaseType != null)
                    return IsMobileType(type.BaseType);
            }
            return false;
        }

        /// <summary>
        /// Checks to see if the request.Path property of the request is matched
        /// by the regular expression returning the result.
        /// </summary>
        /// <param name="request">The HttpRequest to be checked.</param>
        /// <returns>True if this request should be considered associated with
        /// a page designed for mobile.</returns>
        private bool IsMobileRegexPage(HttpRequest request)
        {
            if (_mobilePageRegex != null)
                return _mobilePageRegex.IsMatch(request.AppRelativeCurrentExecutionFilePath);
            return false;
        }

        /// <summary>
        /// Returns true if the current handler relates to a mobile web page.
        /// </summary>
        /// <param name="context">The context associated with the Http request.</param>
        private bool IsMobilePage(HttpContext context)
        {
            return IsMobileType(context.Handler.GetType()) ||
                IsMobileRegexPage(context.Request);
        }

        /// <summary>
        /// Returns true if the page being accessed is the same as the mobile landing page.
        /// Used to prevent continued redirects to the mobile landing page if it's not
        /// been created as a mobile page.
        /// </summary>
        /// <param name="context">The context associated with the Http request.</param>
        private bool IsRedirectPage(HttpContext context)
        {
            return _mobileHomePageUrl.Equals(
                context.Request.AppRelativeCurrentExecutionFilePath,
                StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Records the module being disposed if debug enabled.
        /// </summary>
        public void Dispose()
        {
            EventLog.Debug("Disposing Detector Module");
        }

        #endregion
    }
}