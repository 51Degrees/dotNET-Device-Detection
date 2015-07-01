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
using System.Collections.Generic;
using System.Web;
using System.Linq;
using FiftyOne.Foundation.Mobile.Detection.Entities;
using System.Text.RegularExpressions;
using System.Web.Configuration;

namespace FiftyOne.Foundation.Mobile.Detection
{
    /// <summary>
    /// The this[] property has been changed to also check the 51Degrees.mobi capabilities
    /// before returning a value if the capability has not been found in the 
    /// standard collection.
    /// </summary>
    /// <remarks>
    /// Note: The OBSOLETE_SUPPORT pre-compilation directive should be set if support is needed
    /// for SharePoint or other applications that expect the Request.Browser property to return
    /// an object of type <cref see="System.Web.Mobile.MobileCapabilities"/>. A reference will 
    /// need to be added to the project to System.Web.Mobile. This module has been marked 
    /// obsolete by Microsoft but is still used by SharePoint 2010. Because it is marked
    /// obsolete we decided not to require developers have to reference System.Web.Mobile.
    /// </remarks>
#if OBSOLETE_SUPPORT
    public class FiftyOneBrowserCapabilities : System.Web.Mobile.MobileCapabilities
#else
    public class FiftyOneBrowserCapabilities : HttpBrowserCapabilities
#endif
    {
        #region Fields

        private readonly Match _match;
        private readonly HttpRequest _request;
        private readonly HttpBrowserCapabilities _defaultBrowserCapabilities;

        #endregion

        #region Properties


        #endregion

        #region Constructor

        /// <summary>
        /// Constructs an instance of <cref see="FiftyOneBrowserCapabilities"/>
        /// </summary>
        public FiftyOneBrowserCapabilities(HttpBrowserCapabilities defaultBrowserCapabilities, HttpRequest request, Match match)
            : base()
        {
            _defaultBrowserCapabilities = defaultBrowserCapabilities;
            _request = request;
            _match = match;
        }

        #endregion

        #region Unaltered Properties

        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override bool CanCombineFormsInDeck { get { return _defaultBrowserCapabilities.CanCombineFormsInDeck; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>        public override bool CanRenderAfterInputOrSelectElement { get { return _defaultBrowserCapabilities.CanRenderAfterInputOrSelectElement; } }
        public override bool CanRenderEmptySelects { get { return _defaultBrowserCapabilities.CanRenderEmptySelects; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>public override bool CanRenderInputAndSelectElementsTogether { get { return _defaultBrowserCapabilities.CanRenderInputAndSelectElementsTogether; } }
        public override bool CanRenderMixedSelects { get { return _defaultBrowserCapabilities.CanRenderMixedSelects; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>public override bool CanRenderOneventAndPrevElementsTogether { get { return _defaultBrowserCapabilities.CanRenderOneventAndPrevElementsTogether; } }
        public override bool CanRenderPostBackCards { get { return _defaultBrowserCapabilities.CanRenderPostBackCards; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>        public override bool CanRenderSetvarZeroWithMultiSelectionList { get { return _defaultBrowserCapabilities.CanRenderSetvarZeroWithMultiSelectionList; } }
        public override bool CanSendMail { get { return _defaultBrowserCapabilities.CanSendMail; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override int GatewayMajorVersion { get { return _defaultBrowserCapabilities.GatewayMajorVersion; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override double GatewayMinorVersion { get { return _defaultBrowserCapabilities.GatewayMinorVersion; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override string GatewayVersion { get { return _defaultBrowserCapabilities.GatewayVersion; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override bool HasBackButton { get { return _defaultBrowserCapabilities.HasBackButton; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override bool HidesRightAlignedMultiselectScrollbars { get { return _defaultBrowserCapabilities.HidesRightAlignedMultiselectScrollbars; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override string InputType { get { return _defaultBrowserCapabilities.InputType; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override int MaximumHrefLength { get { return _defaultBrowserCapabilities.MaximumHrefLength; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override int MaximumRenderedPageSize { get { return _defaultBrowserCapabilities.MaximumRenderedPageSize; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override int MaximumSoftkeyLabelLength { get { return _defaultBrowserCapabilities.MaximumSoftkeyLabelLength; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override int NumberOfSoftkeys { get { return _defaultBrowserCapabilities.NumberOfSoftkeys; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override string PreferredRenderingMime { get { return _defaultBrowserCapabilities.PreferredRenderingMime; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override string PreferredRenderingType { get { return _defaultBrowserCapabilities.PreferredRenderingType; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override string PreferredRequestEncoding { get { return _defaultBrowserCapabilities.PreferredRequestEncoding; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override string PreferredResponseEncoding { get { return _defaultBrowserCapabilities.PreferredResponseEncoding; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override bool RendersBreakBeforeWmlSelectAndInput { get { return _defaultBrowserCapabilities.RendersBreakBeforeWmlSelectAndInput; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override bool RendersBreaksAfterHtmlLists { get { return _defaultBrowserCapabilities.RendersBreaksAfterHtmlLists; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override bool RendersBreaksAfterWmlAnchor { get { return _defaultBrowserCapabilities.RendersBreaksAfterWmlAnchor; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override bool RendersBreaksAfterWmlInput { get { return _defaultBrowserCapabilities.RendersBreaksAfterWmlInput; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override bool RendersWmlDoAcceptsInline { get { return _defaultBrowserCapabilities.RendersWmlDoAcceptsInline; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override bool RendersWmlSelectsAsMenuCards { get { return _defaultBrowserCapabilities.RendersWmlSelectsAsMenuCards; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override string RequiredMetaTagNameValue { get { return _defaultBrowserCapabilities.RequiredMetaTagNameValue; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override bool RequiresAttributeColonSubstitution { get { return _defaultBrowserCapabilities.RequiresAttributeColonSubstitution; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override bool RequiresContentTypeMetaTag { get { return _defaultBrowserCapabilities.RequiresContentTypeMetaTag; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override bool RequiresDBCSCharacter { get { return _defaultBrowserCapabilities.RequiresDBCSCharacter; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override bool RequiresHtmlAdaptiveErrorReporting { get { return _defaultBrowserCapabilities.RequiresHtmlAdaptiveErrorReporting; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override bool RequiresLeadingPageBreak { get { return _defaultBrowserCapabilities.RequiresLeadingPageBreak; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override bool RequiresNoBreakInFormatting { get { return _defaultBrowserCapabilities.RequiresNoBreakInFormatting; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override bool RequiresOutputOptimization { get { return _defaultBrowserCapabilities.RequiresOutputOptimization; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override bool RequiresPhoneNumbersAsPlainText { get { return _defaultBrowserCapabilities.RequiresPhoneNumbersAsPlainText; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override bool RequiresSpecialViewStateEncoding { get { return _defaultBrowserCapabilities.RequiresSpecialViewStateEncoding; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override bool RequiresUniqueFilePathSuffix { get { return _defaultBrowserCapabilities.RequiresUniqueFilePathSuffix; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override bool RequiresUniqueHtmlCheckboxNames { get { return _defaultBrowserCapabilities.RequiresUniqueHtmlCheckboxNames; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override bool RequiresUniqueHtmlInputNames { get { return _defaultBrowserCapabilities.RequiresUniqueHtmlInputNames; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override bool RequiresUrlEncodedPostfieldValues { get { return _defaultBrowserCapabilities.RequiresUrlEncodedPostfieldValues; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override int ScreenCharactersHeight { get { return _defaultBrowserCapabilities.ScreenCharactersHeight; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override int ScreenCharactersWidth { get { return _defaultBrowserCapabilities.ScreenCharactersWidth; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override bool SupportsAccesskeyAttribute { get { return _defaultBrowserCapabilities.SupportsAccesskeyAttribute; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override bool SupportsBodyColor { get { return _defaultBrowserCapabilities.SupportsBodyColor; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override bool SupportsBold { get { return _defaultBrowserCapabilities.SupportsBold; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override bool SupportsCacheControlMetaTag { get { return _defaultBrowserCapabilities.SupportsCacheControlMetaTag; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override bool SupportsCss { get { return _defaultBrowserCapabilities.SupportsCss; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override bool SupportsDivAlign { get { return _defaultBrowserCapabilities.SupportsDivAlign; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override bool SupportsDivNoWrap { get { return _defaultBrowserCapabilities.SupportsDivNoWrap; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override bool SupportsEmptyStringInCookieValue { get { return _defaultBrowserCapabilities.SupportsEmptyStringInCookieValue; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override bool SupportsFontColor { get { return _defaultBrowserCapabilities.SupportsFontColor; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override bool SupportsFontName { get { return _defaultBrowserCapabilities.SupportsFontName; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override bool SupportsFontSize { get { return _defaultBrowserCapabilities.SupportsFontSize; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override bool SupportsImageSubmit { get { return _defaultBrowserCapabilities.SupportsImageSubmit; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override bool SupportsIModeSymbols { get { return _defaultBrowserCapabilities.SupportsIModeSymbols; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override bool SupportsInputIStyle { get { return _defaultBrowserCapabilities.SupportsInputIStyle; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override bool SupportsInputMode { get { return _defaultBrowserCapabilities.SupportsInputMode; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override bool SupportsItalic { get { return _defaultBrowserCapabilities.SupportsItalic; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override bool SupportsJPhoneMultiMediaAttributes { get { return _defaultBrowserCapabilities.SupportsJPhoneMultiMediaAttributes; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override bool SupportsJPhoneSymbols { get { return _defaultBrowserCapabilities.SupportsJPhoneSymbols; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override bool SupportsQueryStringInFormAction { get { return _defaultBrowserCapabilities.SupportsQueryStringInFormAction; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override bool SupportsRedirectWithCookie { get { return _defaultBrowserCapabilities.SupportsRedirectWithCookie; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override bool SupportsSelectMultiple { get { return _defaultBrowserCapabilities.SupportsSelectMultiple; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override bool SupportsUncheck { get { return _defaultBrowserCapabilities.SupportsUncheck; } }
        /// <summary>
        /// The unaltered implementation from <see cref="HttpCapabilitiesDefaultProvider"/>
        /// </summary>
        public override bool SupportsXmlHttp { get { return _defaultBrowserCapabilities.SupportsXmlHttp; } }

        #endregion

        #region Private Static Methods

        /// <summary>
        /// Compares two lists to see if they contain at least one value that is the same.
        /// </summary>
        /// <param name="values">Values for a profile</param>
        /// <param name="value">The value being sought</param>
        /// <returns></returns>
        private static bool Contains(Values values, string value)
        {
            return values == null || value == null ? false : values.Any(i => value.Equals(i.Name));
        }

        /// <summary>
        /// The version of the browser.
        /// </summary>
        private Version BrowserVersion
        {
            get
            {
                if (_browserVersion == null)
                {
                    lock (this)
                    {
                        if (_browserVersion == null)
                        {
                            var version = _match["BrowserVersion"];

                            // Use the version from 51Degrees if it's present and contains
                            // numeric segments.
                            int majorVersion = 0, minorVersion = 0, buildVersion = 0, revisionVersion = 0;
                            if (version != null)
                            {
                                MatchCollection segments = Regex.Matches(version[0].Name, @"\d+");
                                if (segments.Count > 0)
                                {
                                    if (segments.Count > 0) int.TryParse(segments[0].Value, out majorVersion);
                                    if (segments.Count > 1) int.TryParse(segments[1].Value, out minorVersion);
                                    if (segments.Count > 2) int.TryParse(segments[2].Value, out buildVersion);
                                    if (segments.Count > 3) int.TryParse(segments[3].Value, out revisionVersion);
                                }
                            }

                            // If no browser version is available then try the default 
                            // properties, or set to an empty instance.
                            if (majorVersion == 0 &&
                                minorVersion == 0 &&
                                buildVersion == 0 &&
                                revisionVersion == 0)
                            {
                                majorVersion = _defaultBrowserCapabilities.MajorVersion;
                                MatchCollection segments = Regex.Matches(_defaultBrowserCapabilities.MinorVersionString, @"\d+");
                                if (segments.Count > 0)
                                {
                                    if (segments.Count > 0) int.TryParse(segments[0].Value, out minorVersion);
                                    if (segments.Count > 1) int.TryParse(segments[1].Value, out buildVersion);
                                }
                            }

                            _browserVersion = new Version(majorVersion, minorVersion, buildVersion, revisionVersion);
                        }
                    }
                }
                return _browserVersion;
            }
        }
        private Version _browserVersion;
        private static readonly Regex BrowserVersionRegex = new Regex(@"\d+", RegexOptions.Compiled);

        /// <summary>
        /// Returns true if the device supports tables.
        /// </summary>
        private bool TablesCapable
        {
            get
            {
                if (_tables == null)
                {
                    lock (this)
                    {
                        if (_tables == null)
                        {
                            var value = _match["TablesCapable"];
                            _tables = value != null ? value.ToBool() : _defaultBrowserCapabilities.Tables;
                        }
                    }
                }
                return _tables.Value;
            }
        }
        private bool? _tables;

        private string PreferredHtmlVersion
        {
            get
            {
                // Working out ASP.NET will support HTML5. Return 4 for the moment.
                return "html4";

                /*

                // Get the list of values.
                var values = new List<double>();
                var versions = device.GetPropertyValueStringIndexes(HtmlVersion);
                if (versions != null)
                {
                    foreach (var index in versions)
                    {
                        double value;
                        if (double.TryParse(_provider.Strings.Get(index), out value))
                            values.Add(value);
                    }
                }
                values.Sort();
                values.Reverse();

                // Find the highest version of HTML supported.
                foreach(double value in values)
                {
                    switch (value.ToString())
                    {
                        default:
                        case "4":
                            return "html4";
                        case "3.2":
                            return "html32";
                        case "5":
                            return "html5";
                    }
                }
 
                // Couldn't find anything return html 4.
                return "html4";
                */
            }
        }

        /// <summary>
        /// Returns if the request is from a browser that supports cookies.
        /// </summary>
        private bool CookieSupport
        {
            get
            {
                if (_cookieSupport == null)
                {
                    lock (this)
                    {
                        if (_cookieSupport == null)
                        {
                            var value = _match["CookiesCapable"];
                            _cookieSupport = value != null ? value.ToBool() : _defaultBrowserCapabilities.Cookies;
                        }
                    }
                }
                return _cookieSupport.Value;
            }
        }
        private bool? _cookieSupport;
                
        /// <summary>
        /// Returns version 2.0 if DOM is supported based on Ajax
        /// being supported, otherwise returns false.
        /// </summary>
        private Version W3CDOMVersion
        {
            get
            {
                if (_w3CDOMVersion == null)
                {
                    lock (this)
                    {
                        if (_w3CDOMVersion == null)
                        {
                            var value = _match["AjaxRequestType"];
                            if (value != null && value.Count > 0 &&
                                "AjaxRequestTypeNotSupported".Equals(value[0].Name))
                            {
                                _w3CDOMVersion = new Version(2, 0);
                            }
                            else
                            {
                                _w3CDOMVersion = _defaultBrowserCapabilities.W3CDomVersion;
                            }
                        }
                    }
                }
                return _w3CDOMVersion;
            }
        }
        private Version _w3CDOMVersion;

        /// <summary>
        /// If the device indicates javascript support then return true.
        /// </summary>
        private bool JavascriptSupport
        {
            get
            {
                if (_javascriptSupport == null)
                {
                    lock (this)
                    {
                        if (_javascriptSupport == null)
                        {
                            var value = _match["Javascript"];
#pragma warning disable 618
                            _javascriptSupport = value != null ? value.ToBool() : _defaultBrowserCapabilities.JavaScript;
#pragma warning restore 618
                        }
                    }
                }
                return _javascriptSupport.Value;
            }
        }
        private bool? _javascriptSupport;

        /// <summary>
        /// Get the javascript version or null if not provided or invalid.
        /// </summary>
        private Version JavascriptVersion
        {
            get
            {
                if (_javascriptVersion == null)
                {
                    lock (this)
                    {
                        if (_javascriptVersion == null)
                        {
                            Version version;
                            var value = _match["JavascriptVersion"];
                            if (value == null || value.Count == 0 ||
                                System.Version.TryParse(value[0].Name, out version) == false)
                            {
                                version = _defaultBrowserCapabilities.JScriptVersion;
                            }
                            _javascriptVersion = version;
                        }
                    }
                }
                return _javascriptVersion;
            }
        }
        private Version _javascriptVersion;

        /// <summary>
        /// Returns the name of the platform from 51Degrees if present in the data set.
        /// </summary>
        private new string Platform
        {
            get
            {
                if (_platform == null)
                {
                    lock (this)
                    {
                        if (_platform == null)
                        {
                            var value = _match["PlatformName"];
                            _platform = value != null && value.Count > 0 ? value[0].Name : _defaultBrowserCapabilities.Platform;
                        }
                    }
                }
                return _platform;
            }
        }
        private string _platform;

        /// <summary>
        /// Returns the name of the browser from 51Degrees if present in the data set.
        /// </summary>
        private new string Browser
        {
            get
            {
                if (_browser == null)
                {
                    lock (this)
                    {
                        if (_browser == null)
                        {
                            var value = _match["BrowserName"];
                            _browser = value != null && value.Count > 0 ? value[0].Name : _defaultBrowserCapabilities.Browser;
                        }
                    }
                }
                return _browser;
            }
        }
        private string _browser;

        /// <summary>
        /// Returns if the request is from a crawler using data from 51Degrees if present.
        /// </summary>
        private bool IsCrawler
        {
            get
            {
                if (_isCrawler == null)
                {
                    lock (this)
                    {
                        if (_isCrawler == null)
                        {
                            var value = _match["IsCrawler"];
                            _isCrawler = value != null ? value.ToBool() : _defaultBrowserCapabilities.Crawler;
                        }
                    }
                }
                return _isCrawler.Value;
            }
        }
        private bool? _isCrawler;

        #endregion

        #region Overriden and Modified Members

        /// <summary>
        /// Gets a value indicating whether the browser is a recognized mobile device.
        /// </summary>
        public override bool IsMobileDevice
        {
            get
            {
                if (_isMobileDevice == null)
                {
                    lock (this)
                    {
                        if (_isMobileDevice == null)
                        {
                            var value = _match["IsMobile"];
                            _isMobileDevice = value != null ? value.ToBool() : _defaultBrowserCapabilities.IsMobileDevice; 
                        }
                    }
                }
                return _isMobileDevice.Value;
            }
        }
        private bool? _isMobileDevice;

        /// <summary>
        /// Returns the height of the display, in pixels.
        /// </summary>
        public override int ScreenPixelsHeight 
        { 
            get 
            {
                if (_screenPixelsHeight == null)
                {
                    lock (this)
                    {
                        if (_screenPixelsHeight == null)
                        {
                            var value = _match["ScreenPixelsHeight"];
                            _screenPixelsHeight = value != null ? value.ToInt() : _defaultBrowserCapabilities.ScreenPixelsHeight;
                        }
                    }
                }
                return _screenPixelsHeight.Value;
            } 
        }
        private int? _screenPixelsHeight;

        /// <summary>
        /// Returns the width of the display, in pixels.
        /// </summary>
        public override int ScreenPixelsWidth
        {
            get
            {
                if (_screenPixelsWidth == null)
                {
                    lock (this)
                    {
                        if (_screenPixelsWidth == null)
                        {
                            var value = _match["ScreenPixelsWidth"];
                            _screenPixelsWidth = value != null ? value.ToInt() : _defaultBrowserCapabilities.ScreenPixelsWidth;
                        }
                    }
                }
                return _screenPixelsWidth.Value;
            }
        }
        private int? _screenPixelsWidth;

        /// <summary>
        /// Gets a value indicating whether the browser device is capable of initiating
        /// a voice call.
        /// </summary>
        public override bool CanInitiateVoiceCall 
        { 
            get 
            {
                if (_canInitiateVoiceCall == null)
                {
                    lock(this)
                    {
                        if (_canInitiateVoiceCall == null)
                        {
                            var value = _match["SupportsPhoneCalls"];
                            _canInitiateVoiceCall = value != null ? value.ToBool() : _defaultBrowserCapabilities.CanInitiateVoiceCall;
                        }
                    }
                }
                return _canInitiateVoiceCall.Value;
            } 
        }
        private bool? _canInitiateVoiceCall;

        /// <summary>
        /// Gets a value indicating whether the browser has a color display.
        /// </summary>
        public override bool IsColor
        {
            get
            {
                return ScreenBitDepth >= 4;
            }
        }

        /// <summary>
        /// Gets the model name of a mobile device, if known.
        /// </summary>
        public override string MobileDeviceModel
        {
            get
            {
                if (_mobileDeviceModel == null)
                {
                    lock (this)
                    {
                        if (_mobileDeviceModel == null)
                        {
                            var value = _match["HardwareModel"];
                            if (value != null && value.Count > 0)
                            {
                                _mobileDeviceModel = value.ToString();
                            }
                            else
                            {
                                value = _match["HardwareName"];
                                if (value != null && value.Count > 0)
                                {
                                    _mobileDeviceModel = value.ToString();
                                }
                                else
                                {
                                    _mobileDeviceModel = _defaultBrowserCapabilities.MobileDeviceModel;
                                }
                            }
                        }
                    }
                }
                return _mobileDeviceModel;
            }
        }
        private string _mobileDeviceModel;

        /// <summary>
        /// Returns the name of the manufacturer of a mobile device, if known.
        /// </summary>
        public override string MobileDeviceManufacturer
        {
            get
            {
                if (_mobileDeviceManufacturer == null)
                {
                    lock (this)
                    {
                        if (_mobileDeviceManufacturer == null)
                        {
                            var value = _match["HardwareVendor"];
                            _mobileDeviceManufacturer = value != null && value.Count > 0 ? 
                                value[0].Name :
                                _defaultBrowserCapabilities.MobileDeviceManufacturer;
                        }
                    }
                }
                return _mobileDeviceManufacturer;
            }
        }
        private string _mobileDeviceManufacturer;

        /// <summary>
        /// Returns the MIME type of the type of image content typically preferred by
        /// the browser.
        /// </summary>
        public override string PreferredImageMime
        { 
            get 
            {
                if (_preferredImageMime == null)
                {
                    lock (this)
                    {
                        if (_preferredImageMime == null)
                        {
                            var values = _match["CcppAccept"];
                            if (values != null && values.Count > 0)
                            {
                                // Look at the database and return the 1st one that matches in order
                                // of preference.
                                if (Contains(values, "ImagePng"))
                                {
                                    _preferredImageMime = "image/png";
                                }
                                else if (Contains(values, "ImageJpeg"))
                                {
                                    _preferredImageMime = "image/jpeg";
                                }
                                else if (Contains(values, "ImageGif"))
                                {
                                    _preferredImageMime = "image/gif";
                                }
                                else
                                {
                                    _preferredImageMime = _defaultBrowserCapabilities.PreferredImageMime;
                                }
                            }
                        }
                    }
                }
                return _preferredImageMime;
            } 
        }
        private string _preferredImageMime;
        
        /// <summary>
        /// Gets a value indicating whether the browser supports callback scripts.
        /// </summary>
        public override bool SupportsCallback
        {
            get
            {
                if (_supportsCallback == null)
                {
                    lock (this)
                    {
                        if (_supportsCallback == null)
                        {
                            var value = _match["AjaxRequestType"];
                            if (value != null && value.Count > 0 && "AjaxRequestTypeNotSupported".Equals(value[0].Name))
                            {
                                _supportsCallback = false;
                            }
                            else
                            {
                                _supportsCallback = _defaultBrowserCapabilities.SupportsCallback;
                            }
                        }
                    }
                }
                return _supportsCallback.Value;
            }
        }
        private bool? _supportsCallback;

        /// <summary>
        /// Returns the depth of the display, in bits per pixel.
        /// </summary>
        public override int ScreenBitDepth 
        { 
            get 
            {
                if (_screenBitDepth == null)
                {
                    lock (this)
                    {
                        if (_screenBitDepth == null)
                        {
                            var value = _match["BitsPerPixel"];
                            _screenBitDepth = value != null ? value.ToInt() : _defaultBrowserCapabilities.ScreenBitDepth;
                        }
                    }
                }
                return _screenBitDepth.Value;
            } 
        }
        private int? _screenBitDepth;
        
        /// <summary>
        /// Returns the value of the property by first checking the results
        /// assigned to the browser capabilities. If no result is found then
        /// base implementation is called.
        /// </summary>
        /// <param name="key">The capability key being sought.</param>
        /// <returns>The value of the key, otherwise null.</returns>
        public override string this[string key]
        {
	        get 
	        {
                switch(key)
                { 
                    case "isMobileDevice":
                        return IsMobileDevice.ToString().ToLowerInvariant();
                    case "crawler":
                        return IsCrawler.ToString().ToLowerInvariant();
                    case "tables":
                        return TablesCapable.ToString().ToLowerInvariant();
                    case "mobileDeviceModel":
                        return MobileDeviceModel;
                    case "mobileDeviceManufacturer":
                        return MobileDeviceManufacturer;
                    case "platform":
                        return Platform;
                // property enhancement can be removed with this compiler flag
#if !REMOVE_OVERRIDE_BROWSER
                    case "browser":
                        return Browser;
#endif
                    case "screenPixelsHeight":
                        return ScreenPixelsHeight.ToString();
                    case "screenPixelsWidth":
                        return ScreenPixelsWidth.ToString();
                    case "screenBitDepth":
                        return ScreenBitDepth.ToString();
                    case "preferredImageMime":
                        return PreferredImageMime;
                    case "isColor":
                        return IsColor.ToString().ToLowerInvariant();
                    case "SupportsCallback":
                    case "supportsCallback":
                        return SupportsCallback.ToString().ToLowerInvariant();
                    case "canInitiateVoiceCall":
                        return CanInitiateVoiceCall.ToString().ToLowerInvariant();
                    case "jscriptversion":
                        return JavascriptVersion.ToString();
                    case "majorversion":
                        return BrowserVersion.Major.ToString();
                    case "minorversion":
                        return BrowserVersion.Minor.ToString();
                    case "version":
                        return String.Format("{0}.{1}", BrowserVersion.Major, BrowserVersion.Minor);
                    case "ecmascriptversion":
                        if (JavascriptSupport)
                        {
                            return "3.0";
                        }
                        return _defaultBrowserCapabilities["ecmascriptversion"];
                    case "w3cdomversion":
                        return W3CDOMVersion.ToString();
                    case "cookies":
                        return CookieSupport.ToString().ToLowerInvariant();;
                    case "preferredRenderingType":
                        return PreferredHtmlVersion;
                    case "preferredRenderingMime":
                        return "text/html";
                    case "matchMethod":
                    case "MatchMethod":
                        return _match.Method.ToString();
                    case "matchDifference":
                    case "MatchDifference":
                        return _match.Method != MatchMethods.None ? _match.Difference.ToString() : "N/A";
                    case "matchSignatureRank":
                    case "MatchSignatureRank":
                        return _match.Signature != null ? _match.Signature.Rank.ToString() : "N/A";
                    case "detectionTime":
                    case Constants.DetectionTimeProperty:
                        return _match.Elapsed.TotalMilliseconds.ToString();
                    default:
                        var value = _match[key];
                        return value != null ?
                            String.Join(Constants.ValueSeperator, value) :
                            _defaultBrowserCapabilities[key];
                }
	        }
        }

        #endregion
    }
}
