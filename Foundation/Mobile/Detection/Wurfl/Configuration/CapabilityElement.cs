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

#region Usings

using System.Configuration;

#endregion

namespace FiftyOne.Foundation.Mobile.Detection.Wurfl.Configuration
{
    /// <summary>
    /// Defines configuration settings for capabilities. This class cannot be inherited.
    /// </summary>
    internal sealed class CapabilityElement : ConfigurationElement
    {
        #region Constructors

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the capability's name.
        /// </summary>
        [ConfigurationProperty("capabilityName", DefaultValue = "model_name", IsRequired = true, IsKey = true)]
        [StringValidator(InvalidCharacters = ".!@#$%^&*()[]{};'\"|\\", MinLength = 1, MaxLength = 255)]
        internal string CapabilityName
        {
            get { return (string) this["capabilityName"]; }
            set { this["capabilityName"] = value; }
        }

        #endregion
    }
}