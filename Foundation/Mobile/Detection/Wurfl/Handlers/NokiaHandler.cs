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
    internal class NokiaHandler : RegexSegmentHandler
    {
        private const string DEFAULT_DEVICE = "nokia_generic_series60";
        
        private static readonly string[] PATTERNS = {
            // Nokia model name
            @"(?:(?<=[Nokia|NOKIA])[^/]+)",
            // Major model version
            @"(?<=(?:[Nokia|NOKIA][^/]+)/)[\d.]+",
            // Minor version
            @"\([\d.]+\)" };

        public NokiaHandler()
            : base(PATTERNS, new int[] { 100, 1, 1 }, true)
        { }

        // Checks UA contains "Nokia".
        internal protected override bool CanHandle(string userAgent)
        {
            return (userAgent.Contains("Nokia") ||
                userAgent.Contains("Symbian OS") ||
                userAgent.Contains("NOKIA")) &&
                base.CanHandle(userAgent); 
        }

        internal protected override Results Match(string userAgent)
        {
            Results results = base.Match(userAgent); ;
            if (results == null)
            {
                DeviceInfo device = null;
                if (userAgent.Contains("Series60"))
                    device = Provider.Instance.GetDeviceInfoFromID("nokia_generic_series60");
                else if (userAgent.Contains("Series80"))
                    device = Provider.Instance.GetDeviceInfoFromID("nokia_generic_series80");
                if (device != null)
                    results = new Results(device);
            }
            return results;
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
