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
using System.Collections.Generic;
using System.Web;
using System.Globalization;
using System.Collections;
using System.Text.RegularExpressions;
using System.Text;
using System.Drawing.Imaging;
using System.Security.Permissions;

namespace FiftyOne.Foundation.Mobile.Detection.Wurfl
{
    /// <summary>
    /// Mobile capabilities populated using the WURFL data source.
    /// </summary>
    public class MobileCapabilities : FiftyOne.Foundation.Mobile.Detection.MobileCapabilities
    {
        #region Constructor

        /// <summary>
        /// Creates an instance of the <see cref="MobileCapabilities"/> class.
        /// </summary>
        public MobileCapabilities(IDictionary capabilities)
            : base(capabilities)
        {
        }

        /// <summary>
        /// Creates an instance of the <see cref="MobileCapabilities"/> class.
        /// </summary>
        public MobileCapabilities(string userAgent)
            : base(userAgent)
        {
            Init(Provider.Instance.GetDeviceInfo(userAgent));
        }

        /// <summary>
        /// Creates an instance of the <see cref="MobileCapabilities"/> class.
        /// </summary>
        public MobileCapabilities(HttpContext context)
            : base(context)
        {
            Init(Provider.Instance.GetDeviceInfo(context));
        }

        #endregion

        #region Properties
        
        /// <summary>
        /// Returns the Wurfl device ID of the device.
        /// </summary>
        public string DeviceId
        {
            get
            {
                return this["deviceid"] as string;
            }
        }
        
        /// <summary>
        /// Returns a value indicating if the device capabilities where
        /// populated from an actualDeviceRoot device.
        /// </summary>
        public bool ActualDeviceRoot
        {
            get
            {
                bool value = false;
                bool.TryParse(this["actualDeviceRoot"], out value);
                return value;
            }
        }

        /// <summary>
        /// Provides the maximum image width in pixels as an integer.
        /// </summary>
        public override int MaxImagePixelsWidth
        {
            get
            {
                int value = 0;
                if (int.TryParse(Capabilities["maxImagePixelsWidth"] as string, out value))
                    return value;
                return ScreenPixelsWidth;
            }
        }


        /// <summary>
        /// Provides the maximum image height in pixels as an integer.
        /// </summary>
        public override int MaxImagePixelsHeight
        {
            get
            {
                int value = 0;
                if (int.TryParse(Capabilities["maxImagePixelsHeight"] as string, out value))
                    return value;
                return ScreenPixelsWidth;
            }
        }

        /// <summary>
        /// Provides details of device support for dual orientation.
        /// </summary>
        public override DualOrientation DualOrientation
        {
            get
            {
                object obj = Capabilities["dualOrientation"];
                if (obj is DualOrientation)
                    return (DualOrientation)obj;
                return DualOrientation.Unknown;
            }
        }

        /// <summary>
        /// Returns true if the browser supports the manipulation of
        /// css properties via javascript.
        /// </summary>
        public override bool CssManipulation
        {
            get
            {
                bool value = false;
                if (bool.TryParse(Capabilities["cssManipulation"] as string, out value) == true)
                    return value;
                return false;
            }
        }

        /// <summary>
        /// Returns the physical width of the screen in milimeters.
        /// </summary>
        public override int PhysicalScreenWidth
        {
            get
            {
                int value;
                int.TryParse(Capabilities["physicalScreenWidth"] as string, out value);
                return value;
            }
        }

        /// <summary>
        /// Returns the physical height of the screen in milimeters.
        /// </summary>
        public override int PhysicalScreenHeight
        {
            get
            {
                int value;
                int.TryParse(Capabilities["physicalScreenHeight"] as string, out value);
                return value;
            }
        }

        /// <summary>
        /// Is the 'Viewport' META tag supported
        /// </summary>
        public override bool MetaViewportSupported
        {
            get
            {
                bool value = false;
                bool.TryParse(Capabilities["viewport_supported"].ToString(), out value);
                return value;
            }
        }

