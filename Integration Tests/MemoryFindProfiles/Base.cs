/* *********************************************************************
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited. 
 * Copyright © 2017 51Degrees Mobile Experts Limited, 5 Charlotte Close,
 * Caversham, Reading, Berkshire, United Kingdom RG4 7BY
 * 
 * This Source Code Form is the subject of the following patents and patent 
 * applications, owned by 51Degrees Mobile Experts Limited of 5 Charlotte
 * Close, Caversham, Reading, Berkshire, United Kingdom RG4 7BY: 
 * European Patent No. 2871816;
 * European Patent Application No. 17184134.9;
 * United States Patent Nos. 9,332,086 and 9,350,823; and
 * United States Patent Application No. 15/686,066.
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

namespace FiftyOne.Tests.Integration.MemoryFindProfiles
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
        protected Utils.MemoryMonitor _memory;

        protected abstract string DataFile { get; }
        

        protected virtual void FindProfiles(double maxAllowedMemory)
        {
            Console.WriteLine("Expected Max Memory: {0:0.0} MB", maxAllowedMemory);
            var checkSum = 0;
            foreach (var property in Constants.FIND_PROFILES_PROPERTIES.Select(i => 
                _dataSet.Properties[i]).Where(i => i != null))
            {
                _memory.CaptureSample();
                foreach (var value in property.Values)
                {
                    var profiles = _dataSet.FindProfiles(property.Name, value.Name);
                    foreach(var profile in profiles)
                    {
                        checkSum += profile.Index;
                    }
                    
                }
                _memory.CaptureSample();
            }
            Console.WriteLine("Checksum: {0}", checkSum);
            Console.WriteLine("Average Memory Used: {0:0.0} MB", _memory.AverageMemoryUsed);
            if (_memory.AverageMemoryUsed > maxAllowedMemory)
            {
                Assert.Inconclusive(String.Format(
                    "Average memory use was '{0:0.0}MB' but max allowed '{1:0.0}MB'",
                    _memory.AverageMemoryUsed,
                    maxAllowedMemory));
            }
        }

        [TestCleanup]
        public void CleanUp()
        {
            _dataSet.Dispose();
            _dataSet = null;
        }

        ~Base()
        {
            Dispose(false);
        }

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
