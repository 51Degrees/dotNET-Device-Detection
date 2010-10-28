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

#if VER4
using System.Linq;
#endif

#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.MobileControls;
using FiftyOne.Foundation.Mobile.Configuration;

#endregion

namespace FiftyOne.Foundation.Mobile.Detection
{
    /// <summary>
    /// Module used to enhance mobile browser information and detect and redirect
    /// mobile devices accessing non-mobile web pages.
    /// </summary>
    public class DetectorModule : IHttpModule
    {
        #region Private Classes

        private class Filter
        {
            #region Fields

            private readonly string _capability;
            private readonly Regex _expression;

            #endregion

            #region Constructor

            internal Filter(string capability, string expression)
            {
                _capability = capability;
                _expression = new Regex(expression, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }

            #endregion

            #region Internal Methods
            
            /// <summary>
            /// Determines if this filter matches the requesting device.
            /// </summary>
            /// <param name="context">Context of the requesting device.</param>
            /// <returns>True if the capability matches, otherwise false.</returns>
            internal bool GetIsMatch(HttpContext context)
            {
                string value = GetPropertyValue(context, _capability);
                if (String.IsNullOrEmpty(value))
                    return false;
                return _expression.IsMatch(value);
            }

            #endregion

            #region Private Members

            /// <summary>
            /// Returns the value for the property requested.
            /// </summary>
            /// <param name="property"></param>
            /// <returns></returns>
            private static string GetPropertyValue(HttpContext context, string property)
            {
                string value;
     
                // Try the standard properties of the browser object.
                Type controlType = context.Request.Browser.GetType();
                System.Reflection.PropertyInfo propertyInfo = controlType.GetProperty(property);
                if (propertyInfo != null && propertyInfo.CanRead)
                    return propertyInfo.GetValue(context.Request.Browser, null).ToString();
     
                // Try wurfl capabilities next.
                var wurflCapabilities = context.Request.Browser.Capabilities["WurflCapabilities"] as SortedList<string, string>;
                if (wurflCapabilities != null && wurflCapabilities.TryGetValue(property, out value))
                    return value;

                // Try the properties of the request.
                controlType = context.Request.GetType();
                propertyInfo = controlType.GetProperty(property);
                if (propertyInfo != null && propertyInfo.CanRead)
                    return propertyInfo.GetValue(context.Request, null).ToString();

                // If not then try and return the value for the collection.
                return context.Request.Browser[property];
            }

            #endregion

        }

        private class HomePage
        {
            #region Fields
            
            internal readonly string _url;
            internal readonly List<Filter> _filters = new List<Filter>();
            internal readonly Regex _replaceRegex;

            #endregion

            #region Constructors

            internal HomePage(string url, string expression)
            {
                _url = url;
                if (String.IsNullOrEmpty(expression) == false)
                    _replaceRegex = new Regex(expression, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }

            #endregion

            #region Properties

            internal List<Filter> Filters
            {
                get { return _filters; }   
            }

            internal string Url
            {
                get
                {
                    if (_replaceRegex != null)
                        return _replaceRegex.Replace(HttpContext.Current.Request.Url.ToString(), _url);
                    return _url;
                }
            }

            internal bool GetIsMatch(HttpContext context)
            {
                foreach(Filter filter in _filters)
                {
                    if (filter.GetIsMatch(context) == false)
                        return false;
                }
                return true;
            }

            #endregion
        }

        #endregion

        #region Constants

        /// <summary>
        /// Full type names of classes representing mobile
        /// page handlers.
        /// </summary>
        private static readonly string[] MOBILE_PAGES = {
                                                            "System.Web.UI.MobileControls.MobilePage"
                                                        };

        /// <summary>
        /// Full type names of classes representing standard
        /// page handlers.
        /// </summary>
        private static readonly string[] PAGES = new[]
                                                     {
                                                         "System.Web.UI.Page",
                                                         "System.Web.UI.MobileControls.MobilePage"
                                                     };

        #endregion

        #region Fields

        /// <summary>
        /// Collection of client targets used by the application with the key
        /// field representing the alias and the value the useragent.
        /// </summary>
        private SortedList<string, string> _clientTargets;

        /// <summary>
        /// If set to true only the first eligable request received by the web
        /// site will be redirected to the mobile landing page contained in
        /// _mobileRedirectUrl.
        /// </summary>
        private bool _firstRequestOnly = true;

        /// <summary>
        /// The login url for forms authentication.
        /// </summary>
        private string _formsLoginUrl;

