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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FiftyOne.Foundation.Mobile.Detection.Factories;
using System.IO;
using FiftyOne.Foundation.Mobile.Detection;

namespace FiftyOne.UnitTests.Performance
{
    [TestClass]
    public abstract class FileTest : Base
    {
        [TestInitialize()]
        public void CreateDataSet()
        {
            var start = DateTime.UtcNow;
            _dataSet = StreamFactory.Create(Path.Combine(DataFile));
            _testInitializeTime = DateTime.UtcNow - start;
        }

        protected void InitializeTime() 
        {
            Assert.IsTrue(_testInitializeTime.TotalMilliseconds < 500);
            Console.WriteLine("{0:0.00}ms", _testInitializeTime.TotalMilliseconds);  
        }

        protected override Utils.Results BadUserAgentsMulti() 
        {
            var results = base.BadUserAgentsMulti();
            Assert.IsTrue(results.AverageTime.Milliseconds < 4, "Average Time");
            Assert.IsTrue(results.GetMethodPercentage(MatchMethods.Exact) < 0.2, "Exact Method");
            Asserts.AssertCacheMissesBad(_dataSet);
            return results;
        }

        protected override Utils.Results BadUserAgentsSingle() 
        {
            var results = base.BadUserAgentsSingle(); 
            Assert.IsTrue(results.AverageTime.Milliseconds < 10, "Average Time");
            Assert.IsTrue(results.GetMethodPercentage(MatchMethods.Exact) < 0.2, "Exact Method");
            Asserts.AssertCacheMissesBad(_dataSet);
            return results;
        }

        protected override Utils.Results DuplicatedUserAgentsMulti() 
        {
            var results = base.DuplicatedUserAgentsMulti();
            Assert.IsTrue(results.AverageTime.Milliseconds < 1, "Average Time");
            Assert.IsTrue(results.GetMethodPercentage(MatchMethods.Exact) > 0.95, "Exact Method");
            Asserts.AssertCacheMissesGood(_dataSet);
            return results;
        }

        protected override Utils.Results DuplicatedUserAgentsSingle() 
        {
            var results = base.DuplicatedUserAgentsSingle(); 
            Assert.IsTrue(results.AverageTime.Milliseconds < 1, "Average Time");
            Assert.IsTrue(results.GetMethodPercentage(MatchMethods.Exact) > 0.95, "Exact Method");
            Asserts.AssertCacheMissesGood(_dataSet);
            return results;
        }

        protected override Utils.Results UniqueUserAgentsMulti() 
        {
            var results = base.UniqueUserAgentsMulti();
            Assert.IsTrue(results.AverageTime.Milliseconds < 1, "Average Time");
            Assert.IsTrue(results.GetMethodPercentage(MatchMethods.Exact) > 0.95, "Exact Method");
            Asserts.AssertCacheMissesGood(_dataSet);
            return results;
        }

        protected override Utils.Results UniqueUserAgentsSingle() 
        {
            var results = base.UniqueUserAgentsSingle(); 
            Assert.IsTrue(results.AverageTime.Milliseconds < 1, "Average Time");
            Assert.IsTrue(results.GetMethodPercentage(MatchMethods.Exact) > 0.95, "Exact Method");
            Asserts.AssertCacheMissesGood(_dataSet);
            return results;
        }

        protected override Utils.Results RandomUserAgentsMulti()
        {
            var results = base.RandomUserAgentsMulti();
            Assert.IsTrue(results.AverageTime.Milliseconds < 3, "Average Time");
            return results;
        }

        protected override Utils.Results RandomUserAgentsSingle()
        {
            var results = base.RandomUserAgentsSingle();
            Assert.IsTrue(results.AverageTime.Milliseconds < 6, "Average Time");
            return results;
        }
    }
}
