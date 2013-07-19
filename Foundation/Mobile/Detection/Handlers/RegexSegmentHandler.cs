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
using System.Collections.Generic;
using System.Text.RegularExpressions;
using FiftyOne.Foundation.Mobile.Detection.Matchers.Segment;

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

        /// <summary>
        /// Lock used to add new segments to the device.
        /// </summary>
        private object _createSegmentsLock = new object();

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

        internal RegexSegmentHandler(Provider provider, string name, string defaultDeviceId, byte confidence, bool checkUAProfs)
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
        /// Returns segments for the index specified checking in the stored results first
        /// if the StoreSegmentResults constant is enabled.
        /// </summary>
        /// <param name="device">The source useragent string.</param>
        /// <param name="index">The index of the regular expression to use to get the segments.</param>
        /// <returns>The list of matching segments.</returns>
        #pragma warning disable 162
        internal override List<Segment> CreateSegments(BaseDeviceInfo device, int index)
        {
            List<Segment> segments = null;
            if (Constants.StoreSegmentResults)
            {
                // Get the handlers data from the device.
                List<List<Segment>> cachedSegments = (List<List<Segment>>)device.GetHandlerData<List<List<Segment>>>(this);

                // If the segment does not already exist then add it.
                if (cachedSegments.Count <= index)
                {
                    lock (_createSegmentsLock)
                    {
                        if (cachedSegments.Count <= index)
                        {
                            while (cachedSegments.Count <= index)
                                cachedSegments.Add(new List<Segment>());
                            segments = CreateSegments(device.UserAgent, _segments[index]);
                            cachedSegments[index] = segments;
                        }
                    }
                }
                else
                    segments = cachedSegments[index];
            }
            else
            {
                segments = CreateSegments(device.UserAgent, _segments[index]);
            }
            return segments;
        }
        #pragma warning restore 162

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
            List<Segment> newSegments = new List<Segment>();
            MatchCollection matches = segment.Pattern.Matches(source);

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