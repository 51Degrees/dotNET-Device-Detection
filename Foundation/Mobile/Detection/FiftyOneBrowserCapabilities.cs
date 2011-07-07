/* *********************************************************************
 * The contents of this file are subject to the Mozilla internal License 
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

using System.Collections.Generic;
using System.Collections;
using System.Web;

namespace FiftyOne.Foundation.Mobile.Detection
{
    /// <summary>
    /// The this[] property has been changed to also check the wurfl capabilities
    /// before returning a value if the capability has not been found in the 
    /// standard collection.
    /// </summary>
    internal class FiftyOneBrowserCapabilities : HttpBrowserCapabilities
    {
        #region Fields

        private SortedList<string, string> _wurflCapabilities;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs an instance of <cref see="FiftyOneBrowserCapabilities"/>
        /// </summary>
        /// <param name="currentCapabilities">Capabilities provided by Microsoft.</param>
        /// <param name="overrideCapabilities">New capabilities provided by 51Degrees.mobi. Can not be null.</param>
        internal FiftyOneBrowserCapabilities(HttpBrowserCapabilities currentCapabilities, IDictionary overrideCapabilities)
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
        /// Returns the WurflCapabilities list if it exists. It should
        /// always exist if this class is being used.
        /// </summary>
        private SortedList<string, string> WurflCapabilities
        {
            get
            {
                if (_wurflCapabilities == null)
                    _wurflCapabilities = Capabilities[Wurfl.Constants.WurflCapabilities] as SortedList<string, string>;
                return _wurflCapabilities;
            }
        }

        #endregion

        #region Overridden Members

        /// <summary>
        /// Returns the value for the capability key from initially the standard
        /// collection of capabilities provided by Microsoft. If a value is not
        /// found then WURFL is checked.
        /// </summary>
        /// <param name="key">The capability key being sought.</param>
        /// <returns>The value of the key, otherwise null.</returns>
        public override string this[string key]
        {
	        get 
	        { 
		        string result = base[key];
                
                // If the base list of capabilities does not return a result 
                // then try the wurfl capabilities.
                if (result == null && 
                    WurflCapabilities != null)
                    WurflCapabilities.TryGetValue(key, out result);
                
                return result;
	        }
        }

        #endregion
    }
}
