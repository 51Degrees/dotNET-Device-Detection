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
 * Mobile Experts Limited are Copyright (C) 2009 - 2012. All Rights Reserved.
 * 
 * Contributor(s):
 *     James Rosewell <james@51degrees.mobi>
 * 
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