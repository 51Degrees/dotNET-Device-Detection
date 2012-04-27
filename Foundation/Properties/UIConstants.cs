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

namespace FiftyOne.Foundation.UI
{
    internal class Constants
    {
        /// <summary>
        /// List of properties which are associated with the physical hardware.
        /// </summary>
        internal static readonly string[] Hardware = new[] {
            "HardwareVendor",
            "HardwareModel",
            "HardwareName",
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
            "ReleaseMonth",
            "ReleaseYear",
            "ScreenMMHeight",
            "ScreenMMWidth",
            "ScreenPixelsHeight",
            "ScreenPixelsWidth",
            "SuggestedImageButtonHeightMms",
            "SuggestedImageButtonHeightPixels",
            "SuggestedLinkSizePixels",
            "SuggestedLinkSizePoints",
            "SupportedBearers" };

        /// <summary>
        /// List of properties associated with the operating system
        /// or software.
        /// </summary>
        internal static readonly string[] Software = new[] {
            "PlatformVendor",
            "PlatformName",
            "PlatformVersion",
            "JavaEnabled" };

        /// <summary>
        /// List of properties associated with the browser.
        /// </summary>
        internal static readonly string[] Browser = new[] {
            "BrowserVendor",
            "BrowserName",
            "BrowserVersion",
            "AjaxRequestType",
            "CookiesCapable",
            "FramesCapable",
            "HtmlVersion",
            "JavacriptPreferredGeoLocApi",
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
            "PreferencesForFrames",
            "TablesCapable",
            "UserAgentProfile",
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
            "PreferencesForFrames",
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
        internal static readonly string[] Content = new[] {
            "CcppAccept",
            "StreamingAccept" };

        /// <summary>
        /// The 51Degrees.mobi thumbnail logo.
        /// </summary>
        internal const string Logo = "http://download.51degrees.mobi/51Degrees%20Logo%20Small.png";
    }
}
