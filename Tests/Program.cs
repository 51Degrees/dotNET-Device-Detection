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
using FiftyOne.Foundation.Mobile.Detection.Factories;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FiftyOne.Foundation.Tests
{
    /// <summary>
    /// Tests the memory and stream factories for performance, memory usage
    /// and identical results.
    /// </summary>
    class Program
    {
        /// <summary>
        /// Delegate for the method used to create the dataset.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private delegate DataSet CreateDataSet(string filePath);
        
        /// <summary>
        /// 1st Arguement is the data file of user agents to process
        /// 2nd Arguement is the directory containing 51Degrees .dat and .trie
        /// files for testing.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            var directory = new DirectoryInfo(args.Length > 0 ? args[0] : "../../data");
            var userAgentFile = args.Length > 1 ? args[1] : "../../data/20000 User Agents.csv";
            
            foreach (var file in directory.GetFiles(
                "*.dat", SearchOption.TopDirectoryOnly))
            {
                Test(file, userAgentFile, StreamFactory.Create);
                Test(file, userAgentFile, MemoryFactory.Create);
            }

            foreach (var file in directory.GetFiles(
                "*.trie", SearchOption.TopDirectoryOnly))
            {
                TestTrie(file, userAgentFile);
            }

            Console.ReadKey();
        }

        private static void TestTrie(FileInfo dataFile, string userAgentsFile)
        {
            DateTime startTime;
            int counter = 0;
            long startMemory = GC.GetTotalMemory(true);

            Console.WriteLine(new String('*', 80));

            startTime = DateTime.UtcNow;

            // Create the dataset and output the creation time and method.
            using (var provider = TrieFactory.Create(dataFile.FullName))
            {
                Console.WriteLine(
                    "Took {0:0} ms to create trie dataset'",
                    (DateTime.UtcNow - startTime).TotalMilliseconds);

                long hashCode = 0;
                long memory = 0;
                var memorySamples = 0;

                // Detect each line in the file.
                foreach(var line in File.ReadLines(userAgentsFile))
                {
                    // Get the device and one property value.
                    var deviceIndex = provider.GetDeviceIndex(line.Trim());
                    var value = provider.GetPropertyValue(deviceIndex, "IsMobile");
                    hashCode += value.GetHashCode();

                    // Update the counters.
                    counter++;

                    // Record memory usage every 1000 detections.
                    if (counter % 1000 == 0)
                    {
                        memorySamples++;
                        memory += GC.GetTotalMemory(false);
                    }
                }

                // Output headline results.
                Console.WriteLine();
                var completeTime = (DateTime.UtcNow - startTime);
                Console.WriteLine("Total of '{0}' detections in '{1:0.00} seconds",
                    counter,
                    completeTime.TotalSeconds);
                Console.WriteLine("Average detection time '{0:0.00}' ms",
                    completeTime.TotalMilliseconds / counter);

                // Average memory used.
                Console.WriteLine();
                Console.WriteLine("Average memory used '{0}' MBs",
                    ((memory / memorySamples) - startMemory) / (1024 * 1024));
            }
        }


        /// <summary>
        /// Tests the provided data file and factory produced data set using the 
        /// file of user agents provided.
        /// </summary>
        /// <param name="dataFile">The file containing the data set</param>
        /// <param name="userAgents">The file containing the user agents</param>
        /// <param name="factory">Method used to create the dataset</param>
        static void Test(FileInfo dataFile, string userAgentsFile, CreateDataSet factory)
        {
            DateTime startTime;
            int counter = 0;
            var methods = new SortedList<MatchMethods, int>();
            methods.Add(MatchMethods.Closest, 0);
            methods.Add(MatchMethods.Exact, 0);
            methods.Add(MatchMethods.Nearest, 0);
            methods.Add(MatchMethods.None, 0);
            methods.Add(MatchMethods.Numeric, 0);
            long startMemory = GC.GetTotalMemory(true);

            Console.WriteLine(new String('*', 80));

            Console.WriteLine("Testing data file '{0}' with factory '{1}'",
                dataFile.Name,
                ((MethodInfo)factory.Method).ReflectedType.Name);

            startTime = DateTime.UtcNow;

            // Create the dataset and output the creation time and method.
            using (var dataSet = factory(dataFile.FullName))
            {
                Console.WriteLine(
                    "Took {0:0} ms to create data set.",
                    (DateTime.UtcNow - startTime).TotalMilliseconds);

                // Get ready to perform detections on the user agents file.
                var provider = new Provider(dataSet);
                long profiles = 0;
                long memory = 0;
                var memorySamples = 0;
                long hashCode = 0;

                // Detect each line in the file.
                startTime = DateTime.UtcNow;
                foreach(var line in File.ReadLines(userAgentsFile))
                {
                    var match = provider.Match(line.Trim());

                    // Get all the values using their hashcodes to change the
                    // running total of all hashcodes.
                    profiles += match.Profiles.SelectMany(i => i.Values).Count();
                    foreach (var property in dataSet.Properties)
                    {
                        var value = match[property.Name];
                        if (value != null)
                        {
                            hashCode += value.ToString().GetHashCode();
                        }
                    }

                    // Update the counters.
                    counter++;
                    methods[match.Method]++;

                    // Record memory usage every 1000 detections.
                    if (counter % 2000 == 0)
                    {
                        memorySamples++;
                        memory += GC.GetTotalMemory(false);
                    }
                }

                // Output headline results.
                Console.WriteLine();
                var completeTime = DateTime.UtcNow - startTime;
                Console.WriteLine("Total of '{0}' detections in '{1:0.00} seconds", 
                    counter, 
                    completeTime.TotalSeconds);
                Console.WriteLine("Average detection time '{0:0.00}' ms",
                    completeTime.TotalMilliseconds / counter);

                // Average memory used.
                Console.WriteLine();
                Console.WriteLine("Average memory used '{0}' MBs",
                    ((memory / memorySamples) - startMemory) / (1024 * 1024));

                // Output the different methods and how often each
                // was used for the results.
                Console.WriteLine();
                foreach(var method in methods) 
                {
                    Console.WriteLine("Method '{0}' used '{1:P2}' of detections", 
                        method.Key,
                        (double)method.Value / (double)counter);
                }

                // Output the totals for profiles and hashcodes. Used to reconcile
                // results between the different factories.
                Console.WriteLine();
                Console.WriteLine("Total '{0}' profiles", profiles);
                Console.WriteLine("Hashcode '{0}' for all detections", hashCode); 

                // Output the cache stats.
                Console.WriteLine();
                Console.WriteLine("Signature switches '{0}', misses '{1:P2}'", dataSet.SignatureCacheSwitches, dataSet.PercentageSignatureCacheMisses);
                Console.WriteLine("Nodes switches '{0}', misses '{1:P2}'", dataSet.NodeCacheSwitches, dataSet.PercentageNodeCacheMisses);
                Console.WriteLine("Values switches '{0}', misses '{1:P2}'", dataSet.ValuesCacheSwitches, dataSet.PercentageValuesCacheMisses);
                Console.WriteLine("Ranked signatures switches '{0}', misses '{1:P2}'", dataSet.RankedSignatureCacheSwitches, dataSet.PercentageRankedSignatureCacheMisses);
                Console.WriteLine("Profiles switches '{0}', misses '{1:P2}'", dataSet.ProfilesCacheSwitches, dataSet.PercentageProfilesCacheMisses);
                Console.WriteLine("Strings switches '{0}', misses '{1:P2}'", dataSet.StringsCacheSwitches, dataSet.PercentageStringsCacheMisses);
                Console.WriteLine("UserAgent switches '{0}', misses '{1:P2}'", provider.CacheSwitches, provider.PercentageCacheMisses);
            }

            // Try and reclaim memory no longer used.
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.WaitForFullGCComplete();

            Console.WriteLine("");
        }
    }
}
