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
    internal class OperaHandler : RegexSegmentHandler
    {
        private const string DEFAULT_DEVICE = "generic_xhtml";

        // The Opera version
        private static readonly string PATTERN = @"(?<=Opera/)[\d+.]+|(?<=Opera )[\d+.]+";

        public OperaHandler() : base(PATTERN)
        {
        }

        // Checks the given UA contains "Opera" indicating the browser
        // is almost certainly of the Opera family.

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

        protected internal override bool CanHandle(string userAgent)
        {
            return userAgent.StartsWith("Opera") &&
                   OperaDesktopHandler.InternalCanHandle(userAgent) == false &&
                   OperaMiniHandler.InternalCanHandle(userAgent) == false &&
                   OperaMobiHandler.InternalCanHandle(userAgent) == false &&
                   base.CanHandle(userAgent);
        }
    }

    internal class OperaDesktopHandler : RegexSegmentHandler
    {
        private const string DEFAULT_DEVICE = "opera";
        private const byte EXTRA_CONFIDENCE = 1;

        // Opera version string.
        private static readonly string PATTERN = @"(?<=$Opera/)[\d+.]+|(?<=Opera )[\d+.]+";
        private static readonly string[] SUPPORTED_ROOT_DEVICES = new[] {DEFAULT_DEVICE};

        public OperaDesktopHandler() : base(PATTERN)
        {
        }

        /// <summary>
        /// Provides a higher degree of confidence because only devices in the "opera"
        /// branch of the device tree are available for matching.
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

        // Checks given UA contains "Safari" as well as "Windows"
        // or "Macintosh" and does not have a "Mobile" version.
        protected internal override bool CanHandle(string userAgent)
        {
            return InternalCanHandle(userAgent) &&
                   OperaMiniHandler.InternalCanHandle(userAgent) == false &&
                   OperaMobiHandler.InternalCanHandle(userAgent) == false &&
                   base.CanHandle(userAgent);
        }

        internal static bool InternalCanHandle(string userAgent)
        {
            return userAgent.StartsWith("Opera");
        }
    }

    internal class OperaMiniHandler : RegexSegmentHandler
    {
        private const string DEFAULT_DEVICE = "opera_mini_ver1";
        private const byte EXTRA_CONFIDENCE = 1;

        // The Opera Mini version
        private static readonly string[] PATTERNS = {
                                                        // Opera mini version.
                                                        @"(?<=Opera Mini/)\d.\d",
                                                        // Opera version
                                                        @"(?<=Opera/)[\d.]+"
                                                    };

        private static readonly string[] SUPPORTED_ROOT_DEVICES = new[] {DEFAULT_DEVICE};

        public OperaMiniHandler() : base(PATTERNS, new[] {1, 1})
        {
            _firstMatchOnly = true;
        }

        /// <summary>
        /// Provides a higher degree of confidence because only devices in the "opera_mini_ver1"
        /// branch of the device tree are available for matching.
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

        // Checks the given UA contains "Opera Mini" indicating the browser
        // is almost certainly of the Opera family.
        protected internal override bool CanHandle(string userAgent)
        {
            return InternalCanHandle(userAgent) &&
                   base.CanHandle(userAgent);
        }

        internal static bool InternalCanHandle(string userAgent)
        {
            return userAgent.Contains("Opera Mini");
        }
    }

    internal class OperaMobiHandler : EditDistanceHandler
    {
        // Checks the given UA contains "Opera Mobi/" indicating the browser
        // is almost certainly of the Opera family.
        protected internal override bool CanHandle(string userAgent)
        {
            return InternalCanHandle(userAgent);
        }

        internal static bool InternalCanHandle(string userAgent)
        {
            return userAgent.Contains("Opera Mobi/");
        }
    }
}