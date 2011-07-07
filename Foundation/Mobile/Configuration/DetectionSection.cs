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

using System.Configuration;

namespace FiftyOne.Foundation.Mobile.Configuration
{
    /// <summary>
    /// Configuration section for generic detection settings shared across 
    /// all detection providers.
    /// </summary>
    public sealed class DetectionSection : ConfigurationSection
    {
        /// <summary>
        /// Returns true if the functionality of <see cref="DetectionSection"/> is enabled.
        /// </summary>
        internal bool Enabled
        {
            get { return ElementInformation.IsPresent; }
        }

        /// <summary>
        /// Gets the device detection provider to be used.
        /// </summary>
        [ConfigurationProperty("provider", IsRequired = true)]
        [RegexStringValidator("WURFL|Framework")]
        public string Provider
        {
            get { return (string)this["provider"]; } 
        }

        /// <summary>
        /// Gets the URL to send new device information to.
        /// </summary>
        /// <remarks>
        /// If provided new device information will be sent to the URL.
        /// </remarks>
        [ConfigurationProperty("newDevicesURL", IsRequired = false)]
        [StringValidator(InvalidCharacters = "!@#$%^&*()[]{};'\"|", MaxLength = 255)]
        public string NewDevicesURL
        {
            get { return (string)this["newDevicesURL"]; }
        }

        /// <summary>
        /// Determines the level of detail recoreded for new devices and the 
        /// associated HTTP request. Valid values are:
        ///     minimum - only the wap profile and useragent are recorded.
        ///     maximum - all the HTTP headers are recorded.
        /// </summary>
        [ConfigurationProperty("newDeviceDetail", IsRequired = false, DefaultValue = "minimum")]
        [StringValidator(InvalidCharacters = "!@#$%^&*()[]{};'\"|", MaxLength = 7)]
        public string NewDeviceDetail
        {
            get { return (string)this["newDeviceDetail"]; }
        }
    }
}
