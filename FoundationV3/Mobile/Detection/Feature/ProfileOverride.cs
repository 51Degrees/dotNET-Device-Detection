/* *********************************************************************
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited. 
 * Copyright © 2017 51Degrees Mobile Experts Limited, 5 Charlotte Close,
 * Caversham, Reading, Berkshire, United Kingdom RG4 7BY
 * 
 * This Source Code Form is the subject of the following patents and patent 
 * applications, owned by 51Degrees Mobile Experts Limited of 5 Charlotte
 * Close, Caversham, Reading, Berkshire, United Kingdom RG4 7BY: 
 * European Patent No. 2871816;
 * European Patent Application No. 17184134.9;
 * United States Patent Nos. 9,332,086 and 9,350,823; and
 * United States Patent Application No. 15/686,066.
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

using FiftyOne.Foundation.Mobile.Detection.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FiftyOne.Foundation.Mobile.Detection.Feature
{
    internal static class ProfileOverride
    {
        /// <summary>
        /// String array used to split the profile ids returned from the javascript.
        /// </summary>
        private static readonly char[] _split = new char[] { '|' };

        /// <summary>
        /// True if profile overrides are enabled.
        /// </summary>
        private static bool? _enabled;

        /// <summary>
        /// Set a static field to quickly determine if profile 
        /// override is supported.
        /// </summary>
        /// <param name="application"></param>
        internal static void Init(HttpApplicationState application)
        {
            if (Configuration.Manager.FeatureDetectionEnabled == false  ||
                WebProvider.ActiveProvider == null)
            {
                _enabled = false;
            }
            else
            {
                var property = WebProvider.ActiveProvider.DataSet.Properties.FirstOrDefault(i =>
                    i.Name == "JavascriptHardwareProfile");
                _enabled = property != null;
            }
            EventLog.Debug(String.Format("Profile Override '{0}'", _enabled));
        }

        /// <summary>
        /// Returns the javascript for the feature.
        /// </summary>
        /// <param name="request">Request the javascript is needed for</param>
        /// <returns>Javascript to support the feature if present</returns>
        private static Values GetJavascriptValues(HttpRequest request)
        {
            Values values = null;
            var match = WebProvider.GetMatch(request);
            if (match != null)
            {
                var javascript = match["JavascriptHardwareProfile"];
                if (javascript != null && javascript.Count > 0)
                {
                    values = javascript;
                }
            }
            return values;
        }

        /// <summary>
        /// Returns the javascript for profile override for the
        /// requesting device.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        internal static string GetJavascript(HttpContext context)
        {
            var javascript = GetJavascriptValues(context.Request);
            return javascript != null ?
                String.Format(
                    "function FODPO() {{ var profileIds = new Array(); " +
                    "{0} " +
                    "document.cookie = \"{1}=\" + profileIds.join(\"|\"); }}",
                    String.Join("\r", javascript),
                    Constants.ProfileOverrideCookieName) : null;
        }

        /// <summary>
        /// Determines if the feature is enabled based on configuration
        /// when initialised.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private static bool GetIsEnabled(HttpContext context)
        {
            if (_enabled.HasValue && _enabled.Value)
            {
                // If the profile javascript is present for this device then it's enabled.
                return  GetJavascriptValues(context.Request) != null;
            }
            return false;
        }

        /// <summary>
        /// Adds the client javascript reference to the page. The javascript is only
        /// added if this is a new session or there is no session.
        /// </summary>
        /// <param name="page"></param>
        internal static bool AddScript(System.Web.UI.Page page)
        {
            var context = HttpContext.Current;
            if (GetIsEnabled(context))
            {
                var session = context.Session;
                if (session == null ||
                    session.IsNewSession)
                {
                    page.ClientScript.RegisterStartupScript(
                        typeof(ProfileOverride),
                        "FODPO",
                        "new FODPO();",
                        true);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Determines if the request contains overrides for any existing values.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        internal static bool HasOverrides(HttpRequest request)
        {
            var cookie = request.Cookies[Constants.ProfileOverrideCookieName];
            return cookie != null && cookie.Values.Count > 0;
        }

        /// <summary>
        /// Gets all profile IDs that should override the detected match. Returns
        /// an empty array if no overrides have been set.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        internal static int[] GetOverrideProfileIds(HttpRequest request)
        {
            var ids = new List<int>();
            var cookie = request.Cookies[Constants.ProfileOverrideCookieName];
            if (cookie != null)
            {
                // Get the profile Ids from the cookie.
                foreach (var profileId in cookie.Value.Split(_split))
                {
                    // Convert the profile Id into an integer value.
                    int value;
                    if (int.TryParse(profileId, out value))
                    {
                        ids.Add(value);
                    }
                    else
                    {
                        EventLog.Debug(String.Format(
                            "'{0}' cookie contained invalid values '{1}'",
                            Constants.ProfileOverrideCookieName,
                            cookie.Value));
                    }
                }
            }
            return ids.ToArray();
        }

        /// <summary>
        /// Overrides profile IDs with ones in the cookie if present.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="match"></param>
        internal static void Override(HttpRequest request, Match match)
        {
            // Get the profile Ids from the cookie.
            var profileIds = GetOverrideProfileIds(request);
            
            foreach (var profileId in profileIds)
            {
                match.UpdateProfile(profileId);
            }
        }
        
    }
}
