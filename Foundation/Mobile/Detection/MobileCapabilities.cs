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
 * Mobile Experts Limited are Copyright (C) 2009 - 2011. All Rights Reserved.
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
using System.Web;
using System.Collections.Generic;
using System.Text.RegularExpressions;

#endregion

namespace FiftyOne.Foundation.Mobile.Detection
{
    /// <summary>
    /// Enhanced mobile capabilities assigned to mobile devices.
    /// </summary>
    internal class MobileCapabilities
    {
        #region String Index Values

        // Gets the indexes of all the key capability strings as static readonly
        // values during static construction to avoid needing to look them up every time.
        private readonly int AjaxRequestType;
        private readonly int AjaxRequestTypeNotSupported;
        private readonly int Javascript;
        private readonly int JavascriptVersion;
        private readonly int CookiesCapable;
        private readonly int BrowserVersion;
        private readonly int PlatformName;
        private readonly int Adapters;
        private readonly int ScreenPixelsHeight;
        private readonly int ScreenPixelsWidth;
        private readonly int BitsPerPixel;
        private readonly int HardwareName;
        private readonly int HardwareModel;
        private readonly int HardwareVendor;
        private readonly int HtmlVersion;
        private readonly int IsMobile;
        private readonly int True;
        private readonly int False;
        private readonly int IsCrawler;
        private readonly int CcppAccept;
        private readonly int[] ImagePng;
        private readonly int[] ImageJpeg;
        private readonly int[] ImageGif;
        private readonly int XHtmlVersion;
        private readonly int TablesCapable;

        #endregion
        
        #region Fields

        /// <summary>
        /// Instance of the provider to use with this class.
        /// </summary>
        private Provider _provider = null;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a new instance of <see cref="MobileCapabilities"/>.
        /// </summary>
        /// <param name="provider">Data provider to use with this capabilities provider.</param>
        internal MobileCapabilities(Provider provider)
        {
            _provider = provider;
            True = _provider.Strings.Add("True");
            False = _provider.Strings.Add("False");
            AjaxRequestType = _provider.Strings.Add("AjaxRequestType");
            AjaxRequestTypeNotSupported = _provider.Strings.Add("AjaxRequestTypeNotSupported");
            Javascript = _provider.Strings.Add("Javascript");
            JavascriptVersion = _provider.Strings.Add("JavascriptVersion");
            CookiesCapable = _provider.Strings.Add("CookiesCapable");
            BrowserVersion = _provider.Strings.Add("BrowserVersion");
            PlatformName = _provider.Strings.Add("PlatformName");
            Adapters = _provider.Strings.Add("Adapters");
            ScreenPixelsHeight = _provider.Strings.Add("ScreenPixelsHeight");
            ScreenPixelsWidth = _provider.Strings.Add("ScreenPixelsWidth");
            BitsPerPixel = _provider.Strings.Add("BitsPerPixel");
            HardwareName = _provider.Strings.Add("HardwareName");
            HardwareModel = _provider.Strings.Add("HardwareModel");
            HardwareVendor = _provider.Strings.Add("HardwareVendor");
            HtmlVersion = _provider.Strings.Add("HtmlVersion");
            XHtmlVersion = _provider.Strings.Add("XHtmlVersion");
            IsMobile = _provider.Strings.Add("IsMobile");
            IsCrawler = _provider.Strings.Add("IsCrawler");
            TablesCapable = _provider.Strings.Add("TablesCapable");
            CcppAccept = _provider.Strings.Add("CcppAccept");
            ImageGif = new[] { 
                _provider.Strings.Add("image/gif") };
            ImagePng = new[] { 
                _provider.Strings.Add("image/png") };
            ImageJpeg = new[] { 
                _provider.Strings.Add("image/jpeg"),
                _provider.Strings.Add("image/jpg") };
        }

        #endregion

        #region Create Methods

