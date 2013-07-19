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
        /// List of properties which are associated with the physical hardware.
        /// </summary>
        internal static readonly string[] Hardware = new string[] {
            "HardwareVendor",
            "HardwareModel",
            "HardwareName",
            "HardwareFamily",
            "HardwareImages",
            "BitsPerPixel",
            "Has3DCamera",
            "Has3DScreen",
            "HasCamera",
            "HasClickWheel",
            "HasKeypad",
            "HasQwertyPad",
            "HasTouchScreen",
            "HasTrackPad",
            "HasVirtualQwerty",
            "IsConsole",
            "IsCrawler",
            "IsEReader",
            "IsMobile",
            "IsTablet",
            "IsSmartPhone",
            "IsSmallScreen",
            "ReleaseMonth",
            "ReleaseYear",
            "ScreenMMHeight",
            "ScreenMMWidth",
            "ScreenMMDiagonal",
            "ScreenInchesHeight",
            "ScreenInchesWidth",
            "ScreenInchesDiagonal",
            "ScreenPixelsHeight",
            "ScreenPixelsWidth",
            "SuggestedImageButtonHeightMms",
            "SuggestedImageButtonHeightPixels",
            "SuggestedLinkSizePixels",
            "SuggestedLinkSizePoints",
            "SupportedBearers",
            "Popularity",
            "HasNFC"};

        /// <summary>
        /// List of properties associated with the operating system
        /// or software.
        /// </summary>
        internal static readonly string[] Software = new string[] {
            "PlatformVendor",
            "PlatformName",
            "PlatformVersion",
            "CLDC",
            "MIDP"};

        /// <summary>
        /// List of properties associated with the browser.
        /// </summary>
        internal static readonly string[] Browser = new string[] {
            "BrowserVendor",
            "BrowserName",
            "BrowserVersion",
            "AjaxRequestType",
            "CookiesCapable",
            "HtmlVersion",
            "IsEmailBrowser",
            "Javascript",
            "JavascriptCanManipulateCSS",
            "JavascriptCanManipulateDOM",
            "JavascriptGetElementById",
            "JavascriptSupportsEventListener",
            "JavascriptSupportsEvents",
            "JavascriptSupportsInnerHtml",
            "JavascriptVersion",
            "jQueryMobileSupport",
            "LayoutEngine",
            "AnimationTiming",
            "BlobBuilder",
            "CssBackground",
            "CssBorderImage",
            "CssCanvas",
            "CssColor",
            "CssColumn",
            "CssFlexbox",
            "CssFont",
            "CssImages",
            "CssMediaQueries",
            "CssMinMax",
            "CssOverflow",
            "CssPosition",
            "CssText",
            "CssTransforms",
            "CssTransitions",
            "CssUI",
            "DataSet",
            "DataUrl",
            "DeviceOrientation",
            "FileReader",
            "FileSaver",
            "FileWriter",
            "FormData",
            "Fullscreen",
            "GeoLocation",
            "History",
            "Html5",
            "Html-Media-Capture",
            "Iframe",
            "IndexedDB",
            "Json",
            "Masking",
            "PostMessage",
            "Progress",
            "Prompts",
            "Selector",
            "Svg",
            "TouchEvents",
            "Track",
            "Video",
            "Viewport",
            "SupportsTls/Ssl"
        };

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
        /// Constructs the static class setting the Premium property
        /// to include all the other string arrays.
        /// </summary>
        static Constants()
        {
            List<string> temp = new List<string>();
            temp.AddRange(Hardware);
            temp.AddRange(Software);
            temp.AddRange(Browser);
            temp.AddRange(Content);
            temp.Sort();
            Premium = temp.ToArray();
        }
    }
}
