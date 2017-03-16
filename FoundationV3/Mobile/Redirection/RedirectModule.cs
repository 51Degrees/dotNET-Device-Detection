/* *********************************************************************
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited. 
 * Copyright © 2017 51Degrees Mobile Experts Limited, 5 Charlotte Close,
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
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using FiftyOne.Foundation.Mobile.Configuration;
using FiftyOne.Foundation.Mobile.Detection;
using System.Linq;

namespace FiftyOne.Foundation.Mobile.Redirection
{
    /// <summary>
    /// Class responsible for handling redirection based on predefined rules.
    /// </summary>
    public class RedirectModule : IHttpModule
    {
        #region Fields

        /// <summary>
        /// Used to lock the initialisation of static fields.
        /// </summary>
        private static readonly object _lock = new object();

        /// <summary>
        /// Indicates if static field initialisation has been completed.
        /// </summary>
        private static bool _staticFieldsInitialised;
        
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
        internal static int _redirectTimeout = 0;

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

        /// <summary>
        /// The request history provider to use with the redirect module.
        /// </summary>
        private static IRequestHistory _requestHistory;

        #endregion

        #region Initialisers

        /// <summary>
        /// Initiliases the HttpMobile registering this modules interest in
        /// all new requests and handler mappings.
        /// </summary>
        /// <param name="application">
        /// <see cref="HttpApplication"/> object for the web application.</param>
        public virtual void Init(HttpApplication application)
        {
            EventLog.Debug("Initialising redirection module");
            
            StaticFieldInit(application);

            // Initialise the request history processor if required.
            if (_firstRequestOnly == true)
            {
#if AZURE
                _requestHistory = new Azure.RequestHistory();
#else
                _requestHistory = new RequestHistory();
#endif
            }
            
            RegisterEventHandlersInit(application);
        }

        /// <summary>
        /// Registers the event handlers if they've not done so already.
        /// </summary>
        /// <param name="application">
        /// <see cref="HttpApplication"/> object for the web application.</param>
        private void RegisterEventHandlersInit(HttpApplication application)
        {
            // Record the original requesting URL.
            application.BeginRequest += OnBeginRequest;

            // Intercept request event after the handler and the state have been assigned
            // to redirect the page.
            application.PostAcquireRequestState += OnPostAcquireRequestState;
        }

        /// <summary>
        /// Initialises the static fields.
        /// </summary>
        private static void StaticFieldInit(HttpApplication application)
        {
            if (_staticFieldsInitialised == false)
            {
                lock (_lock)
                {
                    if (_staticFieldsInitialised == false)
                    {
                        EventLog.Debug("Initialising redirection module static fields.");

                        // Fetch the redirect url, first time redirect indicator and wire up the
                        // events if a url has been provided.
                        if (Manager.Redirect != null && Manager.Redirect.Enabled)
                        {
                            _redirectEnabled = true;
                            _mobileHomePageUrl = Manager.Redirect.MobileHomePageUrl;
                            _firstRequestOnly = Manager.Redirect.FirstRequestOnly;
                            if (_firstRequestOnly == true)
                                _redirectTimeout = Manager.Redirect.Timeout;
                            if (String.IsNullOrEmpty(Manager.Redirect.MobilePagesRegex) == false)
                                _mobilePageRegex = new Regex(Manager.Redirect.MobilePagesRegex,
                                                             RegexOptions.Compiled | RegexOptions.IgnoreCase);
                            _formsLoginUrl = FormsAuthentication.IsEnabled ? FormsAuthentication.LoginUrl : String.Empty;
                            _originalUrlAsQueryString = Manager.Redirect.OriginalUrlAsQueryString;

                            foreach (LocationElement homePage in Manager.Redirect.Locations)
                            {
                                if (homePage.Enabled)
                                {
                                    Location current = new Location(homePage.Name, homePage.Url, homePage.MatchExpression);
                                    foreach (FilterElement filter in homePage)
                                    {
                                        if (filter.Enabled)
                                            current.Filters.Add(new Filter(filter.Property, filter.MatchExpression));
                                    }
                                    _locations.Add(current);
                                }
                            }
                        }

                        // Indicate initialisation is complete.
                        _staticFieldsInitialised = true;
                    }
                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Record the original requesting URL.
        /// </summary>
        /// <param name="sender">HttpApplication related to the request.</param>
        /// <param name="e">EventArgs related to the event. Not used.</param>
        private static void OnBeginRequest(object sender, EventArgs e)
        {
            if (sender is HttpApplication)
            {
                HttpContext context = ((HttpApplication)sender).Context;
                if (context != null)
                {
                    context.Items[Constants.OriginalUrlKey] = context.Request.Url.ToString();
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
        private void OnPostAcquireRequestState(object sender, EventArgs e)
        {
            if (sender is HttpApplication)
            {
                // Initialise the static fields if required.
                StaticFieldInit((HttpApplication)sender);

                HttpContext context = ((HttpApplication)sender).Context;

                if (context != null)
                {

                    RecordNewDevice(context);

                    // Check to see if the request should be redirected.
                    if (_redirectEnabled && ShouldRequestRedirect(context))
                    {
                        string newUrl = GetLocationUrl(context);
                        if (_originalUrlAsQueryString)
                            // Use an encoded version of the requesting Url
                            // as the query string.
                            if (newUrl.Contains("?"))
                            {
                                // Url already has a query string, so add requesting Url to it with a '&'.
                                newUrl = String.Format("{0}&origUrl={1}",
                                    newUrl,
                                    HttpUtility.UrlEncode(context.Request.Url.ToString()));
                            }
                            else
                            {
                                // Url has no query string so create one now.
                                newUrl = String.Format("{0}?origUrl={1}",
                                    newUrl,
                                    HttpUtility.UrlEncode(context.Request.Url.ToString()));
                            }

                        if (EventLog.IsDebug)
                            EventLog.Debug(String.Format("Redirecting device with useragent '{0}' from url '{1}' to '{2}' due to '{3}'.",
                                              context.Request.UserAgent,
                                              context.Items[Constants.OriginalUrlKey],
                                              newUrl,
                                              GetLocationName(context)));

                        context.Response.Redirect(newUrl, true);
                    }
                }
            }
        }
        
        #endregion

        #region Public Methods

        /// <summary>
        /// Returns true if the current handler relates to a mobile web page.
        /// </summary>
        /// <param name="context">The context associated with the Http request.</param>
        /// <returns>
        /// True if the current handler relates to a mobile web page.</returns>
        public static bool IsMobilePage(HttpContext context)
        {
            return IsMobileType(context.Handler.GetType()) ||
                   IsMobileRegexPage(context);
        }

        /// <summary>
        /// Determines if this is the first request received from the device assuming first
        /// request only is set in the configuration.
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

            // Check the various methods of recording previous requests 
            // and return the revised value.
            return IsFirstTime(context, Constants.AllowAlreadyAccessedCookie);
        }

        /// <summary>
        /// Determines if this is the first request received from the device.
        /// </summary>
        /// <param name="context">Context of the request.</param>
        /// <param name="cookies">True if the first time routine is allowed to use response cookies.</param>
        /// <returns>True if this request is the first from the device. Otherwise false.</returns>
        public static bool IsFirstTime(HttpContext context, bool cookies)
        {
            // Try the context collection for the key.
            object isFirstTime = context.Items[Constants.IsFirstTimeKey];

            if (isFirstTime == null)
            {
                // If not set, get it and store.
                isFirstTime = GetIsFirstTime(context, cookies);
                context.Items[Constants.IsFirstTimeKey] = isFirstTime;
            }

            // Return the value found.
            return (bool)isFirstTime;
        }
        
        /// <summary>
        /// Sends any queued data and records the module being 
        /// disposed if debug enabled.
        /// </summary>
        public virtual void Dispose()
        {
            EventLog.Debug("Disposing Redirection Module");
            NewDevice.Send();
        }

        #endregion

        #region Internal and Private Methods

        /// <summary>
        /// Record the new device if we're as certain as we can be that it is
        /// not one we've processed already.
        /// </summary>
        /// <param name="context">Context of the request.</param>
        private void RecordNewDevice(HttpContext context)
        {
            try
            {
                 if (NewDevice.Enabled &&
                    context.Request.HttpMethod == "GET" &&
                    context.Handler != null &&
                    IsPageType(context.Handler.GetType()) &&
                    IsFirstTime(context, false))
                    NewDevice.RecordNewDevice(context.Request);
            }
            catch (Exception ex)
            {
                EventLog.Debug(ex);
            }
        }

        /// <summary>
        /// Returns the Url stored in the context when the request first began before 
        /// other modules may have changed it. If not available returns the Url
        /// using the current request value.
        /// </summary>
        /// <param name="context">The Context of the request.</param>
        /// <returns>The original Url of the request.</returns>
        internal static string GetOriginalUrl(HttpContext context)
        {
            string originalUrl = context.Items[Constants.OriginalUrlKey] as string;
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
            string locationUrl = context.Items[Constants.LocationUrlKey] as string;
            if (locationUrl == null)
            {
                // Use the mobileHomePageUrl setting as the default if this is a
                // mobile device.
                if (context.Request.Browser.IsMobileDevice)
                    locationUrl = _mobileHomePageUrl;

                // Try the locations collection first.
                Location current = GetLocation(context);
                if (current != null)
                    locationUrl = current.GetUrl(context);

                // Store so that the value does not need to be calculated next time.
                context.Items[Constants.LocationUrlKey] = locationUrl;
            }
            return locationUrl;
        }

        /// <summary>
        /// Gets the name of the currently active location if one is being used. Only
        /// used for debugging.
        /// </summary>
        /// <param name="context">Context of the request.</param>
        /// <returns>The name of the location.</returns>
        private static string GetLocationName(HttpContext context)
        {
            Location current = GetLocation(context);
            if (current != null)
                return current._name;
            return "default";
        }

        /// <summary>
        /// Returns the location object to be used for the request if any.
        /// </summary>
        /// <param name="context">Context of the request.</param>
        /// <returns>Null if no location available, or the location object.</returns>
        private static Location GetLocation(HttpContext context)
        {
            foreach (Location location in _locations)
            {
                if (location.GetIsMatch(context))
                    return location;
            }
            return null;
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
            bool value = context.Response.IsRequestBeingRedirected ||
                String.IsNullOrEmpty(context.Response.RedirectLocation) == false;
            
            string currentPage = null;

            if (page != null)
            {
                // Process a standard page derived from System.Web.UI.Page.
                currentPage = page.ResolveUrl(context.Request.AppRelativeCurrentExecutionFilePath);
                value = value ||
                    page.ResolveUrl(GetLocationUrl(context)) == currentPage ||
                    page.ResolveUrl(_formsLoginUrl) == currentPage ||
                    page.ResolveUrl(GetLocationUrl(context)) == originalUrl ||
                    page.ResolveUrl(_formsLoginUrl) == originalUrl;
            }

            EventLog.Debug(String.Format("Request for '{0}' with handler type '{1}' and current page '{2}' is {3} restricted page for redirect.",
                originalUrl,
                context.Handler.GetType().FullName,
                currentPage,
                value ? "a" : "not a"));

            return value;
        }

        /// <summary>
        /// Determines if this is the first request received from the device. Should
        /// only be called once pre request.
        /// </summary>
        /// <param name="context">Context of the request.</param>
        /// <param name="cookies">True is the method is allowed to use cookies.</param>
        /// <returns>True if this request is the first from the device. Otherwise false.</returns>
        private static bool GetIsFirstTime(HttpContext context, bool cookies)
        {
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
                RecordFirstTime(context, cookies);
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
                if (cookies && context.Request.Cookies[Constants.AlreadyAccessedCookieName] != null)
                    WipeResponseCookie(context, context.Request.Cookies[Constants.AlreadyAccessedCookieName]);

                return false;
            }

            // Check to see if our cookie is present from a previous request, that
            // we're allowed to use cookies and that the expiry time is not passed. 
            // If it is present ensure it's expiry time is updated in the response.
            HttpCookie alreadyAccessed = context.Request.Cookies[Constants.AlreadyAccessedCookieName];
            if (alreadyAccessed != null &&
                cookies &&
                long.Parse(alreadyAccessed.Value) >= DateTime.UtcNow.Ticks)
            {
                SetResponseCookie(context, alreadyAccessed);

                // Remove from the devices cache file as cookie can be used.
                if (_requestHistory != null)
                    _requestHistory.Remove(context.Request);

                return false;
            }

            // Check to see if the requested IP address and HTTP headers hashcode is
            // on record as having been seen before.
            if (_requestHistory != null &&
                _requestHistory.IsPresent(context.Request))
            {
                // Update the cache and other methods in case they can
                // be used in the future.
                RecordFirstTime(context, cookies);
                return false;
            }

            // The url referrer, session and cookie checks have all failed.
            // Record the device information using the session if available, a cookie and the
            // request history cache file.
            RecordFirstTime(context, cookies);

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
        /// <param name="cookies">True if response cookies can be used.</param>
        /// <param name="context">The context of the request.</param>
        private static void RecordFirstTime(HttpContext context, bool cookies)
        {
            // Add a parameter to the session if available indicating the time that 
            // the device date should be remvoed from the session.
            if (context.Session != null)
                SetSession(context);

            // Add a cookie to the response setting the expiry time to the 
            // redirection timeout.
            // Modified to check for existance of cookie to avoid recreating.
            if (cookies)
                SetResponseCookie(context, new HttpCookie(Constants.AlreadyAccessedCookieName));

            // Add to the request history cache.
            if (_requestHistory != null)
                _requestHistory.Set(context.Request);
        }

        /// <summary>
        /// Checks the value is contained in the array.
        /// </summary>
        /// <param name="value">Value string.</param>
        /// <param name="array">Array of strings to be checked against.</param>
        /// <returns>True if the string is present. False if not.</returns>
        private static bool IsInArray(string value, string[] array)
        {
            return array.Any(current => value == current);
        }

        /// <summary>
        /// Checks the array of page types to determine if the type past into the method
        /// is in the array. Also checks basetypes where available.
        /// </summary>
        /// <param name="type">The type to be checked.</param>
        /// <returns>True is the request relates to a page.</returns>
        internal static bool IsPageType(Type type)
        {
            if (type != null)
            {
                if (IsInArray(type.FullName, Constants.Pages))
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
        internal static bool IsMobileType(Type type)
        {
            if (type != null)
            {
                if (IsInArray(type.FullName, Constants.MobilePages))
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
        /// <param name="context">The HttpContext to be checked.</param>
        /// <returns>True if this request should be considered associated with
        /// a page designed for mobile.</returns>
        private static bool IsMobileRegexPage(HttpContext context)
        {
            if (_mobilePageRegex != null)
            {
                string originalUrl = GetOriginalUrl(context);
                return _mobilePageRegex.IsMatch(context.Request.AppRelativeCurrentExecutionFilePath) ||
                       _mobilePageRegex.IsMatch(originalUrl);
            }
            return false;
        }

        #endregion
    }
}
