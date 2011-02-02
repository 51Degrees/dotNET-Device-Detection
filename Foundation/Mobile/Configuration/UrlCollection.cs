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