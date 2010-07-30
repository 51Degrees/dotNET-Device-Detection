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

using System.Collections.Generic;
using System;

namespace FiftyOne.Foundation.Mobile.Detection.Wurfl.Matchers.ReducedInitialString
{
    internal class Matcher
    {
        internal static Results Match(string userAgent, FiftyOne.Foundation.Mobile.Detection.Wurfl.Handlers.Handler handler, int tolerance)
        {
            DeviceInfo bestMatch = null;
            int maxInitialString = 0;
            lock (handler.UserAgents)
            {
                foreach (DeviceInfo[] devices in handler.UserAgents)
                {
                    foreach (DeviceInfo device in devices)
                    {
                        Check(userAgent, ref bestMatch, ref maxInitialString, device);
                    }
                }
            }
            if (maxInitialString >= tolerance)
            {
                return new Results(bestMatch);
            }
            return null;
        }

        private static void Check(string userAgent, ref DeviceInfo bestMatch, ref int maxInitialString, DeviceInfo current)
        {
            if ((userAgent.StartsWith(current.UserAgent) == true ||
                current.UserAgent.StartsWith(userAgent) == true) &&
                maxInitialString < current.UserAgent.Length)
            {
                maxInitialString = current.UserAgent.Length;
                bestMatch = current;
            }
        }
    }
}
