/* *********************************************************************
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited. 
 * Copyright © 2017 51Degrees Mobile Experts Limited, 5 Charlotte Close,
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

using System.Collections.Generic;
using FiftyOne.Foundation.Mobile.Detection.Entities;

namespace FiftyOne.Foundation.Mobile.Detection
{
    /// <summary>
    /// Rather then making a copy of a section of an array in memory
    /// to a new array containing a subset of elements this class is 
    /// used to create an IList that will return the subset of the 
    /// underlying larger array.
    /// </summary>
    /// <typeparam name="T">Type of data to hold in the array</typeparam>
    internal class ArrayRange<T> : IList<T>
    {
        internal readonly int _startIndex;
        internal readonly int _length;
        internal readonly T[] _array;

        internal ArrayRange(int startIndex, int length, T[] array)
        {
            _startIndex = startIndex;
            _length = length;
            _array = array;
        }
    
        public int IndexOf(T item)
        {
 	        throw new System.NotImplementedException();
        }

        public void Insert(int index, T item)
        {
 	        throw new System.NotImplementedException();
        }

        public void RemoveAt(int index)
        {
 	        throw new System.NotImplementedException();
        }

        T IList<T>.this[int index]
        {
	        get 
	        {
                return _array[_startIndex + index];
	        }
	        set 
	        { 
		        throw new System.NotImplementedException(); 
	        }
        }

        public void Add(T item)
        {
 	        throw new System.NotImplementedException();
        }

        public void Clear()
        {
 	        throw new System.NotImplementedException();
        }

        public bool Contains(T item)
        {
 	        throw new System.NotImplementedException();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
 	        throw new System.NotImplementedException();
        }

        public int Count
        {
	        get { return _length; }
        }

        public bool IsReadOnly
        {
	        get { return true; }
        }

        public bool Remove(T item)
        {
 	        throw new System.NotImplementedException();
        }

        public IEnumerator<T> GetEnumerator()
        {
 	        for (int i = 0; i < Count; i++)
            {
                yield return _array[_startIndex + i];
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    /// <summary>
    /// Provides the ability to efficiently retrieve the items from the list
    /// using a ranged enumerable. This list can be used with types that
    /// are returned from the <see cref="System.IO.BinaryReader"/> 
    /// implementation where a factory is not required to construct the 
    /// entity.
    /// </summary>
    internal interface ISimpleList
    {
        /// <summary>
        /// Returns the values in the list starting at the index provided.
        /// </summary>
        /// <param name="index">
        /// First index of the range required.
        /// </param>
        /// <param name="count">
        /// Number of elements to return.
        /// </param>
        /// <returns>
        /// A list of the items in the range requested.
        /// </returns>
        IList<int> GetRange(int index, int count);

        /// <summary>
        /// Returns the value in the list at the index provided.
        /// </summary>
        /// <param name="index">Index of the value required.</param>
        /// <returns>Value at the index requested</returns>
        int this[int index] { get; }

        /// <summary>
        /// The number of items in the list.
        /// </summary>
        int Count { get; }
    }
}
