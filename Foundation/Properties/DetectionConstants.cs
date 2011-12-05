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

using System.Collections.Generic;
using System;
namespace FiftyOne.Foundation.Mobile.Detection
{
    internal static class Constants
    {
        /// <summary>
        /// If premium data is being used with Foundation the licence key
        /// can be provided in the following constant or in a file with the
        /// extension .lic in the bin folder.
        /// </summary>
        internal const string PremiumLicenceKey = "";

        /// <summary>
        /// The URL to use to get the latest device data from.
        /// </summary>
        internal const string AutoUpdateUrl = "https://51degrees.mobi/Products/Download/PremiumData.aspx";

        /// <summary>
        /// The length of time to wait before starting the auto update
        /// process. Set to zero to disable auto update.
        /// </summary>
        internal static readonly TimeSpan AutoUpdateDelayedStart =
            new TimeSpan(0, 0, 20);

        /// <summary>
        /// The length of time to wait before checking for a newer
        /// version of the device data file.
        /// </summary>
        internal static readonly TimeSpan AutoUpdateWait =
            new TimeSpan(7, 0, 0, 0);
        
        /// <summary>
        /// Length of time in ms the new devices thread should wait for a response from the
        /// web server used to record new device information.
        /// </summary>
        internal const int NewUrlTimeOut = 5000;

        /// <summary>
        /// Forces the useragent matcher to use a single thread if 
        /// multiple processors are available.
        /// </summary>
        internal const bool ForceSingleProcessor = false;

        /// <summary>
        /// Array of transcoder HTTP headers that represent the useragent string of the
        /// mobile device rather than the desktop browser.
        /// </summary>
        internal static readonly string[] TranscoderUserAgentHeaders = new[]
                                                                             {
                                                                                 "x-Device-User-Agent",
                                                                                 "X-Device-User-Agent",
                                                                                 "X-OperaMini-Phone-UA"
                                                                             };

        /// <summary>
        /// The Http header field that contains the user agent.
        /// </summary>
        internal const string UserAgentHeader = "User-Agent";

        /// <summary>
        /// The character used to seperate property values.
        /// </summary>
        internal const string ValueSeperator = "|";

        /// <summary>
        /// The key used to identify the list of 51Degrees.mobi properties.
        /// </summary>
        internal const string FiftyOneDegreesProperties = "51Degrees.mobi";

        /// <summary>
        /// The URL new device information should be sent to.
        /// </summary>
        internal const string NewDevicesUrl = "http://devices.51degrees.mobi/new.ashx";

        /// <summary>
        /// The detail that should be provided relating to possible new devices.
        /// </summary>
        internal const NewDeviceDetails NewDeviceDetail = NewDeviceDetails.Maximum;

        /// <summary>
        /// The name of the property which contains the user agent profile.
        /// </summary>
        internal static readonly string[] UserAgentProfiles = new[] { "UserAgentProfile" };

        /// <summary>
        /// Extensions to indicate a file is compressed.
        /// </summary>
        internal static readonly List<string> CompressedFileExtensions = new List<string>(new[] { ".gz" } );

        /// <summary>
        /// A list of properties to exclude from the AllProperties results.
        /// </summary>
        internal static readonly List<string> ExcludePropertiesFromAllProperties = new List<string>(new[] { "" });

    }
}