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

namespace FiftyOne.UnitTests.Memory
{
    [TestClass]
    public abstract class Base : IDisposable
    {
        /// <summary>
        /// The data set to be used for the tests.
        /// </summary>
        protected DataSet _dataSet;

        /// <summary>
        /// The memory used to create the dataset.
        /// </summary>
        protected Utils.Memory _memory;

        protected abstract string DataFile { get; }

        protected virtual void UserAgentsSingle(IEnumerable<string> userAgents, double maxAllowedMemory)
        {
            _memory.Reset();
            Utils.DetectLoopSingleThreaded(
                new Provider(_dataSet),
                userAgents,
                Utils.MonitorMemory,
                _memory);
            Console.WriteLine("Memory Used: {0:0.0} MB", _memory.AverageMemoryUsed);
            if (_memory.AverageMemoryUsed > maxAllowedMemory)
            {
                Assert.Inconclusive(String.Format(
                    "Memory use was '{0:0.0}MB' but max allowed '{1:0.0}MB'",
                    _memory.AverageMemoryUsed,
                    maxAllowedMemory));
            }
        }

        protected virtual void UserAgentsMulti(IEnumerable<string> userAgents, double maxAllowedMemory)
        {
            _memory.Reset();
            Utils.DetectLoopMultiThreaded(
                new Provider(_dataSet),
                userAgents,
                Utils.MonitorMemory,
                _memory);
            Console.WriteLine("Memory Used: {0:0.0} MB", _memory.AverageMemoryUsed);
            if (_memory.AverageMemoryUsed > maxAllowedMemory)
            {
                Assert.Inconclusive(String.Format(
                    "Memory use was '{0:0.0}MB' but max allowed '{1:0.0}MB'",
                    _memory.AverageMemoryUsed,
                    maxAllowedMemory));
            }
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
