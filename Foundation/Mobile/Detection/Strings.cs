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

#if VER4

using System.Linq;

#endif

#endregion

namespace FiftyOne.Foundation.Mobile.Detection
{
    /// <summary>
    /// Rather than store a copy of every string held in the data files a list of strings
    /// is used and the index of the string is held in the data classes.
    /// </summary>
    internal class Strings
    {
        #region Fields

        /// <summary>
        /// Index containing the hashcode of the string as the index and either the index in the _values
        /// list as the value or a list of values that match the hashcode. It is possible for several 
        /// different values to share the same hashcode.
        /// </summary>
        private readonly Dictionary<int, object> _index = new Dictionary<int, object>();

        /// <summary>
        /// All the strings used in the 51Degrees.mobi file are held in this stack.
        /// </summary>
        internal readonly List<string> _values = new List<string>();

        #endregion

        #region Properties

        /// <summary>
        /// The number of items in the list.
        /// </summary>
        internal int Count
        {
            get { return _values.Count; }
        }

        #endregion

        #region internal Methods

        /// <summary>
        /// Adds a new string value to the list of strings. If the value already exists
        /// then it's index is returned. If it doesn't then a new entry is added.
        /// </summary>
        /// <param name="value">String value to add.</param>
        /// <returns>Index of the string in the _values list. Used the Get method to retrieve the string value later.</returns>
        internal int Add(string value)
        {
            int hashcode = value.GetHashCode();
            int result = IndexOf(value, hashcode);
            
            // If the string does not exist lock the index and then check it
            // still does not exist before adding to the index and values.
            if (result == -1)
            {
                lock (_index)
                {
                    result = IndexOf(value, hashcode);
                    if (result == -1)
                    {
                        // This hashcode does not exist so add a new entry to the list.
                        result = AddValue(value);
                        _index.Add(hashcode, result);
                        return result;
                    }
                }
            }

            // If this isn't the value we're looking for because another string
            // shares the same hashcode add it's position to the index after the
            // new string has been added to the values list.
            if (_values[result] != value)
            {
                // Create the new list for the indexes.
                List<int> newList = null;
                lock (_index)
                {
                    object obj = _index[hashcode];
                    if (obj is int)
                        newList = new List<int>(new int[] { (int) obj });
                    else
                        newList = new List<int>((int[]) obj);

                    // This is a new value for an existing hashcode. Add it to
                    // the list of strings before updating the index.
                    result = AddValue(value);
                    newList.Add(result);
                    _index[hashcode] = newList.ToArray();
                }
            }
            
            return result;
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Returns the string at the index position provided. If the index is
        /// invalid then return null.
        /// </summary>
        /// <param name="index">Index of string required.</param>
        /// <returns>String value at the specified index.</returns>
        internal string Get(int index)
        {
            if (index < 0) return null;
            return _values[index];
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Adds a value to the list and returns it's index in the list.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private int AddValue(string value)
        {
            int result;
            lock (_values)
            {
                result = _values.Count;
                _values.Add(value);
            }
            return result;
        }

        /// <summary>
        /// Gets the index of the value and hashcode. The hashcode is provided
        /// to avoid calculating when it already exists from the string.
        /// </summary>
        /// <param name="value">The value who's index is required from the list.</param>
        /// <param name="hashcode">The hashcode of the value.</param>
        /// <returns>The integer index of the string value in the list, otherwise -1.</returns>
        private int IndexOf(string value, int hashcode)
        {
            object obj = null;
            // Does the hashcode exist in the list.
            if (_index.TryGetValue(hashcode, out obj))
            {
                // If the object is an integer return the index.
                if (obj is int)
                    return (int) obj;

                // If it's an array of objects, which is very rare because the hashcodes
                // will have to match return the 1st item if only one exists, or one that
                // matches the string value passed into the method.
                int[] list = (int[]) obj;
                if (list.Length == 1)
                {
                    return list[0];
                }

                // Find the matching index.
                foreach (int index in list)
                    if (_values[index] == value)
                        return index;
            }
            return -1;
        }

        #endregion
    }
}