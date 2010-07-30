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

using System.Collections.Generic;
using System.Diagnostics;

namespace FiftyOne.Foundation.Mobile.Detection
{
    /// <summary>
    /// Rather than store a unique copy of every string held in the Wurfl file a list of strings
    /// is used and the index of the string is held in the data classes.
    /// </summary>
    internal class Strings
    {
        /// <summary>
        /// All the strings used in the Wurfl file are held in this list.
        /// </summary>
        private static List<string> _values = new List<string>();
        
        /// <summary>
        /// Index containing the hashcode of the string as the index and either the index in the _values
        /// list as the value or a list of values that match the hashcode. It is possible for several 
        /// different values to share the same hashcode.
        /// </summary>
        private static Dictionary<int, object> _index = new Dictionary<int, object>();

        /// <summary>
        /// Adds a new string value to the list of Wurfl strings. If the value already exists
        /// then it's index is returned. If it doesn't then a new entry is added.
        /// </summary>
        /// <param name="value">String value to add.</param>
        /// <returns>Index of the string in the _values list. Used the Get method to retrieve the string value later.</returns>
        internal static int Add(string value)
        {
            int result = 0;
            int hashcode = value.GetHashCode();
            // Lock the index to ensure no other threads are operating on it
            // before the check and possible update is performed.
            lock (_index)
            {
                result = IndexOf(value, hashcode);
                if (result == -1)
                {
                    // This hashcode does not exist so add a new entry to the list.
                    result = AddValue(value);
                    _index.Add(hashcode, result);
                }
                else
                {
                    // If this isn't the value we're looking for because another string
                    // shares the same hashcode add it's position to the index after the
                    // new string has been added to the values list.
                    if (_values[result] != value)
                    {
                        // Create the new list for the indexes.
                        List<int> newList = null;
                        object obj = _index[hashcode];
                        if (obj is int)
                        {
                            newList = new List<int>();
                            newList.Add((int)obj);
                        }
                        else
                        {
                            newList = new List<int>((int[])obj);
                        }
                        // This is a new value for an existing hashcode. Add it to
                        // the list of strings before updating the index.
                        result = AddValue(value);
                        newList.Add(result);
                        _index[hashcode] = newList.ToArray();
                    }
                }
            }

            // Perform assertion checks to ensure the _values and _index are correct.
            Debug.Assert(result >= 0 && result < _values.Count,
                "Result is out of value range.");
            Debug.Assert(result == _values.IndexOf(value),
                "New value was not found at correct position in the list.");
            Debug.Assert(IndexOf(value) == result,
                "Index does not return matching string position.");

            return result;
        }

        private static int AddValue(string value)
        {
            int result;
            lock (_values)
            {
                _values.Add(value);
                result = _values.IndexOf(value);
            }
            return result;
        }

        internal static bool Contains(string value)
        {
            return IndexOf(value) >= 0;
        }
        
        internal static int IndexOf(string value, int hashcode)
        {
            object obj = null;
            if (_index.TryGetValue(hashcode, out obj) == true)
            {
                if (obj is int)
                    return (int)obj;
                int[] list = (int[])obj;
                if (list.Length == 1)
                {
                    return list[0];
                }
                foreach (int index in list)
                {
                    if (_values[index] == value)
                    {
                        return index;
                    }
                }
            }
            return -1;
        }

        internal static int IndexOf(string value)
        {
            return IndexOf(value, value.GetHashCode());
        }

        internal static string Get(int index)
        {
            return _values[index];
        }

        internal static int Count
        {
            get { return _values.Count; }
        }
    }
}
