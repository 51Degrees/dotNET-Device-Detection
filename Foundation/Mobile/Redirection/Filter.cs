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

            // Try the standard properties of the browser object.
            Type controlType = context.Request.Browser.GetType();
            System.Reflection.PropertyInfo propertyInfo = controlType.GetProperty(property);
            if (propertyInfo != null && propertyInfo.CanRead)
                return propertyInfo.GetValue(context.Request.Browser, null).ToString();

            // Try browser capabilities next.
            value = context.Request.Browser[property];
            if (value != null)
                return value;

            // Try the properties of the request.
            controlType = context.Request.GetType();
            propertyInfo = controlType.GetProperty(property);
            if (propertyInfo != null && propertyInfo.CanRead)
                return propertyInfo.GetValue(context.Request, null).ToString();

            // If not then try and return the value for the collection.
            return context.Request.Browser[property];
        }

        #endregion

    }
}
