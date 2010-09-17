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
    internal class FirefoxHandler : RegexSegmentHandler
    {
        private const string REGEX =
            "Mozilla/5.0\\s*(?:\\(([^;]*);?\\s*U;\\s*([^;]*);?\\s*([^;]*);?\\s*(?:rv.*);?\\s*(?:.*)?\\))(?:.*)Gecko/(?:[\\d]+)?(?:.*)?Firefox/(\\d.\\w)(?:[\\.\\w]+)?\\s*(.*)?";

        // Initialise the regular expression tokens.
        internal FirefoxHandler() : base(REGEX, new[] {1, 1, 1, 2, 0})
        {
        }

        // Checks the given UA contains "Firefox".
        protected internal override bool CanHandle(string userAgent)
        {
            return userAgent.Contains("Firefox");
        }
    }

    internal class FirefoxDesktopHandler : FirefoxHandler
    {
        private const string DEFAULT_DEVICE = "firefox";
        private const byte EXTRA_CONFIDENCE = 1;
        private static readonly string[] SUPPORTED_ROOT_DEVICES = new[] {DEFAULT_DEVICE};

        /// <summary>
        /// Provides a higher degree of confidence because only devices in the "firefox"
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

        // Checks the US does not contain mobile strings and does contain desktop strings.

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
            return
                (userAgent.Contains("Firefox") ||
                 userAgent.Contains("Iceweasel") ||
                 userAgent.Contains("Thunderbird") ||
                 userAgent.Contains("Gecko/"))
                &&
                (userAgent.Contains("(Macintosh;") ||
                 userAgent.Contains("(Windows") ||
                 userAgent.Contains("(X11;"));
        }
    }
}