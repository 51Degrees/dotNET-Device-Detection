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

namespace FiftyOne.UnitTests.HttpHeaders
{
    public class Base
    {
        public class Validation : Dictionary<Property, Regex>
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

        /// <summary>
        /// The data set to be used for the tests.
        /// </summary>
        protected DataSet _dataSet;
        
        protected Utils.Results HttpHeaders(string userAgentPattern, string devicePattern, Utils.ProcessMatch method, object state)
        {
            _dataSet.ResetCache();
            var provider = new Provider(_dataSet);
            var match = provider.CreateMatch();
            var results = new FiftyOne.UnitTests.Utils.Results();
            var random = new Random(0);
            var httpHeaders = _dataSet.HttpHeaders.Where(i => i.Equals("User-Agent") == false).ToArray();

            // Loop through setting 2 user agent headers.
            var userAgentIterator = UserAgentGenerator.GetEnumerable(20000, userAgentPattern).GetEnumerator();
            var deviceIterator = UserAgentGenerator.GetEnumerable(20000, devicePattern).GetEnumerator();
            while(userAgentIterator.MoveNext() &&
                deviceIterator.MoveNext())
            {
                var headers = new NameValueCollection();
                headers.Add(httpHeaders[random.Next(httpHeaders.Length)], deviceIterator.Current);
                headers.Add("User-Agent", userAgentIterator.Current);
                provider.Match(headers, match);
                Assert.IsTrue(match.Signature == null, "Signature not equal null");
                method(match, state);
                results.Methods[match.Method]++;
            }

            return results;
        }

        public static void GenericValidate(FiftyOne.Foundation.Mobile.Detection.Match match, object state)
        {
            var validation = (Validation)state;
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
    }
}
