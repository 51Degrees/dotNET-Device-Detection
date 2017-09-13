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

using System;
using System.Text.RegularExpressions;
using System.Web;

namespace FiftyOne.Foundation.Mobile.Redirection
{
    internal class Filter
    {

        #region Fields

        private readonly string _capability;
        private readonly Regex _expression;

        #endregion

        #region Constructor

        internal Filter(string capability, string expression)
        {
            _capability = capability;
            _expression = new Regex(expression, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Determines if this filter matches the requesting device.
        /// </summary>
        /// <param name="context">Context of the requesting device.</param>
        /// <returns>True if the capability matches, otherwise false.</returns>
        internal bool GetIsMatch(HttpContext context)
        {
            string value = GetPropertyValue(context, _capability);
            if (String.IsNullOrEmpty(value))
                return false;
            return _expression.IsMatch(value);
        }

        #endregion

        #region Private Members

        /// <summary>
        /// Returns the value for the property requested.
        /// </summary>
        /// <param name="property">The property name to be returned.</param>
        /// <param name="context">The context associated with the request.</param>
        /// <returns></returns>
        private static string GetPropertyValue(HttpContext context, string property)
        {
            string value;

            // Check for special properties.
            value = GetSpecialPropertyValue(property, context);
            if (String.IsNullOrEmpty(value) == false)
                return value;

            // Try the standard properties of the browser object. Use a try/catch block
            // incase there is a security exception accessing the reflection methods.
            try
            {
                value = GetHttpBrowserCapabilitiesPropertyValue(property, context.Request.Browser);
                if (String.IsNullOrEmpty(value) == false)
                    return value;
            }
            catch (Exception ex)
            {
                EventLog.Warn(new MobileException(String.Format("Exception getting property value '{0}' from HttpBrowserCapabilities instance.", property), ex));
            }

            // Try the standard properties of the request object. Use a try/catch block
            // incase there is a security exception accessing the reflection methods.
            try
            {
                value = GetHttpRequestPropertyValue(property, context.Request);
                if (String.IsNullOrEmpty(value) == false)
                    return value;
            }
            catch (Exception ex)
            {
                EventLog.Warn(new MobileException(String.Format("Exception getting property value '{0}' from HttpRequest instance.", property), ex));
            }

            // If not then try and return the value for the collection.
            return context.Request.Browser[property];
        }

        /// <summary>
        /// Checks the properties of the HttpBrowserCapabilities instance passed
        /// into the method for the property name contained in the property parameters
        /// string value.
        /// </summary>
        /// <param name="property">Property name to be found.</param>
        /// <param name="capabilities">Capabilities collection to be used.</param>
        /// <returns>If the property exists then return the associated value, otherwise null.</returns>
        private static string GetHttpBrowserCapabilitiesPropertyValue(string property, HttpBrowserCapabilities capabilities)
        {
            Type controlType = capabilities.GetType();
            System.Reflection.PropertyInfo propertyInfo = controlType.GetProperty(property);
            if (propertyInfo != null && propertyInfo.CanRead)
                return propertyInfo.GetValue(capabilities, null).ToString();

            // Try browser capabilities next.
            string value = capabilities[property];
            if (value != null)
                return value;

            return null;
        }

        /// <summary>
        /// Checks the properties of the HttpRequest instance passed
        /// into the method for the property name contained in the property parameters
        /// string value.
        /// </summary>
        /// <param name="property">Property name to be found.</param>
        /// <param name="request">HttpRequest to be used.</param>
        /// <returns>If the property exists then return the associated value, otherwise null.</returns>
        private static string GetHttpRequestPropertyValue(string property, HttpRequest request)
        {
            // Try the properties of the request.
            Type controlType = request.GetType();
            System.Reflection.PropertyInfo propertyInfo = controlType.GetProperty(property);
            if (propertyInfo != null && propertyInfo.CanRead)
                return propertyInfo.GetValue(request, null).ToString();

            return null;
        }

        /// <summary>
        /// Checks the items collection of the HttpContent instance passed
        /// into the method for the property name contained in the property parameters
        /// string value.
        /// </summary>
        /// <param name="property">Property name to be found.</param>
        /// <param name="context">HttpContext to be used.</param>
        /// <returns>If the property exists then return the associated value, otherwise null.</returns>
        private static string GetSpecialPropertyValue(string property, HttpContext context)
        {
            switch(property)
            {
                case Constants.OriginalUrlSpecialProperty:
                    return context.Items[Constants.OriginalUrlKey] as string;
            }
            return null;
        }

        #endregion

    }
}
