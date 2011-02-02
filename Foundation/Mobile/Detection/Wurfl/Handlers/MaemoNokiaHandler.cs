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
 * Mobile Experts Limited are Copyright (C) 2009 - 2011. All Rights Reserved.
 * 
 * Contributor(s):
 *     James Rosewell <james@51degrees.mobi>
 * 
 * ********************************************************************* */

namespace FiftyOne.Foundation.Mobile.Detection.Wurfl.Handlers
{
    internal class MaemoNokiaHandler : EditDistanceHandler
    {
        private const string DEFAULT_DEVICE = "nokia_generic_maemo";

        // Needs to be high to ensure Firefox handler isn't included.
        private const byte EXTRA_CONFIDENCE = 4;

        private static readonly string[] SUPPORTED_ROOT_DEVICES = new[] { DEFAULT_DEVICE };

        /// <summary>
        /// Provides a higher degree of confidence because only devices in the "nokia_generic_maemo"
        /// branch of the device tree are available for matching.
        /// </summary>
        internal override byte Confidence
        {
            get { return (byte)(base.Confidence + EXTRA_CONFIDENCE); }
        }

        /// <summary>
        /// An array of supported root devices that devices from the WURFL
        /// data file need to be children of to be valid for this handler.
        /// </summary>
        protected override string[] SupportedRootDeviceIds
        {
            get { return SUPPORTED_ROOT_DEVICES; }
        }

        // Checks given UA contains "Maemo"
        protected internal override bool CanHandle(string userAgent)
        {
            return userAgent.Contains("Maemo") || userAgent.Contains("maemo");
        }
    }
}