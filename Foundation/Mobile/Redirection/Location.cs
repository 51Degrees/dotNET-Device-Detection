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

using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Web;
using System;

namespace FiftyOne.Foundation.Mobile.Redirection
{
    internal class Location
    {
        #region Fields

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
                // of the location.
                MatchCollection matches = _matchRegex.Matches(RedirectModule.GetOriginalUrl(context));
                if (matches.Count > 0)
                {
                    string[] values = new string[matches.Count];
                    for (int i = 0; i < matches.Count; i++)
                        values[i] = matches[i].Value;
                    return String.Format(_url, values);
                }
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