        /// <summary>
        /// Checks the META tag
        /// Check if the device prevent the browser from trying to adapt the page to fit the mobile screen
        /// </summary>
        public override bool MetaHandHeldFriendlySupported
        {
            get
            {
                bool value = false;
                bool.TryParse(Capabilities["handheldfriendly"].ToString(), out value);
                return value;
            }
        }

        /// <summary>
        /// Checks the META tag
        /// Check if the device prevent the browser from trying to adapt the page to fit the mobile screen
        /// </summary>
        public override bool MetaMobileOptimizedSupported
        {
            get
            {
                bool value = false;
                bool.TryParse(Capabilities["mobileoptimized"].ToString(), out value);
                return value;
            }
        }

        /// <summary>
        /// Check if the device can refer to pictures and use them in different circimstances as backgounds.
        /// </summary>
        public override bool CssSpriting
        {
            get
            {
                bool value = false;
                bool.TryParse(Capabilities["css_spriting"].ToString(), out value);
                return value;
            }
        }

        /// <summary>
        /// Checks if the device work when CSS defined as percentage
        /// </summary>
        public override bool CssSupportsWidthAsPercentage
        {
            get
            {
                bool value = false;
                bool.TryParse(Capabilities["css_supports_width_as_percentage"].ToString(), out value);
                return value;
            }
        }

        /// <summary>
        /// Check which Css group provides border image support for Css
        /// </summary>
        public override CssGroup CssBorderImageSupport
        {
            get
            {
                string value = Capabilities["css_border_image"] as string;
                if (CssGroup.CSS3.ToString().Equals(value))
                    return CssGroup.CSS3;
                if (CssGroup.Mozilla.ToString().Equals(value))
                    return CssGroup.Mozilla;
                if (CssGroup.Opera.ToString().Equals(value))
                    return CssGroup.Opera;
                if (CssGroup.WebKit.ToString().Equals(value))
                    return CssGroup.WebKit;
                return CssGroup.None;
            }
        }

        /// <summary>
        /// Check which Css group provides gradient support for Css
        /// </summary>
        public override CssGroup CssGradientSupport
        {
            get
            {
                string value = Capabilities["css_gradient"] as string;
                if (CssGroup.CSS3.ToString().Equals(value))
                    return CssGroup.CSS3;
                if (CssGroup.Mozilla.ToString().Equals(value))
                    return CssGroup.Mozilla;
                if (CssGroup.Opera.ToString().Equals(value))
                    return CssGroup.Opera;
                if (CssGroup.WebKit.ToString().Equals(value))
                    return CssGroup.WebKit;
                return CssGroup.None;
            }
        }

        /// <summary>
        /// Check which Css group provides rounded corner support for Css
        /// </summary>
        public override CssGroup CssRoundedCornerSupport
        {
            get
            {
                string value = Capabilities["css_rounded_corners"] as string;
                if (CssGroup.CSS3.ToString().Equals(value))
                    return CssGroup.CSS3;
                if (CssGroup.Mozilla.ToString().Equals(value))
                    return CssGroup.Mozilla;
                if (CssGroup.Opera.ToString().Equals(value))
                    return CssGroup.Opera;
                if (CssGroup.WebKit.ToString().Equals(value))
                    return CssGroup.WebKit;
                return CssGroup.None;
            }
        }
        
        /// <summary>
        /// Provides the pointing method used by the mobile device.
        /// </summary>
        public override PointingMethods PointingMethod
        {
            get
            {
                string value = this["pointingMethod"] as string;
                if (PointingMethods.Clickwheel.ToString().Equals(value))
                    return PointingMethods.Clickwheel;
                if (PointingMethods.Joystick.ToString().Equals(value))
                    return PointingMethods.Joystick;
                if (PointingMethods.Stylus.ToString().Equals(value))
                    return PointingMethods.Stylus;
                if (PointingMethods.Touchscreen.ToString().Equals(value))
                    return PointingMethods.Touchscreen;
                return PointingMethods.Unknown;
            }
        }

