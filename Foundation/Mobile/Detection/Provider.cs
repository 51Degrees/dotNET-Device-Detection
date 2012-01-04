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
 * The Initial Developer of the Original Code is owned by `
 * 51 Degrees Mobile Experts Limited. Portions created by 51 Degrees
 * Mobile Experts Limited are Copyright (C) 2009 - 2012. All Rights Reserved.
 * 
 * Contributor(s):
 *     James Rosewell <james@51degrees.mobi>
 *     Andy Allan <andy.allan@mobgets.com>
 * 
 * ********************************************************************* */

#if VER4
using System.Linq;
#endif

#region Usings

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using System.Xml.Schema;
using FiftyOne.Foundation.Mobile.Detection.Configuration;
using FiftyOne.Foundation.Mobile.Detection.Handlers;
using FiftyOne.Foundation.Mobile.Detection.Matchers;
using Matcher=FiftyOne.Foundation.Mobile.Detection.Matchers.Final.Matcher;
using RegexSegmentHandler = FiftyOne.Foundation.Mobile.Detection.Handlers.RegexSegmentHandler;
using System.Collections.Specialized;

#endregion

namespace FiftyOne.Foundation.Mobile.Detection
{
    /// <summary>
    /// Represents all device data and capabilities.
    /// </summary>
    public class Provider : BaseProvider
    {
        #region Internal Constructors

        /// <summary>
        /// Constructs a new instance of <see cref="BaseProvider"/>.
        /// </summary>
        internal Provider()
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns the closest matching device from the result set to the target userAgent.
        /// </summary>
        /// <param name="results">The result set to find a device from.</param>
        /// <param name="userAgent">Target useragent.</param>
        /// <returns>The closest matching device.</returns>
        private BaseDeviceInfo GetDeviceInfoClosestMatch(Results results, string userAgent)
        {
            if (results == null || results.Count == 0)
                return null;

            if (results.Count == 1)
                return results[0].Device as BaseDeviceInfo;
            
            results.Sort();
            BaseDeviceInfo device = Matcher.Match(userAgent, results);
            if (device != null)
                return device;

            return null;
        }

        /// <summary>
        /// Gets the closest matching device based on the HTTP headers.
        /// </summary>
        /// <param name="headers">Collection of Http headers associated with the request.</param>
        /// <returns>The closest matching device.</returns>
        internal BaseDeviceInfo GetDeviceInfo(NameValueCollection headers)
        {
            return GetDeviceInfoClosestMatch(
                GetMatches(headers), GetUserAgent(headers));
        }

        /// <summary>
        /// Gets an array of the devices that match this useragent string.
        /// </summary>
        /// <param name="userAgent">Useragent string associated with the mobile device.</param>
        /// <returns>An array of matching devices.</returns>
        public List<BaseDeviceInfo> GetMatchingDeviceInfo(string userAgent)
        {
            var list = new List<BaseDeviceInfo>();
            var results = GetMatches(userAgent);
            if (results != null)
            {
                foreach (var result in results)
                    list.Add(result.Device);
            }
            return list;
        }

        /// <summary>
        /// Enhances the base implementation to check for devices marked with 
        /// "actual_device_root" and only return these.
        /// </summary>
        /// <param name="userAgent">Useragent string associated with the mobile device.</param>
        /// <returns>The closest matching device.</returns>
        public BaseDeviceInfo GetDeviceInfo(string userAgent)
        {
            return GetDeviceInfoClosestMatch(GetMatches(userAgent), userAgent);
        }

        #endregion
    }
}