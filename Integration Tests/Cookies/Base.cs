/* *********************************************************************
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited. 
 * Copyright © 2015 51Degrees Mobile Experts Limited, 5 Charlotte Close,
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
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace FiftyOne.Tests.Integration.Cookies
{
    public abstract class Base : IDisposable
    {
        protected abstract string DataFile { get; }

        /// <summary>
        /// The data set to be used for the tests.
        /// </summary>
        protected DataSet _dataSet;

        /// <summary>
        /// Sets random cookie values for properties which support JavaScript
        /// Property Value Override and checks that they are returned 
        /// correctly.
        /// </summary>
        /// <remarks>
        /// The test checks that the multiple behaviours of <see cref="Values"/>
        /// work when dynamic values are provided.
        /// </remarks>
        /// <returns></returns>
        internal Utils.Results ProcessAll()
        {
            var target = new Uri("http://localhost");
            var provider = new Provider(_dataSet);
            var match = provider.CreateMatch();
            var results = new FiftyOne.Tests.Integration.Utils.Results();
            var random = new Random(0);
            var httpHeaders = _dataSet.HttpHeaders.Where(i => i.Equals("User-Agent") == false).ToArray();

            // Loop through setting 2 User-Agent headers.
            var userAgentIterator = UserAgentGenerator.GetRandomUserAgents().GetEnumerator();
            while (userAgentIterator.MoveNext())
            {
                var headers = new NameValueCollection();
                headers.Add("User-Agent", userAgentIterator.Current);

                // Add a random value to the cookie.
                var cookies = new CookieContainer();
                var testValues = new Dictionary<Property, string>();
                foreach (var property in _dataSet.JavaScriptProperties.Where(i =>
                    i.Category.Equals(FiftyOne.Foundation.Mobile.Detection.Constants.PropertyValueOverrideCategory)))
                {
                    var propertyName = property.Name.Replace("JavaScript", "");
                    var key = FiftyOne.Foundation.Mobile.Detection.Constants.PropertyValueOverrideCookiePrefix + propertyName;
                    var value = UserAgentGenerator.GetRandomUserAgent(20);
                    cookies.Add(new Cookie(key, HttpUtility.UrlEncode(value), "/", target.Host));
                    testValues.Add(_dataSet.Properties[propertyName], value);
                }
                headers.Add("Cookie", cookies.GetCookieHeader(target));
                
                // Now check the match object returns the correct result
                // for the property. This is the primary test in this 
                // integration test.
                
                // Note:
                // The value returned from the match accessor is passed through
                // UrlDecode as in this test .NET will not have already processed
                // the cookie and decoded it. 
                provider.Match(headers, match);
                foreach (var test in testValues)
                {
                    Assert.IsTrue(
                        HttpUtility.UrlDecode(match[test.Key].ToString()).Equals(test.Value),
                        String.Format(
                            "Test failed for property '{0}'",
                            test.Key));
                }

                results.Methods[match.Method]++;
            }

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
