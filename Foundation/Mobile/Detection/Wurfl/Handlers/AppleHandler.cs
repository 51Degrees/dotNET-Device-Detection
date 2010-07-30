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
    internal class AppleHandler : RegexSegmentHandler
    {
        private const string DEFAULT_DEVICE = "apple_iphone_ver1";

        private static readonly string[] PATTERNS = {
            // Device type.
            @"iPhone|iPod|iPad",
            // Apple Apple Web Kit verion
            @"(?<=AppleWebKit/)[\d.]+",
            // Major version
            @"(?<=Version/)[\d.]+",
            // Mobile version
            @"(?<=Mobile/)[\d\w]+",
            // Safari version
            @"(?<=Safari/)[\d.]+" };
        
        // This handler will only handle specific strings used by Apple devices.
        // However there may be manufacturer handlers than will be more accurate.
        private const int CONFIDENCE = 6;

        internal override byte Confidence
        {
            get { return CONFIDENCE; }
        }

        public AppleHandler()
            : base(PATTERNS, new int[] { 100, 1, 1, 1, 1 })
        { }

        // Checks given UA contains "iPhone", "iPad" or "iPod"
        internal protected override bool CanHandle(string userAgent)
        {
            return userAgent.Contains("iPhone") ||
                userAgent.Contains("iPod") ||
                userAgent.Contains("iPad");
        }

        internal override DeviceInfo DefaultDevice
        {
            get
            {
                DeviceInfo device = Provider.Instance.GetDeviceInfoFromID(DEFAULT_DEVICE);
                if (device != null)
                    return device;
                return base.DefaultDevice;
            }
        }
    }
}
