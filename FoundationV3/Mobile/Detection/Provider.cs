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
        /// Constructs a new provider using the embedded data set.
        /// </summary>
        public Provider()
            : this (GetEmbeddedDataSet())
        {
        }

        /// <summary>
        /// Constructs a new provider using the embedded data set.
        /// <param name="cacheServiceInternal">True to enable caching for the provider</param>
        /// </summary>
        public Provider(int cacheServiceInternal)
            : this(GetEmbeddedDataSet(), cacheServiceInternal)
        {
        }

        /// <summary>
        /// Returns the embedded data set.
        /// </summary>
        /// <returns></returns>
        private static DataSet GetEmbeddedDataSet()
        {
            using (Stream stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream(
                BinaryConstants.EmbeddedDataResourceName))
            {
                using (var reader = new Reader(new GZipStream(stream, CompressionMode.Decompress)))
                {
                    return MemoryFactory.Read(reader, false);
                }
            }
        }

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
        /// <param name="cacheServiceInternal">True to enable caching for the provider</param>
        public Provider(DataSet dataSet, int cacheServiceInternal)
            : this(dataSet, false, cacheServiceInternal)
        {
        }

        /// <summary>
        /// Constructs a new provided using the data set.
        /// </summary>
        /// <param name="dataSet">Data set to use for device detection</param>
        /// <param name="recordDetectionTime">True if the detection time should be recorded</param>
        /// <param name="cacheServiceInternal">True to enable caching for the provider</param>
        public Provider(DataSet dataSet, bool recordDetectionTime, int cacheServiceInternal)
        {
            DataSet = dataSet;
            MethodCounts = new SortedList<MatchMethods, long>();
            MethodCounts.Add(MatchMethods.Closest, 0);
            MethodCounts.Add(MatchMethods.Nearest, 0);
            MethodCounts.Add(MatchMethods.Exact, 0);
            MethodCounts.Add(MatchMethods.Numeric, 0);
            MethodCounts.Add(MatchMethods.None, 0);
            RecordDetectionTime = recordDetectionTime;
            _userAgentCache = cacheServiceInternal > 0 ? new Cache<string, MatchState>(cacheServiceInternal) : null;
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
            // Get the match for the main user agent.
            Match(headers["User-Agent"], match);

            // Get the user agent for the device if a secondary header is present.
            var deviceUserAgent = GetDeviceUserAgent(headers);
            if (deviceUserAgent != null)
            {
                var deviceMatch = Match(deviceUserAgent);
                if (deviceMatch != null)
                {
                    // Update the statistics about the matching process.
                    match._signaturesCompared += deviceMatch._signaturesCompared;
                    match._signaturesRead += deviceMatch._signaturesRead;
                    match._stringsRead += deviceMatch._stringsRead;
                    match._rootNodesEvaluated += deviceMatch._rootNodesEvaluated;
                    match._nodesEvaluated += deviceMatch._nodesEvaluated;
                    match._elapsed += deviceMatch._elapsed;

                    // Replace the Hardware and Software profiles with the ones from
                    // the device match.
                    for (int i = 0; i < match.Profiles.Length && i < deviceMatch.Profiles.Length; i++)
                    {
                        if (match.Profiles[i].Component.ComponentId <= 2 &&
                            match.Profiles[i].Component.ComponentId == deviceMatch.Profiles[i].Component.ComponentId)
                        {
                            // Swap over the profiles if they're the same component.
                            match.Profiles[i] = deviceMatch.Profiles[i];
                        }
                    }

                    // Remove the signature as a single one is not being returned.
                    match._signature = null;
                }
            }

            return match;
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
                if (_userAgentCache.TryGetValue(targetUserAgent, out state) == false)
                {
                    // The user agent has not been checked previously. Therefore perform
                    // the match and store the results in the cache.
                    MatchNoCache(targetUserAgent, match);

                    // Record the match state in the cache for next time.
                    state = new MatchState(match);
                    _userAgentCache.SetActive(targetUserAgent, state);
                }
                else
                {
                    // The state of a previous match exists so the match should
                    // be configured based on the results of the previous state.
                    match.SetState(state);
                }
                _userAgentCache.SetBackground(targetUserAgent, state);
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

        /// <summary>
        /// Used to check other header fields in case a device user agent is being used
        /// and returns the devices useragent string.
        /// </summary>
        /// <param name="headers">Collection of Http headers associated with the request.</param>
        /// <returns>The useragent string of the device.</returns>
        private static string GetDeviceUserAgent(NameValueCollection headers)
        {
            foreach (string current in Detection.Constants.DeviceUserAgentHeaders)
                if (headers[current] != null)
                    return headers[current];
            return null;
        }

        #endregion

        #endregion
    }
}
