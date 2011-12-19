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

#region Usings

using System;
using FiftyOne.Foundation.Mobile.Detection.Matchers.Segment;
using Results=FiftyOne.Foundation.Mobile.Detection.Matchers.Results;
using System.Collections.Generic;

#endregion

namespace FiftyOne.Foundation.Mobile.Detection.Handlers
{
    /// <summary>
    /// An abstract handler used by any handler that breaks a string down into
    /// segments.
    /// </summary>
    public abstract class SegmentHandler : Handler
    {
        #region Constructor

        internal SegmentHandler(BaseProvider provider, string name, string defaultDeviceId, byte confidence, bool checkUAProfs)
            : base(provider, name, defaultDeviceId, confidence, checkUAProfs)
        {
        }

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Creates segments for all regexes.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        internal abstract Segments CreateAllSegments(string source);

        /// <summary>
        /// Creates  segments for the regex index provided.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        internal abstract List<Segment> CreateSegments(string source, int index);

        #endregion

        #region Overridden Methods

        internal override Results Match(string userAgent)
        {
            return Matcher.Match(userAgent, this);
        }

        #endregion
    }
}