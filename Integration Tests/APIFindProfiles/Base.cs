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
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FiftyOne.Foundation.Mobile.Detection.Entities;
using System.IO;
using FiftyOne.Foundation.Mobile.Detection.Factories;
using FiftyOne.Foundation.Mobile.Detection;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Threading;

namespace FiftyOne.Tests.Integration.APIFindProfiles
{
    /// <summary>
    /// Tests the APIs with different combinations of good and bad inputs.
    /// </summary>
    public abstract class Base : IDisposable
    {
        private DataSet _dataSet;

        private Provider _provider;

        /// <summary>
        /// The name of the data file to use with the test.
        /// </summary>
        protected abstract string DataFile { get; }

        protected abstract int ValuesCacheSize { get; }

        [TestInitialize()]
        public void CreateDataSet()
        {
            Utils.CheckFileExists(DataFile);
            _dataSet = DataSetBuilder.Buffer()
                .ConfigureDefaultCaches()
                .SetCacheSize(CacheType.ValuesCache, ValuesCacheSize)
                .Build(File.ReadAllBytes(DataFile));
            _provider = new Provider(_dataSet);
            Console.WriteLine("Dataset: {0}", _dataSet.Name);
            Console.WriteLine("Format: {0}", _dataSet.Format);
        }
        
        [TestMethod]
        [TestCategory("API"), TestCategory("FindProfiles")]
        public void FetchValidFindProfiles()
        {
            foreach (var property in _dataSet.Properties)
            {
                foreach(var value in property.Values)
                {
                    var profiles = property.FindProfiles(value.Name);
                    Assert.IsTrue(profiles.Length > 0, String.Format(
                        "Value '{0}' for property '{1}' return no profiles.",
                        property, value));
                }
            }
        }

        [TestMethod]
        [TestCategory("API"), TestCategory("FindProfiles")]
        public void FetchValidFindProfilesCheckInitialised()
        {
            var random = new Random();
            foreach (var property in _dataSet.Properties)
            {
                foreach (var value in property.Values.Skip(random.Next(property.Values.Count - 1)).Take(1))
                {
                    Assert.IsFalse(property.InitialisedValues, String.Format(
                        "Property '{0}' should not have initialised values " +
                        "before first request", property));
                    var profiles = property.FindProfiles(value.Name);
                    Assert.IsTrue(profiles.Length > 0, String.Format(
                        "Value '{0}' for property '{1}' return no profiles.",
                        property, value));
                }
                foreach (var value in property.Values)
                {
                    Assert.IsTrue(value._profileIndexes != null &&
                                  value._profileIndexes.Length > 0,
                        String.Format(
                            "Value '{0}' must have profile indexes initialised " +
                            "for property '{1}'.",
                            value.Name,
                            property.Name));
                }
            }
        }

        [TestMethod]
        [TestCategory("API"), TestCategory("FindProfiles")]
        public void FetchValidFindProfilesWithEmptyFilter()
        {
            var emptyProfileFilter = new Profile[0];
            foreach (var property in _dataSet.Properties)
            {
                foreach (var value in property.Values)
                {
                    var profiles = property.FindProfiles(value.Name, emptyProfileFilter);
                    Assert.IsTrue(profiles.Length == 0, String.Format(
                        "Value '{0}' for property '{1}' with an empty filter must return no profiles.",
                        property, value));
                }
            }
        }

        [TestMethod]
        [TestCategory("API"), TestCategory("FindProfiles")]
        public void FetchValidFindProfilesWithFullFilter()
        {
            foreach (var property in _dataSet.Properties)
            {
                foreach (var value in property.Values)
                {
                    var profiles = property.FindProfiles(value.Name, value.Profiles);
                    Assert.IsTrue(profiles.SequenceEqual(value.Profiles), 
                        String.Format(
                            "Value '{0}' for property '{1}' returned mismatched results.",
                            value, property));
                }
            }
        }

        [TestMethod]
        [TestCategory("API"), TestCategory("FindProfiles")]
        public void FetchInValidFindProfiles()
        {
            var invalidValue = "XYZ";
            foreach (var property in _dataSet.Properties)
            {
                var profiles = property.FindProfiles(invalidValue);
                Assert.IsTrue(profiles.Length == 0, String.Format(
                    "Invalid value '{0}' for property '{1}' should not return profiles.",
                    property, invalidValue));
            }
        }

        [TestMethod]
        [TestCategory("API"), TestCategory("FindProfiles")]
        [ExpectedException(typeof(ArgumentException), "A property that does not exist was allowed.")]
        public void FetchInvalidPropertyFindProfiles()
        {
            var profiles = _dataSet.FindProfiles("__BAD_PROPERTY__", "__BAD_VALUE__");
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
                _provider.Dispose();
                _dataSet.Dispose();
            }
        }

    }
}
