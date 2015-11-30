/* *********************************************************************
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited. 
 * Copyright © 2015 51Degrees Mobile Experts Limited, 5 Charlotte Close,
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

using System.Collections.Generic;
namespace FiftyOne.Foundation.UI
{
    internal class Constants
    {
        /// <summary>
        /// A list of all properties that need to be present in Premium data.
        /// </summary>
        internal static readonly string[] Premium = null;

        /// <summary>
        /// List of properties included in the CMS product.
        /// </summary>
        internal static readonly string[] CMS = new string[] {
            "IsConsole",
            "IsEReader",
            "IsTablet",
            "IsSmartPhone",
            "IsSmallScreen",
            "SuggestedLinkSizePixels",
            "SuggestedLinkSizePoints" };

        /// <summary>
        /// List of properties associated wtih the type of content
        /// the browser/device can display.
        /// </summary>
        internal static readonly string[] Content = new string[] {
            "CcppAccept",
            "StreamingAccept" };

        /// <summary>
        /// The 51Degrees.mobi thumbnail logo.
        /// </summary>
        internal const string Logo = "http://download.51degrees.mobi/51Degrees%20Logo%20Small.png";

        /// <summary>
        /// The Alt text attribute applied to the logo image.
        /// </summary>
        internal const string LogoAltText = "51Degrees";

        /// <summary>
        /// The number of characters per line when the User-Agent is broken down.
        /// </summary>
        internal const int UserAgentCharactersPerLine = 80;
    }
}
