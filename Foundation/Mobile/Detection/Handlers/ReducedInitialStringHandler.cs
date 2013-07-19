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

using System.Text.RegularExpressions;
using FiftyOne.Foundation.Mobile.Detection.Matchers;
using Matcher = FiftyOne.Foundation.Mobile.Detection.Matchers.ReducedInitialString.Matcher;

#endregion

namespace FiftyOne.Foundation.Mobile.Detection.Handlers
{
    /// <summary>
    /// Device detection handler using the reduced initial string method. The first
    /// part of the strings are checked to determine a match.
    /// </summary>
    public class ReducedInitialStringHandler : Handler
    {
        #region Fields

        private Regex _tolerance = null;

        #endregion

        #region Properties

        /// <summary>
        /// The regular expression used to determine the first X
        /// characters to check of the string.
        /// </summary>
        public Regex Tolerance
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
        internal ReducedInitialStringHandler(Provider provider, string name, string defaultDeviceId, byte confidence, bool checkUAProfs, string tolerance)
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