        /// <summary>
        /// Creates a dictionary of capabilites for the requesting device.
        /// </summary>
        /// <param name="request">Details of the request.</param>
        /// <param name="currentCapabilities">The current capabilities assigned by .NET.</param>
        /// <returns>A dictionary of capabilities for the request.</returns>
        internal IDictionary Create(HttpRequest request, IDictionary currentCapabilities)
        {
            // Use the base class to create the initial list of capabilities.
            IDictionary capabilities = new Hashtable();

            // Use the device for the request to set the capabilities.
            Create(_provider.GetDeviceInfo(request.Headers), capabilities, currentCapabilities);

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
        internal IDictionary Create(string userAgent)
        {
            // Create the mobile capabilities hashtable.
            IDictionary capabilities = new Hashtable();

            // As we can't tell from the headers if javascript is supported assume
            // it can be and that the inheriting class will provide the additional details.
            SetJavaScript(capabilities, true);

            // Use the device for the request to set the capabilities.
            Create((BaseDeviceInfo)_provider.GetDeviceInfo(userAgent), capabilities, null);

            // Initialise any capability values that rely on the settings
            // from the device data source.
            Init(capabilities);

            return capabilities;
        }

        #endregion

        #region Virtual Methods

        /// <summary>
        /// Initialises the IDictionary of capabilities.
        /// </summary>
        protected virtual void Init(IDictionary capabilities)
        {
            // Set the tagwriter.
            capabilities["tagwriter"] = GetTagWriter(capabilities);
        }

        #endregion

        #region Private Methods

        private void Create(BaseDeviceInfo device, IDictionary properties, IDictionary currentProperties)
        {
            // Enhance with the capabilities from the device data.
            if (device != null)
            {
                // Enhance the default capabilities collection based on the device.
                Enhance(properties, currentProperties, device);

                // Add the 51Degrees.mobi device properties to the collection.
                properties.Add(Constants.FiftyOneDegreesProperties, device.GetAllProperties());

                // If an adapters patch file has been loaded then include this
                // capability in the exposed list of capabilities.
                string adapters = GetAdapters(device);
                if (String.IsNullOrEmpty(adapters) == false)
                    SetValue(properties, "adapters", adapters);
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
        /// Updates the capabilities used by Microsoft's implementation of the
        /// HttpBrowserCapabilities class to control the property values it
        /// returns. Only properties exposed by FiftyOneBrowserCapabilities are overriden
        /// by this method.
        /// </summary>
        /// <param name="capabilities">Dictionary of capabilities to be enhanced.</param>
        /// <param name="currentCapabilities">Dictionary of existing capabilities for the device.</param>
        /// <param name="device">Device to use for enhancements.</param>
        private void Enhance(IDictionary capabilities, IDictionary currentCapabilities, BaseDeviceInfo device)
        {
            // Set base capabilities for all mobile devices.
            SetStaticValues(capabilities);

            SetValue(capabilities, "isMobileDevice", GetIsMobileDevice(device));
            SetValue(capabilities, "crawler", GetIsCrawler(device));
            SetValue(capabilities, "mobileDeviceModel", GetMobileDeviceModel(device));
            SetValue(capabilities, "mobileDeviceManufacturer", GetMobileDeviceManufacturer(device));
            SetValue(capabilities, "platform", GetPlatform(device));
            SetValue(capabilities, "type", capabilities["mobileDeviceManufacturer"]);
            SetValue(capabilities, "screenPixelsHeight", GetScreenPixelsHeight(device));
            SetValue(capabilities, "screenPixelsWidth", GetScreenPixelsWidth(device));
            SetValue(capabilities, "screenBitDepth", GetBitsPerPixel(device));
            SetValue(capabilities, "preferredImageMime", GetPreferredImageMime(device, capabilities));
            SetValue(capabilities, "isColor", GetIsColor(device));
            SetValue(capabilities, "SupportsCallback", GetSupportsCallback(device));
            SetValue(capabilities, "canInitiateVoiceCall", GetIsMobileDevice(device));
            SetValue(capabilities, "javascript", GetJavascriptSupport(device));
            SetValue(capabilities, "jscriptversion", GetJavascriptVersion(device));
            SetValue(capabilities, "tables", GetTablesCapable(device));

            // Use the Version class to find the version. If this fails use the 1st two
            // decimal segments of the string.
            string versionString = _provider.Strings.Get(device.GetFirstPropertyValueStringIndex(BrowserVersion));
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

            // All we can determine from the device database is if javascript is supported as a boolean.
            // Use this value to set a ecma version number that is the same for all devices that
            // indicate javascript support.
            bool javaScript = GetJavascriptSupport(device);
            SetJavaScript(capabilities, javaScript);
            SetValue(capabilities, "ecmascriptversion",
                     javaScript ? "3.0" : "0.0");

            // Update the cookies value if we have additional information.
            SetValue(capabilities, "cookies",
                    GetCookieSupport(device,
                                     currentCapabilities != null
                                         ? (string)currentCapabilities["cookies"]
                                         : String.Empty));

            // Only set these values from 51Degrees.mobi if they've not already been set from
            // the Http request header, or the .NET solution.
            if (capabilities.Contains("preferredRenderingType") == false)
            {
                // Set the rendering type for the response.
                SetValue(capabilities, "preferredRenderingType", GetPreferredHtmlVersion(device));

                // Set the Mime type of the response.
                SetValue(capabilities, "preferredRenderingMime", "text/html");
            }
        }

        /// <summary>
        /// Returns true if the device supports tables.
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        private object GetTablesCapable(BaseDeviceInfo device)
        {
            if (device.GetFirstPropertyValueStringIndex(TablesCapable) == this.True)
                return bool.TrueString.ToLowerInvariant();
            return bool.FalseString.ToLowerInvariant(); 
        }

        private string GetPreferredHtmlVersion(BaseDeviceInfo device)
        {
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

        private string GetCookieSupport(BaseDeviceInfo device, string current)
        {
            bool value = false;
            // Return either the capability or the current value as a boolean string.
            if (bool.TryParse(_provider.Strings.Get(device.GetFirstPropertyValueStringIndex(CookiesCapable)), out value) == false)
                bool.TryParse(current, out value);
            return value.ToString();
        }

        /// <summary>
        /// Returns true if the device supports callbacks from the browser.
        /// </summary>
        /// <param name="device">The device to be checked.</param>
        /// <returns>True if callback is supported.</returns>
        private string GetSupportsCallback(BaseDeviceInfo device)
        {
            var values = device.GetPropertyValueStringIndexes(AjaxRequestType);
            if (values != null && values.Contains(AjaxRequestTypeNotSupported))
                return bool.FalseString.ToLowerInvariant();
            return bool.TrueString.ToLowerInvariant();
        }

        /// <summary>
        /// If the device indicates javascript support then return true.
        /// </summary>
        /// <param name="device">The device to be checked.</param>
        /// <returns>True if javascript is supported.</returns>
        private bool GetJavascriptSupport(BaseDeviceInfo device)
        {
            return device.GetFirstPropertyValueStringIndex(Javascript) == this.True;
        }

        private string GetJavascriptVersion(BaseDeviceInfo device)
        {
            return _provider.Strings.Get(device.GetFirstPropertyValueStringIndex(JavascriptVersion));
        }

        private string GetPlatform(BaseDeviceInfo device)
        {
            return _provider.Strings.Get(device.GetFirstPropertyValueStringIndex(PlatformName));
        }

        private string GetIsCrawler(BaseDeviceInfo device)
        {
            if (device.GetFirstPropertyValueStringIndex(IsCrawler) == this.True)
                return bool.TrueString.ToLowerInvariant();
            return bool.FalseString.ToLowerInvariant();
        }

        private string GetAdapters(BaseDeviceInfo device)
        {
            return _provider.Strings.Get(device.GetFirstPropertyValueStringIndex(Adapters));
        }

        private string GetIsMobileDevice(BaseDeviceInfo device)
        {
            if (device.GetFirstPropertyValueStringIndex(IsMobile) == this.True)
                return bool.TrueString.ToLowerInvariant();
            return bool.FalseString.ToLowerInvariant();
        }

        private string GetScreenPixelsHeight(BaseDeviceInfo device)
        {
            return _provider.Strings.Get(device.GetFirstPropertyValueStringIndex(ScreenPixelsHeight));
        }

        private string GetScreenPixelsWidth(BaseDeviceInfo device)
        {
            return _provider.Strings.Get(device.GetFirstPropertyValueStringIndex(ScreenPixelsWidth));
        }

        private string GetIsColor(BaseDeviceInfo device)
        {
            long bitsPerPixel = GetBitsPerPixel(device);
            if (bitsPerPixel >= 256)
                return bool.TrueString.ToLowerInvariant();
            return bool.FalseString.ToLowerInvariant();
        }

        private string GetMobileDeviceModel(BaseDeviceInfo device)
        {
            string value = _provider.Strings.Get(device.GetFirstPropertyValueStringIndex(HardwareModel));
            if (String.IsNullOrEmpty(value))
                value = _provider.Strings.Get(device.GetFirstPropertyValueStringIndex(HardwareName));
            return value;
        }

        private string GetMobileDeviceManufacturer(BaseDeviceInfo device)
        {
            return _provider.Strings.Get(device.GetFirstPropertyValueStringIndex(HardwareVendor));
        }

        private string GetPreferredImageMime(BaseDeviceInfo device, IDictionary capabilities)
        {
            var mimeTypes = device.GetPropertyValueStringIndexes(CcppAccept);
            // Look at the database and return the 1st one that matches in order
            // of preference.
            if (Contains(mimeTypes, ImagePng))
                return "image/png";
            if (Contains(mimeTypes, ImageJpeg))
                return "image/jpeg";
            if (Contains(mimeTypes, ImageGif))
                return "image/gif";
            return null;
        }

        /// <summary>
        /// Compares two lists to see if they contain at least one value that is the same.
        /// </summary>
        /// <param name="list1"></param>
        /// <param name="list2"></param>
        /// <returns></returns>
        private static bool Contains(IList<int> list1, IList<int> list2)
        {
            if (list1 == null || list2 == null)
                return false;

            foreach (int a in list1)
                foreach (int b in list2)
                    if (a == b)
                        return true;
            return false;
        }

        /// <summary>
        /// Returns the number of bits per pixel as a string.
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        private long GetBitsPerPixel(BaseDeviceInfo device)
        {
            long bitsPerPixel = 1;
            long.TryParse(_provider.Strings.Get(device.GetFirstPropertyValueStringIndex(BitsPerPixel)), out bitsPerPixel);
            return bitsPerPixel;
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Sets the javascript boolean string in the capabilities dictionary.
        /// </summary>
        /// <param name="capabilities">Capabilities dictionary.</param>
        /// <param name="javaScript">The value of the jaavscript keys.</param>
        protected static void SetJavaScript(IDictionary capabilities, bool javaScript)
        {
            SetValue(capabilities, "javascript", javaScript.ToString().ToLowerInvariant());
            SetValue(capabilities, "Javascript", javaScript.ToString().ToLowerInvariant());
        }

        /// <summary>
        /// Sets the key in the capabilities dictionary to the object provided. If the key 
        /// already exists the previous value is replaced. If not a new entry is added
        /// to the Dictionary.
        /// </summary>
        /// <param name="capabilities">Dictionary of capabilities to be changed.</param>
        /// <param name="key">Key to be changed or added.</param>
        /// <param name="value">New entry value.</param>
        internal static void SetValue(IDictionary capabilities, string key, object value)
        {
            // Ignore new values that are empty strings.
            if (value == null ||
                String.IsNullOrEmpty(value as string))
                return;

            // Change or add the new capability.
            if (capabilities.Contains(key) == false)
            {
                capabilities.Add(key, value);
            }
            else
            {
                capabilities[key] = value;
            }
        }

        /// <summary>
        /// Returns the class to use as a text writer for the output stream.
        /// </summary>
        /// <param name="capabilities">Dictionary of device capabilities.</param>
        /// <returns>A string containing the text writer class name.</returns>
        private static string GetTagWriter(IDictionary capabilities)
        {
            switch (capabilities["preferredRenderingType"] as string)
            {
                case "xhtml-mp":
                case "xhtml-basic":
                    return "System.Web.UI.XhtmlTextWriter";

                case "chtml10":
                    return "System.Web.UI.ChtmlTextWriter";

                case "html4":
                    return "System.Web.UI.HtmlTextWriter";

                case "html32":
                    return "System.Web.UI.Html32TextWriter";

                default:
                    return "System.Web.UI.Html32TextWriter";
            }
        }

        #endregion
    }
}