        /// <summary>
        /// Returns true if the audio format is supported by the device.
        /// </summary>
        /// <param name="format">Audio format to query.</param>
        /// <returns>True if supported. False if not.</returns>
        internal override bool IsAudioFormatSupported(AudioFormats format)
        {
            switch (format)
            {
                case AudioFormats.Aac:
                    return bool.TrueString.Equals(this["aac"], StringComparison.InvariantCultureIgnoreCase);
                case AudioFormats.Amr:
                    return bool.TrueString.Equals(this["amr"], StringComparison.InvariantCultureIgnoreCase);
                case AudioFormats.Au:
                    return bool.TrueString.Equals(this["au"], StringComparison.InvariantCultureIgnoreCase);
                case AudioFormats.Awb:
                    return bool.TrueString.Equals(this["awb"], StringComparison.InvariantCultureIgnoreCase);
                case AudioFormats.CompactMidi:
                    return bool.TrueString.Equals(this["compactmidi"], StringComparison.InvariantCultureIgnoreCase);
                case AudioFormats.Digiplug:
                    return bool.TrueString.Equals(this["digiplug"], StringComparison.InvariantCultureIgnoreCase);
                case AudioFormats.Evrc:
                    return bool.TrueString.Equals(this["evrc"], StringComparison.InvariantCultureIgnoreCase);
                case AudioFormats.IMelody:
                    return bool.TrueString.Equals(this["imelody"], StringComparison.InvariantCultureIgnoreCase);
                case AudioFormats.MidiMonophonic:
                    return bool.TrueString.Equals(this["midi_monophonic"], StringComparison.InvariantCultureIgnoreCase);
                case AudioFormats.MidiPolyphonic:
                    return bool.TrueString.Equals(this["midi_polyphonic"], StringComparison.InvariantCultureIgnoreCase);
                case AudioFormats.Mld:
                    return bool.TrueString.Equals(this["mld"], StringComparison.InvariantCultureIgnoreCase);
                case AudioFormats.Mmf:
                    return bool.TrueString.Equals(this["mmf"], StringComparison.InvariantCultureIgnoreCase);
                case AudioFormats.Mp3:
                    return bool.TrueString.Equals(this["mp3"], StringComparison.InvariantCultureIgnoreCase);
                case AudioFormats.NokiaRingtone:
                    return bool.TrueString.Equals(this["nokia_ringtone"], StringComparison.InvariantCultureIgnoreCase);
                case AudioFormats.Qcelp:
                    return bool.TrueString.Equals(this["qcelp"], StringComparison.InvariantCultureIgnoreCase);
                case AudioFormats.Rmf:
                    return bool.TrueString.Equals(this["rmf"], StringComparison.InvariantCultureIgnoreCase);
                case AudioFormats.Smf:
                    return bool.TrueString.Equals(this["smf"], StringComparison.InvariantCultureIgnoreCase);
                case AudioFormats.SpMidi:
                    return bool.TrueString.Equals(this["sp_midi"], StringComparison.InvariantCultureIgnoreCase);
                case AudioFormats.Wav:
                    return bool.TrueString.Equals(this["wav"], StringComparison.InvariantCultureIgnoreCase);
                case AudioFormats.Xmf:
                    return bool.TrueString.Equals(this["xmf"], StringComparison.InvariantCultureIgnoreCase);
            }
            return false;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Enhances the this object with capabilities found in the DeviceInfo
        /// object identified in the constructor.
        /// </summary>
        protected void Init(DeviceInfo device)
        {
            // Enhance with the capabilities from the device data.
            if (device != null)
                // Enhance the capabilities collection based on the device.
                Enhance(Capabilities, device);

            base.Init();
        }

        #endregion

        #region Private Methods

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

            // Ensure version numbers are not null to prevent exceptions in some
            // .NET control adapters.
            if (capabilities["majorversion"] == null)
                SetValue(capabilities, "majorversion", "1");
            if (capabilities["minorversion"] == null)
                SetValue(capabilities, "minorversion", "0");
        }

