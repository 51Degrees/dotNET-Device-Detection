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
using System.Collections.Generic;
using FiftyOne.Foundation.Mobile.Detection.Entities;

namespace FiftyOne.UnitTests.Performance
{
    [TestClass]
    public abstract class Memory : Base
    {
        [TestInitialize()]
        public void CreateDataSet()
        {
            var start = DateTime.UtcNow;
            Utils.CheckFileExists(DataFile);
            _dataSet = MemoryFactory.Create(DataFile);
            _testInitializeTime = DateTime.UtcNow - start;
        }

        protected Utils.Results BadUserAgentsMulti(IEnumerable<Property> properties, int maxDetectionTime)
        {
            var results = base.UserAgentsMulti(
                UserAgentGenerator.GetBadUserAgents(), properties, maxDetectionTime);
            Assert.IsTrue(results.GetMethodPercentage(MatchMethods.Exact) < 0.2, "Exact Method");
            return results;
        }

        protected Utils.Results BadUserAgentsSingle(IEnumerable<Property> properties, int maxDetectionTime)
        {
            var results = base.UserAgentsSingle(
                UserAgentGenerator.GetBadUserAgents(), null, maxDetectionTime);
            Assert.IsTrue(results.GetMethodPercentage(MatchMethods.Exact) < 0.2, "Exact Method");
            return results;
        }

        protected Utils.Results RandomUserAgentsMulti(IEnumerable<Property> properties, int maxDetectionTime)
        {
            var results = base.UserAgentsMulti(
                UserAgentGenerator.GetRandomUserAgents(), null, maxDetectionTime);
            Assert.IsTrue(results.GetMethodPercentage(MatchMethods.Exact) > 0.95, "Exact Method");
            return results;
        }

        protected Utils.Results RandomUserAgentsSingle(IEnumerable<Property> properties, int maxDetectionTime)
        {
            var results = base.UserAgentsSingle(
                UserAgentGenerator.GetRandomUserAgents(), null, maxDetectionTime);
            Assert.IsTrue(results.GetMethodPercentage(MatchMethods.Exact) > 0.95, "Exact Method");
            return results;
        }

        protected Utils.Results UniqueUserAgentsMulti(IEnumerable<Property> properties, int maxDetectionTime)
        {
            var results = base.UserAgentsMulti(
                UserAgentGenerator.GetUniqueUserAgents(), null, maxDetectionTime);
            Assert.IsTrue(results.GetMethodPercentage(MatchMethods.Exact) > 0.95, "Exact Method");
            return results;
        }

        protected Utils.Results UniqueUserAgentsSingle(IEnumerable<Property> properties, int maxDetectionTime)
        {
            var results = base.UserAgentsSingle(
                UserAgentGenerator.GetUniqueUserAgents(), properties, maxDetectionTime);
            Assert.IsTrue(results.GetMethodPercentage(MatchMethods.Exact) > 0.95, "Exact Method");
            return results;
        }
    }
}
