/* *********************************************************************
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited. 
 * Copyright © 2014 51Degrees Mobile Experts Limited, 5 Charlotte Close,
 * Caversham, Reading, Berkshire, United Kingdom RG4 7BY
 * 
 * This Source Code Form is the subject of the following patents and patent 
 * applications, owned by 51Degrees Mobile Experts Limited of 5 Charlotte
 * Close, Caversham, Reading, Berkshire, United Kingdom RG4 7BY: 
 * European Patent No. 2871816;
 * European Patent Application No. 17184134.9;
 * United States Patent Nos. 9,332,086 and 9,350,823; and
 * United States Patent Application No. 15/686,066.
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
        /// Sets static capabilities used by mobile controls.
        /// </summary>
        /// <param name="capabilities">Dictionary of capabilities to be changed.</param>
        private static void SetStaticValues(IDictionary capabilities)
        {

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



        #endregion

        #region Static Methods


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