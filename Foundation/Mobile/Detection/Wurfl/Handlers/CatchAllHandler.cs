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

#region Usings

using FiftyOne.Foundation.Mobile.Detection.Wurfl.Matchers;
using Matcher=FiftyOne.Foundation.Mobile.Detection.Wurfl.Matchers.EditDistance.Matcher;

#endregion

namespace FiftyOne.Foundation.Mobile.Detection.Wurfl.Handlers
{
    internal class CatchAllHandler : ReducedInitialStringHandler
    {
        // This is the least precise handler.
        private const int CONFIDENCE = 1;

        internal override byte Confidence
        {
            get { return CONFIDENCE; }
        }

        protected internal override bool CanHandle(string userAgent)
        {
            return true;
        }

        protected internal override Results Match(string userAgent)
        {
            bool isMobile = false;

            // Use RIS to find a match first.
            Results results = base.Match(userAgent);

            // If no match with RIS then try edit distance.
            if (results == null || results.Count == 0)
                results = Matcher.Match(userAgent, this);

            // If a match other than edit distance was used then we'll have more confidence
            // and return the mobile version of the device.
            if (results.GetType() == typeof (Results))
                isMobile = true;

            Results newResults = new Results();

            // Look at the results for one that matches our isMobile setting based on the
            // matcher used.
            foreach (Result result in results)
            {
                if (result.Device.IsMobileDevice == isMobile)
                    newResults.Add(result.Device);
            }

            // Return the new results if any values are available.
            return newResults;
        }
    }
}