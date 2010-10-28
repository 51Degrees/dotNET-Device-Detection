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
 * Mobile Experts Limited are Copyright (C) 2009 - 2010. All Rights Reserved.
 * 
 * Contributor(s):
 *     James Rosewell <james@51degrees.mobi>
 * 
 * ********************************************************************* */

#region

using System;
using System.Configuration;

#endregion

namespace FiftyOne.Foundation.Mobile.Detection.Wurfl.Configuration
{
    /// <summary>
    /// A collection of <see cref="CapabilityElement"/>. This class cannot be inherited.
    /// </summary>
    public sealed class CapabilityCollection : ConfigurationElementCollection
    {
        #region Constructors

        #endregion

        #region Methods

        /// <summary>
        /// Creates a new instance of <see cref="PatchesCollection"/>.
        /// </summary>
        protected override ConfigurationElement CreateNewElement()
        {
            return new CapabilityElement();
        }

        /// <summary>
        /// Gets the element key value.
        /// </summary>
        protected override Object GetElementKey(ConfigurationElement element)
        {
            return ((CapabilityElement) element).CapabilityName;
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
        /// <param name="capability">The capability whos index is being sought.</param>
        /// <returns>The index of the capability requested.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="capability"/> equals null.</exception>
        public int IndexOf(CapabilityElement capability)
        {
            if (capability == null)
                throw new ArgumentNullException("capability");

            return BaseIndexOf(capability);
        }

        /// <summary>
        /// Add element into the collection.
        /// </summary>
        /// <param name="capability">The capability to be added to the whitelist.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="capability"/> equals null.</exception>
        public void Add(CapabilityElement capability)
        {
            if (capability == null)
                throw new ArgumentNullException("capability");

            BaseAdd(capability);
        }

        /// <summary>
        /// Removes a <typeparamref name="System.Configuration.ConfigurationElement"/> from the collection.
        /// </summary>
        /// <param name="capability">The capability to be removed from the whitelist.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="capability"/> equals null.</exception>
        public void Remove(CapabilityElement capability)
        {
            if (capability == null)
                throw new ArgumentNullException("capability");

            if (BaseIndexOf(capability) >= 0)
                BaseRemove(capability.CapabilityName);
        }

        /// <summary>
        /// Removes a <typeparamref name="System.Configuration.ConfigurationElement"/> at the specified index location.
        /// </summary>
        /// <param name="index">The index of the element to be removed.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if <paramref name="index"/> is less than zero or bigger than the number of indexes on the collections.</exception>
        public void RemoveAt(int index)
        {
            if ((index < 0) || (index > base.Count - 1))
                throw new ArgumentOutOfRangeException("index");

            BaseRemoveAt(index);
        }

        /// <summary>
        /// Removes a <typeparamref name="System.Configuration.ConfigurationElement"/> from the collection.
        /// </summary>
        /// <param name="capabilityName">The name of the capability to be removed.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="capabilityName"/> equals null.</exception>
        public void Remove(string capabilityName)
        {
            if (string.IsNullOrEmpty(capabilityName))
                throw new ArgumentNullException("capabilityName");

            BaseRemove(capabilityName);
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
        /// Gets or sets the <typeparamref name="CapabilityElement"/>.
        /// </summary>
        public CapabilityElement this[int index]
        {
            get { return (CapabilityElement) BaseGet(index); }
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
        /// Gets or sets the <typeparamref name="CapabilityElement"/>.
        /// </summary>        
        public new CapabilityElement this[string capabilityName]
        {
            get { return (CapabilityElement) BaseGet(capabilityName); }
        }

        #endregion
    }
}