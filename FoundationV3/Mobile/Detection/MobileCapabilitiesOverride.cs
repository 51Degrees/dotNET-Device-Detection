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

#region Usings

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using FiftyOne.Foundation.Mobile.Detection.Entities;
using System.Web;

#if VER4

using System.Linq;

#endif

#endregion

namespace FiftyOne.Foundation.Mobile.Detection
{
    /// <summary>
    /// Enhanced mobile capabilities assigned to mobile devices.
    /// </summary>
    internal static class MobileCapabilitiesOverride
    {
        #region Internal Create Methods

        /// <summary>
        /// Creates a dictionary of capabilites for the requesting device based on both the 
        /// current capabilities assigned by Microsoft, and the results from 51Degrees.
        /// </summary>
        /// <param name="results">The detection results.</param>
        /// <param name="currentCapabilities">The current capabilities assigned by .NET.</param>
        /// <returns>A dictionary of capabilities for the request.</returns>
        internal static IDictionary EnhancedCapabilities(SortedList<string, string[]> results, HttpBrowserCapabilities currentCapabilities)
        {
            // Use the base class to create the initial list of capabilities.
            IDictionary capabilities = new Hashtable();

            // Add the capabilities for the device.
            EnhancedCapabilities(results, capabilities, currentCapabilities);

            // Initialise any capability values that rely on the settings
            // from the device data source.
            Init(capabilities);

            return capabilities;
        }
        
        #endregion
        
        #region Private Methods

        /// <summary>
        /// Initialises the IDictionary of capabilities.
        /// </summary>
        private static void Init(IDictionary capabilities)
        {
            // Set the tagwriter.
            capabilities["tagwriter"] = GetTagWriter(capabilities);
        }

        private static void EnhancedCapabilities(
            SortedList<string, string[]> results, 
            IDictionary properties, 
            HttpBrowserCapabilities currentProperties)
        {
            // Enhance with the capabilities from the device data.
            if (results != null)
            {
                // Enhance the default capabilities collection based on the device.
                Enhance(properties, currentProperties, results);

                // Add the 51Degrees.mobi device properties to the collection.
                properties.Add(Constants.FiftyOneDegreesProperties, results);

                // If an adapters patch file has been loaded then include this
                // capability in the exposed list of capabilities.
                string adapters = GetAdapters(results);
                if (String.IsNullOrEmpty(adapters) == false)
                    SetValue(properties, "adapters", adapters);
            }
        }

        /// <summary>
        /// Returns the value as a string.
        /// </summary>
        /// <param name="results"></param>
        /// <param name="property"></param>
        /// <returns>The string value, or null if the property does not exit.</returns>
        private static string GetString(this SortedList<string, string[]> results, string property)
        {
            string[] value = null;
            results.TryGetValue(property, out value);
            if (value != null)
                return String.Join(Constants.ValueSeperator, value);
            return null;
        }

        /// <summary>
        /// Returns the value as a boolean.
        /// </summary>
        /// <param name="results"></param>
        /// <param name="property"></param>
        /// <returns>The boolean value, or null if the value is not a boolean type.</returns>
        private static bool? GetBool(this SortedList<string, string[]> results, string property)
        {
            bool value = false;
            if (bool.TryParse(results.GetString(property), out value))
                return value;
            return null;
        }

