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

#region Usings

using System.Configuration;

#endregion

namespace FiftyOne.Foundation.Mobile.Configuration
{
    /// <summary>
    /// Used in the <c>web.config</c> to provide Urls and refresh periods to automatically
    /// update application data files such as Wurfl data files.
    /// </summary>
    public sealed class UrlElement : ConfigurationElement
    {
        #region Constructors

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the url.
        /// </summary>
        [ConfigurationProperty("url", IsRequired = true)]
        [StringValidator(InvalidCharacters = "!@#$%^*()[]{};'\"|", MaxLength = 255)]
        public string Url
        {
            get { return (string) this["url"]; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this url should be used.
        /// </summary>
        [ConfigurationProperty("days", IsRequired = false, DefaultValue = 7)]
        public int Days
        {
            get { return (int) this["days"]; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this url should be used.
        /// </summary>
        [ConfigurationProperty("enabled", IsRequired = false, DefaultValue = true)]
        public bool Enabled
        {
            get { return (bool) this["enabled"]; }
        }

        #endregion
    }
}