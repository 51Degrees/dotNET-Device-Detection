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

using FiftyOne.Foundation.Mobile.Detection.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FiftyOne.Foundation.Mobile.Detection
{
    internal abstract class SearchBase<T, K, L>
    {
        /// <summary>
        /// Returns the number of elements in the list.
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        protected abstract int GetCount(L list);

        /// <summary>
        /// Returns the value from the list at the index provided.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        protected abstract T GetValue(L list, int index);

        /// <summary>
        /// Compares the item to the key.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="key"></param>
        /// <returns></returns>
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
        /// <param name="iterations">Number of iterations needed to find the key.</param>
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

    internal class SearchLists<T, K> : SearchBase<T, K, IList<T>> where T : IComparable<K>
    {
        protected override int GetCount(IList<T> list)
        {
            return list.Count;
        }

        protected override T GetValue(IList<T> list, int index)
        {
            return list[index];
        }

        protected override int CompareTo(T item, K key)
        {
            return item.CompareTo(key);
        }

        internal int BinarySearch(IList<T> list, K key)
        {
            return base.BinarySearchBase(list, key);
        }
    }
}