        /// <summary>
        /// Updates and adds new capabilities to the Dictionary provided based
        /// on the capabilities of the device provided.
        /// </summary>
        /// <param name="capabilities">Dictionaty of capabilities to be enhanced.</param>
        /// <param name="device">Device to use for enhancements.</param>
        private static void Enhance(IDictionary capabilities, DeviceInfo device)
        {
            // Set capabilities.
            SetStaticValues(capabilities);
            SetValue(capabilities, "deviceid", device.DeviceId);
            SetValue(capabilities, "actualDeviceRoot", device.IsActualDeviceRoot.ToString());
            SetValue(capabilities, "isMobileDevice", GetIsMobileDevice(device));
            SetValue(capabilities, "crawler", GetIsCrawler(device));
            SetValue(capabilities, "mobileDeviceModel", GetMobileDeviceModel(device));
            SetValue(capabilities, "mobileDeviceManufacturer", GetMobileDeviceManufacturer(device));
            SetValue(capabilities, "platform", GetPlatform(device));
            SetValue(capabilities, "type", (string)capabilities["mobileDeviceManufacturer"]);
            SetValue(capabilities, "supportsAccessKeyAttribute", GetSupportsAccesskeyAttribute(device));
            SetValue(capabilities, "hasBackButton", GetHasBackButton(device));
            SetValue(capabilities, "screenPixelsHeight", GetScreenPixelsHeight(device));
            SetValue(capabilities, "screenPixelsWidth", GetScreenPixelsWidth(device));
            SetValue(capabilities, "screenBitDepth", GetScreenBitDepth(device));
            SetValue(capabilities, "physicalScreenWidth", GetPhysicalScreenWidth(device));
            SetValue(capabilities, "physicalScreenHeight", GetPhysicalScreenHeight(device));
            SetValue(capabilities, "maxImagePixelsWidth", GetMaxImageWidth(device));
            SetValue(capabilities, "maxImagePixelsHeight", GetMaxImageHeight(device));
            SetValue(capabilities, "preferredImageMime", GetPreferredImageMime(device, capabilities));
            SetValue(capabilities, "isColor", GetIsColor(device));
            SetValue(capabilities, POINTING_METHOD, GetPointingMethod(device));
            SetValue(capabilities, DUAL_ORIENTATION, GetDualOrientation(device));
            SetValue(capabilities, "majorversion", GetMajorVersion(device, (string)capabilities["majorversion"]));
            SetValue(capabilities, "minorversion", GetMinorVersion(device, (string)capabilities["minorversion"]));
            SetValue(capabilities, "version", (string)capabilities["majorversion"] + (string)capabilities["minorversion"]);
            SetValue(capabilities, "supportsImageSubmit", GetSupportsImageSubmit(device));
            SetValue(capabilities, "viewport_supported", GetMetaViewPortSupported(device));
            SetValue(capabilities, "mobileoptimized", GetMetaMobileOptimizedSupported(device));
            SetValue(capabilities, "handheldfriendly", GetMetaHandHeldFriendlySupported(device));
            SetValue(capabilities, "css_supports_width_as_percentage", GetCssSupportsWidthAsPercentage(device));
            SetValue(capabilities, "css_spriting", GetCssSpriting(device));
            SetValue(capabilities, "css_gradient", GetCssSupportGroup(device, "css_gradient"));
            SetValue(capabilities, "css_border_image", GetCssSupportGroup(device, "css_border_image"));
            SetValue(capabilities, "css_rounded_corners", GetCssSupportGroup(device, "css_rounded_corners"));

            // All we can determine from WURFL is if javascript is supported as a boolean.
            // Use this value to set a version number that is the same for all devices that
            // indicate javascript support.
            bool javaScript = false;
            javaScript = GetJavascriptSupport(device, (string)capabilities["javascript"]);
            SetJavaScript(capabilities, javaScript);
            SetValue(capabilities, "ecmascriptversion",
                javaScript ? "3.0.0.0" : "0.0.0.0");
            SetValue(capabilities, "jscriptversion",
                javaScript ? "5.7.0.0" : "0.0.0.0");

            SetValue(capabilities, "cssManipulation", GetCssManipulationSupport(device));
            SetValue(capabilities, "w3cdomversion", GetW3CDOMVersion(device, (string)capabilities["w3cdomversion"]));
            SetValue(capabilities, "cookies", GetCookieSupport(device, (string)capabilities["cookies"]));
            SetAudio(capabilities, device);

            // Set the rendering type for the response.
            string renderingType = GetPreferredRenderingTypeFromWURFL(device);
            if (String.IsNullOrEmpty(renderingType) == false)
                SetValue(capabilities, "preferredRenderingType", renderingType);

            // Set the Mime type of the response.
            string renderingMime = GetPreferredRenderingMimeFromWURFL(device, renderingType);
            if (String.IsNullOrEmpty(renderingMime) == false)
                SetValue(capabilities, "preferredRenderingMime", renderingMime);

            // Set values that require the rendering type or mime.
            SetValue(capabilities, "supportsCss", GetCssSupport(renderingType));
            SetValue(capabilities, "tables", GetTables(device, renderingType));
        }

