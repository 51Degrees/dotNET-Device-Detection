/* *********************************************************************
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited. 
 * Copyright © 2014 51Degrees Mobile Experts Limited, 5 Charlotte Close,
 * Caversham, Reading, Berkshire, United Kingdom RG4 7BY
 * 
 * This Source Code Form is the subject of the following patent 
 * applications, owned by 51Degrees Mobile Experts Limited of 5 Charlotte
 * Close, Caversham, Reading, Berkshire, United Kingdom RG4 7BY: 
 * European Patent Application No. 13192291.6; and
 * United States Patent Application Nos. 14/085,223 and 14/085,301.
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0.
 * 
 * If a copy of the MPL was not distributed with this file, You can obtain
 * one at http://mozilla.org/MPL/2.0/.
 * 
 * This Source Code Form is “Incompatible With Secondary Licenses”, as
 * defined by the Mozilla Public License, v. 2.0.
 * ********************************************************************* */

using System.Collections.Generic;
using FiftyOne.Foundation.Mobile.Detection.Entities;
using System.Diagnostics;
using System.Collections.Specialized;
using System.Linq;
using System.IO;
using System.Reflection;
using FiftyOne.Foundation.Mobile.Detection.Readers;
using System.IO.Compression;
using FiftyOne.Foundation.Mobile.Detection.Factories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FiftyOne.Foundation.Mobile.Detection
{
    /// <summary>
    /// Provider used to perform a detection based on a user agent string. 
    /// </summary>
    public class Provider
    {
        #region Match Method Stats

        /// <summary>
        /// The total number of detections performed by the data set.
        /// </summary>
        public long DetectionCount
        {
            get { return _detectionCount; }
        }
        internal long _detectionCount;

        #endregion

        #region Fields

        /// <summary>
        /// A cache for user agents.
        /// </summary>
        private readonly Cache<string, MatchState> _userAgentCache = null;

        /// <summary>
        /// The number of detections performed using the method.
        /// </summary>
        public readonly SortedList<MatchMethods, long> MethodCounts;

        /// <summary>
        /// The data set associated with the provider.
        /// </summary>
        public readonly DataSet DataSet;

        /// <summary>
        /// True if the detection time should be recorded in the Elapsed
        /// property of the DetectionMatch object.
        /// </summary>
        public readonly bool RecordDetectionTime = false;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a new provider using the data set.
        /// </summary>
        /// <param name="dataSet">Data set to use for device detection</param>
        public Provider(DataSet dataSet) : this (dataSet, false, 0)
        {
        }

        /// <summary>
        /// Constructs a new provider using the data set.
        /// </summary>
        /// <param name="dataSet">Data set to use for device detection</param>
        /// <param name="cacheSize">Size of the cache used with the provider</param>
        public Provider(DataSet dataSet, int cacheSize)
            : this(dataSet, false, cacheSize)
        {
        }

        /// <summary>
        /// Constructs a new provided using the data set.
        /// </summary>
        /// <param name="dataSet">Data set to use for device detection</param>
        /// <param name="recordDetectionTime">True if the detection time should be recorded</param>
        /// <param name="cacheSize">Size of the cache used with the provider</param>
        public Provider(DataSet dataSet, bool recordDetectionTime, int cacheSize)
        {
            DataSet = dataSet;
            MethodCounts = new SortedList<MatchMethods, long>();
            MethodCounts.Add(MatchMethods.Closest, 0);
            MethodCounts.Add(MatchMethods.Nearest, 0);
            MethodCounts.Add(MatchMethods.Exact, 0);
            MethodCounts.Add(MatchMethods.Numeric, 0);
            MethodCounts.Add(MatchMethods.None, 0);
            RecordDetectionTime = recordDetectionTime;
            _userAgentCache = cacheSize > 0 ? new Cache<string, MatchState>(cacheSize) : null;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The percentage of requests for user agents which were not already
        /// contained in the cache.
        /// </summary>
        public double PercentageCacheMisses
        {
            get
            {
                return _userAgentCache != null ? _userAgentCache.PercentageMisses : 0;
            }
        }

        /// <summary>
        /// Number of times the useragents cache was switched.
        /// </summary>
        public long CacheSwitches
        {
            get
            {
                return _userAgentCache != null ? _userAgentCache.Switches : 0;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Resets the match object provided and performs the detection
        /// using that object rather than creating a new match.
        /// </summary>
        /// <param name="headers">
        /// List of HTTP headers to use for the detection
        /// </param>
        /// <param name="match">
        /// A match object created by a previous match, or via the 
        /// <see cref="CreateMatch"/> method.
        /// </param>
        /// <returns>
        /// The match object passed to the method updated with
        /// results for the target user agent
        /// </returns>
        public Match Match(NameValueCollection headers, Match match)
        {
            if (headers == null || headers.Count == 0)
            {
                // Empty headers all default match result.
                Controller.MatchDefault(match);
            }
            else
            {
                // Get the overlapping headers?
                var importantHeaders = headers.AllKeys.Intersect(DataSet.HttpHeaders);

                if (importantHeaders.Count() == 1)
                {
                    // If only 1 header is important then return a simple single match.
                    Match(headers[importantHeaders.First()], match);
                }
                else
                {
                    // Create matches for each of the headers.
                    var matches = MatchForHeaders(match, headers, importantHeaders);

                    // A list of new profiles to use with the match.
                    var newProfiles = new Profile[DataSet.Components.Count];
                    var newProfileIndex = 0;
                    foreach(var component in DataSet.Components)
                    {
                        foreach(var header in component.HttpHeaders)
                        {
                            Match headerMatch;
                            if (matches.TryGetValue(header, out headerMatch))
                            {
                                // Update the statistics about the matching process.
                                match._signaturesCompared += headerMatch._signaturesCompared;
                                match._signaturesRead += headerMatch._signaturesRead;
                                match._stringsRead += headerMatch._stringsRead;
                                match._rootNodesEvaluated += headerMatch._rootNodesEvaluated;
                                match._nodesEvaluated += headerMatch._nodesEvaluated;
                                match._elapsed += headerMatch._elapsed;

                                // Set the profile for this component.
                                newProfiles[newProfileIndex] = headerMatch.ComponentProfiles[component.ComponentId];

                                // Move to the next index.
                                newProfileIndex++;

                                break;
                            }
                        }
                    }

                    // Reset any fields that relate to the profiles assigned
                    // to the match result.
                    match._signature = null;
                    match._componentProfiles = null;
                    match._results = null;

                    // Replace the match profiles with the new ones.
                    match._profiles = newProfiles;
                }
            }
            return match;
        }

        /// <summary>
        /// Matches each of the required headers.
        /// </summary>
        /// <param name="match"></param>
        /// <param name="headers"></param>
        /// <param name="importantHeaders"></param>
        /// <returns></returns>
        private Dictionary<string, Match> MatchForHeaders(Match match, NameValueCollection headers, IEnumerable<string> importantHeaders)
        {
            var matches = new Dictionary<string, Match>();
            var iterator = importantHeaders.GetEnumerator();
            var currentMatch = match;
            while(iterator.MoveNext())
            {
                matches.Add(iterator.Current, currentMatch != null ? currentMatch : CreateMatch());
                currentMatch = null;
            }
            Parallel.ForEach(matches, m =>
            {
                Match(headers[m.Key], m.Value);
            });
            return matches;
        }
        
        /// <summary>
        /// For a given collection of HTTP headers returns a match containing 
        /// information about the capabilities of the device and 
        /// it's components.
        /// </summary>
        /// <param name="headers">
        /// List of HTTP headers to use for the detection
        /// </param>
        /// <returns>
        /// A fresh match object populated with the results
        /// for the target user agent.
        /// </returns>
        public Match Match(NameValueCollection headers)
        {
            return Match(headers, CreateMatch());
        }

        /// <summary>
        /// Resets the match object provided and performs the detection
        /// using that object rather than creating a new match.
        /// </summary>
        /// <param name="targetUserAgent">
        /// The user agent string to use as the target
        /// </param>
        /// <param name="match">
        /// A match object created by a previous match, or via the 
        /// <see cref="CreateMatch"/> method.
        /// </param>
        /// <returns>
        /// The match object passed to the method updated with
        /// results for the target user agent
        /// </returns>
        public Match Match(string targetUserAgent, Match match)
        {
            MatchState state;

            if (_userAgentCache != null &&
                String.IsNullOrEmpty(targetUserAgent) == false)
            {
                // Increase the cache requests.
                Interlocked.Increment(ref _userAgentCache.Requests);

                if (_userAgentCache._itemsActive.TryGetValue(targetUserAgent, out state) == false)
                {
                    // The user agent has not been checked previously. Therefore perform
                    // the match and store the results in the cache.
                    MatchNoCache(targetUserAgent, match);

                    // Record the match state in the cache for next time.
                    state = new MatchState(match);
                    _userAgentCache._itemsActive[targetUserAgent] = state;

                    // Increase the cache misses.
                    Interlocked.Increment(ref _userAgentCache.Misses);
                }
                else
                {
                    // The state of a previous match exists so the match should
                    // be configured based on the results of the previous state.
                    match.SetState(state);
                }
                _userAgentCache.AddRecent(targetUserAgent, state);
            }
            else
            {
                // The cache does not exist so call the non caching method.
                MatchNoCache(targetUserAgent, match);
            }

            return match;
        }

        private void MatchNoCache(string targetUserAgent, Match match)
        {
            // Reset the match instance ready to use the target user agent provided.
            match.Reset(targetUserAgent);

            if (RecordDetectionTime)
            {
                match.Timer.Start();
            }

            Controller.Match(match);

            if (RecordDetectionTime)
            {
                match.Timer.Stop();
                match._elapsed = match.Timer.ElapsedTicks;
                match.Timer.Reset();
            }

            // Update the counts for the provider.
            Interlocked.Increment(ref _detectionCount);
            lock (MethodCounts)
            {
                MethodCounts[match.Method]++;
            }
        }

        /// <summary>
        /// For a given user agent returns a match containing 
        /// information about the capabilities of the device and 
        /// it's components.
        /// </summary>
        /// <param name="targetUserAgent">
        /// The user agent string to use as the target
        /// </param>
        /// <returns>
        /// A fresh match object populated with the results
        /// for the target user agent.
        /// </returns>
        public Match Match(string targetUserAgent)
        {
            return Match(targetUserAgent, CreateMatch());
        }
        
        /// <summary>
        /// Creates a new match object to be used for matching.
        /// </summary>
        /// <returns>
        /// A match object ready to be used with the Match methods
        /// </returns>
        public Match CreateMatch()
        {
            return new Match(DataSet);
        }

        #region Private Methods

        #endregion

        #endregion
    }
}
