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
using FiftyOne.Foundation.Mobile.Detection;
using FiftyOne.Foundation.Mobile.Detection.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FiftyOne.Tests.Integration.HttpHeaders
{
    public abstract class Base : IDisposable
    {
        internal class Validation : Dictionary<Property, Regex>
        {
            private readonly DataSet _dataSet;

            internal Validation(DataSet dataSet)
            {
                _dataSet = dataSet;
            }

            public void Add(string property, string pattern)
            {
                Add(_dataSet.Properties[property], new Regex(pattern, RegexOptions.Compiled));
            }
        }

        protected abstract string DataFile { get; }

        /// <summary>
        /// The data set to be used for the tests.
        /// </summary>
        protected DataSet _dataSet;
        
        internal Utils.Results Process(string userAgentPattern, string devicePattern, Validation state)
        {
            _dataSet.ResetCache();
            var provider = new Provider(_dataSet);
            var match = provider.CreateMatch();
            var results = new FiftyOne.Tests.Integration.Utils.Results();
            var random = new Random(0);
            var httpHeaders = _dataSet.HttpHeaders.Where(i => i.Equals("User-Agent") == false).ToArray();

            // Loop through setting 2 User-Agent headers.
            var userAgentIterator = UserAgentGenerator.GetEnumerable(20000, userAgentPattern).GetEnumerator();
            var deviceIterator = UserAgentGenerator.GetEnumerable(20000, devicePattern).GetEnumerator();
            while(userAgentIterator.MoveNext() &&
                deviceIterator.MoveNext())
            {
                var headers = new NameValueCollection();
                headers.Add(httpHeaders[random.Next(httpHeaders.Length)], deviceIterator.Current);
                headers.Add("User-Agent", userAgentIterator.Current);
                provider.Match(headers, match);
                Assert.IsTrue(match.Signature == null, string.Format("Signature not equal null.\r\nUA: '{0}'\r\nDevice UA: '{1}'", 
                    userAgentIterator.Current, deviceIterator.Current));
                Assert.IsTrue(match.Difference == 0, string.Format("Match difference not equal to zero.\r\nUA: '{0}'\r\nDevice UA: '{1}''", 
                    userAgentIterator.Current, deviceIterator.Current));
                Assert.IsTrue(match.Method == MatchMethods.Exact, string.Format("Match method not equal to Exact.\r\nUA: '{0}'\r\nDevice UA: '{1}'", 
                    userAgentIterator.Current, deviceIterator.Current));
                Validate(match, state);
                results.Methods[match.Method]++;
            }

            return results;
        }

        private static void Validate(FiftyOne.Foundation.Mobile.Detection.Match match, Validation validation)
        {
            foreach(var test in validation)
            {
                var value = match[test.Key].ToString();
                if (test.Value.IsMatch(value) == false)
                {
                    Assert.Fail(String.Format(
                        "HttpHeader test failed for Property '{0}' and test '{1}' with result '{2}'",
                        test.Key,
                        test.Value,
                        value));
                }
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
