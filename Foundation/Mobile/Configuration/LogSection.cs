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

#region

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