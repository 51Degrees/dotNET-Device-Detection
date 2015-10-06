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
using System.Text.RegularExpressions;
using System.Web;

namespace FiftyOne.Foundation.Mobile.Redirection
{
    internal class Location
    {
        #region Fields

        internal static readonly Regex _regexReplaceEmptyTags = new Regex(@"{\d+}", RegexOptions.Compiled);
        internal readonly string _name;
        internal readonly string _url;
        internal readonly List<Filter> _filters = new List<Filter>();
        internal readonly Regex _matchRegex;

        #endregion

        #region Constructors

        internal Location(string name, string url, string expression)
        {
            _name = name;
            _url = url;
            if (String.IsNullOrEmpty(expression) == false)
                _matchRegex = new Regex(expression, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        #endregion

        #region Properties

        internal List<Filter> Filters
        {
            get { return _filters; }
        }

        #endregion

        #region Methods
        
        internal string GetUrl(HttpContext context)
        {
            if (_matchRegex != null)
            {
                // A match regular expression has been found that should be used to
                // extract all the items of interest from the original URL and place
                // them and the positions contains in {} brackets in the URL property
                // of the location. Removes any remaining {} brackets before returning
                // the url for the location.
                MatchCollection matches = _matchRegex.Matches(RedirectModule.GetOriginalUrl(context));
                string url = null;
                if (matches.Count > 0)
                {
                    string[] values = new string[matches.Count];
                    for (int i = 0; i < matches.Count; i++)
                        values[i] = matches[i].Value;
                    url = String.Format(_url, values);
                }
                else
                    url = _url;
                return _regexReplaceEmptyTags.Replace(url, String.Empty);
            }
            // Return the URL unformatted.
            return _url;
        }

        internal bool GetIsMatch(HttpContext context)
        {
            foreach (Filter filter in _filters)
            {
                if (filter.GetIsMatch(context) == false)
                    return false;
            }
            return true;
        }

        #endregion
    }
}
