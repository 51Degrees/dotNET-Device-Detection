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

namespace FiftyOne.Foundation.Mobile.Redirection
{
    internal static class Constants
    {
        /// <summary>
        /// Full type names of classes representing mobile
        /// page handlers.
        /// </summary>
        internal static readonly string[] MobilePages = new string[] {
                                                            "System.Web.UI.MobileControls.MobilePage",
                                                            "FiftyOne.Framework.Mobile.Page"
                                                        };

        /// <summary>
        /// Full type names of classes representing standard
        /// page handlers.
        /// </summary>
        internal static readonly string[] Pages = new string[]
                                                     {
                                                         "System.Web.UI.Page",
                                                         "System.Web.Mvc.MvcHandler",
                                                         "System.Web.Mvc.MvcHttpHandler",
                                                         "System.Web.UI.MobileControls.MobilePage",
                                                         "System.Web.WebPages.WebPageHttpHandler",
                                                         "Orchard.Mvc.Routes.ShellRoute.HttpAsyncHandler"
                                                     };                                             
                         
        /// <summary>
        /// The key in the Items collection of the requesting context used to
        /// store result of the is first time check.
        /// </summary>
        internal const string IsFirstTimeKey = "51D_Is_First_Time";                                                     

        /// <summary>
        /// The key in the Items collection of the requesting context used to
        /// store the Url originally requested by the browser.
        /// </summary>
        internal const string OriginalUrlKey = "51D_Original_Url";

        /// <summary>
        /// The property name to use to access the original Url for the request.
        /// </summary>
        internal const string OriginalUrlSpecialProperty = "origUrl";

        /// <summary>
        /// The key in the Items collection of the requesting context used to
        /// store the home page Url for a possible redirection.
        /// </summary>
        internal const string LocationUrlKey = "51D_Location_Url";

        /// <summary>
        /// Set the UTC date and time that details of the device should be removed
        /// from all storage mechanisims.
        /// </summary>
        internal const string ExpiryTime = "51D_Expiry_Time";

        /// <summary>
        /// Used to indicate the device has already accessed the web site.
        /// </summary>
        internal const string AlreadyAccessedCookieName = "51D";

        /// <summary>
        /// Used to control if the already accessed cookie should be included
        /// in the response.
        /// </summary>
        internal const bool AllowAlreadyAccessedCookie = true;
    }
}
