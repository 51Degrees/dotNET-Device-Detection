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

namespace FiftyOne.Foundation.Mobile.Redirection
{
    internal static class Constants
    {

        /// <summary>
        /// Full type names of classes representing mobile
        /// page handlers.
        /// </summary>
        internal static readonly string[] MOBILE_PAGES = {
                                                            "System.Web.UI.MobileControls.MobilePage",
                                                            "FiftyOne.Framework.Mobile.Page"
                                                        };

        /// <summary>
        /// Full type names of classes representing standard
        /// page handlers.
        /// </summary>
        internal static readonly string[] PAGES = new[]
                                                     {
                                                         "System.Web.UI.Page",
                                                         "System.Web.Mvc.MvcHandler",
                                                         "System.Web.Mvc.MvcHttpHandler",
                                                         "System.Web.UI.MobileControls.MobilePage"
                                                     };

        /// <summary>
        /// The key in the Items collection of the requesting context used to
        /// store the Url originally requested by the browser.
        /// </summary>
        internal const string ORIGINAL_URL_KEY = "51D_Original_Url";

        /// <summary>
        /// The key in the Items collection of the requesting context used to
        /// store the home page Url for a possible redirection.
        /// </summary>
        internal const string LOCATION_URL_KEY = "51D_Location_Url";

        /// <summary>
        /// Set the UTC date and time that details of the device should be removed
        /// from all storage mechanisims.
        /// </summary>
        internal const string ExpiryTime = "51D_Expiry_Time";

        /// <summary>
        /// Used to indicate the device has already accessed the web site.
        /// </summary>
        internal const string AlreadyAccessedCookieName = "51D";
    }
}
