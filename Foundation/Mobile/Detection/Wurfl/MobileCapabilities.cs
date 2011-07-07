/* *********************************************************************
 * The contents of this file are subject to the Mozilla internal License 
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
 * Mobile Experts Limited are Copyright (C) 2009 - 2011. All Rights Reserved.
 * 
 * Contributor(s):
 *     James Rosewell <james@51degrees.mobi>
 * 
 * ********************************************************************* */

#region Usings

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
using FiftyOne.Foundation.Image;

#endregion

namespace FiftyOne.Foundation.Mobile.Detection.Wurfl
{
    /// <summary>
    /// Mobile capabilities populated using the WURFL data source.
    /// </summary>
    internal class MobileCapabilities : Detection.MobileCapabilities
    {
        #region String Index Values

        // Gets the indexes of all the key capability strings as static readonly
        // values during static construction to avoid needing to look them up every time.
        private readonly int AjaxXhrType;
        private readonly int XhtmlSupportLevel;
        private readonly int CookieSupport;
        private readonly int AjaxSupportJavascript;
        private readonly int MobileBrowserVersion;
        private readonly int DeviceOs;
        private readonly int Adapters;
        private readonly int ResolutionHeight;
        private readonly int ResolutionWidth;
        private readonly int Colors;
        private readonly int BuiltInBackButtonSupport;
        private readonly int AccessKeySupport;
        private readonly int MarketingName;
        private readonly int ModelName;
        private readonly int BrandName;
        private readonly int Jpg;
        private readonly int Gif;
        private readonly int Png;
        private readonly int XhtmlmpPreferredMimeType;
        private readonly int HtmlWiOmaXhtmlmp10;
        private readonly int HtmlWiW3Xhtmlbasic;
        private readonly int HtmlWiImodeCompactGeneric;
        private readonly int HtmlWeb40;
        private readonly int HtmlWeb32;
        private readonly int PreferredMarkup;
        private readonly int MaxDeckSize;
        private readonly int BreakListOfLinksWithBrElementRecommended;
        private readonly int InsertBrElementAfterWidgetRecommended;
        private readonly int Rows;
        private readonly int Columns;
        private readonly int Utf8Support;
        private readonly int XhtmlTableSupport;
        private readonly int XhtmlNoWrapMode;
        private readonly int SoftkeySupport;
        
        #endregion

        #region Fields

        /// <summary>
        /// Instance of the provider to use with this class.
        /// </summary>
        private Provider _provider = null;

        #endregion

        #region Properties