        /// <summary>
        /// A collection of homepages that could be used for redirection. 
        /// Evaluated before the _mobileHomePageUrl value is used.
        /// Initialised from the <c>web.config</c> file when the module 
        /// is created.
        /// </summary>
        private List<HomePage> _homePages = new List<HomePage>();

        /// <summary>
        /// The URL to use to redirect a mobile device accessing
        /// a non mobile web page to. Initialised from the <c>web.config</c> file
        /// when the module is created.
        /// </summary>
        private string _mobileHomePageUrl;

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
        private Regex _mobilePageRegex;

        /// <summary>
        /// If set to true the original URL of the request is added to the redirected
        /// querystring in a paramter called origUrl.
        /// </summary>
        private bool _originalUrlAsQueryString;

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
            if (Manager.Redirect != null && Manager.Redirect.Enabled)
            {
                _mobileHomePageUrl = Manager.Redirect.MobileHomePageUrl;
                _firstRequestOnly = Manager.Redirect.FirstRequestOnly;
                if (String.IsNullOrEmpty(Manager.Redirect.MobilePagesRegex) == false)
                    _mobilePageRegex = new Regex(Manager.Redirect.MobilePagesRegex,
                                                 RegexOptions.Compiled | RegexOptions.IgnoreCase);
                _formsLoginUrl = FormsAuthentication.LoginUrl;
                _originalUrlAsQueryString = Manager.Redirect.OriginalUrlAsQueryString;

                foreach (LocationElement homePage in Manager.Redirect.Locations)
                {
                    if (homePage.Enabled)
                    {
                        HomePage current = new HomePage(homePage.Url, homePage.ReplaceExpression);
                        foreach (FilterElement filter in homePage)
                        {
                            if (filter.Enabled)
                                current.Filters.Add(new Filter(filter.Property, filter.MatchExpression));
                        }
                        _homePages.Add(current);
                    }
                }
            }

            // Get a list of the client target names.
            _clientTargets = GetClientTargets();

            // Intercept the beginning of the request to override the capabilities.
            application.BeginRequest += OnBeginRequest;

            // Intercept request event after the hander and the state have been assigned
            // to redirect the page.
            application.PostAcquireRequestState += OnPostAcquireRequestState;

            // Check for a MobilePage handler being used.
            application.PreRequestHandlerExecute += SetPreferredRenderingType;

            // If client targets are specified then check to see if one is being used
            // and override the requesting device information.
            if (_clientTargets != null)
                application.PreRequestHandlerExecute += SetPagePreIntClientTargets;
        }

        #endregion

        #region Events

