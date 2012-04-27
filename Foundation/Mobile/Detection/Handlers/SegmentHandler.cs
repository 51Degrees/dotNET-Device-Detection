/* *********************************************************************
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0.
 * 
 * If a copy of the MPL was not distributed with this file, You can obtain
 * one at http://mozilla.org/MPL/2.0/.
 * 
 * This Source Code Form is “Incompatible With Secondary Licenses”, as
 * defined by the Mozilla Public License, v. 2.0.
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
        /// <param name="userAgent">The useragent segments should be returned for.</param>
        /// <returns>The list of segments.</returns>
        internal abstract Segments CreateAllSegments(string userAgent);

        /// <summary>
        /// Creates segments for the regex index provided.
        /// </summary>
        /// <param name="device">The device to get the segments from.</param>
        /// <param name="index">The index of the segment required.</param>
        /// <returns>The list of segments.</returns>
        internal abstract List<Segment> CreateSegments(BaseDeviceInfo device, int index);

        #endregion

        #region Overridden Methods

        internal override Results Match(string userAgent)
        {
            return Matcher.Match(userAgent, this);
        }

        #endregion
    }
}