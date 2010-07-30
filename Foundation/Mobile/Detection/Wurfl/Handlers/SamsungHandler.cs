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

using FiftyOne.Foundation.Mobile.Detection.Wurfl.Matchers;
namespace FiftyOne.Foundation.Mobile.Detection.Wurfl.Handlers
{
    internal class SamsungHandler : RegexSegmentHandler
    {
        private const byte EXTRA_CONFIDENCE = 1;
        private static readonly string[] PATTERNS = {
            // Samsung model details.
            @"(?i)(?:(?<=samsung|SEC-)[^/]+)",
            // Version details.
            @"(?i)(?:(?<=samsung|SEC-[^/]+/)[^\s]+)" };

        internal SamsungHandler() : base(PATTERNS, new int[] { 100, 1 }) { }

        /// <summary>
        /// Provides a higher degree of confidence so that this handler will not be overruled
        /// by desktop handlers.
        /// </summary>
        internal override byte Confidence { get { return (byte)(base.Confidence + EXTRA_CONFIDENCE); } }

        // Checks given UA contains "Samsung" in any case.
        internal protected override bool CanHandle(string userAgent)
        {
            return ((userAgent.IndexOf("samsung", System.StringComparison.InvariantCultureIgnoreCase) > -1) ||
                userAgent.StartsWith("SEC-")) &&
                base.CanHandle(userAgent);
        }

        internal protected override Results Match(string userAgent)
        {
            Results results =  base.Match(userAgent);
            // Use Edit Distance if the segment match failed.
            if (results == null || results.Count == 0)
                results = FiftyOne.Foundation.Mobile.Detection.Wurfl.Matchers.EditDistance.Matcher.Match(userAgent, this);
            return results;
        }
    }
}