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
            "~/Web.config" };

#if AZURE
        /// <summary>
        /// Name for Azure cloud storage
        /// </summary>
        internal const string AZURE_STORAGE_NAME = "fiftyonedegrees";
#endif
    }
}
