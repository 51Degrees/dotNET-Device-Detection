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
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class API
    {
        private DataSet _dataSet;
        private Provider _provider;

        [TestInitialize()]
        public void CreateDataSet()
        {
            _dataSet = StreamFactory.Create(Path.Combine(Constants.LITE_PATTERN_V31));
            _provider = new Provider(_dataSet);
        }

        [TestMethod]
        public void API_NullUserAgent()
        {
            FetchAllProperties(_provider.Match((string)null));
        }

        [TestMethod]
        public void API_EmptyUserAgent()
        {
            FetchAllProperties(_provider.Match(String.Empty));
        }

        [TestMethod]
        public void API_LongUserAgent()
        {
            var userAgent = String.Join(" ", UserAgentGenerator.GetEnumerable(10, 10));
            FetchAllProperties(_provider.Match(userAgent));
        }

        [TestMethod]
        public void API_NullHeaders()
        {
            FetchAllProperties(_provider.Match((NameValueCollection)null));
        }

        [TestMethod]
        public void API_EmptyHeaders()
        {
            var headers = new NameValueCollection();
            FetchAllProperties(_provider.Match(headers));
        }

        [TestMethod]
        public void API_AllHeaders()
        {
            var headers = new NameValueCollection();
            foreach(var header in _dataSet.HttpHeaders)
            {
                headers.Add(header, UserAgentGenerator.GetRandomUserAgent(0));
            }
            _provider.Match(headers);
        }

        [TestMethod]
        public void API_DuplicateHeaders()
        {
            var headers = new NameValueCollection();
            for(var i = 0; i < 5; i++)
            {
                foreach (var header in _dataSet.HttpHeaders)
                {
                    headers.Add(header, UserAgentGenerator.GetRandomUserAgent(0));
                }
            }
            _provider.Match(headers);
        }

        private void FetchAllProperties(Match match)
        {
            var checkSum = 0;
            foreach(var property in match.DataSet.Properties)
            {
                Console.WriteLine("Property: {0} with value {1}",
                    property.Name,
                    match[property.Name]);
                checkSum += match[property.Name].ToString().GetHashCode();
            }
            Console.WriteLine("Check sum: {0}", checkSum);
        }
    }
}
