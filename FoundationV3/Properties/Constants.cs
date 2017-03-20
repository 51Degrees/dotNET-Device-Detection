/* *********************************************************************
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited. 
 * Copyright © 2017 51Degrees Mobile Experts Limited, 5 Charlotte Close,
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

namespace FiftyOne.Foundation.Mobile
{
    internal static class Constants
    {
        /// <summary>
        /// Array of file locations to check for configuration information.
        /// </summary>
        internal static readonly string[] ConfigFileNames = new string[] {
            "~/51Degrees.mobi.config",
            "~/App_Data/51Degrees.mobi.config",
            "~/51Degrees.config",
            "~/App_Data/51Degrees.config",
            "~/Web.config" };
        
#if AZURE
        /// <summary>
        /// Name for Azure cloud storage
        /// </summary>
        internal const string AZURE_STORAGE_NAME = "fiftyonedegrees";
#endif
    }
}
