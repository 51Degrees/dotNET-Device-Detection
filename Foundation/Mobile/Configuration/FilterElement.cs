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
using System;

#endregion

namespace FiftyOne.Foundation.Mobile.Configuration
{
    /// <summary>
    /// Contains information for all filters of a location element in the
    /// <c>web.config</c> file.
    /// </summary>
    public sealed class FilterElement : ConfigurationElement
    {
        #region Fields

        private readonly Guid _uniqueId = Guid.NewGuid();

        #endregion

        #region Properties

        /// <summary>
        /// Used as the internal unique key when the property is empty or null.
        /// </summary>
        internal Guid UniqueId
        {
            get { return _uniqueId; }
        }

        /// <summary>
        /// Gets or sets the name of the property. 
        /// </summary>
        [ConfigurationProperty("property", IsRequired = true, IsKey = true)]
        public string Property
        {
            get { return (string)this["property"]; }
            set { this["property"] = value; }
        }

        /// <summary>
        /// Gets or sets the expression to be used to determine a match. 
        /// </summary>
        [ConfigurationProperty("matchExpression", IsRequired = true)]
        public string MatchExpression
        {
            get { return (string)this["matchExpression"]; }
            set { this["matchExpression"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this filter should be used.
        /// </summary>
        [ConfigurationProperty("enabled", IsRequired = false, DefaultValue = true)]
        public bool Enabled
        {
            get { return (bool)this["enabled"]; }
            set { this["enabled"] = value; }
        }

        #endregion
    }
}
