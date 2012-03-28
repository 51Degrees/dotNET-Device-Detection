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
    /// A collection of <see cref="UrlCollection"/>. This class cannot be inherited.
    /// </summary>
    public sealed class UrlCollection : ConfigurationElementCollection
    {
        #region Constructors

        #endregion

        #region Methods

        /// <summary>
        /// Creates a new instance of <see cref="UrlElement"/>.
        /// </summary>
        protected override ConfigurationElement CreateNewElement()
        {
            return new UrlElement();
        }

        /// <summary>
        /// Gets the element key value.
        /// </summary>
        protected override Object GetElementKey(ConfigurationElement element)
        {
            return ((UrlElement) element).Url;
        }

        /// <summary>
        /// Add element to the base collection.
        /// </summary>
        protected override void BaseAdd(ConfigurationElement element)
        {
            BaseAdd(element, false);
        }

        /// <summary>
        /// Gets the index of the specific element inside the collection.
        /// </summary>
        /// <param name="element">a <see cref="UrlElement"/> to locate in the the collection.</param>
        /// <returns>The index of the element in the collection.</returns>
        /// <exception cref="System.ArgumentNullException"> thrown if <paramref name="element"/> equals null.</exception>
        public int IndexOf(UrlElement element)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            return BaseIndexOf(element);
        }

        /// <summary>
        /// Add element into the collection.
        /// </summary>
        /// <param name="element">a <see cref="UrlElement"/> to add to the collection.</param>
        /// <exception cref="System.ArgumentNullException"> thrown if <paramref name="element"/> equals null.</exception>
        public void Add(UrlElement element)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            BaseAdd(element);
        }

        /// <summary>
        /// Removes a <see cref="UrlElement"/> from the collection.
        /// </summary>
        /// <param name="element">a <see cref="UrlElement"/> to remove from the collection.</param>
        /// <exception cref="System.ArgumentNullException"> thrown if <paramref name="element"/> equals null.</exception>
        public void Remove(UrlElement element)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            if (BaseIndexOf(element) >= 0)
                BaseRemove(element.Url);
        }

        /// <summary>
        /// Removes a <see cref="UrlElement"/> at the specified index location.
        /// </summary>
        /// <param name="index">Index of the element to remove in the collection.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if <paramref name="index"/> is less than zero or bigger than the number of indexes on the collections.</exception>
        public void RemoveAt(int index)
        {
            if ((index < 0) || (index > base.Count - 1))
                throw new ArgumentOutOfRangeException("index");
            BaseRemoveAt(index);
        }

        /// <summary>
        /// Removes a <see cref="System.Configuration.ConfigurationElement"/> from the collection.
        /// </summary>
        /// <param name="url">Url of the element in the collection to remove.</param>
        /// <exception cref="System.ArgumentNullException"> thrown if <paramref name="url"/> equals null.</exception>
        public void Remove(string url)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException("url");

            BaseRemove(url);
        }

        /// <summary>
        /// Removes all configuration elements from the collection.
        /// </summary>
        public void Clear()
        {
            BaseClear();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="UrlElement"/>.
        /// </summary>
        public UrlElement this[int index]
        {
            get { return (UrlElement) BaseGet(index); }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="UrlElement"/>.
        /// </summary>        
        public new UrlElement this[string name]
        {
            get { return (UrlElement) BaseGet(name); }
        }

        #endregion
    }
}