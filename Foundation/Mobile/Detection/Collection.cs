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
using System.Collections.Generic;

#endregion

namespace FiftyOne.Foundation.Mobile.Detection
{
    /// <summary>
    /// A collection of string indexes.
    /// </summary>
    internal class Collection : Dictionary<int, IList<int>>
    {
        #region Fields

        private readonly Strings _strings;

        #endregion

        #region Constructor

        internal Collection(Strings strings) : base()
        {
            _strings = strings;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sets the capabilityName and Value in the collection.
        /// </summary>
        /// <param name="capabilityName">Name of the capability being set.</param>
        /// <param name="values">Values of the capability being set.</param>
        internal void Set(string capabilityName, string[] values)
        {
            int capabilityNameIndex = _strings.Add(capabilityName);
            if (capabilityNameIndex >= 0)
            {
                var stringIndexes = new List<int>();
                foreach (string value in values)
                {
                    stringIndexes.Add(_strings.Add(value));
                }
                Set(capabilityNameIndex, stringIndexes);
            }
        }

        /// <summary>
        /// Sets the capabilityName and Value in the collection.
        /// </summary>
        /// <param name="capabilityNameIndex">String index of the capability being set.</param>
        /// <param name="values">Value of the capability being set.</param>
        internal void Set(int capabilityNameIndex, IList<int> values)
        {
            lock (this)
            {
                // Does this capability already exist in the list?
                if (ContainsKey(capabilityNameIndex) == false)
                {
                    // No. Create a new value and add it to the list.
                    base.Add(capabilityNameIndex, values);
                }
                else
                {
                    // Yes. Replace it's value with the current one.
                    var list = base[capabilityNameIndex];
                    foreach(int value in values)
                        if (list.Contains(value) == false)
                            list.Add(value);
                }
            }
        }

        /// <summary>
        /// Checks the other Collection object instance contains identical keys and values
        /// as this one.
        /// </summary>
        /// <param name="other">Other Collection object.</param>
        /// <returns>True if the object instances contain the same values.</returns>
        internal bool Equals(Collection other)
        {
            foreach(var key in Keys)
                if (other[key] != this[key])
                    return false;
            return true;
        }

        #endregion
    }
}