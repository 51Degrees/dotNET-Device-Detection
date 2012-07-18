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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;

#if VER4 || VER35

using System.Linq;

#endif

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

        private SortedList<string, List<string>> _fiftyOneProperties;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs an instance of <cref see="FiftyOneBrowserCapabilities"/>
        /// </summary>
        /// <param name="currentCapabilities">Capabilities provided by Microsoft.</param>
        /// <param name="overrideCapabilities">New capabilities provided by 51Degrees.mobi. Can not be null.</param>
        public FiftyOneBrowserCapabilities(HttpBrowserCapabilities currentCapabilities, IDictionary overrideCapabilities)
        {
            // Initialise the hashtable for capabilities.
            Capabilities = new Hashtable();

            // Copy the keys from both the original and new capabilities.
            foreach (object key in currentCapabilities.Capabilities.Keys)
                Capabilities[key] = currentCapabilities.Capabilities[key];
            foreach (object key in overrideCapabilities.Keys)
                Capabilities[key] = overrideCapabilities[key];

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

        #region Properties

        /// <summary>
        /// Returns the 51Degrees.mobi list if it exists. It should
        /// always exist if this class is being used.
        /// </summary>
        private SortedList<string, List<string>> FiftyOneProperties
        {
            get
            {
                if (_fiftyOneProperties == null)
                    _fiftyOneProperties = Capabilities[Constants.FiftyOneDegreesProperties] as SortedList<string, List<string>>;
                return _fiftyOneProperties;
            }
        }

        #endregion

        #region Overridden Members

        /// <summary>
        /// Returns the value for the property key from the standard
        /// collection of capabilities provided by Microsoft. If a value is not
        /// found 51Degrees.mobi properties are checked using first a case
        /// sensitive, and then insensitive match.
        /// </summary>
        /// <param name="key">The capability key being sought.</param>
        /// <returns>The value of the key, otherwise null.</returns>
        public override string this[string key]
        {
	        get 
	        { 
		        string result = base[key];
                
                // If the base list of capabilities does not return a result 
                // then try the 51degrees.mobi capabilities.
                if (result == null &&
                    FiftyOneProperties != null)
                {
                    List<string> values;
                    if (FiftyOneProperties.TryGetValue(key, out values))
                        result = String.Join(Constants.ValueSeperator, values.ToArray());

                    // If the key can't be found try a case insensitive search.
                    else
                    {
#if VER4 || VER35
                        var matches = FiftyOneProperties.Where(i =>
                            i.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase)).ToList();
#else
                        List<KeyValuePair<string, List<string>>> matches = new List<KeyValuePair<string, List<string>>>();
                        foreach(KeyValuePair<string, List<string>> item in FiftyOneProperties)
                            if (item.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase))
                                matches.Add(item);
#endif
                        if (matches != null && matches.Count > 0)
                            result = String.Join(Constants.ValueSeperator, matches[0].Value.ToArray());
                    }
                }
                
                return result;
	        }
        }

        #endregion
    }
}
