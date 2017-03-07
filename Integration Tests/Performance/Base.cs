/* *********************************************************************
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited. 
 * Copyright © 2015 51Degrees Mobile Experts Limited, 5 Charlotte Close,
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
using FiftyOne.Foundation.Mobile.Detection.Entities.Stream;
using FiftyOne.Foundation.Mobile.Detection.Caching;

namespace FiftyOne.Tests.Integration.Performance
{
    [TestClass]
    public abstract class Base : IDisposable
    {
        /// <summary>
        /// The data set to be used for the tests.
        /// </summary>
        protected FiftyOne.Foundation.Mobile.Detection.Entities.DataSet _dataSet;

        /// <summary>
        /// Name of the data file to use for the tests.
        /// </summary>
        protected abstract string DataFile { get; }

        /// <summary>
        /// Time taken to initialise the data set for the tests.
        /// </summary>
        protected TimeSpan _testInitializeTime;

        protected abstract int MaxInitializeTime { get; }

        protected virtual void InitializeTime()
        {
            Assert.IsTrue(_testInitializeTime.TotalMilliseconds < MaxInitializeTime,
                String.Format("Initialisation time greater than '{0}' ms", MaxInitializeTime));
            Console.WriteLine("{0:0.00}ms", _testInitializeTime.TotalMilliseconds);
        }

        protected virtual Utils.Results UserAgentsSingle(IEnumerable<string> userAgents, Utils.ProcessMatch method, object state)
        {
            Console.WriteLine("Method: {0}", method.Method.Name);
            return Utils.DetectLoopSingleThreaded(
                new Provider(_dataSet),
                userAgents,
                method,
                state);
        }

        protected virtual Utils.Results UserAgentsMulti(IEnumerable<string> userAgents, Utils.ProcessMatch method, object state)
        {
            Console.WriteLine(String.Empty); 
            Console.WriteLine("Method: {0}", method.Method.Name);
            return Utils.DetectLoopMultiThreaded(
                new Provider(_dataSet),
                userAgents,
                method,
                state);
        }

        protected virtual Utils.Results UserAgentsMulti(IEnumerable<string> userAgents, IEnumerable<Property> properties, int guidanceTime)
        {
            var results = UserAgentsMulti(userAgents, Utils.RetrievePropertyValues, properties);
            Console.WriteLine("Values check sum: '{0}'", results.CheckSum);
            Assert.IsTrue(results.AverageTime.TotalMilliseconds < guidanceTime,
                String.Format("Average time of '{0:0.000}' ms exceeded guidance time of '{1}' ms",
                    results.AverageTime.TotalMilliseconds,
                    guidanceTime));
            return results;
        }

        protected virtual Utils.Results UserAgentsSingle(IEnumerable<string> userAgents, IEnumerable<Property> properties, int guidanceTime)
        {
            var results = UserAgentsSingle(userAgents, Utils.RetrievePropertyValues, properties);
            Console.WriteLine("Values check sum: '{0}'", results.CheckSum);
            Assert.IsTrue(results.AverageTime.TotalMilliseconds < guidanceTime,
                String.Format("Average time of '{0:0.000}' ms exceeded guidance time of '{1}' ms",
                    results.AverageTime.TotalMilliseconds,
                    guidanceTime));
            return results;
        }
        
        [TestCleanup]
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_dataSet != null)
            {
                _dataSet.Dispose();
            }
        }
    }
}
