/* *********************************************************************
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0.
 * 
 * If a copy of the MPL was not distributed with this file, You can obtain
 * one at http://mozilla.org/MPL/2.0/.
 * 
 * This Source Code Form is “Incompatible With Secondary Licenses”, as
 * defined by the Mozilla Public License, v. 2.0.
 * ********************************************************************* */

#region Usings

using System.Configuration;

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
        /// Gets or sets the path to access the binary file.
        /// </summary>
        [ConfigurationProperty("binaryFilePath", IsRequired = false)]
        [StringValidator(InvalidCharacters = "!@#$%^&*()[]{};'\"|", MaxLength = 255)]
        internal string BinaryFilePath
        {
            get { return (string)this["binaryFilePath"]; }
            set { this["binaryFilePath"] = value; }
        }

        /// <summary>
        /// Real usage information provides 51Degrees.mobi insight to improve this products
        /// performance and identify new or less popular devices quickly. It is amalgamated
        /// with other data sources to bring you this solution. We ask you to leave this 
        /// property set to true.
        /// </summary>
        [ConfigurationProperty("shareUsage", IsRequired = false, DefaultValue = "true")]
        internal bool ShareUsage
        {
            get { return (bool)this["shareUsage"]; }
            set { this["shareUsage"] = value; }
        }

        #endregion
    }
}