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

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace FiftyOne.Foundation.Mobile.Detection.Wurfl.Handlers
{
    internal abstract class VersionHandler : EditDistanceHandler
    {
        private Regex[] _versionRegexes = null;

        // This is a very precise method of matching a useragent to a device and
        // we can therefore assign a very high level of confidence.
        private const int CONFIDENCE = 9;
                
        internal VersionHandler(string[] patterns)
        {
            _versionRegexes = new Regex[patterns.Length];
            for(int i = 0; i < _versionRegexes.Length; i++)
                _versionRegexes[i] = new Regex(patterns[i], RegexOptions.Compiled);
        }

        internal override byte Confidence
        {
            get { return CONFIDENCE; }
        }

        internal VersionHandler(string pattern)
        {
            _versionRegexes = new Regex[] {
                new Regex(pattern, RegexOptions.Compiled) };
        }

        internal Regex[] VersionRegexes
        {
            get { return _versionRegexes; }
        }

        internal protected override Matchers.Results Match(string userAgent)
        {
            Matchers.Results results = FiftyOne.Foundation.Mobile.Detection.Wurfl.Matchers.Version.Matcher.Match(userAgent, this);
            if (results == null)
                return base.Match(userAgent);
            return results;
        }
    }
}
