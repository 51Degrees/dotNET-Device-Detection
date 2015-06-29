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
using FiftyOne.Foundation.Mobile.Detection;
using FiftyOne.Foundation.Mobile.Detection.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FiftyOne.UnitTests
{
    public static class Utils
    {
        public class Results
        {
            private static readonly Dictionary<MatchMethods, int> _matchMethods;

            static Results()
            {
                _matchMethods = new Dictionary<MatchMethods, int>();
                _matchMethods.Add(MatchMethods.Closest, 0);
                _matchMethods.Add(MatchMethods.Exact, 0);
                _matchMethods.Add(MatchMethods.Nearest, 0);
                _matchMethods.Add(MatchMethods.None, 0);
                _matchMethods.Add(MatchMethods.Numeric, 0);
            }

            public readonly Dictionary<MatchMethods, int> Methods;

            public readonly DateTime StartTime;

            public int Count
            {
                get { return Methods.Sum(i => i.Value); }
            }

            public TimeSpan ElapsedTime
            {
                get { return DateTime.UtcNow - StartTime; }
            }

            public TimeSpan AverageTime
            {
                get { return new TimeSpan(ElapsedTime.Ticks / Count); }
            }

            public Results()
            {
                Methods = new Dictionary<MatchMethods, int>(_matchMethods);
                StartTime = DateTime.UtcNow;
            }

            public double GetMethodPercentage(MatchMethods method)
            {
                return (double)Methods[method] / (double)Count;
            }
        }

        

        /// <summary>
        /// Creates a data set that is connected to the file provided.
        /// </summary>
        /// <param name="filePath">Path to the 51Degrees data file</param>
        /// <returns>A fresh data set connected to the file</returns>
        public delegate DataSet CreateFileDataSet(string filePath);

        /// <summary>
        /// Creates a data set connected to the byte array in memory
        /// provided.
        /// </summary>
        /// <param name="data">Byte array containing the data source</param>
        /// <returns>A fresh data set connected to the byte array</returns>
        public delegate DataSet CreateMemoryDataSet(byte[] data);

        /// <summary>
        /// Passed a match to perform what ever tests are required.
        /// </summary>
        /// <param name="match">Match of a detection.</param>
        public delegate void ProcessMatch(Match match, object state);

        /// <summary>
        /// In a single thread loops through the useragents in the file
        /// provided perform a match with the data set provided passing
        /// control back to the method provided for further processing.
        /// </summary>
        /// <param name="dataSet"></param>
        /// <param name="userAgents"></param>
        /// <param name="method"></param>
        /// <returns>Counts for each of the methods used</returns>
        internal static Results DetectLoopSingleThreaded(DataSet dataSet, IEnumerable<string> userAgents, ProcessMatch method, object state)
        {
            dataSet.ResetCache();
            var provider = new Provider(dataSet);
            var match = provider.CreateMatch();
            var results = new Results();
            foreach (var line in userAgents)
            {
                provider.Match(line.Trim(), match);
                method(match, state);
                results.Methods[match.Method]++;
            }
            ReportMethods(results.Methods);
            ReportTime(results);
            return results;
        }

        /// <summary>
        /// Using multiple threads loops through the useragents in the file
        /// provided perform a match with the data set provided passing
        /// control back to the method provided for further processing.
        /// </summary>
        /// <param name="dataSet"></param>
        /// <param name="userAgents"></param>
        /// <param name="method"></param>
        /// <returns>Counts for each of the methods used</returns>
        internal static Results DetectLoopMultiThreaded(DataSet dataSet, IEnumerable<string> userAgents, ProcessMatch method, object state)
        {
            dataSet.ResetCache();
            var provider = new Provider(dataSet);
            var results = new Results();
            Parallel.ForEach(userAgents, line =>
            {
                var match = provider.Match(line.Trim());
                method(match, state);
                lock (results.Methods)
                {
                    results.Methods[match.Method] = results.Methods[match.Method] + 1;
                }
            });
            ReportMethods(results.Methods);
            ReportTime(results);
            return results;
        }

        public static void ReportMethods(Dictionary<MatchMethods, int> methods)
        {
            var total = methods.Sum(i => i.Value);
            foreach(var method in methods)
            {
                Console.WriteLine("Method '{0}' used '{1:P2}'",
                    method.Key,
                    (double)method.Value / (double)total);
            }
        }

        public static void ReportCache(DataSet dataSet)
        {
            Console.WriteLine("Node cache switches '{0}' with '{1:P2} misses",
                dataSet.NodeCacheSwitches,
                dataSet.PercentageNodeCacheMisses);
            Console.WriteLine("Profiles cache switches '{0}' with '{1:P2} misses",
                dataSet.ProfilesCacheSwitches,
                dataSet.PercentageProfilesCacheMisses);
            Console.WriteLine("Ranked Signatures cache switches '{0}' with '{1:P2} misses",
                dataSet.RankedSignatureCacheSwitches,
                dataSet.PercentageRankedSignatureCacheMisses);
            Console.WriteLine("Signatures cache switches '{0}' with '{1:P2} misses",
                dataSet.SignatureCacheSwitches,
                dataSet.PercentageSignatureCacheMisses);
            Console.WriteLine("Strings cache switches '{0}' with '{1:P2} misses",
                dataSet.StringsCacheSwitches,
                dataSet.PercentageStringsCacheMisses);
            Console.WriteLine("Values cache switches '{0}' with '{1:P2} misses",
                dataSet.ValuesCacheSwitches,
                dataSet.PercentageValuesCacheMisses);
        }

        internal static void ReportTime(Results results)
        {
            Console.WriteLine("Total of '{0:0.00}'s for '{1}' tests.",
                results.ElapsedTime.TotalSeconds,
                results.Count);
            Console.WriteLine("Average '{0:0.000}'ms per test.",
                results.ElapsedTime.TotalMilliseconds / results.Count);
        }

        public static void DoNothing(Match match, object state)
        {
            // Do nothing.
        }
    }
}