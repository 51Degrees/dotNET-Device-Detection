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

#region

using System.Collections.Generic;
using System.Text.RegularExpressions;

#endregion

namespace FiftyOne.Foundation.Mobile.Detection.Wurfl.Matchers
{
    internal class UserAgentParser
    {
        #region Classes

        private class ReplaceFilter
        {
            private readonly Regex _regex;
            private readonly string _replacement;

            internal ReplaceFilter(string expression, string replacement)
            {
                _regex = new Regex(expression, RegexOptions.Compiled);
                _replacement = replacement;
            }

            internal string ParseString(string useragent)
            {
                return _regex.Replace(useragent, _replacement);
            }
        }

        #endregion

        #region Static Fields

        private static readonly List<ReplaceFilter> ReplaceFilters = new List<ReplaceFilter>();

        #endregion

        #region Methods

        /// <summary>
        /// Create the regular expressions that are used to filter out common
        /// problems with useragent strings.
        /// </summary>
        private static void InitReplaceFilters()
        {
            lock (ReplaceFilters)
            {
                if (ReplaceFilters.Count != 0) return;
                ReplaceFilters.Add(new ReplaceFilter(@"UP.Link/[\d.]+", ""));
                // Removes UP.Link from user agents strings that should not be in the WURFL.
                ReplaceFilters.Add(new ReplaceFilter(@"/IMEI/SN[\d|X]+", ""));
                // Removes IMEI numbers added to the useragent string.
                ReplaceFilters.Add(new ReplaceFilter(@"^\s+|\s+$", "")); // Removes leading and trailing spaces.
            }
        }

        /// <summary>
        /// Check the user agent string for common errors that hinder matching. 
        /// </summary>
        /// <param name="userAgent">A useragent string to be cleaned.</param>
        /// <returns>A cleaned useragent string.</returns>
        internal static string Parse(string userAgent)
        {
            InitReplaceFilters();
            foreach (ReplaceFilter filter in ReplaceFilters)
                userAgent = filter.ParseString(userAgent);
            return userAgent;
        }

        #endregion
    }
}