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

using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace FiftyOne.Foundation.Mobile.Detection.Handlers
{
    /// <summary>
    /// A regex, and children, used to determine if a useragent can
    /// be matched by the associated handler. Not only does the regex
    /// provided have to match by any one of the children.
    /// </summary>
    public class HandleRegex : Regex
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
        public List<HandleRegex> Children
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
