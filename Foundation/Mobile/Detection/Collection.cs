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

using System.Collections.Generic;

#endregion

namespace FiftyOne.Foundation.Mobile.Detection
{
    /// <summary>
    /// A collection of string indexes.
    /// </summary>
    internal class Collection : Dictionary<int, List<int>>
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
                List<int> stringIndexes = new List<int>();
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
        internal void Set(int capabilityNameIndex, List<int> values)
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
                    List<int> list = base[capabilityNameIndex];
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
            foreach(int key in Keys)
                if (other[key] != this[key])
                    return false;
            return true;
        }

        #endregion
    }
}