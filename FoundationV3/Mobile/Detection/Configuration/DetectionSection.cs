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
    /// Configures the detection section. This class cannot be inherited.
    /// </summary>
    internal sealed class DetectionSection : ConfigurationSection
    {
        #region Properties

        /// <summary>
        /// Determines if device detection should be enabled.
        /// </summary>
        [ConfigurationProperty("enabled", IsRequired = false, DefaultValue = "true")]
        internal bool Enabled
        {
            get { return ElementInformation.IsPresent && (bool)this["enabled"]; }
            set { this["enabled"] = value; }
        }

        /// <summary>
        /// Determines if device data should automatically be updated when a
        /// licence key is provided.
        /// </summary>
        [ConfigurationProperty("autoUpdate", IsRequired = false, DefaultValue = "true")]
        internal bool AutoUpdate
        {
            get { return (bool)this["autoUpdate"]; }
            set { this["autoUpdate"] = value; }
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

        /// <summary>
        /// True if the data set should be loaded into memory. Detection performance will be
        /// significantly faster at the expense of a longer startup time and increased
        /// memory usage.
        /// </summary>
        [ConfigurationProperty("memoryMode", IsRequired = false, DefaultValue = "false")]
        internal bool MemoryMode
        {
            get { return (bool)this["memoryMode"]; }
            set { this["memoryMode"] = value; }
        }

        /// <summary>
        /// When set to true enables bandwidth monitoring. Also requires the data set specified in 
        /// the binaryFilePath attribute to support bandwidth monitoring. 
        /// </summary>
        [ConfigurationProperty("bandwidthMonitoringEnabled", IsRequired = false, DefaultValue = "true")]
        internal bool BandwidthMonitoringEnabled
        {
            get { return (bool)this["bandwidthMonitoringEnabled"]; }
            set { this["bandwidthMonitoringEnabled"] = value; }
        }

        /// <summary>
        /// When set to true enables feature detection. Also requires the data set specified in the
        /// binaryFilePath attribute to support feature detection. 
        /// </summary>
        [ConfigurationProperty("featureDetectionEnabled", IsRequired = false, DefaultValue = "true")]
        internal bool FeatureDetectionEnabled
        {
            get { return (bool)this["featureDetectionEnabled"]; }
            set { this["featureDetectionEnabled"] = value; }
        }

        #endregion
    }
}