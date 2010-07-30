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

using System;
using System.Text;

namespace FiftyOne.Foundation.Mobile.Detection.Wurfl
{
    /// <summary>
    /// Holds all constants and read only strings used throughout this library.
    /// </summary>
    internal static class Constants
    {
        /// <summary>
        /// Contains the default device ids to be searched for.
        /// </summary>
        internal readonly static string[] DefaultDeviceId = new string[] { "generic_web_browser", "generic" };

        /// <summary>
        /// Node name used to specify a device inside the WURFL xml file.
        /// </summary>
        internal const string CreatedDate = "Created";

        /// <summary>
        /// Node name used to specify a device inside the WURFL xml file.
        /// </summary>
        internal const string DeviceNodeName = "device";

        /// <summary>
        /// Node name used to especify a capability inside the WURFL xml file.
        /// </summary>
        internal const string CapabilityNodeName = "capability";

        /// <summary>
        /// Attribute name used to especify an id inside the WURFL xml file.
        /// </summary>
        internal const string IdAttributeName = "id";

        /// <summary>
        /// Attribute name used to especify a user agent inside the WURFL xml file.
        /// </summary>
        internal const string UserAgentAttributeName = "user_agent";

        /// <summary>
        /// Attribute name used to especify a name inside the WURFL xml file.
        /// </summary>
        internal const string NameAttributeName = "name";

        /// <summary>
        /// Attribute name used to especify a value inside the WURFL xml file.
        /// </summary>
        internal const string ValueAttributeName = "value";

        /// <summary>
        /// Node name used to especify a fall back inside the WURFL xml file.
        /// </summary>
        internal const string FallbackAttributeName = "fall_back";

        /// <summary>
        /// The attribute used to determine if the device is a root.
        /// </summary>
        internal const string ActualDeviceRoot = "actual_device_root";

        /// <summary>
        /// Message used to throw an exception when the wurfl xml file is not found.
        /// </summary>
        internal const string WurflFileNotFound = "The WURFL XML file was not found, check the file path and try again.";

        /// <summary>
        /// Exception message used if the XML can not be created for a new device.
        /// </summary>
        internal const string WurflNewDeviceXMLException = "The device '{0}' could not be written to '{1}'.";

        /// <summary>
        /// Holds an estimative of how many devices we'll need to store,
        /// use this value when creating collections to store devices.
        /// </summary>
        internal const int EstimateNumberOfDevices = 14000;
 
        /// <summary>
        /// Holds the minimum set of capabilities to be loaded.
        /// These are the capabilities used to override the ASP.Net Browser Capabilities.
        /// </summary>
        internal static readonly string[] DefaultUsedCapabilities = new string[] {
            "is_wireless_device", 
            "max_image_height", 
            "max_image_width", 
            "resolution_height",
            "resolution_width",
            "colors", 
            "cookie_support",
            "built_in_back_button_support", 
            "table_support", 
            "access_key_support", 
            "model_name", 
            "brand_name", 
            "marketing_name",
            "xhtmlmp_preferred_mime_type", 
            "html_wi_oma_xhtmlmp_1_0", 
            "html_wi_w3_xhtmlbasic", 
            "html_web_3_2", 
            "html_wi_imode_compact_generic", 
            "wml_1_2",
            "has_pointing_device",
            "pointing_method",
            "chtml_table_support",
            "xhtml_table_support",
            "device_os",
            "mobile_browser_version",
            "uaprof",
            "uaprof2",
            "uaprof3",
            "physical_screen_width",
            "physical_screen_height",
            "viewport_supported",
            "mobileoptimized",
            "handheldfriendly",
            "css_supports_width_as_percentage",
            "css_spriting",
            "css_gradient",
            "css_border_image",
            "css_rounded_corners",

            // Audio properties.
            "rmf",
            "qcelp",
            "awb",
            "smf",
            "wav",
            "nokia_ringtone",
            "aac",
            "digiplug",
            "sp_midi",
            "compactmidi",
            "mp3",
            "mld",
            "evrc",
            "amr",
            "xmf",
            "mmf",
            "imelody",
            "midi_monophonic",
            "au",
            "midi_polyphonic"};

        internal static readonly string[] Audio_Formats = new string[] {
            "rmf",
            "qcelp",
            "awb",
            "smf",
            "wav",
            "nokia_ringtone",
            "aac",
            "digiplug",
            "sp_midi",
            "compactmidi",
            "mp3",
            "mld",
            "evrc",
            "amr",
            "xmf",
            "mmf",
            "imelody",
            "midi_monophonic",
            "au",
            "midi_polyphonic"};

        /// <summary>
        /// Length of time in ms the new devices thread should wait for a response from the
        /// web server used to record new device information.
        /// </summary>
        internal const int NewURLTimeOut = 5000;
    }
}