        /// <summary>
        /// Returns the device detection provider assigned to the 
        /// mobile capabilities.
        /// </summary>
        internal Provider Provider
        {
            get { return _provider; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a new instance of <see cref="MobileCapabilities"/>.
        /// </summary>
        /// <param name="provider">Data provider to use with this capabilities provider.</param>
        internal MobileCapabilities(Provider provider)
        {
            _provider = provider;
            AjaxXhrType = _provider.Strings.Add("ajax_xhr_type");
            XhtmlSupportLevel = _provider.Strings.Add("xhtml_support_level");
            CookieSupport = _provider.Strings.Add("cookie_support");
            AjaxSupportJavascript = _provider.Strings.Add("ajax_support_javascript");
            MobileBrowserVersion = _provider.Strings.Add("mobile_browser_version");
            DeviceOs = _provider.Strings.Add("device_os");
            Adapters = _provider.Strings.Add("adapters");
            ResolutionHeight = _provider.Strings.Add("resolution_height");
            ResolutionWidth = _provider.Strings.Add("resolution_width");
            Colors = _provider.Strings.Add("colors");
            BuiltInBackButtonSupport = _provider.Strings.Add("built_in_back_button_support");
            AccessKeySupport = _provider.Strings.Add("access_key_support");
            MarketingName = _provider.Strings.Add("marketing_name");
            ModelName = _provider.Strings.Add("model_name");
            BrandName = _provider.Strings.Add("brand_name");
            Jpg = _provider.Strings.Add("jpg");
            Gif = _provider.Strings.Add("gif");
            Png = _provider.Strings.Add("png");
            XhtmlmpPreferredMimeType = _provider.Strings.Add("xhtmlmp_preferred_mime_type");
            HtmlWiOmaXhtmlmp10 = _provider.Strings.Add("html_wi_oma_xhtmlmp_1_0");
            HtmlWiW3Xhtmlbasic = _provider.Strings.Add("html_wi_w3_xhtmlbasic");
            HtmlWiImodeCompactGeneric = _provider.Strings.Add("html_wi_imode_compact_generic");
            HtmlWeb40 = _provider.Strings.Add("html_web_4_0");
            HtmlWeb32 = _provider.Strings.Add("html_web_3_2");
            PreferredMarkup = _provider.Strings.Add("preferred_markup");
            MaxDeckSize = _provider.Strings.Add("max_deck_size");
            BreakListOfLinksWithBrElementRecommended = _provider.Strings.Add("break_list_of_links_with_br_element_recommended");
            InsertBrElementAfterWidgetRecommended = _provider.Strings.Add("insert_br_element_after_widget_recommended");
            Rows = _provider.Strings.Add("rows");
            Columns = _provider.Strings.Add("columns");
            Utf8Support = _provider.Strings.Add("utf8_support");
            XhtmlTableSupport = _provider.Strings.Add("xhtml_table_support");
            XhtmlNoWrapMode = _provider.Strings.Add("xhtml_nowrap_mode");
            SoftkeySupport = _provider.Strings.Add("softkey_support");
        }

        #endregion

        #region Override Methods

        /// <summary>
        /// Creates a dictionary of capabilites for the requesting device.
        /// </summary>
        /// <param name="request">Details of the request.</param>
        /// <param name="currentCapabilities">The current capabilities assigned by .NET.</param>
        /// <returns>A dictionary of capabilities for the request.</returns>
        internal override IDictionary Create(HttpRequest request, IDictionary currentCapabilities)
        {
            // Use the base class to create the initial list of capabilities.
            IDictionary capabilities = base.Create(request, currentCapabilities);

            // Use the device for the request to set the capabilities.
            Create((DeviceInfo)_provider.GetDeviceInfo(request), capabilities, currentCapabilities);

            // Initialise any capability values that rely on the settings
            // from the device data source.
            Init(capabilities);

            return capabilities;
        }

        /// <summary>
        /// Creates a dictionary of capabilites for the useragent string.
        /// </summary>
        /// <param name="userAgent">The useragent string associated with the device.</param>
        /// <returns>A dictionary of capabilities for the request.</returns>
        internal override IDictionary Create(string userAgent)
        {
            // Use the base class to create the initial list of capabilities.
            IDictionary capabilities = base.Create(userAgent);

            // Use the device for the request to set the capabilities.
            Create((DeviceInfo)_provider.GetDeviceInfo(userAgent), capabilities, null);

            // Initialise any capability values that rely on the settings
            // from the device data source.
            Init(capabilities);

            return capabilities;
        }

        #endregion

        #region Private Methods

        private void Create(DeviceInfo device, IDictionary capabilities, IDictionary currentCapabilities)
        {
            // Enhance with the capabilities from the device data.
            if (device != null)
            {
                // Enhance the default capabilities collection based on the device.
                Enhance(capabilities, currentCapabilities, device);

                // If an adapters patch file has been loaded then include this
                // capability in the exposed list of capabilities.
                string adapters = GetAdapters(device);
                if (String.IsNullOrEmpty(adapters) == false)
                    SetValue(capabilities, "adapters", adapters);

                // Add all the Wurfl capabilities available for the device.
                capabilities.Add(Constants.WurflCapabilities, CreateWurflCapabilities(device));
            }
        }

        /// <summary>
        /// Sets static capabilities used by mobile controls.
        /// </summary>
        /// <param name="capabilities">Dictionary of capabilities to be changed.</param>
        private static void SetStaticValues(IDictionary capabilities)
        {
            SetValue(capabilities, "requiresSpecialViewStateEncoding", "true");
            SetValue(capabilities, "requiresUniqueFilePathSuffix", "true");
            SetValue(capabilities, "requiresUniqueHtmlCheckboxNames", "true");
            SetValue(capabilities, "requiresUniqueHtmlInputNames", "true");
            SetValue(capabilities, "requiresUrlEncodedPostfieldValues", "true");
            SetValue(capabilities, "requiresOutputOptimization", "true");
            SetValue(capabilities, "requiresControlStateInSession", "true");
        }

        /// <summary>
        /// Creates a SortedList of capability names and values for the device.
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        private SortedList<string, string> CreateWurflCapabilities(DeviceInfo device)
        {
            var capabilities = new SortedList<string, string>();

            // Get the list of capability names for the device.
            List<int> names = new List<int>();
            AddWurflCapabilityNames(names, device);

            // Add the device id even thought this is not a capability. It
            // may be used by some applications.
            capabilities.Add("deviceid", device.DeviceId);

            // Add the actualDeviceRoot value as it is needed by some developers.
            capabilities.Add("actual_device_root", device.IsActualDeviceRoot.ToString());

            // Add all the capabilities for the device.
            foreach (int name in names)
                capabilities[_provider.Strings.Get(name)] = _provider.Strings.Get(device.GetCapability(name));

            return capabilities;
        }

        /// <summary>
        /// Adds capabilitiy names associated with the device to the list
        /// of capability strings. If a fallback device is specified this
        /// devices capability names are also added until there is no further
        /// fallback device.
        /// </summary>
        /// <param name="names">The list of capability name indexes.</param>
        /// <param name="device">The device who's capabilities should be added.</param>
        private static void AddWurflCapabilityNames(List<int> names, DeviceInfo device)
        {
            names.AddRange(device.Capabilities.Keys);
            if (device.FallbackDevice != null)
                AddWurflCapabilityNames(names, device.FallbackDevice);
        }

        /// <summary>
        /// Updates the capabilities used by Microsoft's implementation of the
        /// HttpBrowserCapabilities class to control the property values it
        /// returns. Only properties exposed by FiftyOneBrowserCapabilities are overriden
        /// by this method.
        /// </summary>
        /// <param name="capabilities">Dictionary of capabilities to be enhanced.</param>
        /// <param name="currentCapabilities">Dictionary of existing capabilities for the device.</param>
        /// <param name="device">Device to use for enhancements.</param>
        private void Enhance(IDictionary capabilities, IDictionary currentCapabilities, DeviceInfo device)
        {
            // Set base capabilities for all mobile devices.
            SetStaticValues(capabilities);

            SetValue(capabilities, "actualDeviceRoot", device.IsActualDeviceRoot.ToString());
            SetValue(capabilities, "isMobileDevice", GetIsMobileDevice(device));
            SetValue(capabilities, "crawler", GetIsCrawler(device));
            SetValue(capabilities, "mobileDeviceModel", GetMobileDeviceModel(device));
            SetValue(capabilities, "mobileDeviceManufacturer", GetMobileDeviceManufacturer(device));
            SetValue(capabilities, "platform", GetPlatform(device));
            SetValue(capabilities, "type", capabilities["mobileDeviceManufacturer"]);
            SetValue(capabilities, "supportsAccessKeyAttribute", GetSupportsAccesskeyAttribute(device));
            SetValue(capabilities, "hasBackButton", GetHasBackButton(device));
            SetValue(capabilities, "screenPixelsHeight", GetScreenPixelsHeight(device));
            SetValue(capabilities, "screenPixelsWidth", GetScreenPixelsWidth(device));
            SetValue(capabilities, "screenBitDepth", GetScreenBitDepth(device));
            SetValue(capabilities, "preferredImageMime", GetPreferredImageMime(device, capabilities));
            SetValue(capabilities, "isColor", GetIsColor(device));
            SetValue(capabilities, "SupportsCallback", GetSupportsCallback(device));
            SetValue(capabilities, "maximumRenderedPageSize", GetMaximumRenderedPageSize(device));
            SetValue(capabilities, "rendersBreaksAfterWmlAnchor", GetRendersBreaksAfterWmlAnchor(device));
            SetValue(capabilities, "rendersBreaksAfterWmlInput", GetRendersBreaksAfterWmlInput(device));
            SetValue(capabilities, "screenCharactersHeight", GetScreenCharactersHeight(device));
            SetValue(capabilities, "screenCharactersWidth", GetScreenCharactersWidth(device));
            SetValue(capabilities, "requiresUTF8ContentEncoding", GetRequiresUTF8ContentEncoding(device));
            
            SetValue(capabilities, "canInitiateVoiceCall", GetIsMobileDevice(device));
            SetValue(capabilities, "javascript", GetJavascriptSupport(device));
            SetValue(capabilities, "supportsNoWrapStyle", GetSupportsNoWrapStyle(device));
            SetValue(capabilities, "supportsStyleElement", GetPreferredRenderingTypeFromWURFL(device));
            SetValue(capabilities, "maximumSoftKeyLabelLength", GetMaximumSoftKeyLabelLength(device));

            // Use the Version class to find the version. If this fails use the 1st two
            // decimal segments of the string.
            string versionString = _provider.Strings.Get(device.GetCapability(MobileBrowserVersion));
            if (String.IsNullOrEmpty(versionString) == false)
            {
                try
                {
                    Version version = new Version(versionString);
                    SetValue(capabilities, "majorversion", version.Major.ToString());
                    SetValue(capabilities, "minorversion", String.Format(".{0}", version.Minor));
                    SetValue(capabilities, "version", version.ToString());
                }
                catch (FormatException)
                {
                    SetVersion(capabilities, versionString);
                }
                catch (ArgumentException)
                {
                    SetVersion(capabilities, versionString);
                }
            }
            else
            {
                // Transfer the current version capabilities to the new capabilities.
                SetValue(capabilities, "majorversion", currentCapabilities != null ? currentCapabilities["majorversion"] : null);
                SetValue(capabilities, "minorversion", currentCapabilities != null ? currentCapabilities["minorversion"] : null);
                SetValue(capabilities, "version", currentCapabilities != null ? currentCapabilities["version"] : null);

                // Ensure the version values are not null to prevent null arguement exceptions
                // with some controls.
                versionString = currentCapabilities != null ? currentCapabilities["version"] as string : "0.0";
                SetVersion(capabilities, versionString);
            }

            SetValue(capabilities, "supportsImageSubmit", GetSupportsImageSubmit(device));

            // All we can determine from WURFL is if javascript is supported as a boolean.
            // Use this value to set a version number that is the same for all devices that
            // indicate javascript support.
            bool javaScript = GetJavascriptSupport(device);
            SetJavaScript(capabilities, javaScript);
            SetValue(capabilities, "ecmascriptversion",
                     javaScript ? "3.0" : "0.0");
            SetValue(capabilities, "jscriptversion",
                     javaScript ? "5.7" : "0.0");

            SetValue(capabilities, "w3cdomversion",
                     GetW3CDOMVersion(device,
                                      currentCapabilities != null
                                          ? (string) currentCapabilities["w3cdomversion"]
                                          : String.Empty));
            
            SetValue(capabilities, "cookies",
                    GetCookieSupport(device,
                                     currentCapabilities != null
                                         ? (string)currentCapabilities["cookies"]
                                         : String.Empty));

            // Only set these values from WURFL if they've not already been set from
            // the Http request header.
            if (capabilities.Contains("preferredRenderingType") == false)
            {
                // Set the rendering type for the response.
                string renderingType = GetPreferredRenderingTypeFromWURFL(device);
                if (String.IsNullOrEmpty(renderingType) == false)
                    SetValue(capabilities, "preferredRenderingType", renderingType);

                // Set the Mime type of the response.
                if (capabilities.Contains("preferredRenderingMime") == false)
                {
                    string renderingMime = GetPreferredRenderingMimeFromWURFL(device, renderingType);
                    if (String.IsNullOrEmpty(renderingMime) == false)
                        SetValue(capabilities, "preferredRenderingMime", renderingMime);
                }
            }

            SetValue(capabilities, "tables",
                    GetTables(device,
                                     currentCapabilities != null
                                         ? (string)currentCapabilities["tables"]
                                         : String.Empty));
        }

       

        /// <summary>
        /// Sets the version using a regular expression to find numeric segments of
        /// the provided version string. If the version already exists in the
        /// new dictionary of capabilities a new value will not be written.
        /// </summary>
        /// <param name="capabilities"></param>
        /// <param name="version"></param>
        private static void SetVersion(IDictionary capabilities, string version)
        {
            if (version != null)
            {
                MatchCollection segments = Regex.Matches(version, @"\d+");
                string majorVersion = segments.Count > 0 ? segments[0].Value : "0";
                string minorVersion = segments.Count > 1 ? segments[1].Value : "0";
                if (String.IsNullOrEmpty(capabilities["majorversion"] as string))
                    SetValue(capabilities, "majorversion", majorVersion);
                if (String.IsNullOrEmpty(capabilities["minorversion"] as string))
                    SetValue(capabilities, "minorversion", minorVersion);
                if (String.IsNullOrEmpty(capabilities["version"] as string))
                    SetValue(capabilities, "version", String.Format("{0}.{1}", majorVersion, minorVersion));
            }
        }

        private string GetSupportsImageSubmit(DeviceInfo device)
        {
            int value = 0;
            if (int.TryParse(_provider.Strings.Get(device.GetCapability(XhtmlSupportLevel)), out value) &&
                value >= 3)
                return bool.TrueString.ToLowerInvariant();

            return bool.FalseString.ToLowerInvariant();
        }

        private string GetW3CDOMVersion(DeviceInfo device, string current)
        {
            int level = 0;
            Version version = new Version(0, 0);
            if (int.TryParse(_provider.Strings.Get(device.GetCapability(XhtmlSupportLevel)), out level) &&
                level >= 4)
                version = new Version("1.0.0.0");
            else
            {
                try
                {
                    version = new Version(current);
                }
                catch (ArgumentException)
                {
                    // Do nothing and let the default value be returned.
                }
            }
            return version.ToString(2);
        }

        private string GetCookieSupport(DeviceInfo device, string current)
        {
            bool value = false;
            // Return either the capability or the current value as a boolean string.
            if (bool.TryParse(_provider.Strings.Get(device.GetCapability(CookieSupport)), out value) == false)
                bool.TryParse(current, out value);
            return value.ToString();
        }

        /// <summary>
        /// Returns true if the device supports callbacks from the browser
        /// using XMLHttpRequest using any type of method.
        /// </summary>
        /// <param name="device">The device to be checked.</param>
        /// <returns>True if XMLHttpRequest is supported.</returns>
        private string GetSupportsCallback(DeviceInfo device)
        {
            string value = _provider.Strings.Get(device.GetCapability(AjaxXhrType));
            if (String.IsNullOrEmpty(value) || 
                value.Equals("none", StringComparison.InvariantCultureIgnoreCase))
                return bool.FalseString.ToLowerInvariant();
            return bool.TrueString.ToLowerInvariant();
        }

        /// <summary>
        /// If the wurfl database indicates javascript support then return true.
        /// </summary>
        /// <param name="device">The device to be checked.</param>
        /// <returns>True if javascript is supported.</returns>
        private bool GetJavascriptSupport(DeviceInfo device)
        {
            bool java = false;
            if (bool.TryParse(_provider.Strings.Get(device.GetCapability(AjaxSupportJavascript)), out java) &&
                java)
            {
                return true;
            }
            return false;
        }

        private string GetPlatform(DeviceInfo device)
        {
            return _provider.Strings.Get(device.GetCapability(DeviceOs));
        }

        private static string GetIsCrawler(DeviceInfo device)
        {
            if (device.DeviceId == "generic_web_crawler" || device.DeviceId.Contains("crawler"))
                return bool.TrueString.ToLowerInvariant();
            if (device.FallbackDevice != null)
                return GetIsCrawler(device.FallbackDevice);
            return bool.FalseString.ToLowerInvariant();
        }

        private string GetAdapters(DeviceInfo device)
        {
            return _provider.Strings.Get(device.GetCapability(Adapters));
        }

        private static string GetIsMobileDevice(DeviceInfo device)
        {
            if (device.IsMobileDevice)
                return bool.TrueString.ToLowerInvariant();
            return bool.FalseString.ToLowerInvariant();
        }

        private string GetScreenPixelsHeight(DeviceInfo device)
        {
            return _provider.Strings.Get(device.GetCapability(ResolutionHeight));
        }

        private string GetScreenPixelsWidth(DeviceInfo device)
        {
            return _provider.Strings.Get(device.GetCapability(ResolutionWidth));
        }

        private string GetIsColor(DeviceInfo device)
        {
            long numberOfColors = 0;
            long.TryParse(_provider.Strings.Get(device.GetCapability(Colors)), out numberOfColors);
            bool isColor = (numberOfColors >= 256);
            return isColor.ToString(CultureInfo.InvariantCulture);
        }

        private string GetHasBackButton(DeviceInfo device)
        {
            return _provider.Strings.Get(device.GetCapability(BuiltInBackButtonSupport));
        }

        private string GetSupportsAccesskeyAttribute(DeviceInfo device)
        {
            return _provider.Strings.Get(device.GetCapability(AccessKeySupport));
        }

        private string GetMobileDeviceModel(DeviceInfo device)
        {
            string value = _provider.Strings.Get(device.GetCapability(MarketingName));
            if (String.IsNullOrEmpty(value))
                value = _provider.Strings.Get(device.GetCapability(ModelName));
            return value;
        }

        private string GetMobileDeviceManufacturer(DeviceInfo device)
        {
            return _provider.Strings.Get(device.GetCapability(BrandName));
        }

        private string GetPreferredImageMime(DeviceInfo device, IDictionary capabilities)
        {
            // Look at the database and return the 1st one that matches in order
            // of preference.
            if (bool.TrueString.Equals(_provider.Strings.Get(device.GetCapability(Png)), StringComparison.InvariantCultureIgnoreCase))
                return "image/png";
            if (bool.TrueString.Equals(_provider.Strings.Get(device.GetCapability(Jpg)), StringComparison.InvariantCultureIgnoreCase))
                return "image/jpeg";
            if (bool.TrueString.Equals(_provider.Strings.Get(device.GetCapability(Gif)), StringComparison.InvariantCultureIgnoreCase))
                return "image/gif";
            return null;
        }

        private string GetScreenBitDepth(DeviceInfo device)
        {
            long fScreenBitDepth = 1;
            long numberOfColors = 256;
            long.TryParse(_provider.Strings.Get(device.GetCapability(Colors)), out numberOfColors);
            fScreenBitDepth = Support.GetBitsPerPixel(numberOfColors);
            return fScreenBitDepth.ToString(CultureInfo.InvariantCulture);
        }

        private string GetPreferredRenderingMimeFromWURFL(DeviceInfo device, string renderingType)
        {
            switch (renderingType)
            {
                case "xhtml-mp":
                case "xhtml-basic":
                    return _provider.Strings.Get(device.GetCapability(XhtmlmpPreferredMimeType));

                case "chtml10":
                case "html4":
                case "html32":
                    return "text/html";

                default:
                    return null;
            }
        }

        private string GetFirstRenderingTypeFromWURFL(DeviceInfo device)
        {
            // The value returned when checking the capability.
            bool value;

            // Look at all the possible markups that could be supported
            // and returned the 1st one found.
            if (bool.TryParse(_provider.Strings.Get(device.GetCapability(HtmlWiOmaXhtmlmp10)), out value) && value)
                return "xhtml-mp";
            if (bool.TryParse(_provider.Strings.Get(device.GetCapability(HtmlWiW3Xhtmlbasic)), out value) && value)
                return "xhtml-basic";
            if (bool.TryParse(_provider.Strings.Get(device.GetCapability(HtmlWiImodeCompactGeneric)), out value) && value)
                return "chtml10";
            if (bool.TryParse(_provider.Strings.Get(device.GetCapability(HtmlWeb40)), out value) && value)
                return "html32";
            if (bool.TryParse(_provider.Strings.Get(device.GetCapability(HtmlWeb32)), out value) && value)
                return "html32";
                return null;
        }

        private string GetPreferredRenderingTypeFromWURFL(DeviceInfo device)
        {
            switch (_provider.Strings.Get(device.GetCapability(PreferredMarkup)))
            {
                case "html_web_3_2":
                    return "html32";
                case "html_web_4_0":
                    return "html4";
                case "html_wi_oma_xhtmlmp_1_0":
                    return "xhtml-mp";
                case "html_wi_w3_xhtmlbasic":
                    return "xhtml-basic";
                case "html_wi_imode_htmlx_1":
                case "html_wi_imode_html_1":
                case "html_wi_imode_html_2":
                case "html_wi_imode_html_3":
                case "html_wi_imode_html_4":
                case "html_wi_imode_html_5":
                case "html_wi_imode_html_6":
                case "html_wi_imode_htmlx_1_1":
                case "html_wi_imode_compact_generic":
                    return "chtml10";
                default:
                    return GetFirstRenderingTypeFromWURFL(device);
            }
        }


        //The following methods and bindings were suggested by forum member 'fravelgue'
        private string GetMaximumSoftKeyLabelLength(DeviceInfo device)
        {
            string value = _provider.Strings.Get(device.GetCapability(SoftkeySupport));
            if (String.IsNullOrEmpty(value) || value.Equals("false", StringComparison.InvariantCultureIgnoreCase))
                return bool.FalseString.ToLowerInvariant();
            return bool.TrueString.ToLowerInvariant();
        }

        private string GetSupportsNoWrapStyle(DeviceInfo device)
        {
            string value = _provider.Strings.Get(device.GetCapability(XhtmlNoWrapMode));
            if (String.IsNullOrEmpty(value) || value.Equals("false", StringComparison.InvariantCultureIgnoreCase))
                return bool.FalseString.ToLowerInvariant();
            return bool.TrueString.ToLowerInvariant();
        }

        private string GetTables(DeviceInfo device, string current)
        {
            if (_provider.Strings.Get(device.GetCapability(PreferredMarkup)).Contains("xhtml"))
            {
                bool value =  false;
                if (bool.TryParse(_provider.Strings.Get(device.GetCapability(XhtmlTableSupport)), out value) == false)
                    bool.TryParse(current, out value);
                return value.ToString();
            }
            return current;
        }

        private string GetRequiresUTF8ContentEncoding(DeviceInfo device)
        {
            string value = _provider.Strings.Get(device.GetCapability(Utf8Support));
            if (String.IsNullOrEmpty(value) || value.Equals("false", StringComparison.InvariantCultureIgnoreCase))
                return bool.FalseString.ToLowerInvariant();
            return bool.TrueString.ToLowerInvariant();
        }

        private string GetScreenCharactersWidth(DeviceInfo device)
        {
            return _provider.Strings.Get(device.GetCapability(Columns));
        }

        private string GetScreenCharactersHeight(DeviceInfo device)
        {
            return _provider.Strings.Get(device.GetCapability(Rows));
        }

        private string GetRendersBreaksAfterWmlInput(DeviceInfo device)
        {
            string value = _provider.Strings.Get(device.GetCapability(InsertBrElementAfterWidgetRecommended));
            if (String.IsNullOrEmpty(value) || value.Equals("false", StringComparison.InvariantCultureIgnoreCase))
                return bool.FalseString.ToLowerInvariant();
            return bool.TrueString.ToLowerInvariant();
        }

        private string GetRendersBreaksAfterWmlAnchor(DeviceInfo device)
        {
            string value = _provider.Strings.Get(device.GetCapability(BreakListOfLinksWithBrElementRecommended));
            if (String.IsNullOrEmpty(value) || value.Equals("false", StringComparison.InvariantCultureIgnoreCase))
                return bool.FalseString.ToLowerInvariant();
            return bool.TrueString.ToLowerInvariant();
        }

        private string GetMaximumRenderedPageSize(DeviceInfo device)
        {
            return _provider.Strings.Get(device.GetCapability(MaxDeckSize));
        }

        #endregion
    }
}