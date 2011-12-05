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
 *     Andy Allan <andy.allan@mobgets.com>
 * 
 * ********************************************************************* */

#region Usings

using System.Configuration;
using FiftyOne.Foundation.Mobile.Configuration;

#endregion

namespace FiftyOne.Foundation.Mobile.Detection.Configuration
{
    /// <summary>
    /// Configures the detection section. This class cannot be inherited.
    /// </summary>
    internal sealed class DetectionSection : ConfigurationSection
    {
        #region Properties

        /// <summary>
        /// Gets the collection of xml files to be processed.
        /// </summary>
        [ConfigurationProperty("xmlFiles", IsRequired = false)]
        internal FilesCollection XmlFiles
        {
            get { return (FilesCollection)this["xmlFiles"]; }
        }

        /// <summary>
        /// Gets the path to access the binary file.
        /// </summary>
        [ConfigurationProperty("binaryFilePath", IsRequired = false)]
        [StringValidator(InvalidCharacters = "!@#$%^&*()[]{};'\"|", MaxLength = 255)]
        internal string BinaryFilePath
        {
            get { return (string)this["binaryFilePath"]; }
        }

        #endregion
    }
}