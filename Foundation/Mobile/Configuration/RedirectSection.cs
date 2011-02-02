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
 *     Thomas Holmes <tom@51degrees.mobi>
 * 
 * ********************************************************************* */

#region Usings

using System.Configuration;

#endregion

namespace FiftyOne.Foundation.Mobile.Configuration
{
    /// <summary>
    /// Settings for automatic redirection of mobile devices.
    /// </summary>
    public class RedirectSection : ConfigurationSection
    {
        #region Constructors

        #endregion

        #region Properties

        /// <summary>
        /// Returns true if the functionality of <see cref="RedirectSection"/> is enabled.
        /// </summary>
        internal bool Enabled
        {
            get { return ElementInformation.IsPresent; }
        }

        /// <summary>
        /// A file used to store the details of devices that have previously accessed the web
        /// site to determine if they're making a subsequent request. Needed to ensure 
        /// multiple worker processes have a consistent view of previous activity. 
        /// (Optional – random behaviour will be experienced if not specified on web sites
        /// with more than one worker processes)
        /// </summary>
        [ConfigurationProperty("devicesFile", IsRequired = false)]
        [StringValidator(InvalidCharacters = "!@#$%^&*()[]{};'\"|", MaxLength = 255)]
        public string DevicesFile
        {
            get { return (string) this["devicesFile"]; }
        }

        /// <summary>
        /// The number of minutes of inactivity that should occur before the requesting
        /// device should be treated as making a new request to the web site for the
        /// purposes of redirection. If the session is available the session timeout
        /// will be used and override this value. (Optional - defaults to 20 minutes)
        /// </summary>
        [ConfigurationProperty("timeout", IsRequired = false, DefaultValue = "20")]
        public int Timeout
        {
            get { return (int) this["timeout"]; }
        }

        /// <summary>
        /// If set to true only the first request received by the web site is redirected
        /// to the mobileUrl when the site is accessed from a mobile device.
        /// (Optional – defaults to true)
        /// </summary>
        [ConfigurationProperty("firstRequestOnly", IsRequired = false, DefaultValue = "true")]
        public bool FirstRequestOnly
        {
            get { return (bool) this["firstRequestOnly"]; }
        }

        /// <summary>
        /// This attribute is used when redirecting to a mobile home page.
        /// If set to true the original URL will be encoded and set
        /// as the query string for the <see cref="MobileHomePageUrl"/>.
        /// (Optional – defaults to false)
        /// </summary>
        [ConfigurationProperty("originalUrlAsQueryString", IsRequired = false, DefaultValue = "false")]
        public bool OriginalUrlAsQueryString
        {
            get { return (bool) this["originalUrlAsQueryString"]; }
        }

        /// <summary>
        /// Previously mobileRedirectUrl under the mobile/toolkit element. A url to direct 
        /// mobile devices to instead of the normal web sites landing page. (Optional)
        /// </summary>
        [ConfigurationProperty("mobileHomePageUrl", IsRequired = false, DefaultValue = "")]
        [StringValidator(MaxLength = 255)]
        public string MobileHomePageUrl
        {
            get { return (string) this["mobileHomePageUrl"]; }
        }

        /// <summary>
        /// A regular expression used to identify pages on the web site that are designed 
        /// for mobile phones. Any page that derives from System.Web.UI.MobileControls.MobilePage 
        /// will automatically be treated as a mobile page. Use this attribute to tell redirection
        /// about mobile pages derived from other base classes such as System.Web.UI.Page.
        /// The value is evaluated against the HttpRequests AppRelativeCurrentExecutionFilePath and
        /// Url properties.
        /// Redirection needs to be aware of mobile pages so that requests to these pages can be
        /// ignored. (Optional)
        /// </summary>
        [ConfigurationProperty("mobilePagesRegex", IsRequired = false, DefaultValue = "")]
        [StringValidator(MaxLength = 2048)]
        public string MobilePagesRegex
        {
            get { return (string) this["mobilePagesRegex"]; }
        }

        /// <summary>
        /// An optional collection of homepages that uses regular expressions to see if a mobile 
        /// device should be redirected to one of these pages. A device is defined in terms of 
        /// the manufacturer and model.
        /// </summary>
        [ConfigurationProperty("locations", IsRequired = false, IsDefaultCollection = true)]
        [ConfigurationCollection(typeof(LocationsCollection), AddItemName = "location")]
        public LocationsCollection Locations
        {
            get
            {
                return (LocationsCollection)this["locations"];
            }
        }
        #endregion
    }
}