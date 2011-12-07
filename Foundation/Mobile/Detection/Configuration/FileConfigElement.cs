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

namespace FiftyOne.Foundation.Mobile.Detection.Configuration
{
    /// <summary>
    /// Defines configuration settings for data files. This class cannot be inherited.
    /// </summary>
    internal sealed class FileConfigElement : ConfigurationElement
    {
        #region Properties

        // Note: The default value is set for required unique properties to prevent
        // .NET throwing an exception when validating the default value.

        /// <summary>
        /// Gets or sets the name of the file. 
        /// </summary>
        [ConfigurationProperty("name", IsRequired = true, IsKey = true, DefaultValue="name")]
        [StringValidator(InvalidCharacters = "!@#$%^&*()[]{};'\"|", MinLength = 1, MaxLength = 60)]
        internal string Name
        {
            get { return (string) this["name"]; }
            set { this["name"] = value; }
        }

        /// <summary>
        /// Gets or sets the file path.
        /// </summary>
        [ConfigurationProperty("filePath", IsRequired = true, DefaultValue="filePath")]
        [StringValidator(InvalidCharacters = "!@#$%^&*()[]{};'\"|", MinLength = 1, MaxLength = 255)]
        internal string FilePath
        {
            get { return (string) this["filePath"]; }
            set { this["filePath"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this file patch should be used.
        /// </summary>
        [ConfigurationProperty("enabled", IsRequired = false, DefaultValue = true)]
        internal bool Enabled
        {
            get { return (bool) this["enabled"]; }
            set { this["enabled"] = value; }
        }

        #endregion
    }
}