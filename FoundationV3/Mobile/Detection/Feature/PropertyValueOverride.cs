/* *********************************************************************
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited. 
 * Copyright © 2015 51Degrees Mobile Experts Limited, 5 Charlotte Close,
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
    internal static class PropertyValueOverride
    {
        #region Internal Methods

        /// <summary>
        /// Set a property in the application state to quickly determine if property
        /// value override is supported.
        /// </summary>
        /// <param name="application"></param>
        internal static void Init(HttpApplicationState application)
        {
            if (Configuration.Manager.FeatureDetectionEnabled == false ||
                WebProvider.ActiveProvider == null)
            {
                application[Constants.PropertyValueOverrideFlag] = 
                    new bool?(false);
            }
            else
            {
                application[Constants.PropertyValueOverrideFlag] = new bool?(
                    WebProvider.GetActiveProvider().DataSet.
                    PropertyValueOverrideProperties.Length > 0);
            }
            EventLog.Debug(String.Format(
                "Property Value Override '{0}'", 
                application[Constants.PropertyValueOverrideFlag]));
        }

        /// <summary>
        /// Returns the JavaScript for the Property Value Override feature.
        /// </summary>
        /// <param name="context"></param>
        /// <returns>JavaScript for the device, otherwise null.</returns>
        internal static string GetJavascript(HttpContext context)
        {
            var enabled = context.Application[Constants.PropertyValueOverrideFlag] as bool?;
            if (enabled.HasValue && enabled.Value)
            {
                var jsvalues = GetJavascriptValues(context.Request);
                if (jsvalues.Count > 0)
                {
                    return String.Format(
                        "// Property Value Overrides - Start\r" +
                        "{0}\r" +
                        "// Property Value Overrides - End\r",
                        String.Join("\r", jsvalues.Select(i => i.ToString())));
                }
            }
            return null;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Returns all the JavaScript for both the Propery Value Override
        /// feature AND the request provided.
        /// </summary>
        /// <param name="request">Request the javascript is needed for</param>
        /// <returns>Javascript to support the feature if present</returns>
        private static List<Value> GetJavascriptValues(HttpRequest request)
        {
            var javaScriptValues = new List<Value>();
            var match = WebProvider.GetMatch(request);
            if (match != null)
            {
                foreach (var property in WebProvider.GetActiveProvider().
                    DataSet.PropertyValueOverrideProperties)
                {
                    foreach (var value in match[property])
                    {
                        javaScriptValues.Add(value);
                    }
                }
            }
            return javaScriptValues;
        }

        #endregion
    }
}
