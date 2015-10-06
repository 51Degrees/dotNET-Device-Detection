/* *********************************************************************
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited. 
 * Copyright © 2015 51Degrees Mobile Experts Limited, 5 Charlotte Close,
 * Caversham, Reading, Berkshire, United Kingdom RG4 7BY
 * 
 * This Source Code Form is the subject of the following patent 
 * applications, owned by 51Degrees Mobile Experts Limited of 5 Charlotte
 * Close, Caversham, Reading, Berkshire, United Kingdom RG4 7BY: 
 * European Patent Application No. 13192291.6; and
 * United States Patent Application Nos. 14/085,223 and 14/085,301.
 *
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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FiftyOne.Foundation.Mobile.Detection.Entities
{
    /// <summary>
    /// Encapsulates a list of one or more values. Provides methods
    /// to return boolean, double and string representations of the
    /// values list.
    /// </summary>
    /// <para>
    /// The class contains helper methods to make consuming the data set easier.
    /// </para>
    /// <para>
    /// For more information see https://51degrees.com/Support/Documentation/Net
    /// </para>
    public class Values : IList<Value>
    {
        #region Fields

        /// <summary>
        /// The property the list of values relates to.
        /// </summary>
        private readonly Property _property;

        /// <summary>
        /// An array of values to expose.
        /// </summary>
        private readonly Value[] _values;

        #endregion

        #region Properties

        /// <summary>
        /// Returns true if any of the values in this list are the
        /// default ones for the property.
        /// </summary>
        public bool IsDefault
        {
            get
            {
                return this.Any(i => i.IsDefault);
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a new instance of the values list.
        /// </summary>
        /// <param name="property">Property the values list relates to</param>
        /// <param name="values">An array of values to use with the list</param>
        internal Values(Property property, Value[] values)
        {
            _property = property;
            _values = values;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the index of the value name.
        /// </summary>
        /// <param name="valueName">Value name required</param>
        /// <returns>The index matching the value name, otherwise a negative number</returns>
        private int GetValueIndex(string valueName)
        {
            int lower = 0;
            var upper = Count - 1;
            int middle = 0;

            while (lower <= upper)
            {
                middle = lower + (upper - lower) / 2;
                var comparisonResult = this[middle].Name.CompareTo(valueName);
                if (comparisonResult == 0)
                    return middle;
                else if (comparisonResult > 0)
                    upper = middle - 1;
                else
                    lower = middle + 1;
            }

            return ~middle;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns the value associated with the name provided.
        /// </summary>
        /// <param name="valueName"></param>
        /// <returns></returns>
        public Value this[string valueName]
        {
            get
            {
                var index = GetValueIndex(valueName);
                return index >= 0 ? this[index] : null;
            }
        }

        /// <summary>
        /// The value represented as a boolean.
        /// </summary>
        /// <returns>A boolean representation of the only item in the list.</returns>
        /// <exception cref="MobileException">
        /// Thrown if the method is called for a property with multiple values
        /// </exception>
        public bool ToBool()
        {
            if (_property.IsList)
                throw new MobileException("ToBool can only be used on non List properties");
            return Count > 0 ? this[0].ToBool() : false;
        }

        /// <summary>
        /// The value represented as a double.
        /// </summary>
        /// <returns>A double representation of the only item in the list.</returns>
        /// <exception cref="MobileException">
        /// Thrown if the method is called for a property with multiple values
        /// </exception>
        public double ToDouble()
        {
            if (_property.IsList)
                throw new MobileException("ToDouble can only be used on non List properties");
            return Count > 0 ? this[0].ToDouble() : 0;
        }

        /// <summary>
        /// The value represented as a integer.
        /// </summary>
        /// <returns>A integer representation of the only item in the list.</returns>
        /// <exception cref="MobileException">
        /// Thrown if the method is called for a property with multiple values
        /// </exception>
        public int ToInt()
        {
            if (_property.IsList)
                throw new MobileException("ToInt can only be used on non List properties");
            return Count > 0 ? this[0].ToInt() : 0;
        }

        /// <summary>
        /// Returns the values as a string array.
        /// </summary>
        /// <returns></returns>
        public string[] ToStringArray()
        {
            return this.Select(i => i.Name).ToArray();
        }

        /// <summary>
        /// The values represented as a string where multiple values are seperated 
        /// by colons.
        /// </summary>
        /// <returns>The values as a string</returns>
        public override string ToString()
        {
            return String.Join(
                Constants.ValueSeperator,
                this.Select(i => i.ToString()).ToArray());
        }

        #endregion

        #region IList Interface Members

        public int IndexOf(Value item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, Value item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public Value this[int index]
        {
            get
            {
                return _values[index];
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void Add(Value item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(Value item)
        {
            return _values.Contains(item);
        }

        public void CopyTo(Value[] array, int arrayIndex)
        {
            _values.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return _values.Length; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public bool Remove(Value item)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<Value> GetEnumerator()
        {
            return _values.Select(i => i).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        #endregion
    }
}
