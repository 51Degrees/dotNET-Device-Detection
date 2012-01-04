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
 * Mobile Experts Limited are Copyright (C) 2009 - 2012. All Rights Reserved.
 * 
 * Contributor(s):
 *     James Rosewell <james@51degrees.mobi>
 * 
 * ********************************************************************* */

#region Usings

using System.Collections.Generic;
using System.Text.RegularExpressions;
using FiftyOne.Foundation.Mobile.Detection.Matchers.Segment;
using System;

#endregion

namespace FiftyOne.Foundation.Mobile.Detection.Handlers
{
    /// <summary>
    /// Device detection handler using regular expressions to segment strings
    /// before matching specific segments.
    /// </summary>
    public class RegexSegmentHandler : SegmentHandler
    {
        #region Classes

        /// <summary>
        /// Contains regular expression and weight to apply to
        /// each segment of the user agent string.
        /// </summary>
        public class RegexSegment
        {
            #region Fields

            private Regex _pattern;
            private int _weight;

            #endregion

            #region Properties

            /// <summary>
            /// The regular expression to use to get the segment.
            /// </summary>
            public Regex Pattern { get { return _pattern; }}

            /// <summary>
            /// The weight that should be given to the segment. The lower 
            /// the number the greater the significance.
            /// </summary>
            public int Weight { get { return _weight; } }

            #endregion

            #region Constructor

            /// <summary>
            /// Constructs a new instance of <see cref="RegexSegment"/>.
            /// </summary>
            /// <param name="pattern">The regular expression for the segment.</param>
            /// <param name="weight">The relative weight to apply to the segment.</param>
            internal RegexSegment(string pattern, int weight)
            {
                _pattern = new Regex(pattern, RegexOptions.Compiled | RegexOptions.CultureInvariant);
                _weight = weight;
            }

            #endregion
        }

        #endregion

        #region Fields
    
        /// <summary>
        /// A list of segments to be found and matched by the handler.
        /// </summary>
        private List<RegexSegment> _segments = new List<RegexSegment>();

        #endregion

        #region Properties

        /// <summary>
        /// A list of the regular expressions used to create segments.
        /// </summary>
        public List<RegexSegment> Segments
        {
            get { return _segments; }
        }

        #endregion

        #region Constructor

        internal RegexSegmentHandler(BaseProvider provider, string name, string defaultDeviceId, byte confidence, bool checkUAProfs)
            : base(provider, name, defaultDeviceId, confidence, checkUAProfs)
        {
        }

        #endregion

        #region Methods

        internal void AddSegment(string pattern, int weight)
        {
            _segments.Add(new RegexSegment(pattern, weight));
        }

        #endregion

        #region Overridden Methods

        /// <summary>
        /// Returns true if the handler can match the requests useragent string
        /// and at least one valid segment ise returned as a segment.
        /// </summary>
        /// <param name="userAgent"></param>
        /// <returns></returns>
        public override bool CanHandle(string userAgent)
        {
            if (base.CanHandle(userAgent) == false)
                return false;

            foreach (RegexSegment segment in _segments)
                if (segment.Pattern.IsMatch(userAgent))
                    return true;

            return false;
        }

        #endregion

        #region Abstract Method Implementation

        /// <summary>
        /// Returns segments for the index specified.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        internal override List<Segment> CreateSegments(string source, int index)
        {
             return CreateSegments(source, _segments[index]);
        }

        /// <summary>
        /// Returns the segments from the source string. Where a segment returns nothing
        /// a single empty segment will be added.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        internal override Segments CreateAllSegments(string source)
        {
            Segments results = new Segments();
            foreach (RegexSegment segment in _segments)
            {
                results.Add(CreateSegments(source, segment));
            }
            return results;
        }

        private List<Segment> CreateSegments(string source, RegexSegment segment)
        {
            bool matched = false;
            var newSegments = new List<Segment>();
            var matches = segment.Pattern.Matches(source);

            // Add a segment for each match found.
            foreach (Match match in matches)
            {
                if (match.Success)
                {
                    newSegments.Add(new Segment(match.Value, segment.Weight));
                    matched = true;
                }
            }
            if (matched == false)
            {
                // Add an empty segment to avoid problems of missing segments
                // stopping others being compared correctly.
                newSegments.Add(new Segment(String.Empty, segment.Weight));
            }

            return newSegments;
        }

        #endregion
    }
}