/* *********************************************************************
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
using System.Collections.Generic;

namespace FiftyOne.Foundation.Mobile.Detection
{
    /// <summary>
    /// Constants used to control major aspects of redirection.
    /// </summary>
    public static class Constants
    {
        #region Public Constants

        /// <summary>
        /// The default path to use for the binary data file.
        /// </summary>
        public const string DefaultBinaryFilePath = "~/App_Data/51Degrees-Premium.dat";

        /// <summary>
        /// The preferred name of the licence key file.
        /// </summary>
        public const string LicenceKeyFileName = "51Degrees.mobi.lic";

        /// <summary>
        /// The character used to seperate property values when concatenated as a single string.
        /// </summary>
        public const string ValueSeperator = "|";

        /// <summary>
        /// The character used to seperate profile integer values  in the device id.
        /// </summary>
        public const string ProfileSeperator = "-";

        /// <summary>
        /// The key used to identify the list of 51Degrees.mobi properties in the 
        /// capabilities collection of the HttpBrowserCapabilities class.
        /// </summary>
        public const string FiftyOneDegreesProperties = "51Degrees.mobi";

        /// <summary>
        /// The length of time taken to locate the device returned. This property is 
        /// set in the MobileCapabilities class and is not part of the Providers.
        /// </summary>
        public const string DetectionTimeProperty = "DetectionTime";

        /// <summary>
        /// The confidence of the results found.
        /// </summary>
        public const string ConfidenceProperty = "MatchConfidence";

        /// <summary>
        /// The difference between the user agent requested and the one found.
        /// </summary>
        public const string DifferenceProperty = "MatchDifference";

        /// <summary>
        /// The name of the unique property key used to return the device id.
        /// </summary>
        public const string DeviceId = "Id";

        /// <summary>
        /// A regular expression used to valid licence key formats.
        /// </summary>
        public const string LicenceKeyValidationRegex = @"^[A-Z\d]+$";

        #endregion

        #region Internal Constants

        /// <summary>
        /// If premium data is being used with Foundation the licence key
        /// can be provided in the following constant or in a file with the
        /// extension .lic in the bin folder.
        /// </summary>
        internal const string PremiumLicenceKey = "";

        /// <summary>
        /// The URL to use to get the latest device data from if a Premium licence key is provided.
        /// </summary>
        internal const string AutoUpdateUrl = "https://51degrees.mobi/Products/Downloads/Premium.aspx";

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
            new TimeSpan(6, 0, 0, 0);

        /// <summary>
        /// The length of time to sleep before checking for new device
        /// data again.
        /// </summary>
        internal static readonly TimeSpan AutoUpdateSleep =
            new TimeSpan(0, 0, 6, 0);

        /// <summary>
        /// The length of time to wait before setting the local data file
        /// check process.
        /// </summary>
        internal static readonly TimeSpan FileUpdateDelayedStart =
            new TimeSpan(0, 0, 2, 0);

        /// <summary>
        /// The length of time between local data file checks.
        /// </summary>
        internal static readonly TimeSpan FileUpdateSleep =
            new TimeSpan(0, 0, 2, 0);

        /// <summary>
        /// Length of time in ms the new devices thread should wait for a response from the
        /// web server used to record new device information.
        /// </summary>
        internal const int NewUrlTimeOut = 10000;

        /// <summary>
        /// The maximum number of continous timeouts that are allowed
        /// before the new device function is disabled.
        /// </summary>
        internal const int NewUrlMaxTimeouts = 10;

        /// <summary>
        /// Forces the useragent matcher to use a single thread if 
        /// multiple processors are available.
        /// </summary>
        internal const bool ForceSingleProcessor = false;

        /// <summary>
        /// Improves performance of segment handlers by storing the results of 
        /// useragent segment matches to improve performance at the expense
        /// of memory consumption. Set this to false to reduce memory consumption.
        /// </summary>
        internal const bool StoreSegmentResults = true;

        /// <summary>
        /// Array of HTTP headers that represent the useragent string of the
        /// device rather than the browser.
        /// </summary>
        internal static readonly string[] DeviceUserAgentHeaders = new string[]
            {
                "Device-Stock-UA",
                "x-Device-User-Agent",
                "X-Device-User-Agent",
                "X-OperaMini-Phone-UA"
            };

        /// <summary>
        /// The Http header field that contains the user agent.
        /// </summary>
        internal const string UserAgentHeader = "User-Agent";

        /// <summary>
        /// The URL usage sharing information should be sent to.
        /// </summary>
        internal const string NewDevicesUrl = "http://devices.51degrees.mobi/new.ashx";
        
        /// <summary>
        /// The detail that should be provided relating to new devices.
        /// </summary>
        internal const NewDeviceDetails NewDeviceDetail = NewDeviceDetails.Maximum;

        /// <summary>
        /// The number of requests that should be held in the queue before
        /// transmission.
        /// </summary>
        internal const int NewDeviceQueueLength = 50;

        /// <summary>
        /// The name of the property which contains the user agent profile.
        /// </summary>
        internal static readonly string[] UserAgentProfiles = new string[] { "UserAgentProfile" };

        /// <summary>
        /// Extensions to indicate a file is compressed.
        /// </summary>
        internal static readonly List<string> CompressedFileExtensions = new List<string>(new string[] { ".gz" });

        /// <summary>
        /// A list of properties to exclude from the AllProperties results.
        /// </summary>
        internal static readonly List<string> ExcludePropertiesFromAllProperties = new List<string>(new string[] { "" });

        /// <summary>
        /// An array of default values for properties where they can't be found
        /// and a value must be provided.
        /// </summary>
        internal static readonly string[,] DefaultPropertyValues = new string[,] {
            { "screenPixelsHeight", "480" },
            { "screenPixelsWidth", "640" },
            { "screenCharactersHeight", "40" },
            { "screenCharactersWidth", "80" } };

        #endregion
    }
}