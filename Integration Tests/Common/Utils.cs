/* *********************************************************************
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited. 
 * Copyright © 2017 51Degrees Mobile Experts Limited, 5 Charlotte Close,
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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FiftyOne.Tests.Integration
{
    public static class Utils
    {
        #region Classes

        /// <summary>
        /// Used to monitor memory allocation for the test process.
        /// </summary>
        public class MemoryMonitor
        {
            /// <summary>
            /// Number of bytes in 1 megabyte.
            /// </summary>
            internal const double MEGABYTE = (double)(1024 * 1024);

            /// <summary>
            /// The memory allocated when the monitor was started.
            /// </summary>
            public long StartMemory { get; private set; }

            /// <summary>
            /// The total amount of memory summed across all samples.
            /// </summary>
            private long _totalMemory = 0;

            /// <summary>
            /// The number of memory samples that have been taken.
            /// </summary>
            private int _memorySamples = 0;

            /// <summary>
            /// Constructs a new instance of <see cref="MemoryMonitor"/>
            /// </summary>
            internal MemoryMonitor()
            {
                Reset();
            }

            /// <summary>
            /// Forces garbage collection and then returns the private memory
            /// allocation for the current process.
            /// </summary>
            /// <returns></returns>
            private static long GetCurrentMemory()
            {
                GC.Collect(int.MaxValue, GCCollectionMode.Forced, true);
                GC.WaitForPendingFinalizers();
                return GC.GetTotalMemory(true);
            }

            /// <summary>
            /// Captures the current memory state.
            /// </summary>
            internal void CaptureSample()
            {
                Interlocked.Increment(ref _memorySamples);
                Interlocked.Add(ref _totalMemory, GetCurrentMemory());
            }

            /// <summary>
            /// The average memory used in mega bytes.
            /// </summary>
            internal double AverageMemoryUsed
            {
                get 
                { 
                    return ((_totalMemory / _memorySamples) - StartMemory) /
                        MEGABYTE; 
                }
            }

            /// <summary>
            /// Resets the memory counters.
            /// </summary>
            internal void Reset()
            {
                _totalMemory = 0;
                _memorySamples = 0;
                StartMemory = GetCurrentMemory();
                Console.WriteLine(
                    "Start Memory Used: '{0:0.0}MB'", 
                    StartMemory / MEGABYTE);
            }
        }

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

            public int Count = 0;

            public long CheckSum = 0;

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

        #endregion

        #region Test Delegates

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
        /// <param name="results">The results data for the loop.</param>
        /// <param name="state">State used by the method.</param>
        public delegate void ProcessMatch(Results results, Match match, object state);

        /// <summary>
        /// Passed a device index and performs tests on the result.
        /// </summary>
        /// <param name="results"></param>
        /// <param name="deviceIndex"></param>
        /// <param name="state"></param>
        public delegate void ProcessTrie(Results results, int deviceIndex, object state);

        #endregion

        #region Shared Methods

        public static string GetDataFile(string fileName)
        {
            var dataFile = GetDataFile(
                fileName, 
                new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory));
            CheckFileExists(dataFile == null ? fileName : dataFile);
            return dataFile;
        }

        public static string GetDataFile(string fileName, DirectoryInfo directory)
        {
            var dataFile = directory.GetFiles(fileName).FirstOrDefault();
            if (dataFile != null)
            {
                return dataFile.FullName;
            }

            var dataDirectory = Constants.DATA_DIRECTORIES.Select(i => 
                new DirectoryInfo(Path.Combine(directory.FullName, i))).FirstOrDefault(i => 
                    i.Exists);
            if (dataDirectory != null)
            {
                return GetDataFile(fileName, dataDirectory);
            }
            
            if (directory.Parent != null &&
                Constants.DATA_DIRECTORIES.Contains(directory.Name) == false)
            {
                return GetDataFile(fileName, directory.Parent);
            }

            return null;
        }

        /// <summary>
        /// In a single thread loops through the useragents in the file
        /// provided perform a match with the data set provided passing
        /// control back to the method provided for further processing.
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="userAgents"></param>
        /// <param name="method"></param>
        /// <returns>Counts for each of the methods used</returns>
        internal static Results DetectLoopSingleThreaded(Provider provider, IEnumerable<string> userAgents, ProcessMatch method, object state)
        {
            provider.DataSet.ResetCache();
            var match = provider.CreateMatch();
            var results = new Results();
            foreach (var line in userAgents)
            {
                provider.Match(line.Trim(), match);
                method(results, match, state);
                results.Count++;
                results.Methods[match.Method]++;
            }
            AssertPool(provider);
            ReportMethods(results.Methods);
            ReportProvider(provider);
            ReportTime(results);
            return results;
        }

        /// <summary>
        /// In a single thread loops through the useragents in the file
        /// provided perform a match with the data set provided passing
        /// control back to the method provided for further processing.
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="userAgents"></param>
        /// <param name="method"></param>
        /// <returns>Counts for each of the methods used</returns>
        internal static Results DetectLoopSingleThreaded(TrieProvider provider, IEnumerable<string> userAgents, ProcessTrie method, object state)
        {
            var results = new Results();
            foreach (var line in userAgents)
            {
                method(results, provider.GetDeviceIndex(line.Trim()), state);
                results.Count++;
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
        /// <param name="provider"></param>
        /// <param name="userAgents"></param>
        /// <param name="method"></param>
        /// <returns>Counts for each of the methods used</returns>
        internal static Results DetectLoopMultiThreaded(Provider provider, IEnumerable<string> userAgents, ProcessMatch method, object state)
        {
            provider.DataSet.ResetCache();
            var results = new Results();
            Parallel.ForEach(userAgents, line =>
            {
                var match = provider.Match(line.Trim());
                method(results, match, state);
                Interlocked.Increment(ref results.Count);
                lock (results.Methods)
                {
                    results.Methods[match.Method] = results.Methods[match.Method] + 1;
                }
            });
            AssertPool(provider);
            ReportMethods(results.Methods);
            ReportProvider(provider);
            ReportTime(results);
            return results;
        }

        /// <summary>
        /// Using multiple threads loops through the useragents in the file
        /// provided perform a match with the data set provided passing
        /// control back to the method provided for further processing.
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="userAgents"></param>
        /// <param name="method"></param>
        /// <returns>Counts for each of the methods used</returns>
        internal static Results DetectLoopMultiThreaded(TrieProvider provider, IEnumerable<string> userAgents, ProcessTrie method, object state)
        {
            var results = new Results();
            Parallel.ForEach(userAgents, line =>
            {
                method(results, provider.GetDeviceIndex(line.Trim()), state);
                Interlocked.Increment(ref results.Count);
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

        public static void ReportPool(FiftyOne.Foundation.Mobile.Detection.Entities.Stream.DataSet dataSet)
        {
            Console.WriteLine("Readers in queue '{0}'", dataSet.ReadersQueued);
            Console.WriteLine("Readers created '{0}'", dataSet.ReadersCreated);
        }

        public static void ReportCache(DataSet dataSet)
        {
            Console.WriteLine("Node cache misses '{0:P2}'",
                dataSet.PercentageNodeCacheMisses);
            Console.WriteLine("Profiles cache misses '{0:P2}'",
                dataSet.PercentageProfilesCacheMisses);
            Console.WriteLine("Signatures cache misses '{0:P2}'",
                dataSet.PercentageSignatureCacheMisses);
            Console.WriteLine("Strings cache misses '{0:P2}'",
                dataSet.PercentageStringsCacheMisses);
            Console.WriteLine("Values cache misses '{0:P2}'",
                dataSet.PercentageValuesCacheMisses);
        }

        internal static void ReportTime(Results results)
        {
            Console.WriteLine("Total of '{0:0.00}'s for '{1}' tests.",
                results.ElapsedTime.TotalSeconds,
                results.Count);
            Console.WriteLine("Average time per test '{0:0.000}'.",
                results.ElapsedTime.TotalMilliseconds / results.Count);
        }

        internal static void AssertPool(Provider provider)
        {
            if (provider.DataSet is FiftyOne.Foundation.Mobile.Detection.Entities.Stream.DataSet)
            {
                var dataSet = (FiftyOne.Foundation.Mobile.Detection.Entities.Stream.DataSet)provider.DataSet;
                Assert.IsTrue(dataSet.ReadersCreated == dataSet.ReadersQueued,
                    String.Format(
                        "DataSet pooled readers mismatched. '{0}' created and '{1}' queued.",
                        dataSet.ReadersCreated,
                        dataSet.ReadersQueued));
            }
        }

        internal static void ReportProvider(Provider provider)
        {
            Console.WriteLine("User-Agent cache misses '{0:P2}'.",
                provider.PercentageCacheMisses);
            if (provider.DataSet is FiftyOne.Foundation.Mobile.Detection.Entities.Stream.DataSet)
            {
                ReportCache(provider.DataSet);
                ReportPool((FiftyOne.Foundation.Mobile.Detection.Entities.Stream.DataSet)provider.DataSet);
            }
        }

        /// <summary>
        /// Gets all the property values for the match and adds their
        /// summed hashcodes to the overall check sum for the test.
        /// </summary>
        /// <param name="results"></param>
        /// <param name="match"></param>
        /// <param name="state"></param>
        public static void RetrievePropertyValues(Results results, Match match, object state)
        {
            if (state != null)
            {
                var checkSum = 0;
                foreach (var property in (IEnumerable<Property>)state)
                {
                    checkSum += match[property].ToString().GetHashCode();
                }
                Interlocked.Add(ref results.CheckSum, checkSum);
            }
        }

        public static void MonitorMemory(Results results, Match match, object state)
        {
            MonitorMemory(results, state);
        }

        public static void MonitorTrieMemory(Results results, int deviceIndex, object state)
        {
            MonitorMemory(results, state);
        }

        private static void MonitorMemory(Results results, object state)
        {
            if (results.Count % 1000 == 0)
            {
                lock (state)
                {
                    if (results.Count % 1000 == 0)
                    {
                        ((MemoryMonitor)state).CaptureSample();
                    }
                }
            }
        }

        public static void TrieDoNothing(Results results, int deviceIndex, object state)
        {
            // Do nothing.
        }

        public static void RetrieveTriePropertyValues(Results results, int deviceIndex, object state)
        {
            if (state != null)
            {
                var checkSum = 0;
                foreach (var property in ((TrieProvider)state).PropertyNames)
                {
                    checkSum += ((TrieProvider)state).GetPropertyValue(deviceIndex, property).GetHashCode();
                }
                Interlocked.Add(ref results.CheckSum, checkSum);
            }
        }

        internal static void CheckFileExists(string dataFile)
        {
            if (dataFile == null || File.Exists(dataFile) == false)
            {
                Assert.Inconclusive(
                    "Data file '{0}' could not be found. " +
                    "See https://51degrees.com/compare-data-options to obtain the data files needed to complete this test.",
                    dataFile);
            }
        }

        public static bool CompareDataSets(DataSet dataset1, DataSet dataset2)
        {
            bool result = true;

            result = result && CompareLists(dataset1.Values, dataset2.Values);
            result = result && CompareLists(dataset1.Profiles, dataset2.Profiles);
            result = result && CompareLists(dataset1.Signatures, dataset2.Signatures);
            result = result && CompareLists(dataset1.Strings, dataset2.Strings);
            result = result && CompareLists(dataset1.Nodes, dataset2.Nodes);

            return result;
        }

        public static bool CompareLists<T>(IReadonlyList<T> list1, IReadonlyList<T> list2)
        {
            bool result = true;

            var enumerator1 = list1.GetEnumerator();
            var enumerator2 = list2.GetEnumerator();

            while(enumerator1.MoveNext() && result)
            {
                enumerator2.MoveNext();
                result = enumerator1.Current.ToString()
                    .Equals(enumerator2.Current.ToString());
            }

            return result;
        }


        #endregion
    }
}