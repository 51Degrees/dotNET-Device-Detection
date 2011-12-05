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
            // Confirm input strings are valid.
            if (str1 == null) throw new ArgumentNullException("str1");
            if (str2 == null) throw new ArgumentNullException("str2");

            // Get string lengths and check for zero length.
            int l1 = str1.Length, l2 = str2.Length;
            if (l1 == 0) return l2;
            if (l2 == 0) return l1;

            // Initialise the data structures.
            int curRow = 0, nextRow = 1;
            int[][] rows = new[] {new int[l1 + 1], new int[l1 + 1]};
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