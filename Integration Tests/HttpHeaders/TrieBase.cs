/* *********************************************************************
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited. 
 * Copyright © 2017 51Degrees Mobile Experts Limited, 5 Charlotte Close,
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
    public abstract class TrieBase : IDisposable
    {
        internal class Validation : Dictionary<string, Regex>
        {
            internal readonly TrieProvider Provider;

            internal Validation(TrieProvider provider)
            {
                Provider = provider;
            }

            public void Add(string property, string pattern)
            {
                Add(property, new Regex(pattern, RegexOptions.Compiled));
            }
        }

        protected abstract string DataFile { get; }

        protected TrieProvider _provider;
        
        internal Utils.Results Process(string userAgentPattern, string devicePattern, Validation state)
        {
            var results = new FiftyOne.Tests.Integration.Utils.Results();
            var random = new Random(0);
            var httpHeaders = _provider.HttpHeaders.Where(i => i.Equals("User-Agent") == false).ToArray();

            // Loop through setting 2 User-Agent headers.
            var userAgentIterator = UserAgentGenerator.GetEnumerable(20000, userAgentPattern).GetEnumerator();
            var deviceIterator = UserAgentGenerator.GetEnumerable(20000, devicePattern).GetEnumerator();
            while(userAgentIterator.MoveNext() &&
                deviceIterator.MoveNext())
            {
                var headers = new NameValueCollection();
                headers.Add(httpHeaders[random.Next(httpHeaders.Length)], deviceIterator.Current);
                headers.Add("User-Agent", userAgentIterator.Current);
                var indexes = _provider.GetDeviceIndexes(headers);
                Assert.IsTrue(indexes.Count > 0, "No indexes were found");
                Validate(indexes, state);
            }

            return results;
        }

        private static void Validate(IDictionary<string, int> indexes, Validation validation)
        {
            foreach(var test in validation)
            {
                var value = validation.Provider.GetPropertyValue(indexes, test.Key);
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
            if (_provider != null)
            {
                _provider.Dispose();
            }
        }
    }
}
