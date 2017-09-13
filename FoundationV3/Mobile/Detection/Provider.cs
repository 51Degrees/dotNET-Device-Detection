/* *********************************************************************
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited. 
 * Copyright © 2017 51Degrees Mobile Experts Limited, 5 Charlotte Close,
 * Caversham, Reading, Berkshire, United Kingdom RG4 7BY
 * 
 * This Source Code Form is the subject of the following patents and patent 
 * applications, owned by 51Degrees Mobile Experts Limited of 5 Charlotte
 * Close, Caversham, Reading, Berkshire, United Kingdom RG4 7BY: 
 * European Patent No. 2871816;
 * European Patent Application No. 17184134.9;
 * United States Patent Nos. 9,332,086 and 9,350,823; and
 * United States Patent Application No. 15/686,066.
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
using System.Collections.Specialized;
using System.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using FiftyOne.Foundation.Mobile.Detection.Caching;

namespace FiftyOne.Foundation.Mobile.Detection
{
    /// <summary>
    /// Exposes several match methods to be used for device detection.
    /// Provider requires a <see cref="DataSet"/>
    /// object connected to one of the 51Degrees 
    /// device data files in order to perform device detection. 
    /// Can be created like:
    /// <list>
    /// <item>For use with Stream factory:
    /// <para>
    /// <code>
    /// Provider p = new Provider(StreamFactory.Create("path_to_file"));
    /// </code>
    /// </para>
    /// </item>
    /// <item>For use with Memory factory:
    /// <para>
    /// <code>
    /// Provider p = new Provider(MemoryFactory.Create("path_to_file"));
    /// </code>
    /// </para>
    /// </item>
    /// </list>
    /// For explanation on the difference between stream and memory see:
    /// <a href="https://51degrees.com/support/documentation/pattern">
    /// how device detection works</a> "Modes of operation" section.
    /// <para>
    /// Match methods return a 
    /// <see cref="FiftyOne.Foundation.Mobile.Detection.Match"/>
    /// object that contains detection results 
    /// for a specific User-Agent string, collection of 
    /// HTTP headers of a device Id.
    /// Use it to retrieve detection results like: 
    /// <code>
    /// match.Properties["IsMobile"].Values;
    /// </code>
    /// </para>
    /// <para>
    /// You can access the underlying data set like: 
    /// <code>
    /// provider.dataset.publised
    /// </code>
    /// to retrieve various meta information like 
    /// the published date and the next update date as well as the list of various 
    /// entities like
    /// <see cref="FiftyOne.Foundation.Mobile.Detection.Entities.Profile"/>
    /// and 
    /// <see cref="FiftyOne.Foundation.Mobile.Detection.Entities.Property"/>.
    /// </para>
    /// <para>
    /// Provider used to perform a detection based on a User-Agent string. 
    /// </para>
    /// </summary>
    public class Provider : IDisposable
    {
        #region Static Fields

        /// <summary>
        /// Used to split device Id strings into their integer parts.
        /// </summary>
        private static readonly Regex _deviceIdRegex = new Regex(
            @"\d+", RegexOptions.Compiled);

        #endregion

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
        /// A cache for User-Agents.
        /// </summary>
        private readonly ILoadingCache<string, MatchResult> _userAgentCache = null;

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
        public Provider(DataSet dataSet) : this(dataSet, false, 0)
        { }

        /// <summary>
        /// Constructs a new provider using the data set.
        /// </summary>
        /// <param name="dataSet">Data set to use for device detection</param>
        /// <param name="cacheSize">Size of the cache used with the provider</param>
        public Provider(DataSet dataSet, int cacheSize)
            : this(dataSet, false, cacheSize)
        { }

        /// <summary>
        /// Constructs a new provided using the data set.
        /// </summary>
        /// <param name="dataSet">Data set to use for device detection</param>
        /// <param name="recordDetectionTime">True if the detection time should be recorded</param>
        /// <param name="cacheSize">Size of the cache used with the provider</param>
        public Provider(DataSet dataSet, bool recordDetectionTime, 
            int cacheSize)
            : this(dataSet, recordDetectionTime, cacheSize, new LruCacheBuilder())
        { }

        /// <summary>
        /// Constructs a new provided using the data set.
        /// </summary>
        /// <param name="dataSet">Data set to use for device detection</param>
        /// <param name="recordDetectionTime">True if the detection time should be recorded</param>
        /// <param name="cacheSize">the size of the cache to use</param>
        /// <param name="cacheBuilder">cache builder to use when building the cache</param>
        public Provider(DataSet dataSet, bool recordDetectionTime, 
            int cacheSize, ILoadingCacheBuilder cacheBuilder)
        {
            DataSet = dataSet;
            MethodCounts = new SortedList<MatchMethods, long>();
            MethodCounts.Add(MatchMethods.Closest, 0);
            MethodCounts.Add(MatchMethods.Nearest, 0);
            MethodCounts.Add(MatchMethods.Exact, 0);
            MethodCounts.Add(MatchMethods.Numeric, 0);
            MethodCounts.Add(MatchMethods.None, 0);
            RecordDetectionTime = recordDetectionTime;
            if (cacheSize > 0)
            {
                _userAgentCache = cacheBuilder.Build<string, MatchResult>(cacheSize);
            }
        }


        #endregion

        #region Destructor

        /// <summary>
        /// Ensures the User-Agent cache is disposed if one was created.
        /// </summary>
        ~Provider()
        {
            Dispose(false);
        }

        /// <summary>
        /// Disposes of the User-Agent cache if one was created.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Disposes of the User-Agent cache if one was created.
        /// </summary>
        /// <param name="disposing">
        /// True if the calling method is Dispose, false for the finaliser.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (_userAgentCache != null)
            {
                _userAgentCache.Dispose();
            }
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Properties

        /// <summary>
        /// The percentage of requests for User-Agents which were not already
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
        [Obsolete("Replacement LRU cache does not support switching.")]
        public long CacheSwitches
        {
            get
            {
                return 0;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns the result of a match based on the device Id returned from
        /// a previous match operation.
        /// </summary>
        /// <param name="deviceIdArray">Byte array representation of the device Id</param>
        /// <returns>
        /// The <see cref="FiftyOne.Foundation.Mobile.Detection.Match"/>
        /// object passed to the method updated with
        /// results for the device id.
        /// </returns>
        public Match MatchForDeviceId(byte[] deviceIdArray)
        {
            return MatchForDeviceId(deviceIdArray, CreateMatch());
        }

        /// <summary>
        /// Returns the result of a match based on the device Id returned from
        /// a previous match operation.
        /// </summary>
        /// <param name="deviceId">String representation of the device Id</param>
        /// <returns>
        /// The <see cref="FiftyOne.Foundation.Mobile.Detection.Match"/>
        /// object passed to the method updated with
        /// results for the device id.
        /// </returns>
        public Match MatchForDeviceId(string deviceId)
        {
            return MatchForDeviceId(deviceId, CreateMatch());
        }

        /// <summary>
        /// Returns the result of a match based on the device Id returned from
        /// a previous match operation.
        /// </summary>
        /// <param name="profileIds">List of profile ids as integers</param>
        /// <returns>
        /// The <see cref="FiftyOne.Foundation.Mobile.Detection.Match"/>
        /// object passed to the method updated with
        /// results for the device id.
        /// </returns>
        public Match MatchForDeviceId(IList<int> profileIds)
        {
            return MatchForDeviceId(profileIds, CreateMatch());
        }

        /// <summary>
        /// Returns the result of a match based on the device Id returned from
        /// a previous match operation.
        /// </summary>
        /// <param name="deviceIdArray">Byte array representation of the device Id</param>
        /// <param name="match">
        /// A <see cref="FiftyOne.Foundation.Mobile.Detection.Match"/>
        /// object created by a previous match, or via the 
        /// <see cref="CreateMatch"/> method.
        /// </param>
        /// <returns>
        /// The Match object passed to the method updated with
        /// results for the device id.
        /// </returns>
        public Match MatchForDeviceId(byte[] deviceIdArray, Match match)
        {
            var profileIds = new List<int>();
            for (int i = 0; i < deviceIdArray.Length; i += 4)
            {
                profileIds.Add(BitConverter.ToInt32(deviceIdArray, i));
            }
            return MatchForDeviceId(profileIds, match);
        }

        /// <summary>
        /// Returns the result of a match based on the device Id returned from
        /// a previous match operation.
        /// </summary>
        /// <param name="deviceId">String representation of the device Id</param>
        /// <param name="match">
        /// A <see cref="FiftyOne.Foundation.Mobile.Detection.Match"/>
        /// object created by a previous match, or via the 
        /// <see cref="CreateMatch"/> method.
        /// </param>
        /// <returns>
        /// The <see cref="FiftyOne.Foundation.Mobile.Detection.Match"/>
        /// object passed to the method updated with
        /// results for the device id.
        /// </returns>
        public Match MatchForDeviceId(string deviceId, Match match)
        {
            MatchCollection profileIdMatches = _deviceIdRegex.Matches(deviceId);
            var profileIds = new List<int>(profileIdMatches.Count);
            foreach(System.Text.RegularExpressions.Match profileIdMatch in profileIdMatches)
            {
                int profileId;
                if (profileIdMatch.Success &&
                    int.TryParse(profileIdMatch.Value, out profileId))
                {
                    profileIds.Add(profileId); 
                }
            }
            return MatchForDeviceId(profileIds, match);
        }

        /// <summary>
        /// Returns the result of a match based on the device Id returned from
        /// a previous match operation.
        /// </summary>
        /// <param name="profileIds">List of profile ids as integers</param>
        /// <param name="match">
        /// A <see cref="FiftyOne.Foundation.Mobile.Detection.Match"/>
        /// object created by a previous match, or via the 
        /// <see cref="CreateMatch"/> method.
        /// </param>
        /// <returns>
        /// The <see cref="FiftyOne.Foundation.Mobile.Detection.Match"/>
        /// object passed to the method updated with
        /// results for the device id.
        /// </returns>
        public Match MatchForDeviceId(IList<int> profileIds, Match match)
        {
            match.Reset();
            foreach(var profileId in profileIds)
            {
                var profile = DataSet.FindProfile(profileId);
                if (profile != null)
                {
                    match.State.ExplicitProfiles.Add(profile);
                }
            }
            return match;
        }

        /// <summary>
        /// For a given User-Agent returns a match containing 
        /// information about the capabilities of the device and 
        /// it's components.
        /// </summary>
        /// <param name="targetUserAgent">
        /// The User-Agent string to use as the target
        /// </param>
        /// <returns>
        /// A fresh <see cref="FiftyOne.Foundation.Mobile.Detection.Match"/>
        /// object populated with the results
        /// for the target User-Agent.
        /// </returns>
        public Match Match(string targetUserAgent)
        {
            return Match(targetUserAgent, CreateMatch());
        }

        /// <summary>
        /// Creates a new <see cref="FiftyOne.Foundation.Mobile.Detection.Match"/>
        /// object to be used for matching.
        /// </summary>
        /// <returns>
        /// A <see cref="FiftyOne.Foundation.Mobile.Detection.Match"/>
        /// object ready to be used with the Match methods
        /// </returns>
        public Match CreateMatch()
        {
            return new Match(this);
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
        /// A fresh <see cref="FiftyOne.Foundation.Mobile.Detection.Match"/>
        /// object populated with the results
        /// for the target User-Agent.
        /// </returns>
        public Match Match(NameValueCollection headers)
        {
            return Match(headers, CreateMatch());
        }

        /// <summary>
        /// Resets the <see cref="FiftyOne.Foundation.Mobile.Detection.Match"/>
        /// object provided and performs the detection
        /// using that object rather than creating a new match.
        /// </summary>
        /// <param name="targetUserAgent">
        /// The User-Agent string to use as the target
        /// </param>
        /// <param name="match">
        /// A <see cref="FiftyOne.Foundation.Mobile.Detection.Match"/>
        /// object created by a previous match, or via the 
        /// <see cref="CreateMatch"/> method.
        /// </param>
        /// <returns>
        /// The <see cref="FiftyOne.Foundation.Mobile.Detection.Match"/>
        /// object passed to the method updated with
        /// results for the target User-Agent
        /// </returns>
        public Match Match(string targetUserAgent, Match match)
        {
            match.Result = Match(targetUserAgent, match.State);
            return match;
        }

        /// <summary>
        /// Resets the <see cref="FiftyOne.Foundation.Mobile.Detection.Match"/>
        /// object provided and performs the detection
        /// using that object rather than creating a new match.
        /// </summary>
        /// <param name="headers">
        /// List of HTTP headers to use for the detection
        /// </param>
        /// <param name="match">
        /// A <see cref="FiftyOne.Foundation.Mobile.Detection.Match"/>
        /// object created by a previous match, or via the 
        /// <see cref="CreateMatch"/> method.
        /// </param>
        /// <returns>
        /// The <see cref="FiftyOne.Foundation.Mobile.Detection.Match"/>
        /// object passed to the method updated with
        /// results for the target User-Agent
        /// </returns>
        public Match Match(NameValueCollection headers, Match match)
        {
            match.Reset();

            if (headers == null || headers.Count == 0)
            {
                // Empty headers all default match result.
                Controller.MatchDefault(match.State);
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
                    var componentIndex = 0;
                    foreach(var component in DataSet.Components)
                    {
                        // Get the profile for this component.
                        var profile = GetMatchingHeaderProfile(match.State, matches, component);

                        // Add the profile found, or the default one if not found.
                        match.State.ExplicitProfiles.Add(profile == null ? component.DefaultProfile : profile);

                        // Move to the next array element.
                        componentIndex++;
                    }

                    // Reset any fields that relate to the profiles assigned
                    // to the match result or that can't contain a value when
                    // HTTP headers are used.
                    match.State.Signature = null;
                    match.State.TargetUserAgent = null;
                }

                // If the Cookie header is present then record this as it maybe
                // needed when a Property Value Override property is requested.
                match._cookie = headers["Cookie"];
            }
            return match;
        }

        #endregion

        #region Internal & Private Methods

        /// <summary>
        /// See if any of the headers can be used for this
        /// components profile. As soon as one matches then
        /// stop and don't look at any more. They are ordered
        /// in preferred sequence such that the first item is 
        /// the most preferred.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="matches"></param>
        /// <param name="component"></param>
        /// <returns></returns>
        private static Profile GetMatchingHeaderProfile(MatchState state, Dictionary<string, MatchState> matches, Component component)
        {
            foreach (var header in component.HttpHeaders)
            {
                MatchState headerMatchState;
                if (matches.TryGetValue(header, out headerMatchState) &&
                    headerMatchState.Signature != null)
                {
                    // Update the statistics about the matching process.
                    state.SignaturesCompared += headerMatchState.SignaturesCompared;
                    state.SignaturesRead += headerMatchState.SignaturesRead;
                    state.StringsRead += headerMatchState.StringsRead;
                    state.RootNodesEvaluated += headerMatchState.RootNodesEvaluated;
                    state.NodesEvaluated += headerMatchState.NodesEvaluated;
                    state.Elapsed += headerMatchState.Elapsed;
                    state.LowestScore += headerMatchState.LowestScore;

                    // If the header match used is worst than the current one
                    // then update the method used for the match returned.
                    if ((int)headerMatchState.Method > (int)state.Method)
                    {
                        state.Method = headerMatchState.Method;
                    }

                    // Return the profile for this component.
                    return headerMatchState.Signature.Profiles.FirstOrDefault(i =>
                        component.Equals(i.Component));
                }
            }
            return null;
        }

        /// <summary>
        /// For each of the important HTTP headers provides a mapping to a match result.
        /// </summary>
        /// <param name="match">
        /// A match object created by a previous match, or via the 
        /// <see cref="CreateMatch"/> method.
        /// </param>
        /// <param name="headers">The HTTP headers available for matching</param>
        /// <param name="importantHeaders">HTTP header names important to the match process</param>
        /// <returns>A map of HTTP headers and match instances containing results for them</returns>
        private Dictionary<string, MatchState> MatchForHeaders(Match match, NameValueCollection headers, IEnumerable<string> importantHeaders)
        {
            // Relates HTTP header names to match resutls.
            var matches = new Dictionary<string, MatchState>();

            // Iterates through the important header names.
            var iterator = importantHeaders.GetEnumerator();

            // Make the first match used the match passed
            // into the method. Subsequent matches will
            // use a new instance.
            while(iterator.MoveNext())
            {
                matches.Add(iterator.Current, new MatchState(match) { TargetUserAgent = headers[iterator.Current] });
            }

            // Using each of the match instances pass the 
            // value to the match method and set the results.
            // Done in parallel to improve performance if 
            // multi threading available.
            Parallel.ForEach(matches, m =>
            {
                Match(headers[m.Key], m.Value);
            });
            return matches;
        }
        
        /// <summary>
        /// Resets the match object provided and performs the detection
        /// using that object rather than creating a new match.
        /// </summary>
        /// <param name="targetUserAgent">
        /// The User-Agent string to use as the target
        /// </param>
        /// <param name="state">
        /// A match object created by a previous match, or via the 
        /// <see cref="CreateMatch"/> method.
        /// </param>
        /// <returns>
        /// The match object passed to the method updated with
        /// results for the target User-Agent
        /// </returns>
        private MatchResult Match(string targetUserAgent, MatchState state)
        {
            MatchResult result;

            if (targetUserAgent == null)
            {
                // Handle null User-Agents as empty strings.
                targetUserAgent = String.Empty;
            }

            if (_userAgentCache != null)
            {
                // Fetch the item using the cache.
                result = _userAgentCache[targetUserAgent, state];
            }
            else
            {
                // The cache does not exist so call the non caching method.
                MatchNoCache(targetUserAgent, state);
                result = new MatchResult(state);
            }

            return result;
        }

        /// <summary>
        /// Matches the User-Agent setting the state without using a cache.
        /// </summary>
        /// <param name="targetUserAgent"></param>
        /// <param name="state"></param>
        internal void MatchNoCache(string targetUserAgent, MatchState state)
        {
            long startTickCount = 0;

            state.Reset(targetUserAgent);

            if (RecordDetectionTime)
            {
                startTickCount = DateTime.UtcNow.Ticks;
            }

            Controller.Match(state);

            if (RecordDetectionTime)
            {
                state.Elapsed = DateTime.UtcNow.Ticks - startTickCount;
            }

            // Update the counts for the provider.
            Interlocked.Increment(ref _detectionCount);
            lock (MethodCounts)
            {
                MethodCounts[state.Method]++;
            }
        }
        
        #endregion
    }
}
