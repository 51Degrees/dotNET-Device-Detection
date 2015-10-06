/* *********************************************************************
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited. 
 * Copyright © 2015 51Degrees Mobile Experts Limited, 5 Charlotte Close,
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
using System.Text;
using System.Xml;

#endregion

namespace FiftyOne.Foundation.Mobile.Configuration
{
    /// <summary>
    /// Settings for automatic redirection of mobile devices.
    /// </summary>
    public sealed class RedirectSection : ConfigurationSection
    {
        #region Constructors

        /// <summary>
        /// Default constructor for an instance of <see cref="RedirectSection"/>
        /// </summary>
        public RedirectSection() {}

        #endregion

        #region Methods

        /// <summary>
        /// Simple settings to remove the declaration.
        /// </summary>
        private XmlWriterSettings Settings
        {
            get
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.OmitXmlDeclaration = true;
                return settings;
            }
        }

        /// <summary>
        /// Returns the XML that needs to be written to the configuration file.
        /// </summary>
        /// <returns></returns>
        internal string GetXmlElement()
        {
            StringBuilder sb = new StringBuilder();
            using (XmlWriter writer = XmlWriter.Create(sb, Settings))
                base.SerializeToXmlElement(writer, "redirect");
            return sb.ToString();
        }

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
            set { this["devicesFile"] = value; }
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
            set { this["timeout"] = value; }
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
            set { this["firstRequestOnly"] = value; }
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
            set { this["originalUrlAsQueryString"] = value; }
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
            set { this["mobileHomePageUrl"] = value; }
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
            set { this["mobilePagesRegex"] = value; }
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
            set
            {
                this["locations"] = value;
            }
        }
        
        #endregion
    }
}