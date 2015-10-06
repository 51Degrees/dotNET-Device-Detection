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
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FiftyOne.Foundation.Mobile.Detection.Entities;
using System.IO;
using FiftyOne.Foundation.Mobile.Detection.Factories;
using FiftyOne.Foundation.Mobile.Detection;
using System.Collections.Specialized;

namespace FiftyOne.UnitTests.API
{
    /// <summary>
    /// Tests the APIs with different combinations of good and bad inputs.
    /// </summary>
    public abstract class TrieBase : IDisposable
    {
        private TrieProvider _provider;

        /// <summary>
        /// The name of the data file to use with the test.
        /// </summary>
        protected abstract string DataFile { get; }

        [TestInitialize()]
        public void CreateDataSet()
        {
            Utils.CheckFileExists(DataFile);
            _provider = TrieFactory.Create(DataFile, false);
        }

        [TestMethod]
        public void API_Trie_NullUserAgent()
        {
            FetchAllProperties(_provider.GetDeviceIndex((string)null));
        }

        [TestMethod]
        public void API_Trie_EmptyUserAgent()
        {
            FetchAllProperties(_provider.GetDeviceIndex(String.Empty));
        }

        [TestMethod]
        public void API_Trie_LongUserAgent()
        {
            var userAgent = String.Join(" ", UserAgentGenerator.GetEnumerable(10, 10));
            FetchAllProperties(_provider.GetDeviceIndex(userAgent));
        }

        [TestMethod]
        public void API_Trie_NullHeaders()
        {
            FetchAllProperties(_provider.GetDeviceIndexes((NameValueCollection)null));
        }

        [TestMethod]
        public void API_Trie_EmptyHeaders()
        {
            var headers = new NameValueCollection();
            FetchAllProperties(_provider.GetDeviceIndexes(headers));
        }

        [TestMethod]
        public void API_Trie_AllHeaders()
        {
            var headers = new NameValueCollection();
            foreach (var header in _provider.HttpHeaders)
            {
                headers.Add(header, UserAgentGenerator.GetRandomUserAgent(0));
            }
            FetchAllProperties(_provider.GetDeviceIndexes(headers));
        }

        [TestMethod]
        public void API_Trie_AllHeadersNull()
        {
            var headers = new NameValueCollection();
            foreach (var header in _provider.HttpHeaders)
            {
                headers.Add(header, null);
            }
            FetchAllProperties(_provider.GetDeviceIndexes(headers));
        }

        [TestMethod]
        public void API_Trie_DuplicateHeaders()
        {
            var headers = new NameValueCollection();
            for(var i = 0; i < 5; i++)
            {
                foreach (var header in _provider.HttpHeaders)
                {
                    headers.Add(header, UserAgentGenerator.GetRandomUserAgent(0));
                }
            }
            FetchAllProperties(_provider.GetDeviceIndexes(headers));
        }

        [TestMethod]
        public void API_Trie_DuplicateHeadersNull()
        {
            var headers = new NameValueCollection();
            for (var i = 0; i < 5; i++)
            {
                foreach (var header in _provider.HttpHeaders)
                {
                    headers.Add(header, null);
                }
            }
            FetchAllProperties(_provider.GetDeviceIndexes(headers));
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

        private void FetchAllProperties(IDictionary<string, int> deviceIndexes)
        {
            var checkSum = 0;
            foreach (var propertyName in _provider.PropertyNames)
            {
                var value = _provider.GetPropertyValue(deviceIndexes, propertyName);
                Console.WriteLine("Property: {0} with value {1}",
                    propertyName,
                    value);
                if (value != null)
                {
                    checkSum += _provider.GetPropertyValue(deviceIndexes, propertyName).GetHashCode();
                }
            }
            Console.WriteLine("Check sum: {0}", checkSum);
        }

        private void FetchAllProperties(int deviceIndex)
        {
            var checkSum = 0;
            foreach(var propertyName in _provider.PropertyNames)
            {
                var value = _provider.GetPropertyValue(deviceIndex, propertyName);
                Console.WriteLine("Property: {0} with value {1}",
                    propertyName,
                    value);
                if (value != null)
                {
                    checkSum += _provider.GetPropertyValue(deviceIndex, propertyName).GetHashCode();
                }
            }
            Console.WriteLine("Check sum: {0}", checkSum);
        }
    }
}