        private static string GetSupportsImageSubmit(DeviceInfo device)
        {
            int value = 0;
            if (int.TryParse(device.GetCapability("xhtml_support_level"), out value) &&
                value >= 3)
                return bool.TrueString.ToLowerInvariant();

            return bool.FalseString.ToLowerInvariant();
        }

        private static string GetDualOrientation(DeviceInfo device)
        {
            if (bool.TrueString.Equals(
                device.GetCapability("dual_orientation"), StringComparison.InvariantCultureIgnoreCase))
                return bool.TrueString.ToLowerInvariant();
            return bool.FalseString.ToLowerInvariant();
        }

        private static void SetAudio(IDictionary capabilities, DeviceInfo device)
        {
            foreach (string format in Constants.Audio_Formats)
            {
                SetValue(capabilities, format, GetAudioFormatSupport(device, format));
            }
        }

        private static string GetAudioFormatSupport(DeviceInfo device, string format)
        {
            string value = device.GetCapability(format);
            if (bool.TrueString.Equals(value, StringComparison.InvariantCultureIgnoreCase) ||
                bool.FalseString.Equals(value, StringComparison.InvariantCultureIgnoreCase))
                return value;
            return bool.FalseString.ToLowerInvariant(); ;
        }

        private static string GetW3CDOMVersion(DeviceInfo device, string current)
        {
            int level = 0;
            Version version = new Version(0, 0);
            if (int.TryParse(device.GetCapability("xhtml_support_level"), out level) == true &&
                level >= 4)
                    version = new Version("1.0.0.0");
            else
            {
                try
                {
                    version = new Version(current);
                }
                catch (ArgumentNullException)
                {
                    // To nothing and let the default value be returned.
                }
            }
            return version.ToString();
        }

        private static string GetCookieSupport(DeviceInfo device, string current)
        {
            bool value = false;
            // Return either the capability or the current value as a boolean string.
            if (bool.TryParse(device.GetCapability("cookie_support"), out value) == false)
                bool.TryParse(current, out value);
            return value.ToString();
        }
        
        /// <summary>
        /// If the current capability already indicates javascript is supported and the
        /// wurfl database indicates javascript support then return true.
        /// </summary>
        /// <param name="device">The device to be checked.</param>
        /// <param name="current">The current setting from the headers.</param>
        /// <returns>True if javascript is supported.</returns>
        private static bool GetJavascriptSupport(DeviceInfo device, string current)
        {
            bool java = false;
            if (bool.TryParse(current, out java) == true && 
                java == true &&
                bool.TryParse(device.GetCapability("ajax_support_javascript"), out java) == true &&
                java == true)
            {
                return true;
            }
            return false;
        }

