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
    internal class SafariHandler : RegexSegmentHandler 
    {
        private const string DEFAULT_DEVICE = "generic_xhtml";
        private static readonly string[] UNSUPPORTED_ROOT_DEVICES = new string[] { "safari" };

        private static readonly string[] PATTERNS = {
            // Details about the device.
            @"(?<=Mozilla/\d.\d \()[^)]+",
            // Apple Apple Web Kit verion
            @"(?<=AppleWebKit/)[\d.]+",
            // Major version
            @"(?<=Version/)[\d.]+",
            // Safari version
            @"(?<=Safari/)[\d.]+" };

        internal SafariHandler(string[] regexs, int[] weights) : base(regexs, weights) { _firstMatchOnly = true; }
        internal SafariHandler() : base(PATTERNS, new int[] { 3, 1, 1, 1 }) { _firstMatchOnly = true; }
        
        internal protected override bool CanHandle(DeviceInfo device)
        {
            return base.CanHandle(device) &&
                CanHandleDevice(device);
        }

        /// <summary>
        /// Checks to ensure the device is not associated with root devices handled
        /// by the SafariDesktopHandler.
        /// </summary>
        /// <param name="device">Device to be checked.</param>
        /// <returns>True if the device is not associated with the unsupported devices.</returns>
        private bool CanHandleDevice(DeviceInfo device)
        {
            foreach (string deviceId in UNSUPPORTED_ROOT_DEVICES)
            {
                if (deviceId == device.DeviceId)
                    return false;
            }
            if (device.FallbackDevice != null)
                return CanHandleDevice(device.FallbackDevice);
            return true;
        }
        
        // Checks given UA contains "Safari"
        internal protected override bool CanHandle(string userAgent)
        {
            return  userAgent.StartsWith("Mozilla") == true && 
                    userAgent.Contains("Safari") == true &&
                    userAgent.Contains("iPhone") == false &&
                    userAgent.Contains("iPod") == false &&
                    userAgent.Contains("iPad") == false;
        }
    }

    internal class SafariDesktopHandler : RegexSegmentHandler
    {
        private const string DEFAULT_DEVICE = "safari";
        private const byte EXTRA_CONFIDENCE = 1;
        private static readonly string[] SUPPORTED_ROOT_DEVICES = new string[] { DEFAULT_DEVICE };

        private static readonly string[] PATTERNS = {
            // Base OS platform.
            @"(?<=Mozilla/\d.\d \()[^;]+",
            // Safari major version
            @"(?<=Safari/)[\d.]+" };

        internal SafariDesktopHandler() : base(PATTERNS, new int[] { 3, 1 }) { _firstMatchOnly = true; }

        /// <summary>
        /// Provides a higher degree of confidence because only devices in the "safari"
        /// branch of the device tree are available for matching.
        /// </summary>
        internal override byte Confidence { get { return (byte)(base.Confidence + EXTRA_CONFIDENCE); } }

        /// <summary>
        /// An array of supported root devices that devices from the WURFL
        /// data file need to be children of to be valid for this handler.
        /// </summary>
        protected override string[] SupportedRootDeviceIds { get { return SUPPORTED_ROOT_DEVICES; } }

        // Checks given UA contains "Safari" as well as "Windows"
        // or "Macintosh" and does not have a "Mobile" version.
        internal protected override bool CanHandle(string userAgent)
        {
            return userAgent.StartsWith("Mozilla") &&
                userAgent.Contains("Safari") && 
                (userAgent.Contains("Windows") ||
                userAgent.Contains("Macintosh") ||
                userAgent.Contains("X11"));
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