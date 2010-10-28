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
 *     Thomas Holmes <thomasholmes_5@hotmail.com>
 * 
 * ********************************************************************* */

using System.Configuration;
using System.Web;

namespace FiftyOne.Foundation.Mobile.Configuration
{
    /// <summary>
    /// Contains information for all location specified in the <c>web.config</c> file.
    /// </summary>
    public sealed class LocationElement : ConfigurationElementCollection
    {
        # region Properties

        /// <summary>
        /// Gets the url of the webpage
        /// </summary>
        [ConfigurationProperty("url", IsRequired = true, IsKey = true)]
        public string Url
        {
            get
            {
                return (string)this["url"];
            }
        }

        /// <summary>
        /// Gets the url of the webpage
        /// </summary>
        [ConfigurationProperty("replaceExpression", IsRequired = false)]
        public string ReplaceExpression
        {
            get
            {
                return (string)this["replaceExpression"];
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this homepage should be used.
        /// </summary>
        [ConfigurationProperty("enabled", IsRequired = false, DefaultValue = true)]
        public bool Enabled
        {
            get { return (bool)this["enabled"]; }
        }
        
        #endregion

        #region ConfigurationElementCollection Members

        protected override ConfigurationElement CreateNewElement()
        {
            return new FilterElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((FilterElement)element).Property;
        }

        #endregion
    }
}
