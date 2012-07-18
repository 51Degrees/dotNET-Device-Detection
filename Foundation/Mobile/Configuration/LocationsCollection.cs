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

using System;
using System.Configuration;

#endregion

namespace FiftyOne.Foundation.Mobile.Configuration
{
    /// <summary>
    /// A collection of <see cref="LocationElement"/> objects. This class cannot be inherited.
    /// </summary>
    public sealed class LocationsCollection : ConfigurationElementCollection
    {
        #region Methods

        /// <summary>
        /// Creates a new <see cref="LocationElement"/> for the collection
        /// </summary>
        protected override ConfigurationElement CreateNewElement()
        {
            return new LocationElement();
        }

        /// <summary>
        /// Get the element key. Check for empty strings and return null
        /// to avoid a problem with the defaultvalue property of the key
        /// element becoming an empty string and causing a duplicate key
        /// exception within .NET.
        /// </summary>
        protected override object GetElementKey(ConfigurationElement element)
        {
            string key = ((LocationElement)element).Name;
            if (String.IsNullOrEmpty(key))
                return ((LocationElement)element).UniqueId;
            return key;
        }

        #endregion 

        #region Internal Methods

        /// <summary>
        /// Adds a new element to the collection.
        /// </summary>
        /// <param name="element">Element to be added.</param>
        internal void Add(LocationElement element)
        {
            base.BaseAdd(element);
        }

        /// <summary>
        /// Removes all elements from the configuration.
        /// </summary>
        internal void Clear()
        {
            base.BaseClear();
        }

        #endregion
    }
}