        /// <summary>
        /// If the handler is a MobilePage then ensure a compaitable textwriter will be used.
        /// </summary>
        /// <param name="sender">HttpApplication related to the request.</param>
        /// <param name="e">EventArgs related to the event. Not used.</param>
        private void SetPreferredRenderingType(object sender, EventArgs e)
        {
            if (sender is HttpApplication)
            {
                HttpContext context = ((HttpApplication)sender).Context;

                // Check to see if the handler is a mobile page. If so then the preferred markup
                // needs to change as the legacy mobile controls do not work with html4 specified.
                // If these lines are removed a "No mobile controls device configuration was registered 
                // for the requesting device" exception is likely to occur.
                if (context.Handler is MobilePage &&
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
                            builder.Append(Environment.NewLine).Append(key).Append("=").Append(
                                context.Request.Headers[key]);

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
                    if (ShouldRequestRedirect(context))
                    {
                        string newUrl = GetHomePageUrl(context);
                        if (_originalUrlAsQueryString)
                            // Use an encoded version of the requesting Url
                            // as the query string.
                            newUrl = String.Format("{0}?origUrl={1}",
                                                   newUrl,
                                                   HttpUtility.UrlEncode(context.Request.Url.ToString()));

                        EventLog.Debug(String.Format("Redirecting '{0}' to '{1}'.", context.Request.Url, newUrl));
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
                    var clientNames = new SortedList<string, string>();
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
        private static HttpBrowserCapabilities AddNewCapabilities(HttpBrowserCapabilities currentCapabilities, IDictionary overrideCapabilities)
        {
            // We can't do anything with null capabilities. Return the current ones.
            if (overrideCapabilities == null)
                return currentCapabilities;

            var capabilities = new System.Web.Mobile.MobileCapabilities();
            capabilities.Capabilities = new Hashtable();

            // Copy the keys from both the original and new capabilities.
            foreach (object key in currentCapabilities.Capabilities.Keys)
                capabilities.Capabilities[key] = currentCapabilities.Capabilities[key];
            foreach (object key in overrideCapabilities.Keys)
                capabilities.Capabilities[key] = overrideCapabilities[key];

            // Copy the adapters from the original.
            foreach (object key in currentCapabilities.Adapters.Keys)
                capabilities.Adapters.Add(key, currentCapabilities.Adapters[key]);

            return capabilities;
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
            context.Request.Browser = AddNewCapabilities(context.Request.Browser, Factory.Create(context));
        }

        /// <summary>
        /// Adds new capabilities for the useragent provided rather than the request details
        /// provided in the request paramters.
        /// </summary>
        /// <param name="request">The request who's capabilities collection should be updated.</param>
        /// <param name="userAgent">The useragent string of the device requiring capabilities to be added.</param>
        private static void OverrideCapabilities(HttpRequest request, string userAgent)
        {
            request.Browser = AddNewCapabilities(request.Browser, Factory.Create(userAgent));
        }

        #endregion

        #region Methods

        /// <summary>
        /// Records the module being disposed if debug enabled.
        /// </summary>
        public void Dispose()
        {
            EventLog.Debug("Disposing Detector Module");
        }

        /// <summary>
        /// Evaluates the home page that should be used when redirecting 
        /// the requesting context. If the locations collection does
        /// not provide a home page then the mobile home page url will
        /// be used only if the device is a mobile.
        /// </summary>
        /// <param name="context">Context of the request.</param>
        /// <returns>The url to redirect the request to, if any.</returns>
        private string GetHomePageUrl(HttpContext context)
        {
            foreach(HomePage homePage in _homePages)
            {
                if (homePage.GetIsMatch(context))
                    return homePage.Url;
            }
            if (context.Request.Browser.IsMobileDevice)
                return _mobileHomePageUrl;
            return null;
        }

        /// <summary>
        /// Returns true if the request should be redirected.
        /// </summary>
        /// <param name="context">The HttpContext of the request.</param>
        /// <returns>True if the request should be redirected.</returns>
        private bool ShouldRequestRedirect(HttpContext context)
        {
            return context.Handler != null &&
                   String.IsNullOrEmpty(GetHomePageUrl(context)) == false &&
                   IsPage(context) &&
                   IsMobilePage(context) == false &&
                   IsFirstTime(context, _firstRequestOnly) &&
                   IsRestrictedPageForRedirect(context) == false;
        }

        /// <summary>
        /// Checks the page being requested to determine if it is eligable
        /// for redirection. The mobile home page and the forms authentication
        /// login page are restricted from redirection. If the response
        /// is already being redirected it should be ignored.
        /// </summary>
        /// <param name="context">The context of the request.</param>
        /// <returns>True if the page is restricted from being redirected.</returns>
        private bool IsRestrictedPageForRedirect(HttpContext context)
        {
            Page page = context.Handler as Page;

            string currentPage = page.ResolveUrl(context.Request.AppRelativeCurrentExecutionFilePath);
            
            bool value =
                context.Response.IsRequestBeingRedirected ||
                String.IsNullOrEmpty(context.Response.RedirectLocation) == false ||
                page.ResolveUrl(GetHomePageUrl(context)) == currentPage ||
                page.ResolveUrl(_formsLoginUrl) == currentPage;
            
            EventLog.Debug(String.Format("Request '{0}' with handler type '{1}' is {2} restricted page for redirect.", 
                context.Request.Url,
                context.Handler.GetType().FullName,
                value ? "a" : "not a" ));

            return value;
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
            if (RequestHistory.IsPresent(context.Request))
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
            if (new List<string>(context.Response.Cookies.AllKeys).Contains(Constants.AlreadyAccessedCookieName) ==
                false)
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
#if VER4
            return array.Any(current => value == current);
#elif VER2
            foreach (string current in array)
            {
                if (value == current)
                    return true;
            }
            return false;
#endif
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
                if (IsInArray(type.FullName, PAGES))
                    return true;
                if (type.BaseType != null)
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
                if (IsInArray(type.FullName, MOBILE_PAGES))
                    return true;
                if (type.BaseType != null)
                    return IsMobileType(type.BaseType);
            }
            return false;
        }

        /// <summary>
        /// Checks to see if the regular expression provided matches either the
        /// relative executing path or the Url of the request.
        /// </summary>
        /// <param name="request">The HttpRequest to be checked.</param>
        /// <returns>True if this request should be considered associated with
        /// a page designed for mobile.</returns>
        private bool IsMobileRegexPage(HttpRequest request)
        {
            if (_mobilePageRegex != null)
                return _mobilePageRegex.IsMatch(request.AppRelativeCurrentExecutionFilePath) ||
                       _mobilePageRegex.IsMatch(request.Url.ToString());
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

        #endregion
    }
}