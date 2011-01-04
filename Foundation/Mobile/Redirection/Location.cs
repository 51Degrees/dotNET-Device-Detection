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
