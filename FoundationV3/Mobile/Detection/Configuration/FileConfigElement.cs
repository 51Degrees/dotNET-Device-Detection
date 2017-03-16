/* *********************************************************************
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited. 
 * Copyright © 2017 51Degrees Mobile Experts Limited, 5 Charlotte Close,
 * Caversham, Reading, Berkshire, United Kingdom RG4 7BY
 * 
 * This Source Code Form is the subject of the following patent 
 * applications, owned by 51Degrees Mobile Experts Limited of 5 Charlotte
 * Close, Caversham, Reading, Berkshire, United Kingdom RG4 7BY: 
 * European Patent Application No. 13192291.6; and
 * United States Patent Application Nos. 14/085,223 and 14/085,301.
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