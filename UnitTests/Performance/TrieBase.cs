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
using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using FiftyOne.Foundation.Mobile.Detection.Entities;
using FiftyOne.Foundation.Mobile.Detection.Factories;
using FiftyOne.Foundation.Mobile.Detection;
using System.Collections.Generic;

namespace FiftyOne.UnitTests.Performance
{
    [TestClass]
    public abstract class TrieBase : IDisposable
    {
        protected TrieProvider _provider;

        /// <summary>
        /// Name of the data file to use for the tests.
        /// </summary>
        protected abstract string DataFile { get; }

        /// <summary>
        /// Time taken to initialise the data set for the tests.
        /// </summary>
        protected TimeSpan _testInitializeTime;

        protected virtual int MaxInitializeTime { get { return 500; } }

        protected virtual int GuidanceTime { get { return 1; } }
        
        protected virtual void InitializeTime()
        {
            Assert.IsTrue(_testInitializeTime.TotalMilliseconds < MaxInitializeTime,
                String.Format("Initialisation time greater than '{0}' ms", MaxInitializeTime));
            Console.WriteLine("{0:0.00}ms", _testInitializeTime.TotalMilliseconds);
        }

        protected virtual Utils.Results UserAgentsSingle(IEnumerable<string> userAgents)
        {
            return Utils.DetectLoopSingleThreaded(
                _provider,
                userAgents,
                Utils.TrieDoNothing,
                _provider);
        }

        protected virtual Utils.Results UserAgentsMulti(IEnumerable<string> userAgents)
        {
            return Utils.DetectLoopMultiThreaded(
                _provider,
                userAgents,
                Utils.TrieDoNothing,
                _provider);
        }

        protected virtual Utils.Results UserAgentsMultiAll(IEnumerable<string> userAgents)
        {
            var results = Utils.DetectLoopMultiThreaded(_provider, userAgents, Utils.RetrieveTriePropertyValues, _provider);
            Console.WriteLine("Values check sum: '{0}'", results.CheckSum);
            Assert.IsTrue(results.AverageTime.TotalMilliseconds < GuidanceTime,
                String.Format("Average time of '{0:0.000}' ms exceeded guidance time of '{1}' ms",
                    results.AverageTime.TotalMilliseconds,
                    GuidanceTime));
            return results;
        }

        protected virtual Utils.Results UserAgentsSingleAll(IEnumerable<string> userAgents)
        {
            var results = Utils.DetectLoopSingleThreaded(_provider, userAgents, Utils.RetrieveTriePropertyValues, _provider);
            Console.WriteLine("Values check sum: '{0}'", results.CheckSum);
            Assert.IsTrue(results.AverageTime.TotalMilliseconds < GuidanceTime,
                String.Format("Average time of '{0:0.000}' ms exceeded guidance time of '{1}' ms",
                    results.AverageTime.TotalMilliseconds,
                    GuidanceTime));
            return results;
        }

        protected void BadUserAgentsMulti()
        {
            UserAgentsMulti(UserAgentGenerator.GetBadUserAgents());
        }

        protected void BadUserAgentsSingle()
        {
            UserAgentsSingle(UserAgentGenerator.GetBadUserAgents());
        }

        protected void UniqueUserAgentsMulti()
        {
            UserAgentsMulti(UserAgentGenerator.GetUniqueUserAgents());
        }

        protected void UniqueUserAgentsSingle()
        {
            UserAgentsSingle(UserAgentGenerator.GetUniqueUserAgents());
        }

        protected void RandomUserAgentsMulti()
        {
            UserAgentsMulti(UserAgentGenerator.GetRandomUserAgents());
        }

        protected void RandomUserAgentsSingle()
        {
            UserAgentsSingle(UserAgentGenerator.GetRandomUserAgents());
        }

        protected void BadUserAgentsMultiAll()
        {
            UserAgentsMultiAll(UserAgentGenerator.GetBadUserAgents());
        }

        protected void BadUserAgentsSingleAll()
        {
            UserAgentsSingleAll(UserAgentGenerator.GetBadUserAgents());
        }

        protected void UniqueUserAgentsMultiAll()
        {
            UserAgentsMultiAll(UserAgentGenerator.GetUniqueUserAgents());
        }

        protected void UniqueUserAgentsSingleAll()
        {
            UserAgentsSingleAll(UserAgentGenerator.GetUniqueUserAgents());
        }

        protected void RandomUserAgentsMultiAll()
        {
            UserAgentsMultiAll(UserAgentGenerator.GetRandomUserAgents());
        }

        protected void RandomUserAgentsSingleAll()
        {
            UserAgentsSingleAll(UserAgentGenerator.GetRandomUserAgents());
        }

        [TestCleanup]
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_provider != null)
            {
                _provider.Dispose();
            }
        }
    }
}
