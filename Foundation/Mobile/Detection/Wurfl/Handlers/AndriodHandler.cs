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

namespace FiftyOne.Foundation.Mobile.Detection.Wurfl.Handlers
{
    internal class AndriodHandler : RegexSegmentHandler
    {
        private const byte EXTRA_CONFIDENCE = 1;

        private static readonly string[] PATTERNS = {
                                                        // Details about the device.
                                                        @"(?<=Mozilla/\d.\d \()[^)]+",
                                                        // Android version.
                                                        @"Android [\d.]+"
                                                    };

        internal AndriodHandler() : base(PATTERNS, new[] {3, 1})
        {
            _firstMatchOnly = true;
        }

        /// <summary>
        /// Provides a higher degree of confidence because only devices in the "firefox"
        /// branch of the device tree are available for matching.
        /// </summary>
        internal override byte Confidence
        {
            get { return (byte) (base.Confidence + EXTRA_CONFIDENCE); }
        }


        // Checks given UA containts with "Android"
        protected internal override bool CanHandle(string userAgent)
        {
            return (userAgent.Contains("Android")) &&
                   base.CanHandle(userAgent);
        }
    }
}