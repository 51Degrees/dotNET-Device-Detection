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
using System.Text;
using System.Xml;

#endregion

namespace FiftyOne.Foundation.Mobile.Configuration
{
    /// <summary>
    /// Settings for image optimiser.
    /// </summary>
    public sealed class ImageOptimisationSection : ConfigurationSection
    {
        #region Constructors

        /// <summary>
        /// Default constructor for an instance of <see cref="RedirectSection"/>
        /// </summary>
        public ImageOptimisationSection() { }

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
        /// <returns>XML</returns>
        internal string GetXmlElement()
        {
            StringBuilder sb = new StringBuilder();
            using (XmlWriter writer = XmlWriter.Create(sb, Settings))
                base.SerializeToXmlElement(writer, "imageOptimiser");
            return sb.ToString();
        }

        #endregion

        #region Properties
        
        /// <summary>
        /// Returns true if the functionality of <see cref="ImageOptimisationSection"/> is enabled.
        /// </summary>
        [ConfigurationProperty("enabled", IsRequired = false, DefaultValue = "true")]
        internal bool Enabled
        {
            get { return ElementInformation.IsPresent && (bool)this["enabled"]; }
            set { SetImageOptimisation(value); WebConfig.SetWebConfigurationModules(); }
        }

        /// <summary>
        /// The maximum width in pixels any image is allowed to be resized to. 0 indicates
        /// no limit. (Optional - defaults to 0)
        /// </summary>
        [ConfigurationProperty("maxWidth", IsRequired = false, DefaultValue = "0")]
        public int MaxWidth
        {
            get { return (int)this["maxWidth"]; }
            set { this["maxWidth"] = value; }
        }

        /// <summary>
        /// The maximum height in pixels any image is allowed to be resized to. 0 indicates
        /// no limit. (Optional - defaults to 0)
        /// </summary>
        [ConfigurationProperty("maxHeight", IsRequired = false, DefaultValue = "0")]
        public int MaxHeight
        {
            get { return (int)this["maxHeight"]; }
            set { this["maxHeight"] = value; }
        }

        /// <summary>
        /// Defines what the width parameter should be in image url.
        /// (Optional - defaults to width)
        /// </summary>
        [ConfigurationProperty("widthParam", IsRequired = false, DefaultValue = "w")]
        public string WidthParam
        {
            get { return (string)this["widthParam"]; }
            set { this["widthParam"] = value; }
        }

        /// <summary>
        /// Defines what the height parameter should be in image url.
        /// (Optional - defaults to height)
        /// </summary>
        [ConfigurationProperty("heightParam", IsRequired = false, DefaultValue = "h")]
        public string HeightParam
        {
            get { return (string)this["heightParam"]; }
            set { this["heightParam"] = value; }
        }

        /// <summary>
        /// Defines a common factor the image dimensions must adhere to. Dimensions are rounded.
        /// (Optional - defaults to 1)
        /// </summary>
        [ConfigurationProperty("factor", IsRequired = false, DefaultValue = "1")]
        public int Factor
        {
            get { return (int)this["factor"]; }
            set { this["factor"] = value; }
        }

        /// <summary>
        /// Defines a value to set to width or height if they set as auto.
        /// (Optional - defaults to 50)
        /// </summary>
        [ConfigurationProperty("defaultAuto", IsRequired = false, DefaultValue = "50")]
        public int DefaultAuto
        {
            get { return (int)this["defaultAuto"]; }
            set { this["defaultAuto"] = value; }
        }
                
        #endregion

        #region Methods
        
        /// <summary>
        /// Sets the image optimisation value.
        /// </summary>
        /// <param name="value">True or false depending on whether
        /// image optimisation should be enabled.</param>
        private void SetImageOptimisation(bool value)
        {
            this["enabled"] = value;
            Support.SetWebApplicationSection(this);
        }
                
        #endregion
    }
}