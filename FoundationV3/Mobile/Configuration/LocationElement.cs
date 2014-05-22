/* *********************************************************************
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited. 
 * Copyright © 2014 51Degrees Mobile Experts Limited, 5 Charlotte Close,
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

using System;
using System.Configuration;

#endregion

namespace FiftyOne.Foundation.Mobile.Configuration
{
    /// <summary>
    /// Contains information for all location specified in the <c>web.config</c> file.
    /// </summary>
    public sealed class LocationElement : ConfigurationElementCollection
    {
        #region Fields

        private readonly Guid _uniqueId = Guid.NewGuid();

        #endregion

        #region Properties

        /// <summary>
        /// Used as the internal unique key when the name is empty or null.
        /// </summary>
        internal Guid UniqueId
        {
            get { return _uniqueId; }
        }

        /// <summary>
        /// Gets or sets the unique name of the redirection element.
        /// </summary>
        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        public string Name
        {
            get
            {
                return (string) this["name"];
            }
            set
            {
                this["name"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the url of the webpage
        /// </summary>
        [ConfigurationProperty("url", IsRequired = true, IsKey = false)]
        public string Url
        {
            get
            {
                return (string)this["url"];
            }
            set
            {
                this["url"] = value;
            }
        }

        /// <summary>
        /// A regular expression used to find macthing elements of the 
        /// original URL associated with the request.
        /// </summary>
        [ConfigurationProperty("matchExpression", IsRequired = false)]
        public string MatchExpression
        {
            get
            {
                return (string)this["matchExpression"];
            }
            set
            {
                this["matchExpression"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this homepage should be used.
        /// </summary>
        [ConfigurationProperty("enabled", IsRequired = false, DefaultValue = true)]
        public bool Enabled
        {
            get 
            { 
                return (bool)this["enabled"]; 
            }
            set
            {
                this["enabled"] = value;
            }
        }
        
        #endregion

        #region ConfigurationElementCollection Members

        /// <summary>
        /// Creates a new instance of <see cref="FilterElement"/>.
        /// </summary>
        protected override ConfigurationElement CreateNewElement()
        {
            return new FilterElement();
        }

        /// <summary>
        /// Get the element key. Check for empty strings and return null
        /// to avoid a problem with the defaultvalue property of the key
        /// element becoming an empty string and causing a duplicate key
        /// exception within .NET.
        /// </summary>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return element.GetHashCode();
        }

        #endregion

        #region Internal Members

        /// <summary>
        /// Adds a new filter to the location collection.
        /// </summary>
        /// <param name="element"></param>
        internal void Add(FilterElement element)
        {
            BaseAdd(element);
        }

        #endregion
    }
}
