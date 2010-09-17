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

#region

using System;

#endregion

namespace FiftyOne.Foundation.Mobile.Detection.Wurfl.Handlers
{
    internal class BoltHandler : SafariHandler
    {
        private const string DEFAULT_DEVICE = "generic_bolt_ver1";
        private const byte EXTRA_CONFIDENCE = 1;

        private static readonly string[] PATTERNS = {
                                                        // Details about the device.
                                                        @"(?<=Mozilla/\d.\d \()[^)]+",
                                                        // Apple Apple Web Kit verion
                                                        @"(?<=AppleWebKit/)[\d.]+",
                                                        // Major version
                                                        @"(?<=Version/)[\d.]+",
                                                        // Safari version
                                                        @"(?<=Safari/)[\d.]+",
                                                        // Bolt version
                                                        @"(?i)(?<=bolt/)[\d.]+"
                                                    };

        private static readonly string[] SUPPORTED_ROOT_DEVICES = new[] {DEFAULT_DEVICE};

        public BoltHandler() : base(PATTERNS, new[] {3, 1, 1, 1, 1})
        {
        }

        /// <summary>
        /// Provides a higher degree of confidence because only devices in the "generic_bolt_ver1"
        /// branch of the device tree are available for matching and checks are performed for
        /// version of the bolt browser.
        /// </summary>
        internal override byte Confidence
        {
            get { return (byte) (base.Confidence + EXTRA_CONFIDENCE); }
        }

        /// <summary>
        /// An array of supported root devices that devices from the WURFL
        /// data file need to be children of to be valid for this handler.
        /// </summary>
        protected override string[] SupportedRootDeviceIds
        {
            get { return SUPPORTED_ROOT_DEVICES; }
        }

        /// <summary>
        /// Check for the presence of bolt in addition to the base safari checks.
        /// </summary>
        /// <param name="userAgent"></param>
        /// <returns></returns>
        protected internal override bool CanHandle(string userAgent)
        {
            return base.CanHandle(userAgent) &&
                   userAgent.IndexOf("bolt", StringComparison.InvariantCultureIgnoreCase) >= 0;
        }
    }
}