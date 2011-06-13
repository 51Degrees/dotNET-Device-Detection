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
 *     Andy Allan <andy.allan@mobgets.com>
 * 
 * ********************************************************************* */

namespace FiftyOne.Foundation.Mobile.Detection.Wurfl
{
    /// <summary>
    /// Holds all constants and read only strings used throughout this library.
    /// </summary>
    internal static class Constants
    {
        /// <summary>
        /// The attribute used to determine if the device is a root.
        /// </summary>
        internal const string ActualDeviceRoot = "actual_device_root";

        /// <summary>
        /// Node name used to especify a capability inside the WURFL xml file.
        /// </summary>
        internal const string CapabilityNodeName = "capability";

        /// <summary>
        /// Node name used to specify a device inside the WURFL xml file.
        /// </summary>
        internal const string DeviceNodeName = "device";

        /// <summary>
        /// Holds an estimative of how many devices we'll need to store,
        /// use this value when creating collections to store devices.
        /// </summary>
        internal const int EstimateNumberOfDevices = 14000;

        /// <summary>
        /// Node name used to especify a fall back inside the WURFL xml file.
        /// </summary>
        internal const string FallbackAttributeName = "fall_back";

        /// <summary>
        /// Attribute name used to especify an id inside the WURFL xml file.
        /// </summary>
        internal const string IdAttributeName = "id";

        /// <summary>
        /// Attribute name used to especify a name inside the WURFL xml file.
        /// </summary>
        internal const string NameAttributeName = "name";

        /// <summary>
        /// Length of time in ms the new devices thread should wait for a response from the
        /// web server used to record new device information.
        /// </summary>
        internal const int NewUrlTimeOut = 5000;

        /// <summary>
        /// Attribute name used to especify a user agent inside the WURFL xml file.
        /// </summary>
        internal const string UserAgentAttributeName = "user_agent";

        /// <summary>
        /// Attribute name used to especify a value inside the WURFL xml file.
        /// </summary>
        internal const string ValueAttributeName = "value";

        /// <summary>
        /// The name of the key used to retrieve the SortedList object from
        /// the FiftyOneBrowserCapabilities collection.
        /// </summary>
        internal const string WurflCapabilities = "WurflCapabilities";

        /// <summary>
        /// Message used to throw an exception when the wurfl xml file is not found.
        /// </summary>
        internal const string WurflFileNotFound = "The WURFL XML file was not found, check the file path and try again.";

        /// <summary>
        /// Exception message used if the XML can not be created for a new device.
        /// </summary>
        internal const string WurflNewDeviceXmlException = "The device '{0}' could not be written to '{1}'.";

        /// <summary>
        /// Contains the default device ids to be searched for.
        /// </summary>
        internal static readonly string[] DefaultDeviceId = new[] {"generic_web_browser", "generic"};

        /// <summary>
        /// Holds the minimum set of capabilities to be loaded.
        /// These are the capabilities used to override the ASP.Net Browser Capabilities.
        /// </summary>
        internal static readonly string[] DefaultUsedCapabilities = new[]
                                                                        {
                                                                            // These capabilities are always needed.
                                                                            "is_wireless_device",
                                                                            "is_tablet",
                                                                            "resolution_height",
                                                                            "resolution_width",
                                                                            "uaprof",
                                                                            "uaprof2",
                                                                            "uaprof3",
                                                                            "marketing_name",
                                                                            "model_name",
                                                                            "brand_name",
                                                                            "device_os",
                                                                            "access_key_support",
                                                                            "built_in_back_button_support",
                                                                            "colors",
                                                                            "png",
                                                                            "gif",
                                                                            "jpg",
                                                                            "mobile_browser_version",
                                                                            "ajax_support_javascript",
                                                                            "xhtml_support_level",
                                                                            "cookie_support",
                                                                            "preferred_markup",
                                                                            "xhtmlmp_preferred_mime_type",
                                                                            
                                                                            // These capabilities are almost always used and are 
                                                                            // included by default.
                                                                            "physical_screen_width",
                                                                            "physical_screen_height",
                                                                            "max_image_width",
                                                                            "max_image_height",

                                                                            // These capabilities are used internally by 51degrees.
                                                                            "adapters",
                                                                            "adapters_js",
                                                                            "default_image_button_height_mms",
                                                                            "default_image_button_height_pixels",
                                                                            "default_selectable_font_size_points",
                                                                            "xhtml_file_upload"
                                                                        };
    }
}