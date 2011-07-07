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
 * Mobile Experts Limited are Copyright (C) 2009 - 2011. All Rights Reserved.
 * 
 * Contributor(s):
 *     James Rosewell <james@51degrees.mobi>
 * 
 * ********************************************************************* */

using System.Text.RegularExpressions;
using System.Web;
using System;
using System.Collections.Generic;

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
                case Constants.ORIGINAL_URL_SPECIAL_PROPERTY:
                    return context.Items[Constants.ORIGINAL_URL_KEY] as string;
            }
            return null;
        }

        #endregion

    }
}
