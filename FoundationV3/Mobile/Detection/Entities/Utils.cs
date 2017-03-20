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
using System.IO;

namespace FiftyOne.Foundation.Mobile.Detection.Entities
{
    internal static class Utils
    {
        #region Constants

        /// <summary>
        /// List if powers used to determine numeric differences.
        /// </summary>
        private static readonly int[] Powers = new[] { 1, 10, 100, 1000, 10000 };

        #endregion

        #region Static Methods

        /// <summary>
        /// An enumerable that can be used to read through the entries.
        /// </summary>
        /// <param name="reader">Reader set to the position at the start of the list</param>
        /// <param name="count">The number of integers to read to form the array</param>
        /// <returns>Iterator to read each integer entry.</returns>
        internal static IEnumerable<int> GetIntegerEnumerator(BinaryReader reader, int count)
        {
            for (int i = 0; i < count; i++)
            {
                yield return reader.ReadInt32();
            }
        }

        /// <summary>
        /// Reads an integer array where the first integer is the number of 
        /// following integers.
        /// </summary>
        /// <param name="reader">Reader set to the position at the start of the list</param>
        /// <param name="count">The number of integers to read to form the array</param>
        /// <returns>An array of integers</returns>
        internal static int[] ReadIntegerArray(BinaryReader reader, int count)
        {
            var array = new int[count];
            for (int i = 0; i < array.Length; i++)
                array[i] = reader.ReadInt32();
            return array;
        }

        /// <summary>
        /// Returns an integer representation of the characters between start and end.
        /// Assumes that all the characters are numeric characters.
        /// </summary>
        /// <param name="array">
        /// Array of characters with numeric characters present between start and end
        /// </param>
        /// <param name="start">
        /// The first character to use to convert to a number
        /// </param>
        /// <param name="length">
        /// The number of characters to use in the conversion
        /// </param>
        /// <returns></returns>
        internal static int GetNumber(byte[] array, int start, int length)
        {
            int value = 0;
            for (int i = start + length - 1, p = 0; i >= start && p < Powers.Length; i--, p++)
            {
                value += Powers[p] * ((byte)array[i] - (byte)'0');
            }
            return value;
        }

        /// <summary>
        /// Determines if the value is an ASCII numeric value.
        /// </summary>
        /// <param name="value">Byte value to be checked</param>
        /// <returns>True if the value is an ASCII numeric character</returns>
        internal static bool GetIsNumeric(byte value)
        {
            return (value >= (byte)'0' && value <= (byte)'9');
        }

        #endregion
    }
}
