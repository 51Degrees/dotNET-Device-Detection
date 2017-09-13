/* *********************************************************************
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited. 
 * Copyright © 2017 51Degrees Mobile Experts Limited, 5 Charlotte Close,
 * Caversham, Reading, Berkshire, United Kingdom RG4 7BY
 * 
 * This Source Code Form is the subject of the following patents and patent 
 * applications, owned by 51Degrees Mobile Experts Limited of 5 Charlotte
 * Close, Caversham, Reading, Berkshire, United Kingdom RG4 7BY: 
 * European Patent No. 2871816;
 * European Patent Application No. 17184134.9;
 * United States Patent Nos. 9,332,086 and 9,350,823; and
 * United States Patent Application No. 15/686,066.
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

namespace FiftyOne.Foundation.Mobile.Detection
{
    /// <summary>
    /// Generic search class used to perform a binary search where the
    /// type of items in the list and the key are different types.
    /// </summary>
    /// <remarks>Not intended to be used directly by 3rd parties.</remarks>
    /// <typeparam name="T">The type of items in the list</typeparam>
    /// <typeparam name="K">The type of the key field in the list</typeparam>
    /// <typeparam name="L">The type of the list</typeparam>
    public abstract class SearchBase<T, K, L>
    {
        /// <summary>
        /// Returns the number of items in the list provided.
        /// </summary>
        /// <param name="list">List to be counted.</param>
        /// <returns>Number of items in the list.</returns>
        protected abstract int GetCount(L list);

        /// <summary>
        /// Returns the item at the index provided.
        /// </summary>
        /// <param name="list">List to get item from.</param>
        /// <param name="index">Index of the element to return.</param>
        /// <returns>The value at the index provided.</returns>
        protected abstract T GetValue(L list, int index);

        /// <summary>
        /// Compares the item to the key.
        /// </summary>
        /// <param name="item">Item to be compared.</param>
        /// <param name="key">Key to be compared to the item.</param>
        /// <returns>Difference between the item and the key.</returns>
        protected abstract int CompareTo(T item, K key);

        /// <summary>
        /// Single implementation of the Binary Search algorithm.
        /// </summary>
        /// <param name="list">The list order by keys to be searched</param>
        /// <param name="key">The key to be found</param>
        /// <returns>
        /// Index of the item which matches the key, or the ones complement 
        /// of the index to add the value at.
        /// </returns>
        protected int BinarySearchBase(L list, K key)
        {
            var lower = 0;
            var upper = GetCount(list) - 1;
            var middle = 0;
            while (lower <= upper)
            {
                middle = lower + (upper - lower) / 2;
                var comparisonResult = CompareTo(GetValue(list, middle), key);
                if (comparisonResult == 0)
                {
                    return middle;
                }
                else if (comparisonResult > 0)
                {
                    upper = middle - 1;
                }
                else
                {
                    lower = middle + 1;
                }
            }
            return ~lower;
        }

        /// <summary>
        /// Single implementation of the Binary Search algorithm.
        /// </summary>
        /// <param name="list">The list order by keys to be searched</param>
        /// <param name="key">The key to be found</param>
        /// <param name="iterations">
        /// Number of iterations needed to find the key.
        /// </param>
        /// <returns>
        /// Index of the item which matches the key, or the ones complement 
        /// of the index to add the value at.
        /// </returns>
        protected int BinarySearchBase(L list, K key, out int iterations)
        {
            var lower = 0;
            var upper = GetCount(list) - 1;
            var middle = 0;
            iterations = 0;
            while (lower <= upper)
            {
                middle = lower + (upper - lower) / 2;
                iterations++;
                var comparisonResult = CompareTo(GetValue(list, middle), key);
                if (comparisonResult == 0)
                {
                    return middle;
                }
                else if (comparisonResult > 0)
                {
                    upper = middle - 1;
                }
                else
                {
                    lower = middle + 1;
                }
            }
            return ~lower;
        }
    }

    /// <summary>
    /// Used to search lists of order items using a key that this not the same
    /// types as the items in the list.
    /// </summary>
    /// <typeparam name="T">The type of items in the list</typeparam>
    /// <typeparam name="K">The type of the key field in the list</typeparam>
    public class SearchLists<T, K> : 
        SearchBase<T, K, IList<T>> where T : IComparable<K>
    {
        /// <summary>
        /// Returns the number of items in the list provided.
        /// </summary>
        /// <param name="list">List to be counted.</param>
        /// <returns>Number of items in the list.</returns>
        protected override int GetCount(IList<T> list)
        {
            return list.Count;
        }

        /// <summary>
        /// Returns the item at the index provided.
        /// </summary>
        /// <param name="list">List to get item from.</param>
        /// <param name="index">Index of the element to return.</param>
        /// <returns>The value at the index provided.</returns>
        protected override T GetValue(IList<T> list, int index)
        {
            return list[index];
        }

        /// <summary>
        /// Compares the item to the key.
        /// </summary>
        /// <param name="item">Item to be compared.</param>
        /// <param name="key">Key to be compared to the item.</param>
        /// <returns>Difference between the item and the key.</returns>
        protected override int CompareTo(T item, K key)
        {
            return item.CompareTo(key);
        }

        /// <summary>
        /// Searches the list provided for the key.
        /// </summary>
        /// <param name="list">List to search.</param>
        /// <param name="key">Key to find.</param>
        /// <returns>
        /// Index of the item in the list, or the twos complement.
        /// </returns>
        public int BinarySearch(IList<T> list, K key)
        {
            return base.BinarySearchBase(list, key);
        }
    }

    /// <summary>
    /// The list of complex types referenced by the index integer used in
    /// the search. i.e. the source list could be a list of values and
    /// the list passed into the method a list of integer indexes to
    /// values. This class avoids the need to create arrays of all complex
    /// types when performing a search that will in practice only need to 
    /// retrieve a small subset improving memory efficiency.
    /// </summary>
    internal class SearchReadonlyList<T, K> : 
        SearchBase<T, K, IList<int>> where T : IComparable<K>
    {
        /// <summary>
        /// The list of complex values to use with the index. This may be a 
        /// memory list, or a stream list where values are retrieved from the
        /// data source or cache. Used with the GetValue implementation to 
        /// return the complex type associated with the integer index.
        /// </summary>
        private readonly IReadonlyList<T> _source;

        /// <summary>
        /// Constructs a new instance of <see cref="SearchReadonlyList{T, K}"/>.
        /// </summary>
        /// <param name="source">
        /// The list of complex values to use with the index.
        /// </param>
        internal SearchReadonlyList(IReadonlyList<T> source)
        {
            _source = source;
        }

        protected override int GetCount(IList<int> list)
        {
            return list.Count;
        }

        protected override T GetValue(IList<int> list, int index)
        {
            return _source[list[index]];
        }

        protected override int CompareTo(T item, K key)
        {
            return item.CompareTo(key);
        }

        internal int BinarySearch(IList<int> list, K key)
        {
            return base.BinarySearchBase(list, key);
        }
    }
}