/* *********************************************************************
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited. 
 * Copyright © 2014 51Degrees Mobile Experts Limited, 5 Charlotte Close,
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Linq;
using FiftyOne.Foundation.Mobile.Detection.Entities;

namespace FiftyOne.Foundation.Mobile.Detection
{
    /// <summary>
    /// The this[] property has been changed to also check the 51Degrees.mobi capabilities
    /// before returning a value if the capability has not been found in the 
    /// standard collection.
    /// </summary>
    /// <remarks>
    /// Note: The OBSOLETE_SUPPORT pre-compilation directive should be set if support is needed
    /// for SharePoint or other applications that expect the Request.Browser property to return
    /// an object of type <cref see="System.Web.Mobile.MobileCapabilities"/>. A reference will 
    /// need to be added to the project to System.Web.Mobile. This module has been marked 
    /// obsolete by Microsoft but is still used by SharePoint 2010. Because it is marked
    /// obsolete we decided not to require developers have to reference System.Web.Mobile.
    /// </remarks>
#if OBSOLETE_SUPPORT
    public class FiftyOneBrowserCapabilities : System.Web.Mobile.MobileCapabilities
#else
    public class FiftyOneBrowserCapabilities : HttpBrowserCapabilities
#endif
    {
        #region Fields

        private readonly SortedList<string, string[]> _results;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs an instance of <cref see="FiftyOneBrowserCapabilities"/>
        /// </summary>
        /// <param name="results">The results of the match</param>
        /// <param name="currentCapabilities">Capabilities provided by Microsoft.</param>
        /// <param name="overrideCapabilities">New capabilities provided by 51Degrees.mobi. Can not be null.</param>
        public FiftyOneBrowserCapabilities(
            SortedList<string, string[]> results,
            HttpBrowserCapabilities currentCapabilities,
            IDictionary overrideCapabilities)
        {
            _results = results;

            // Initialise the hashtable for capabilities.
            Capabilities = new Hashtable(StringComparer.InvariantCultureIgnoreCase);

            // Copy the keys from both the original and new capabilities.
            foreach (object key in currentCapabilities.Capabilities.Keys)
                Capabilities[key] = currentCapabilities.Capabilities[key];

            foreach (string key in overrideCapabilities.Keys)
            {
                // Do not override the preferredRenderingType if original
                // .NET mobile controls are being used as values greater 
                // than html32 result in runtime exceptions.
#if OBSOLETE_SUPPORT
                if (key.Equals("preferredRenderingType") == false)
                {
#endif
                Capabilities[key] = overrideCapabilities[key];
#if OBSOLETE_SUPPORT
                }
#endif
            }
            
            // Copy the adapters from the original.
            foreach (object key in currentCapabilities.Adapters.Keys)
                Adapters.Add(key, currentCapabilities.Adapters[key]);

            // Copy the browsers from the original to prevent the Browsers
            // property returning null.
            if (currentCapabilities.Browsers != null)
                foreach (string browser in currentCapabilities.Browsers)
                    AddBrowser(browser);
        }

        #endregion

        #region Overridden Members

        /// <summary>
        /// Returns the value of the property by first checking the results
        /// assigned to the browser capabilities. If no result is found then
        /// base implementation is called.
        /// </summary>
        /// <param name="key">The capability key being sought.</param>
        /// <returns>The value of the key, otherwise null.</returns>
        public override string this[string key]
        {
	        get 
	        { 
                string[] result;
                if (_results.TryGetValue(key, out result))
                    return String.Join(Constants.ValueSeperator, result);
                try
                {
                    return base[key];
                }
                catch
                {
                    return null;
                }
	        }
        }

        #endregion
    }
}
