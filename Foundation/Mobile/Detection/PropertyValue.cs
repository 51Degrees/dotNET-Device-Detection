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

using System;

namespace FiftyOne.Foundation.Mobile.Detection
{
    /// <summary>
    /// Describes a name that can be assigned to a property.
    /// </summary>
    public class PropertyValue
    {
        #region Fields

        private int _nameStringIndex = -1;
        private string _name;
        private string _description;
        private Uri _url;
        private Provider _provider;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs an instance of <see cref="PropertyValue"/>.
        /// </summary>
        /// <param name="provider">The provider the property was created from.</param>
        /// <param name="name">The string name.</param>
        internal PropertyValue(Provider provider, string name)
        {
            _provider = provider;
            _name = name;
        }

        /// <summary>
        /// Constructs an instance of <see cref="PropertyValue"/>.
        /// </summary>
        /// <param name="provider">The provider the property was created from.</param>
        /// <param name="name">The string name.</param>
        /// <param name="description">The description of the name.</param>
        internal PropertyValue(Provider provider, string name, string description)
            : this(provider, name)
        {
            _description = description;
        }

        /// <summary>
        /// Constructs an instance of <see cref="PropertyValue"/>.
        /// </summary>
        /// <param name="provider">The provider the property was created from.</param>
        /// <param name="name">The string name.</param>
        /// <param name="description">The description of the name.</param>
        /// <param name="url">An optional URL linking to more information about the name.</param>
        internal PropertyValue(Provider provider, string name, string description, string url)
            : this(provider, name, description)
        {
            Uri.TryCreate(url, UriKind.Absolute, out _url);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Returns the string index of the name.
        /// </summary>
        internal int NameStringIndex
        {
            get 
            { 
                if (_nameStringIndex < 0)
                    _nameStringIndex = _provider.Strings.Add(Name);
                return _nameStringIndex;
            }
        }

        /// <summary>
        /// The string name.
        /// </summary>
        public string Name { get { return _name; } }

        /// <summary>
        /// A description of the name.
        /// </summary>
        public string Description { get { return _description; } }

        /// <summary>
        /// A url to more information about the name.
        /// </summary>
        public Uri Url { get { return _url; } }

        /// <summary>
        /// The provider the property or value is associated with.
        /// </summary>
        public Provider Provider { get { return _provider; } }

        #endregion
    }
}