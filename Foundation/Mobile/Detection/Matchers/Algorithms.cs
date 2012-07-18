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

using System;

#endregion

namespace FiftyOne.Foundation.Mobile.Detection.Matchers
{
    /// <summary>
    /// Contains major matching algorithms used by the solution.
    /// </summary>
    public class Algorithms
    {
        /// <summary>
        /// Measures the amount of difference between two strings using the Levenshtein 
        /// Distance algorithm. This implementation uses a modified version of the pseudo
        /// code found at http://en.wikipedia.org/wiki/Levenshtein_distance.
        /// The logic has been modified to ignore string comparisions that will return
        /// a value greater than the lowest one found so far. This significantly improves
        /// performance as we can determine earlier if there is any point completing the 
        /// calculation.
        /// </summary>
        /// <param name="str1">1st string to compare.</param>
        /// <param name="str2">2nd string to compare.</param>
        /// <param name="maxValue">The maximum value we're interested in. Anything higher can be ignored.</param>
        /// <returns></returns>
        public static int EditDistance(string str1, string str2, int maxValue)
        {
            return EditDistance(
                new int[][] {new int[str1.Length + 1], new int[str1.Length + 1]},
                str1, str2, maxValue);
        }


        /// <summary>
        /// Measures the amount of difference between two strings using the Levenshtein 
        /// Distance algorithm. This implementation uses a modified version of the pseudo
        /// code found at http://en.wikipedia.org/wiki/Levenshtein_distance.
        /// The logic has been modified to ignore string comparisions that will return
        /// a value greater than the lowest one found so far. This significantly improves
        /// performance as we can determine earlier if there is any point completing the 
        /// calculation.
        /// Requires the integer array to preallocated to improve memory management.
        /// </summary>
        /// <param name="rows">Preallocated memory for the calculation.</param>
        /// <param name="str1">1st string to compare.</param>
        /// <param name="str2">2nd string to compare.</param>
        /// <param name="maxValue">The maximum value we're interested in. Anything higher can be ignored.</param>
        /// <returns></returns>
        public static int EditDistance(int[][] rows, string str1, string str2, int maxValue)
        {
            // Confirm input strings are valid.
            if (str1 == null) throw new ArgumentNullException("str1");
            if (str2 == null) throw new ArgumentNullException("str2");

            // Get string lengths and check for zero length.
            int l1 = str1.Length, l2 = str2.Length;
            if (l1 == 0) return l2;
            if (l2 == 0) return l1;

            // Initialise the data structures.
            int curRow = 0, nextRow = 1;
            for (int x = 0; x <= l1; ++x) rows[curRow][x] = x;

            for (int y = 1; y <= l2; ++y)
            {
                int lowest = int.MaxValue;
                rows[nextRow][0] = y;
                for (int x = 1; x <= l1; ++x)
                {
                    // Calculate the edit distant value for the current cell.
                    int value = Math.Min(
                        rows[curRow][x] + 1,
                        Math.Min(rows[nextRow][x - 1] + 1,
                                 rows[curRow][x - 1] + ((str1[x - 1] == str2[y - 1]) ? 0 : 1)));
                    rows[nextRow][x] = value;

                    // Record the lowest value on this row.
                    if (value < lowest)
                        lowest = value;
                }

                // If the lowest value found so far is greater than the maximum value
                // we're interested in return a large number that will be ignored.
                if (lowest > maxValue)
                    return int.MaxValue;

                // Swap the current and next rows
                if (curRow == 0)
                {
                    curRow = 1;
                    nextRow = 0;
                }
                else
                {
                    curRow = 0;
                    nextRow = 1;
                }
            }

            return rows[curRow][l1];
        }
    }
}