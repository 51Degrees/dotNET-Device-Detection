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

using System.Text.RegularExpressions;
using System.Collections.Generic;
using FiftyOne.Foundation.Mobile.Detection.Wurfl.Matchers.Segment;

namespace FiftyOne.Foundation.Mobile.Detection.Wurfl.Handlers 
{
    internal abstract class RegexSegmentHandler : SegmentHandler
    {
        private const string DEFAULT_PATTERN = @"[^ ]+";
  
        // This is a more precise method of matching a useragent to a device and
        // we can therefore assign a higher level of confidence.
        internal const byte CONFIDENCE = 7;
                
        protected Regex[] _patterns;
        protected int[] _weights;
        protected bool _firstMatchOnly = false;

        internal override byte Confidence
        {
            get { return CONFIDENCE; }
        }

        /// <summary>
        /// Constructs an instance of <cref see="RegexSegmentHandler"/>.
        /// </summary>
        /// <param name="regex">The regular expression to use for a single segment.</param>
        internal RegexSegmentHandler(string regex)
        {
            Init(new string[] { regex }, new int[] { 1 }, false);
        }

        /// <summary>
        /// Constructs an instance of <cref see="RegexSegmentHandler"/>.
        /// </summary>
        /// <param name="regex">The regular expression to use for a single segment.</param>
        /// <param name="weights">A single item array with the weight of the item.</param>
        internal RegexSegmentHandler(string regex, int[] weights)
        {
            Init(new string[] { regex }, weights, false);
        }

        /// <summary>
        /// Constructs an instance of <cref see="RegexSegmentHandler"/>.
        /// </summary>
        /// <param name="regexs">An array of regular expressions for each segment.</param>
        /// <param name="weights">An array of weights assigned to each segment. The higher the value the more significance given.</param>
        internal RegexSegmentHandler(string[] regexs, int[] weights)
        {
            Init(regexs, weights, false);
        }

        /// <summary>
        /// Constructs an instance of <cref see="RegexSegmentHandler"/>.
        /// </summary>
        /// <param name="regex">An array of regular expressions for each segment.</param>
        /// <param name="weights">An array of weights assigned to each segment. The higher the value the more significance given.</param>
        /// <param name="firstMatchOnly">True if the 1st device matched should be returned.</param>
        internal RegexSegmentHandler(string regex, int[] weights, bool firstMatchOnly)
        {
            Init(new string[] { regex }, weights, firstMatchOnly);
        }

        /// <summary>
        /// Constructs an instance of <cref see="RegexSegmentHandler"/>.
        /// </summary>
        /// <param name="regexs">An array of regular expressions for each segment.</param>
        /// <param name="weights">An array of weights assigned to each segment. The higher the value the more significance given.</param>
        /// <param name="firstMatchOnly">True if the 1st device matched should be returned.</param>
        internal RegexSegmentHandler(string[] regexs, int[] weights, bool firstMatchOnly)
        {
            Init(regexs, weights, firstMatchOnly);
        }

        private void Init(string[] regexs, int[] weights, bool firstMatchOnly)
        {
            _patterns = new Regex[regexs.Length];
            for (int i = 0; i < regexs.Length; i++)
            {
                _patterns[i] = new Regex(regexs[i], RegexOptions.Compiled);
            }
            _weights = weights;
            _firstMatchOnly = firstMatchOnly;
        }

        internal override Segments CreateSegments(string source)
        {
            if (_firstMatchOnly == true)
                return CreateSegmentsFirstMatch(source);
            else
                return CreateSegmentsMultipleMatches(source);
        }

        private Segments CreateSegmentsMultipleMatches(string source)
        {
            Segments segments = new Segments();
            foreach (Regex pattern in _patterns)
            {
                MatchCollection matches = pattern.Matches(source);
                if (matches != null)
                {
                    foreach (Match match in matches)
                    {
                        segments.Add(new Segment(match.Value));
                    }
                }
            }
            return segments;
        }

        private Segments CreateSegmentsFirstMatch(string source)
        {
            Segments segments = new Segments();
            foreach (Regex pattern in _patterns)
            {
                Match match = pattern.Match(source);
                if (match != null && match.Success == true)
                    segments.Add(new Segment(match.Value));
                else
                    segments.Add(new Segment(string.Empty));
            }
            return segments;
        }

        internal override int GetSegmentWeight(int index, int numberOfSegments)
        {
            if (index < _weights.Length)
                // We have a weight specified so return this one.
                return _weights[index];
            else
                // Weight segments in reverse order as the initial segments
                // usually have the most impact on the match.
                return numberOfSegments - index;
        }

        /// <summary>
        /// Returns true if the handler can match the requests useragent string.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        internal override bool CanHandle(System.Web.HttpRequest request)
        {
            return CanHandle(Provider.GetUserAgent(request));
        }

        /// <summary>
        /// Returns true if the handler can match the requests useragent string.
        /// </summary>
        /// <param name="userAgent"></param>
        /// <returns></returns>
        internal protected override bool CanHandle(string userAgent)
        {
            foreach (Regex pattern in _patterns)
            {
                if (pattern.IsMatch(userAgent) == true)
                    return true;
            }
            return false;
        }
    }
}
