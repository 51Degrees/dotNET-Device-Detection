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

#region Usings

using System;
using System.Text.RegularExpressions;
using FiftyOne.Foundation.Mobile.Detection.Matchers;
using Matcher=FiftyOne.Foundation.Mobile.Detection.Matchers.ReducedInitialString.Matcher;

#endregion

namespace FiftyOne.Foundation.Mobile.Detection.Handlers
{
    /// <summary>
    /// Device detection handler using the reduced initial string method. The first
    /// part of the strings are checked to determine a match.
    /// </summary>
    internal class ReducedInitialStringHandler : Handler
    {
        #region Fields

        private Regex _tolerance = null;

        #endregion

        #region Properties

        /// <summary>
        /// The regular expression used to determine the first X
        /// characters to check of the string.
        /// </summary>
        internal Regex Tolerance
        {
            get { return _tolerance;  }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Constucts an instance of <see cref="ReducedInitialStringHandler"/>.
        /// </summary>
        /// <param name="provider">Reference to the provider instance the handler will be associated with.</param>
        /// <param name="name">Name of the handler for debugging purposes.</param>
        /// <param name="defaultDeviceId">The default device ID to return if no match is possible.</param>
        /// <param name="confidence">The confidence this handler should be given compared to others.</param>
        /// <param name="checkUAProfs">True if UAProfs should be checked.</param>
        /// <param name="tolerance">Regex used to calculate how many characters should be matched at the beginning of the useragent.</param>
        internal ReducedInitialStringHandler(BaseProvider provider, string name, string defaultDeviceId, byte confidence, bool checkUAProfs, string tolerance)
            : base(provider, name, defaultDeviceId, confidence, checkUAProfs)
        {
            _tolerance = new Regex(tolerance, RegexOptions.Compiled);
        }

        #endregion

        #region Methods

        internal override Results Match(string userAgent)
        {
            return Matcher.Match(userAgent, this, _tolerance.Match(userAgent).Length);
        }

        #endregion

 
    }
}