        /// <summary>
        /// Returns the value as a double.
        /// </summary>
        /// <param name="results"></param>
        /// <param name="property"></param>
        /// <returns>The double value, or null if the value is not a numeric type.</returns>
        private static double? GetDouble(this SortedList<string, string[]> results, string property)
        {
            double value = 0;
            if (double.TryParse(results.GetString(property), out value))
                return value;
            return null;
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
        /// <param name="results">The match result to use for the enhancement.</param>
        private static void Enhance(
            IDictionary capabilities,
            HttpBrowserCapabilities currentCapabilities,
            SortedList<string, string[]> results)
        {
            // Set base capabilities for all mobile devices.
            SetStaticValues(capabilities);

            SetValue(capabilities, "isMobileDevice", GetIsMobileDevice(results));
            SetValue(capabilities, "crawler", GetIsCrawler(results));
            SetValue(capabilities, "mobileDeviceModel", GetMobileDeviceModel(results));
            SetValue(capabilities, "mobileDeviceManufacturer", GetMobileDeviceManufacturer(results));
            SetValue(capabilities, "platform", GetPlatform(results));
            // property enhancement can be removed with this compiler flag
#if !REMOVE_OVERRIDE_BROWSER
            SetValue(capabilities, "browser", GetBrowser(results));
#endif
            SetValue(capabilities, "type", capabilities["mobileDeviceManufacturer"]);
            SetValue(capabilities, "screenPixelsHeight", GetScreenPixelsHeight(results) ??
                GetDefaultValue("screenPixelsHeight", currentCapabilities));
            SetValue(capabilities, "screenPixelsWidth", GetScreenPixelsWidth(results) ??
                GetDefaultValue("screenPixelsWidth", currentCapabilities));
            SetValue(capabilities, "screenBitDepth", GetBitsPerPixel(results));
            SetValue(capabilities, "preferredImageMime", GetPreferredImageMime(results));
            SetValue(capabilities, "isColor", GetIsColor(results));
            SetValue(capabilities, "supportsCallback", GetSupportsCallback(results));
            SetValue(capabilities, "SupportsCallback", GetSupportsCallback(results));
            SetValue(capabilities, "canInitiateVoiceCall", GetIsMobileDevice(results));
            SetValue(capabilities, "jscriptversion", GetJavascriptVersion(results));

            // The following values are set to prevent exceptions being thrown in
            // the standard .NET base classes if the property is accessed.
            SetValue(capabilities, "screenCharactersHeight", 
                GetDefaultValue("screenCharactersHeight", currentCapabilities));
            SetValue(capabilities, "screenCharactersWidth",
                GetDefaultValue("screenCharactersWidth", currentCapabilities));

            // Use the Version class to find the version. If this fails use the 1st two
            // decimal segments of the string.
            string versionValue = results.GetString("BrowserVersion");
            if (versionValue != null)
            {
#if VER4
                Version version = null;
                if (Version.TryParse(versionValue, out version))
                {
                    SetVersion(capabilities, version);
                }
                else
                {
                    SetVersion(capabilities, versionValue.ToString());
                }
#else
                try
                {
                    SetVersion(capabilities, new Version(versionValue));
                }
                catch (FormatException)
                {
                    SetVersion(capabilities, versionValue.ToString());
                }
                catch (ArgumentException)
                {
                    SetVersion(capabilities, versionValue.ToString());
                }
#endif
            }
            else
            {
                // Transfer the current version capabilities to the new capabilities.
                SetValue(capabilities, "majorversion", currentCapabilities != null ? currentCapabilities["majorversion"] : null);
                SetValue(capabilities, "minorversion", currentCapabilities != null ? currentCapabilities["minorversion"] : null);
                SetValue(capabilities, "version", currentCapabilities != null ? currentCapabilities["version"] : null);

                // Ensure the version values are not null to prevent null arguement exceptions
                // with some controls.
                var versionString = currentCapabilities != null ? currentCapabilities["version"] as string : "0.0";
                SetVersion(capabilities, versionString);
            }

            // All we can determine from the device database is if javascript is supported as a boolean.
            // If the value is not provided then null is returned and the capabilities won't be altered.
            var javaScript = GetJavascriptSupport(results);
            if (javaScript.HasValue)
            {
                SetJavaScript(capabilities, javaScript.Value);
                SetValue(capabilities, "ecmascriptversion",
                         (bool)javaScript ? "3.0" : "0.0");
            }

            // Sets the W3C DOM version.
            SetValue(capabilities, "w3cdomversion",
                GetW3CDOMVersion(results,
                    currentCapabilities != null
                        ? (string)currentCapabilities["w3cdomversion"]
                        : String.Empty));

            // Update the cookies value if we have additional information.
            SetValue(capabilities, "cookies",
                    GetCookieSupport(results,
                                     currentCapabilities != null
                                         ? (string)currentCapabilities["cookies"]
                                         : String.Empty));

            // Only set these values from 51Degrees.mobi if they've not already been set from
            // the Http request header, or the .NET solution.
            if (capabilities.Contains("preferredRenderingType") == false)
            {
                // Set the rendering type for the response.
                SetValue(capabilities, "preferredRenderingType", GetPreferredHtmlVersion(results));

                // Set the Mime type of the response.
                SetValue(capabilities, "preferredRenderingMime", "text/html");
            }
        }

        /// <summary>
        /// Returns true if the device supports tables.
        /// </summary>
        /// <param name="results">The match result for the current request.</param>
        /// <returns></returns>
        private static string GetTablesCapable(SortedList<string, string[]> results)
        {
            return results.GetString("TablesCapable");
        }

        private static string GetPreferredHtmlVersion(SortedList<string, string[]> results)
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

        /// <summary>
        /// Sets the version using the version object provided.
        /// </summary>
        /// <param name="capabilities"></param>
        /// <param name="version"></param>
        private static void SetVersion(IDictionary capabilities, Version version)
        {
            SetValue(capabilities, "majorversion", version.Major.ToString());
            SetValue(capabilities, "minorversion", String.Format(".{0}", version.Minor));
            SetValue(capabilities, "version", version.ToString());
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

        private static string GetCookieSupport(SortedList<string, string[]> results, string current)
        {
            bool? value = results.GetBool("CookiesCapable");
            if (value.HasValue)
                return value.Value.ToString();
            return true.ToString();
        }

        /// <summary>
        /// Returns true if the device supports callbacks from the browser.
        /// </summary>
        /// <param name="results">The match result for the current request.</param>
        /// <returns>True if callback is supported.</returns>
        private static string GetSupportsCallback(SortedList<string, string[]> results)
        {
            var value = results.GetString("AjaxRequestType");
            if (value != null && value == "AjaxRequestTypeNotSupported")
                return false.ToString();
            return true.ToString();
        }

        /// <summary>
        /// Returns version 1.0 if DOM is supported based on Ajax
        /// being supported, otherwise returns false.
        /// </summary>
        /// <param name="results">The match result for the current request.</param>
        /// <param name="current">The current value of the property.</param>
        /// <returns>1.0, 0.0 or the current value.</returns>
        private static string GetW3CDOMVersion(SortedList<string, string[]> results, string current)
        {
            Version version = new Version(0, 0);

            // Set the version to the current version.
            try
            {
                version = new Version(current);
            }
            catch (ArgumentException)
            {
                // Do nothing and let the default value be returned.
            }

            // Try and set version 1.0 if ajax is supported.
            var value = results.GetString("AjaxRequestType");
            if (value != null && value == "AjaxRequestTypeNotSupported")
                version = new Version("2.0.0.0");

            return version.ToString(2);
        }

        /// <summary>
        /// If the device indicates javascript support then return true.
        /// </summary>
        /// <param name="results">The match result for the current request.</param>
        /// <returns>True if javascript is supported.</returns>
        private static bool? GetJavascriptSupport(SortedList<string, string[]> results)
        {
            var value = results.GetBool("Javascript");
            if (value.HasValue)
                return value.Value;
            return null;
        }

        /// <summary>
        /// Get the javascript version or null if not provided or invalid.
        /// </summary>
        /// <param name="results">The match result for the current request.</param>
        /// <returns></returns>
        private static string GetJavascriptVersion(SortedList<string, string[]> results)
        {
            var value = results.GetString("JavascriptVersion");
            if (value == null)
                return null;

            // Check if the version value is valid in the version
            // class. If not then return null.
#if VER4
            Version version;
            if (Version.TryParse(value, out version))
                return value.ToString();
            return null;
#else
            try
            {
                new Version(value);
                return value;
            }
            catch
            {
                return null;
            }
#endif
        }

        private static string GetPlatform(SortedList<string, string[]> results)
        {
            return results.GetString("PlatformName");
        }

        private static string GetBrowser(SortedList<string, string[]> results)
        {
            return results.GetString("BrowserName");
        }

        /// <summary>
        /// If the data set does not contain the IsCrawler property null is returned.
        /// If it is present and contains the value true or false then a value
        /// is returned.
        /// </summary>
        /// <param name="results">The match result for the current request.</param>
        /// <returns></returns>
        private static string GetIsCrawler(SortedList<string, string[]> results)
        {
            var value = results.GetBool("IsCrawler");
            if (value.HasValue)
            {
                return value.Value.ToString();
            }
            return null;
        }

        private static string GetAdapters(SortedList<string, string[]> results)
        {
            var value = results.GetString("Adapters");
            if (value != null)
                return value;
            return null;
        }

        private static string GetIsMobileDevice(SortedList<string, string[]> results)
        {
            var value = results.GetBool("IsMobile");
            if (value.HasValue)
            {
                return value.Value.ToString();
            }
            return false.ToString();
        }

        private static string GetScreenPixelsHeight(SortedList<string, string[]> results)
        {
            var value = results.GetDouble("ScreenPixelsHeight");
            if (value.HasValue)
            {
                return value.Value.ToString();
            }
            return null;
        }

        private static string GetScreenPixelsWidth(SortedList<string, string[]> results)
        {
            var value = results.GetDouble("ScreenPixelsWidth");
            if (value.HasValue)
            {
                return value.Value.ToString();
            }
            return null;
        }

        private static string GetIsColor(SortedList<string, string[]> results)
        {
            long bitsPerPixel = GetBitsPerPixel(results);
            if (bitsPerPixel >= 4)
                return bool.TrueString.ToLowerInvariant();
            return bool.FalseString.ToLowerInvariant();
        }

        private static string GetMobileDeviceModel(SortedList<string, string[]> results)
        {
            var value = results.GetString("HardwareModel");
            if (value == null)
                value = results.GetString("HardwareName");
            return value;
        }

        private static string GetMobileDeviceManufacturer(SortedList<string, string[]> results)
        {
            return results.GetString("HardwareVendor");
        }

        private static string GetPreferredImageMime(SortedList<string, string[]> results)
        {
            var values = results.GetString("CcppAccept");
            if (values != null)
            {
                var mimeTypes = values.Split(
                    new string[] { Constants.ValueSeperator },
                    StringSplitOptions.RemoveEmptyEntries);

                // Look at the database and return the 1st one that matches in order
                // of preference.
                if (Contains(mimeTypes, "ImagePng"))
                    return "image/png";
                if (Contains(mimeTypes, "ImageJpeg"))
                    return "image/jpeg";
                if (Contains(mimeTypes, "ImageGif"))
                    return "image/gif";
            }
            return null;
        }

        /// <summary>
        /// Compares two lists to see if they contain at least one value that is the same.
        /// </summary>
        /// <param name="list1"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private static bool Contains(IList<string> list1, string value)
        {
            if (list1 == null || value == null)
                return false;

            foreach (string a in list1)
                if (a == value)
                    return true;
            return false;
        }

        /// <summary>
        /// Returns the number of bits per pixel as a long, or 16 if not found.
        /// </summary>
        /// <param name="results">The match result for the current request.</param>
        /// <returns></returns>
        private static long GetBitsPerPixel(SortedList<string, string[]> results)
        {
            var value = results.GetDouble("BitsPerPixel");
            if (value.HasValue)
            {
                return (long)value.Value;
            }
            return 16;
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Returns the current capabilities value if it exists, otherwise
        /// uses the provided default values.
        /// </summary>
        /// <param name="key">The property key to be returned.</param>
        /// <param name="currentCapabilities">The current capabilities found by .NET.</param>
        /// <returns>A default value.</returns>
        private static string GetDefaultValue(string key, HttpBrowserCapabilities currentCapabilities)
        {
            string currentValue = currentCapabilities[key] as string;
            if (currentValue != null)
                return currentValue;
            return GetDefaultValue(key);
        }

        /// <summary>
        /// Returns the default value for the key to use if one can not be 
        /// found.
        /// </summary>
        /// <param name="key">The property key to be returned.</param>
        /// <returns>The hardcoded default value.</returns>
        private static string GetDefaultValue(string key)
        {
            for (int i = 0; i < Constants.DefaultPropertyValues.Length; i++)
                if (Constants.DefaultPropertyValues[i, 0] == key)
                    return Constants.DefaultPropertyValues[i, 1];
            return null;
        }

        /// <summary>
        /// Sets the javascript boolean string in the capabilities dictionary.
        /// </summary>
        /// <param name="capabilities">Capabilities dictionary.</param>
        /// <param name="javaScript">The value of the jaavscript keys.</param>
        private static void SetJavaScript(IDictionary capabilities, bool javaScript)
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