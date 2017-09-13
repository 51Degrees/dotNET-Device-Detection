/* *********************************************************************
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited. 
 * Copyright © 2017 51Degrees Mobile Experts Limited, 5 Charlotte Close,
 * Caversham, Reading, Berkshire, United Kingdom RG4 7BY
 * 
 * This Source Code Form is the subject of the following patents and patent 
 * applications, owned by 51Degrees Mobile Experts Limited of 5 Charlotte
 * Close, Caversham, Reading, Berkshire, United Kingdom RG4 7BY: 
 * European Patent No. 2871816;
 * European Patent Application No. 17184134.9;
 * United States Patent Nos. 9,332,086 and 9,350,823; and
 * United States Patent Application No. 15/686,066.
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

namespace FiftyOne.Foundation.Mobile.Configuration
{
    /// <summary>
    /// Configuration settings for the log file creation and detail.
    /// </summary>
    public class LogSection : ConfigurationSection
    {
        #region Constructors

        #endregion

        #region Properties

        /// <summary>
        /// Returns true if the functionality of <see cref="LogSection"/> is enabled.
        /// </summary>
        internal bool Enabled
        {
            get { return ElementInformation.IsPresent; }
        }

        /// <summary>
        /// Log file used by Log class to record events. If not provided no logging will occur.
        /// </summary>
        [ConfigurationProperty("logFile", IsRequired = false)]
        [StringValidator(InvalidCharacters = "!@#$%^&*()[]{};'\"|", MaxLength = 255)]
        public string LogFile
        {
            get { return (string) this["logFile"]; }
        }

        /// <summary>
        /// The level to use when writing log entries. Valid values are:
        ///     Debug
        ///     Info
        ///     Warn
        ///     Fatal
        /// </summary>
        [ConfigurationProperty("logLevel", IsRequired = false)]
        [StringValidator(InvalidCharacters = "!@#$%^&*()[]{};'\"|", MaxLength = 5)]
        public string LogLevel
        {
            get { return (string) this["logLevel"]; }
        }

        /// <summary>
        /// <para>
        /// Optional attribute <c>activityLogDirectory</c>.
        /// </para>
        /// </summary>
        /// <para>
        /// If specified log files for page activity will be output to this directory. Unlike
        /// IIS log files the mobile number and mobile device information will also be included.
        /// </para>
        /// <para>
        /// Files will be written for each UTC day in the format YYYYMMDD.txt.
        /// </para>
        [ConfigurationProperty("pageLogDirectory", IsRequired = false)]
        [StringValidator(InvalidCharacters = "!@#$%^&*()[]{};'\"|", MaxLength = 255)]
        public string PageLogDirectory
        {
            get { return Support.GetFilePath((string) this["pageLogDirectory"]); }
        }

        #endregion
    }
}