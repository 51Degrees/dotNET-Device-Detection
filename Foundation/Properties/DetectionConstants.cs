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
 * 
 * ********************************************************************* */

using System;
using System.Text;

namespace FiftyOne.Foundation.Mobile.Detection
{
    internal class Constants
    {
        /// <summary>
        /// Forces the useragent matcher to use a single thread if 
        /// multiple processors are available.
        /// </summary>
        internal const bool ForceSingleProcessor = false;

        /// <summary>
        /// Used to indicate the device has already accessed the web site.
        /// </summary>
        internal const string AlreadyAccessedCookieName = "51D";

        /// <summary>
        /// When set to true indicates the request being processed is the 1st
        /// one from that device. False if a subsequent request.
        /// </summary>
        internal const string IsFirstRequest = "51dIsFirstRequest";
        
        /// <summary>
        /// Array of transcoder HTTP headers that represent the useragent string of the
        /// mobile device rather than the desktop browser.
        /// </summary>
        internal static readonly string[] TRANSCODER_USERAGENT_HEADERS = new string[] {
            "x-Device-User-Agent",
            "X-Device-User-Agent",
            "X-OperaMini-Phone-UA"};

        /// <summary>
        /// An array of accept types that indicate javascript support.
        /// </summary>
        internal static readonly string[] JAVASCRIPT_ACCEPTS = new string[] {
            "application/x-javascript",
            "text/javascript",
            "text/x-javascript",
            "text/js",
            "text/x-js",
            "text/jscript",
            "text/ecmascript",
            "*/*",
            "application/vnd.rim.jscriptc"
        };
    }
}
