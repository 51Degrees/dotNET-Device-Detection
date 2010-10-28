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

#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
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
        private static readonly int XhtmlSupportLevel = Strings.Add("xhtml_support_level");
        private static readonly int CookieSupport = Strings.Add("cookie_support");
        private static readonly int AjaxSupportJavascript = Strings.Add("ajax_support_javascript");
        private static readonly int MobileBrowserVersion = Strings.Add("mobile_browser_version");
        private static readonly int DeviceOs = Strings.Add("device_os");
        private static readonly int Adapters = Strings.Add("adapters");
        private static readonly int ResolutionHeight = Strings.Add("resolution_height");
        private static readonly int ResolutionWidth = Strings.Add("resolution_width");
        private static readonly int Colors = Strings.Add("colors");
        private static readonly int BuiltInBackButtonSupport = Strings.Add("built_in_back_button_support");
        private static readonly int AccessKeySupport = Strings.Add("access_key_support");
        private static readonly int MarketingName = Strings.Add("marketing_name");
        private static readonly int ModelName = Strings.Add("model_name");
        private static readonly int BrandName = Strings.Add("brand_name");
        private static readonly int Jpg = Strings.Add("jpg");
        private static readonly int Gif = Strings.Add("gif");
        private static readonly int Png = Strings.Add("png");
        private static readonly int XhtmlmpPreferredMimeType = Strings.Add("xhtmlmp_preferred_mime_type");
        private static readonly int HtmlWiOmaXhtmlmp10 = Strings.Add("html_wi_oma_xhtmlmp_1_0");
        private static readonly int HtmlWiW3Xhtmlbasic = Strings.Add("html_wi_w3_xhtmlbasic");
        private static readonly int HtmlWiImodeCompactGeneric = Strings.Add("html_wi_imode_compact_generic");
        private static readonly int HtmlWeb40 = Strings.Add("html_web_4_0");
        private static readonly int HtmlWeb32 = Strings.Add("html_web_3_2");
        private static readonly int PreferredMarkup = Strings.Add("preferred_markup");

        #endregion

        #region Constructor

        internal MobileCapabilities() : base()
        {
        }

        #endregion

        #region Override Methods

        internal override IDictionary Create(HttpContext context)
        {
            // Use the base class to create the initial list of capabilities.
            IDictionary capabilities = base.Create(context);

            // Use the device for the request to set the capabilities.
            Create(Provider.Instance.GetDeviceInfo(context), capabilities, context.Request.Browser.Capabilities);

            // Initialise any capability values that rely on the settings
            // from the device data source.
            Init(capabilities);

            return capabilities;
        }

        internal override IDictionary Create(string userAgent)
        {
            // Use the base class to create the initial list of capabilities.
            IDictionary capabilities = base.Create(userAgent);

            // Use the device for the request to set the capabilities.
            Create(Provider.Instance.GetDeviceInfo(userAgent), capabilities, null);

            // Initialise any capability values that rely on the settings
            // from the device data source.
            Init(capabilities);

            return capabilities;
        }

        #endregion

        #region Private Methods

        private static void Create(DeviceInfo device, IDictionary capabilities, IDictionary currentCapabilities)
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
        private static SortedList<string, string> CreateWurflCapabilities(DeviceInfo device)
        {
            var capabilities = new SortedList<string, string>();

            // Get the list of capability names for the device.
            List<int> names = new List<int>();
            AddWurflCapabilityNames(names, device);

            // Add the device id even thought his is not a capability. It
            // may be used by some applications.
            capabilities.Add("deviceid", device.DeviceId);

            // Add the actualDeviceRoot value as it is needed by some developers.
            capabilities.Add("actual_device_root", device.IsActualDeviceRoot.ToString());

            // Add all the capabilities for the device.
            foreach (int name in names)
                capabilities[Strings.Get(name)] = Strings.Get(device.GetCapability(name));

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
        /// returns. Only properties exposed by HttpBrowserCapabilities are overriden
        /// by this method.
        /// </summary>
        /// <param name="capabilities">Dictionary of capabilities to be enhanced.</param>
        /// <param name="currentCapabilities">Dictionary of existing capabilities for the device.</param>
        /// <param name="device">Device to use for enhancements.</param>
        private static void Enhance(IDictionary capabilities, IDictionary currentCapabilities, DeviceInfo device)
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

            // Use the Version class to find the version. If this fails use the 1st two
            // decimal segments of the string.
            string versionString = Strings.Get(device.GetCapability(MobileBrowserVersion));
            if (String.IsNullOrEmpty(versionString) == false)
            {
                try
                {
                    Version version = new Version(versionString);
                    SetValue(capabilities, "majorversion", version.Major);
                    SetValue(capabilities, "minorversion", "." + version.Minor.ToString());
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

        }

        /// <summary>
        /// Sets the version using a regular expression to find numeric segments of
        /// the provided version string.
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
                SetValue(capabilities, "majorversion", majorVersion);
                SetValue(capabilities, "minorversion", minorVersion);
                SetValue(capabilities, "version", majorVersion + "." + minorVersion);
            }
        }

        private static string GetSupportsImageSubmit(DeviceInfo device)
        {
            int value = 0;
            if (int.TryParse(Strings.Get(device.GetCapability(XhtmlSupportLevel)), out value) &&
                value >= 3)
                return bool.TrueString.ToLowerInvariant();

            return bool.FalseString.ToLowerInvariant();
        }

        private static string GetW3CDOMVersion(DeviceInfo device, string current)
        {
            int level = 0;
            Version version = new Version(0, 0);
            if (int.TryParse(Strings.Get(device.GetCapability(XhtmlSupportLevel)), out level) &&
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

        private static string GetCookieSupport(DeviceInfo device, string current)
        {
            bool value = false;
            // Return either the capability or the current value as a boolean string.
            if (bool.TryParse(Strings.Get(device.GetCapability(CookieSupport)), out value) == false)
                bool.TryParse(current, out value);
            return value.ToString();
        }

        /// <summary>
        /// If the wurfl database indicates javascript support then return true.
        /// </summary>
        /// <param name="device">The device to be checked.</param>
        /// <returns>True if javascript is supported.</returns>
        private static bool GetJavascriptSupport(DeviceInfo device)
        {
            bool java = false;
            if (bool.TryParse(Strings.Get(device.GetCapability(AjaxSupportJavascript)), out java) &&
                java)
            {
                return true;
            }
            return false;
        }

        private static string GetPlatform(DeviceInfo device)
        {
            return Strings.Get(device.GetCapability(DeviceOs));
        }

        private static string GetIsCrawler(DeviceInfo device)
        {
            if (device.DeviceId == "generic_web_crawler" || device.DeviceId.Contains("crawler"))
                return bool.TrueString.ToLowerInvariant();
            ;
            if (device.FallbackDevice != null)
                return GetIsCrawler(device.FallbackDevice);
            return bool.FalseString.ToLowerInvariant();
            ;
        }

        private static string GetAdapters(DeviceInfo device)
        {
            return Strings.Get(device.GetCapability(Adapters));
        }

        private static string GetIsMobileDevice(DeviceInfo device)
        {
            if (device.IsMobileDevice)
                return bool.TrueString.ToLowerInvariant();
            return bool.FalseString.ToLowerInvariant();
        }

        private static string GetScreenPixelsHeight(DeviceInfo device)
        {
            return Strings.Get(device.GetCapability(ResolutionHeight));
        }

        private static string GetScreenPixelsWidth(DeviceInfo device)
        {
            return Strings.Get(device.GetCapability(ResolutionWidth));
        }

        private static string GetIsColor(DeviceInfo device)
        {
            long numberOfColors = 0;
            long.TryParse(Strings.Get(device.GetCapability(Colors)), out numberOfColors);
            bool isColor = (numberOfColors >= 256);
            return isColor.ToString(CultureInfo.InvariantCulture);
        }

        private static string GetHasBackButton(DeviceInfo device)
        {
            return Strings.Get(device.GetCapability(BuiltInBackButtonSupport));
        }

        private static string GetSupportsAccesskeyAttribute(DeviceInfo device)
        {
            return Strings.Get(device.GetCapability(AccessKeySupport));
        }

        private static string GetMobileDeviceModel(DeviceInfo device)
        {
            string value = Strings.Get(device.GetCapability(MarketingName));
            if (String.IsNullOrEmpty(value))
                value = Strings.Get(device.GetCapability(ModelName));
            return value;
        }

        private static string GetMobileDeviceManufacturer(DeviceInfo device)
        {
            return Strings.Get(device.GetCapability(BrandName));
        }

        private static string GetPreferredImageMime(DeviceInfo device, IDictionary capabilities)
        {


            // Look at the database and return the 1st one that matches in order
            // of preference.
            if (bool.TrueString.Equals(Strings.Get(device.GetCapability(Png)), StringComparison.InvariantCultureIgnoreCase))
                return "image/png";
            if (bool.TrueString.Equals(Strings.Get(device.GetCapability(Jpg)), StringComparison.InvariantCultureIgnoreCase))
                return "image/jpeg";
            if (bool.TrueString.Equals(Strings.Get(device.GetCapability(Gif)), StringComparison.InvariantCultureIgnoreCase))
                return "image/gif";
            return null;
        }

        private static string GetScreenBitDepth(DeviceInfo device)
        {
            long fScreenBitDepth = 1;
            long numberOfColors = 256;
            long.TryParse(Strings.Get(device.GetCapability(Colors)), out numberOfColors);
            fScreenBitDepth = Support.GetBitsPerPixel(numberOfColors);
            return fScreenBitDepth.ToString(CultureInfo.InvariantCulture);
        }

        private static string GetPreferredRenderingMimeFromWURFL(DeviceInfo device, string renderingType)
        {
            switch (renderingType)
            {
                case "xhtml-mp":
                case "xhtml-basic":
                    return Strings.Get(device.GetCapability(XhtmlmpPreferredMimeType));

                case "chtml10":
                case "html4":
                case "html32":
                    return "text/html";

                default:
                    return null;
            }
        }

        private static string GetFirstRenderingTypeFromWURFL(DeviceInfo device)
        {
            // The value returned when checking the capability.
            bool value;

            // Look at all the possible markups that could be supported
            // and returned the 1st one found.
            if (bool.TryParse(Strings.Get(device.GetCapability(HtmlWiOmaXhtmlmp10)), out value) && value)
                return "xhtml-mp";
            if (bool.TryParse(Strings.Get(device.GetCapability(HtmlWiW3Xhtmlbasic)), out value) && value)
                return "xhtml-basic";
            if (bool.TryParse(Strings.Get(device.GetCapability(HtmlWiImodeCompactGeneric)), out value) && value)
                return System.Web.Mobile.MobileCapabilities.PreferredRenderingTypeChtml10;
            if (bool.TryParse(Strings.Get(device.GetCapability(HtmlWeb40)), out value) && value)
                return System.Web.Mobile.MobileCapabilities.PreferredRenderingTypeHtml32;
            if (bool.TryParse(Strings.Get(device.GetCapability(HtmlWeb32)), out value) && value)
                return System.Web.Mobile.MobileCapabilities.PreferredRenderingTypeHtml32;
                return null;
        }

        private static string GetPreferredRenderingTypeFromWURFL(DeviceInfo device)
        {
            switch (Strings.Get(device.GetCapability(PreferredMarkup)))
            {
                case "html_web_3_2":
                    return System.Web.Mobile.MobileCapabilities.PreferredRenderingTypeHtml32;
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
                    return System.Web.Mobile.MobileCapabilities.PreferredRenderingTypeChtml10;
                default:
                    return GetFirstRenderingTypeFromWURFL(device);
            }
        }

        #endregion
    }
}