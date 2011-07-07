/* *********************************************************************
 * The contents of this file are subject to the Mozilla internal License 
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

using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace FiftyOne.Foundation.Mobile.Detection.Handlers
{
    /// <summary>
    /// A regex, and children, used to determine if a useragent can
    /// be matched by the associated handler. Not only does the regex
    /// provided have to match by any one of the children.
    /// </summary>
    internal class HandleRegex : Regex
    {
        #region Fields

        /// <summary>
        /// A list of children.
        /// </summary>
        private List<HandleRegex> _children = new List<HandleRegex>();

        #endregion

        #region Properties

        /// <summary>
        /// A list of children.
        /// </summary>
        internal List<HandleRegex> Children
        {
            get { return _children;  }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a new instance of <see cref="HandleRegex"/>.
        /// </summary>
        /// <param name="pattern">Regex pattern.</param>
        internal HandleRegex(string pattern)
            : base(pattern, RegexOptions.Compiled | RegexOptions.CultureInvariant)
        {
        }

        #endregion

        #region Overriden Properties

        /// <summary>
        /// Returns true if the regex and any one of it's child match.
        /// </summary>
        /// <param name="useragent">The useragent string to check.</param>
        /// <returns>True if a match is found.</returns>
        internal new bool IsMatch(string useragent)
        {
            if (base.IsMatch(useragent))
            {
                if (_children.Count == 0)
                    return true;

                foreach (HandleRegex child in _children)
                {
                    if (child.IsMatch(useragent))
                        return true;
                }
            }
            return false;
        }

        #endregion
    }
}
