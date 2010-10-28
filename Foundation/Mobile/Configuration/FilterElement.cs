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

using System.Configuration;

namespace FiftyOne.Foundation.Mobile.Configuration
{
    public sealed class FilterElement : ConfigurationElement
    {
        #region Properties

        /// <summary>
        /// Gets or sets the name of the property. 
        /// </summary>
        [ConfigurationProperty("property", IsRequired = true, IsKey = true)]
        public string Property
        {
            get { return (string)this["property"]; }
            set { this["property"] = value; }
        }

        /// <summary>
        /// Gets or sets the expression to be used to determine a match. 
        /// </summary>
        [ConfigurationProperty("matchExpression", IsRequired = true)]
        public string MatchExpression
        {
            get { return (string)this["matchExpression"]; }
            set { this["matchExpression"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this filter should be used.
        /// </summary>
        [ConfigurationProperty("enabled", IsRequired = false, DefaultValue = true)]
        public bool Enabled
        {
            get { return (bool)this["enabled"]; }
        }

        #endregion
    }
}
