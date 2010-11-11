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

#region Usings

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
            /// <param name="property">The property name to be returned.</param>
            /// <param name="context">The context associated with the request.</param>
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

        private class Location
        {
            #region Fields
            
            internal readonly string _url;
            internal readonly List<Filter> _filters = new List<Filter>();
            internal readonly Regex _matchRegex;

            #endregion

            #region Constructors

            internal Location(string url, string expression)
            {
                _url = url;
                if (String.IsNullOrEmpty(expression) == false)
                    _matchRegex = new Regex(expression, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }

            #endregion

            #region Properties

            internal List<Filter> Filters
            {
                get { return _filters; }   
            }

            #endregion

            #region Methods

            internal string GetUrl(HttpContext context)
            {
                if (_matchRegex != null)
                {
                    // A match regular expression has been found that should be used to
                    // extract all the items of interest from the original URL and place
                    // them and the positions contains in {} brackets in the URL property
                    // of the location.
                    MatchCollection matches = _matchRegex.Matches(GetOriginalUrl(context));
                    if (matches.Count > 0)
                    {
                        string[] values = new string[matches.Count];
                        for (int i = 0; i < matches.Count; i++)
                            values[i] = matches[i].Value;
                        return String.Format(_url, values);
                    }
                }
                // Return the URL unformatted.
                return _url;
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

        /// <summary>
        /// The key in the Items collection of the requesting context used to
        /// store the Url originally requested by the browser.
        /// </summary>
        private const string ORIGINAL_URL_KEY = "51D_Original_Url";

        /// <summary>
        /// The key in the Items collection of the requesting context used to
        /// store the home page Url for a possible redirection.
        /// </summary>
        private const string LOCATION_URL_KEY = "51D_Location_Url";

        #endregion

        #region Fields

        /// <summary>
        /// Used to lock the initialisation of static fields.
        /// </summary>
        private static readonly object _lock = new object();

        /// <summary>
        /// Indicates if static initialisation has been completed.
        /// </summary>
        private static bool _initialised;

        /// <summary>
        /// Collection of client targets used by the application with the key
        /// field representing the alias and the value the useragent.
        /// </summary>
        private static SortedList<string, string> _clientTargets;

        /// <summary>
        /// If set to true only the first eligable request received by the web
        /// site will be redirected to the mobile landing page contained in
        /// _mobileRedirectUrl.
        /// </summary>
        private static bool _firstRequestOnly = true;

        /// <summary>
        /// The number of minutes that should elapse before the record of 
        /// previous access for the device should be removed from all
        /// possible storage mechanisims.
        /// </summary>
        private static int _redirectTimeout = 0;

        /// <summary>
        /// The login url for forms authentication.
        /// </summary>
        private static string _formsLoginUrl;

        /// <summary>
        /// A collection of homepages that could be used for redirection. 
        /// Evaluated before the _mobileHomePageUrl value is used.
        /// Initialised from the <c>web.config</c> file when the module 
        /// is created.
        /// </summary>
        private static readonly List<Location> _locations = new List<Location>();

        /// <summary>
        /// The URL to use to redirect a mobile device accessing
        /// a non mobile web page to. Initialised from the <c>web.config</c> file
        /// when the module is created.
        /// </summary>
        private static string _mobileHomePageUrl;

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
        private static Regex _mobilePageRegex;

        /// <summary>
        /// If set to true the original URL of the request is added to the redirected
        /// querystring in a paramter called origUrl.
        /// </summary>
        private static bool _originalUrlAsQueryString;

        /// <summary>
        /// If set to true redirection is enabled.
        /// </summary>
        private static bool _redirectEnabled;

        #endregion

        #region Initialisers

        /// <summary>
        /// Initiliases the HttpMobile registering this modules interest in
        /// all new requests and handler mappings.
        /// </summary>
        /// <param name="application">HttpApplication object for the web application.</param>
        public void Init(HttpApplication application)
        {
            // Initialise the static fields if required.
            StaticFieldInit();

            // Intercept the beginning of the request to override the capabilities.
            application.BeginRequest += OnBeginRequest;

            // Intercept request event after the hander and the state have been assigned
            // to redirect the page if redirect is enabled.
            if (_redirectEnabled)
                application.PostAcquireRequestState += OnPostAcquireRequestState;

            // Check for a MobilePage handler being used.
            application.PreRequestHandlerExecute += SetPreferredRenderingType;

            // If client targets are specified then check to see if one is being used
            // and override the requesting device information.
            if (_clientTargets != null)
                application.PreRequestHandlerExecute += SetPagePreIntClientTargets;
        }

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
                        EventLog.Debug("Initialising Detector Module");

                        // Fetch the redirect url, first time redirect indicator and wire up the
                        // events if a url has been provided.
                        if (Manager.Redirect != null && Manager.Redirect.Enabled)
                        {
                            _redirectEnabled = true;
                            _mobileHomePageUrl = Manager.Redirect.MobileHomePageUrl;
                            _firstRequestOnly = Manager.Redirect.FirstRequestOnly;
                            _redirectTimeout = Manager.Redirect.Timeout;
                            if (String.IsNullOrEmpty(Manager.Redirect.MobilePagesRegex) == false)
                                _mobilePageRegex = new Regex(Manager.Redirect.MobilePagesRegex,
                                                             RegexOptions.Compiled | RegexOptions.IgnoreCase);
                            _formsLoginUrl = FormsAuthentication.LoginUrl;
                            _originalUrlAsQueryString = Manager.Redirect.OriginalUrlAsQueryString;

                            foreach (LocationElement homePage in Manager.Redirect.Locations)
                            {
                                if (homePage.Enabled)
                                {
                                    Location current = new Location(homePage.Url, homePage.MatchExpression);
                                    foreach (FilterElement filter in homePage)
                                    {
                                        if (filter.Enabled)
                                            current.Filters.Add(new Filter(filter.Property, filter.MatchExpression));
                                    }
                                    _locations.Add(current);
                                }
                            }
                        }

                        // Get a list of the client target names.
                        _clientTargets = GetClientTargets();

                        // Indicate initialisation is complete.
                        _initialised = true;
                    }
                }
            }
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
                if (context.Handler != null &&
                    IsMobileType(context.Handler.GetType()) &&
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

                    // If redirect is enabled store the source URL incase a redirect
                    // is performed by another module.
                    if (_redirectEnabled)
                        context.Items[ORIGINAL_URL_KEY] = context.Request.Url.ToString();
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
                        string newUrl = GetLocationUrl(context);
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

            var capabilities = new System.Web.HttpBrowserCapabilities();
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
        /// Returns the Url stored in the context when the request first began before 
        /// other modules may have changed it. If not available returns the Url
        /// using the current request value.
        /// </summary>
        /// <param name="context">The Context of the request.</param>
        /// <returns>The original Url of the request.</returns>
        private static string GetOriginalUrl(HttpContext context)
        {
            string originalUrl = context.Items[ORIGINAL_URL_KEY] as string;
            if (String.IsNullOrEmpty(originalUrl) == false)
                return originalUrl;
            return context.Request.Url.ToString();
        }

        /// <summary>
        /// Evaluates the location that should be used when redirecting 
        /// the requesting context. If the locations collection does
        /// not provide a location then the mobile home page url will
        /// be used only if the device is a mobile.
        /// </summary>
        /// <param name="context">Context of the request.</param>
        /// <returns>The url to redirect the request to, if any.</returns>
        private static string GetLocationUrl(HttpContext context)
        {
            string locationUrl = context.Items[LOCATION_URL_KEY] as string;
            if (locationUrl == null)
            {
                // Use the mobileHomePageUrl setting as the default if this is a
                // mobile device.
                if (context.Request.Browser.IsMobileDevice)
                    locationUrl = _mobileHomePageUrl;

                // Try the locations collection first.
                foreach (Location location in _locations)
                {
                    if (location.GetIsMatch(context))
                    {
                        locationUrl = location.GetUrl(context);
                        break;
                    }
                }
                
                // Store so that the value does not need to be calculated next time.
                context.Items[LOCATION_URL_KEY] = locationUrl;
            }
            return locationUrl;
        }

        /// <summary>
        /// Returns true if the request should be redirected.
        /// </summary>
        /// <param name="context">The HttpContext of the request.</param>
        /// <returns>True if the request should be redirected.</returns>
        private static bool ShouldRequestRedirect(HttpContext context)
        {
            return context.Handler != null &&
                   String.IsNullOrEmpty(GetLocationUrl(context)) == false &&
                   IsPage(context) &&
                   IsMobilePage(context) == false &&
                   IsFirstTime(context) &&
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
        private static bool IsRestrictedPageForRedirect(HttpContext context)
        {
            Page page = context.Handler as Page;

            string originalUrl = GetOriginalUrl(context);
            string currentPage = page.ResolveUrl(context.Request.AppRelativeCurrentExecutionFilePath);
            
            bool value =
                context.Response.IsRequestBeingRedirected ||
                String.IsNullOrEmpty(context.Response.RedirectLocation) == false ||
                page.ResolveUrl(GetLocationUrl(context)) == currentPage ||
                page.ResolveUrl(_formsLoginUrl) == currentPage ||
                page.ResolveUrl(GetLocationUrl(context)) == originalUrl ||
                page.ResolveUrl(_formsLoginUrl) == originalUrl; ;
            
            EventLog.Debug(String.Format("Request '{0}' with handler type '{1}' is {2} restricted page for redirect.", 
                context.Request.Url,
                context.Handler.GetType().FullName,
                value ? "a" : "not a" ));

            return value;
        }

        /// <summary>
        /// Determines if this is the first request received from the device.
        /// </summary>
        /// <param name="context">Context of the request.</param>
        /// <returns>True if this request is the first from the device. Otherwise false.</returns>
        public static bool IsFirstTime(HttpContext context)
        {
            // If the parameter indicating only the first request should be redirect
            // is false then return true as the implication is all requests should
            // be redirected.
            if (_firstRequestOnly == false)
                return true;

            // Check to see if the Referrer URL contains the same host name
            // as the current page request. If there is a match this request has
            // come from another web page on the same host and is not the 1st request.
            // The logic is only applied if an infinite timeout value is provided
            // because using this method there is no way of knowing when the referrer
            // url details were populated.
            if (_redirectTimeout == 0 &&
                context.Request.UrlReferrer != null &&
                context.Request.UrlReferrer.Host == context.Request.Url.Host)
            {
                // In some situations the same web application may be split across
                // different host names. Record the first time details using other
                // methods to ensure this method returns the correct value when
                // used with subsequent requests from the same device.
                RecordFirstTime(context);
                return false;
            }

            // If the session is available and it's timeout is greater then or equal to
            // the timeout to be used for redirection check to see if we have a 
            // session parameter indicating an expiry time for the current device. 
            // If the expiry time has not elpased then reset it and return a value 
            // indicating this is not the first time the device has been seen.
            if (_redirectTimeout != 0 &&
                context.Session != null &&
                context.Session.Timeout >= _redirectTimeout &&
                context.Session[Constants.ExpiryTime] != null &&
                (long)context.Session[Constants.ExpiryTime] >= DateTime.UtcNow.Ticks)
            {
                // Update the session key to indicate a new expiry time.
                SetSession(context);

                // Remove our own cookie from the response as it's not 
                // needed because the session is working.
                if (context.Request.Cookies[Constants.AlreadyAccessedCookieName] != null)
                    WipeResponseCookie(context, context.Request.Cookies[Constants.AlreadyAccessedCookieName]);

                // Remove from the devices cache file as session can be used.
                RequestHistory.Remove(context.Request);

                return false;
            }

            // Check to see if our cookie is present from a previous request and that 
            // the expiry time is not passed. If it is present ensure it's expiry time
            // is updated in the response.
            HttpCookie alreadyAccessed = context.Request.Cookies[Constants.AlreadyAccessedCookieName];
            if (alreadyAccessed != null &&
                long.Parse(alreadyAccessed.Value) >= DateTime.UtcNow.Ticks)
            {
                SetResponseCookie(context, alreadyAccessed);
                
                // Remove from the devices cache file as cookie can be used.
                RequestHistory.Remove(context.Request);

                return false;
            }

            // Check to see if the requested IP address and HTTP headers hashcode is
            // on record as having been seen before.
            if (RequestHistory.IsPresent(context.Request))
            {
                // Update the cache and other methods in case they can
                // be used in the future.
                RecordFirstTime(context);
                return false;
            }

            // The url referrer, session and cookie checks have all failed.
            // Record the device information using the session if available, a cookie and the
            // request history cache file.
            RecordFirstTime(context);

            return true;
        }

        /// <summary>
        /// Returns a long value representing the expiry date time to be used
        /// for the current request.
        /// </summary>
        /// <returns></returns>
        private static long GetExpiryDateTime()
        {
            if (_redirectTimeout == 0)
                return DateTime.MaxValue.Ticks;
            return DateTime.UtcNow.AddMinutes(_redirectTimeout).Ticks;
        }

        /// <summary>
        /// Sets the session key to the expiry time for the current device.
        /// </summary>
        /// <param name="context"></param>
        private static void SetSession(HttpContext context)
        {
            context.Session[Constants.ExpiryTime] = GetExpiryDateTime();
        }

        /// <summary>
        /// Removes the cookie from the browser by setting it's expiry time to a date
        /// in the past.
        /// </summary>
        /// <param name="context">The context of the request.</param>
        /// <param name="alreadyAccessed">The already accessed cookie.</param>
        private static void WipeResponseCookie(HttpContext context, HttpCookie alreadyAccessed)
        {
            alreadyAccessed.Expires = DateTime.MinValue;
            context.Response.Cookies.Add(alreadyAccessed);
        }

        /// <summary>
        /// Sets the response cookie expiry time.
        /// </summary>
        /// <param name="context">The context of the request.</param>
        /// <param name="alreadyAccessed">The already accessed cookie.</param>
        private static void SetResponseCookie(HttpContext context, HttpCookie alreadyAccessed)
        {
            alreadyAccessed.Expires = DateTime.MaxValue;
            alreadyAccessed.Value = GetExpiryDateTime().ToString();
            context.Response.Cookies.Add(alreadyAccessed);
        }

        /// <summary>
        /// Records in the session (if present) and in a cookie
        /// the requesting devices first request. This ensures subsequent calls
        /// to IsFirstTime return the correct value.
        /// </summary>
        /// <param name="context">The context of the request.</param>
        private static void RecordFirstTime(HttpContext context)
        {
            // Add a parameter to the session if available indicating the time that 
            // the device date should be remvoed from the session.
            if (context.Session != null)
                SetSession(context);

            // Add a cookie to the response setting the expiry time to the 
            // redirection timeout.
            // Modified to check for existance of cookie to avoid recreating.
            SetResponseCookie(context, new HttpCookie(Constants.AlreadyAccessedCookieName));

            // Add to the request history cache.
            RequestHistory.Add(context.Request);
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
        private static bool IsMobileRegexPage(HttpRequest request)
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
        public static bool IsMobilePage(HttpContext context)
        {
            return IsMobileType(context.Handler.GetType()) ||
                   IsMobileRegexPage(context.Request);
        }

        #endregion
    }
}