        private static string GetCssManipulationSupport(DeviceInfo device)
        {
            bool cssManipulation = false;
            if (bool.TryParse(device.GetCapability("ajax_manipulate_css"), out cssManipulation) == true &&
                cssManipulation == true)
            {
                return bool.TrueString.ToLowerInvariant();
            }
            return bool.FalseString.ToLowerInvariant();
        }

        private static MatchCollection SegmentVersionString(string value)
        {
            if (value != null)
                return Regex.Matches(value, @"\d+");
            return null;
        }

        private static string GetMajorVersion(string value)
        {
            MatchCollection components = SegmentVersionString(value);
            if (components != null && components.Count >= 1)
            {
                return components[0].Value;
            }
            return null;
        }

        private static string GetMinorVersion(string value)
        {
            MatchCollection components = SegmentVersionString(value);
            if (components != null && components.Count > 1)
            {
                StringBuilder version = new StringBuilder();
                for(int i = 1; i < components.Count; i++)
                {
                    version.Append(components[i].Value);
                    if (i < (components.Count - 1))
                        version.Append(".");
                }
                return version.ToString();
            }
            return null;
        }

        private static string GetMajorVersion(DeviceInfo device, string current)
        {
            decimal version = 0;
            if (decimal.TryParse(GetMajorVersion(device.GetCapability("mobile_browser_version")), out version) == false)
                decimal.TryParse(current, out version);
            return version.ToString();
        }

        private static string GetMinorVersion(DeviceInfo device, string current)
        {
            decimal version = 0;
            if (decimal.TryParse(GetMinorVersion(device.GetCapability("mobile_browser_version")), out version) == false)
                decimal.TryParse(current, out version);
            return version.ToString(".0");
        }

        private static string GetPointingMethod(DeviceInfo device)
        {
            string value = device.GetCapability("pointing_method");
            if (PointingMethods.Clickwheel.ToString().Equals(value, StringComparison.InvariantCultureIgnoreCase))
                return PointingMethods.Clickwheel.ToString();
            if (PointingMethods.Joystick.ToString().Equals(value, StringComparison.InvariantCultureIgnoreCase))
                return PointingMethods.Joystick.ToString();
            if (PointingMethods.Stylus.ToString().Equals(value, StringComparison.InvariantCultureIgnoreCase))
                return PointingMethods.Stylus.ToString();
            if (PointingMethods.Touchscreen.ToString().Equals(value, StringComparison.InvariantCultureIgnoreCase))
                return PointingMethods.Touchscreen.ToString();
            return PointingMethods.Unknown.ToString();
        }

        private static string GetCssSupport(string renderingType)
        {
            if (renderingType.Contains("html") == true)
            {
                return bool.TrueString.ToLowerInvariant(); ;
            }
            return bool.FalseString.ToLowerInvariant(); ;
        }

        private static string GetPlatform(DeviceInfo device)
        {
            return device.GetCapability("device_os");
        }

        private static string GetIsCrawler(DeviceInfo device)
        {
            if (device.DeviceId == "generic_web_crawler" || device.DeviceId.Contains("crawler"))
                return bool.TrueString.ToLowerInvariant(); ;
            if (device.FallbackDevice != null)
                return GetIsCrawler(device.FallbackDevice);
            return bool.FalseString.ToLowerInvariant(); ;
        }

        private static string GetIsMobileDevice(DeviceInfo device)
        {
            bool value = false;
            if (bool.TryParse(device.GetCapability("is_wireless_device"), out value) == true &&
                value == true)
                return bool.TrueString.ToLowerInvariant(); ;
            return bool.FalseString.ToLowerInvariant(); ;
        }

