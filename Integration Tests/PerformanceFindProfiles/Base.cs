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

namespace FiftyOne.Tests.Integration.PerformanceFindProfiles
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
                

        protected virtual void FindProfiles(double guidanceTime)
        {
            Console.WriteLine("Expected Time: {0:0.000} ms", guidanceTime);
            var startTime = DateTime.UtcNow;
            var checkSum = 0;
            var count = 0;
            foreach (var property in Constants.FIND_PROFILES_PROPERTIES.Select(i =>
                _dataSet.Properties[i]).Where(i => i != null))
            {
                var values = property.Values.Select(i => i.Name).ToArray();
                _dataSet.ResetCache();
                foreach (var valueName in values)
                {
                    var profiles = _dataSet.FindProfiles(property.Name, valueName);
                    count++;
                    foreach (var profile in profiles)
                    {
                        checkSum += profile.Index;
                    }
                }
            }
            var averageTime = (double)(DateTime.UtcNow - startTime).TotalMilliseconds / (double)count;
            Console.WriteLine("Checksum: {0}", checkSum);
            Console.WriteLine("Average time: {0:0.000} ms", averageTime);
            if (averageTime > guidanceTime)
            {
                Assert.IsTrue(averageTime < guidanceTime,
                    String.Format("Average time of '{0:0.000}' ms exceeded guidance time of '{1:0.000}' ms",
                        averageTime,
                        guidanceTime));
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
