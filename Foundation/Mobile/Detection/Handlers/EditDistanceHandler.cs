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

using FiftyOne.Foundation.Mobile.Detection.Matchers;
using Matcher=FiftyOne.Foundation.Mobile.Detection.Matchers.EditDistance.Matcher;

#endregion

namespace FiftyOne.Foundation.Mobile.Detection.Handlers
{
    /// <summary>
    /// Device detection handler using the EditDistance method of matching devices.
    /// </summary>
    public class EditDistanceHandler : Handler
    {
        #region Constructor

        internal EditDistanceHandler(BaseProvider provider, string name, string defaultDeviceId, byte confidence, bool checkUAProfs)
            : base(provider, name, defaultDeviceId, confidence, checkUAProfs)
        {
        }

        #endregion

        #region Overridden Methods

        internal override Results Match(string userAgent)
        {
            return Matcher.Match(userAgent, this);
        }

        #endregion
    }
}