        private static string GetScreenPixelsHeight(DeviceInfo device)
        {
            return device.GetCapability("resolution_height");
        }

        private static string GetScreenPixelsWidth(DeviceInfo device)
        {
            return device.GetCapability("resolution_width");
        }

        private static string GetIsColor(DeviceInfo device)
        {
            long numberOfColors = 0;
            device.GetCapability("colors", out numberOfColors);
            bool isColor = (numberOfColors >= 256);
            return isColor.ToString(CultureInfo.InvariantCulture);
        }

        private static string GetHasBackButton(DeviceInfo device)
        {
            return device.GetCapability("built_in_back_button_support");
        }

        private static string GetTables(DeviceInfo device, string renderType)
        {
            bool result = false;
            string value = null;
            if (renderType.Contains("chtml"))
                value = device.GetCapability("chtml_table_support");
            else if (renderType.Contains("xhtml"))
                value = device.GetCapability("xhtml_table_support");
            else if (renderType.Contains("wml"))
                value = device.GetCapability("table_support");
            else if (renderType.Contains("html"))
                value = bool.TrueString;
            else
                value = bool.FalseString;
            if (bool.TryParse(value, out result) == true &&
                result == true)
                return bool.TrueString.ToLowerInvariant(); ;
            return bool.FalseString.ToLowerInvariant(); ;
        }

        private static string GetSupportsAccesskeyAttribute(DeviceInfo device)
        {
            return device.GetCapability("access_key_support");
        }

        private static string GetMobileDeviceModel(DeviceInfo device)
        {
            string value = device.GetCapability("marketing_name");
            if (String.IsNullOrEmpty(value) == true)
                value = device.GetCapability("model_name");
            return value;
        }

        private static string GetMobileDeviceManufacturer(DeviceInfo device)
        {
            return device.GetCapability("brand_name");
        }
        
        private static string GetPreferredImageMime(DeviceInfo device, IDictionary capabilities)
        {
            // Look at the database and return the 1st one that matches in order
            // of preference.
            if (bool.TrueString.Equals(device.GetCapability("png"), StringComparison.InvariantCultureIgnoreCase))
                return "image/png";
            if (bool.TrueString.Equals(device.GetCapability("jpg"), StringComparison.InvariantCultureIgnoreCase))
                return "image/jpeg";
            if (bool.TrueString.Equals(device.GetCapability("gif"), StringComparison.InvariantCultureIgnoreCase))
                return "image/gif";
            return null;
        }

        private static string GetScreenBitDepth(DeviceInfo device)
        {
            long fScreenBitDepth = 1;
            long numberOfColors = 256;
            device.GetCapability("colors", out numberOfColors);
            fScreenBitDepth = Image.Support.GetBitsPerPixel(numberOfColors);
            return fScreenBitDepth.ToString(CultureInfo.InvariantCulture);
        }

        private static string GetPhysicalScreenWidth(DeviceInfo deviceInfo)
        {
            int physicalScreenWidth = 0;
            deviceInfo.GetCapability("physical_screen_width", out physicalScreenWidth);
            return physicalScreenWidth.ToString();
        }

        private static string GetPhysicalScreenHeight(DeviceInfo deviceInfo)
        {
            int physicalScreenHeight = 0;
            deviceInfo.GetCapability("physical_screen_height", out physicalScreenHeight);
            return physicalScreenHeight.ToString();
        }

        private static string GetMaxImageWidth(DeviceInfo deviceInfo)
        {
            int physicalScreenWidth = 0;
            deviceInfo.GetCapability("max_image_width", out physicalScreenWidth);
            return physicalScreenWidth.ToString();
        }

        private static string GetMaxImageHeight(DeviceInfo deviceInfo)
        {
            int physicalScreenHeight = 0;
            deviceInfo.GetCapability("max_image_height", out physicalScreenHeight);
            return physicalScreenHeight.ToString();
        }

        private static bool GetMetaViewPortSupported(DeviceInfo deviceInfo)
        {
            bool retValue = false;
            deviceInfo.GetCapability("viewport_supported", out retValue);
            return retValue;
        }

        private static bool GetMetaMobileOptimizedSupported(DeviceInfo deviceInfo)
        {
            bool retValue = false;
            deviceInfo.GetCapability("mobileoptimized", out retValue);
            return retValue;
        }

        private static bool GetMetaHandHeldFriendlySupported(DeviceInfo deviceInfo)
        {
            bool retValue = false;
            deviceInfo.GetCapability("handheldfriendly", out retValue);
            return retValue;
        }

        private static bool GetCssSupportsWidthAsPercentage(DeviceInfo deviceInfo)
        {
            bool retValue = false;
            deviceInfo.GetCapability("css_supports_width_as_percentage", out retValue);
            return retValue;
        }

        private static bool GetCssSpriting(DeviceInfo deviceInfo)
        {
            bool retValue = false;
            deviceInfo.GetCapability("css_spriting", out retValue);
            return retValue;
        }

        private static string GetCssSupportGroup(DeviceInfo device, string capabilityName)
        {
            string value = device.GetCapability(capabilityName);
            if (CssGroup.CSS3.ToString().Equals(value, StringComparison.InvariantCultureIgnoreCase))
                return CssGroup.CSS3.ToString();
            if (CssGroup.Mozilla.ToString().Equals(value, StringComparison.InvariantCultureIgnoreCase))
                return CssGroup.Mozilla.ToString();
            if (CssGroup.Opera.ToString().Equals(value, StringComparison.InvariantCultureIgnoreCase))
                return CssGroup.Opera.ToString();
            if (CssGroup.WebKit.ToString().Equals(value, StringComparison.InvariantCultureIgnoreCase))
                return CssGroup.WebKit.ToString();
            return CssGroup.None.ToString();
        }

        private static string GetPreferredRenderingMimeFromWURFL(DeviceInfo device, string renderingType)
        {
            switch (renderingType)
            {
                case "xhtml-mp":
                case "xhtml-basic":
                    return device.GetCapability("xhtmlmp_preferred_mime_type");

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
            if (device.GetCapability("html_wi_oma_xhtmlmp_1_0", out value) && value == true)
                return "xhtml-mp";
            else if (device.GetCapability("html_wi_w3_xhtmlbasic", out value) && value == true)
                return "xhtml-basic";
            else if (device.GetCapability("html_wi_imode_compact_generic", out value) && value == true)
                return MobileCapabilities.PreferredRenderingTypeChtml10;
            else if (device.GetCapability("html_web_4_0", out value) && value == true)
                return MobileCapabilities.PreferredRenderingTypeHtml32;
            else if (device.GetCapability("html_web_3_2", out value) && value == true)
                return MobileCapabilities.PreferredRenderingTypeHtml32;
            else
                return null;
        }

        private static string GetPreferredRenderingTypeFromWURFL(DeviceInfo device)
        {
            switch (device.GetCapability("preferred_markup"))
            {
                case "html_web_3_2": return MobileCapabilities.PreferredRenderingTypeHtml32;
                case "html_web_4_0": return "html4"; 
                case "html_wi_oma_xhtmlmp_1_0": return "xhtml-mp"; 
                case "html_wi_w3_xhtmlbasic": return "xhtml-basic"; 
                case "html_wi_imode_htmlx_1":
                case "html_wi_imode_html_1":
                case "html_wi_imode_html_2":
                case "html_wi_imode_html_3":
                case "html_wi_imode_html_4":
                case "html_wi_imode_html_5":
                case "html_wi_imode_html_6":
                case "html_wi_imode_htmlx_1_1":
                case "html_wi_imode_compact_generic": return MobileCapabilities.PreferredRenderingTypeChtml10; 
                default: return GetFirstRenderingTypeFromWURFL(device); 
            }
        }

        #endregion
    }
}