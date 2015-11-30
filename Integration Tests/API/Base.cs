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

namespace FiftyOne.Tests.Integration.API
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

        [TestInitialize()]
        public void CreateDataSet()
        {
            Utils.CheckFileExists(DataFile);
            _dataSet = StreamFactory.Create(DataFile, false);
            _provider = new Provider(_dataSet);
        }

        [TestMethod]
        public void API_ReadAllSignaturesMissingProperty()
        {
            Parallel.ForEach(_dataSet.Signatures, signature =>
            {
                Assert.IsNull(
                    signature["__MissingProperty__"], 
                    "Missing properties should return null values");
            });
        }

        [TestMethod]
        public void API_ReadAllProfilesMissingProperty()
        {
            Parallel.ForEach(_dataSet.Components.SelectMany(i => i.Profiles), profile =>
            {
                Assert.IsNull(
                    profile["__MissingProperty__"],
                    "Missing properties should return null values");
            });
        }

        [TestMethod]
        public void API_ReadAllSignatures()
        {
            int hashcode = 0;
            Parallel.ForEach(_dataSet.Signatures, signature =>
            {
                int signatureHashCode = 0;
                foreach(var property in _dataSet.Properties)
                {
                    foreach(var value in signature[property])
                    {
                        signatureHashCode += value.GetHashCode();
                    }
                }
                Interlocked.Add(ref hashcode, signatureHashCode);
            });
            Console.WriteLine("Signatures Hashcode '{0}'", hashcode);
        }

        [TestMethod]
        public void API_ReadAllProfiles()
        {
            int hashcode = 0;
            Parallel.ForEach(_dataSet.Components.SelectMany(i => i.Profiles), profile =>
            {
                int signatureHashCode = 0;
                foreach (var property in _dataSet.Properties)
                {
                    foreach (var value in profile[property])
                    {
                        signatureHashCode += value.GetHashCode();
                    }
                }
                Interlocked.Add(ref hashcode, signatureHashCode);
            });
            Console.WriteLine("Signatures Hashcode '{0}'", hashcode);
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
            FetchAllProperties(_provider.Match(headers));
        }

        [TestMethod]
        public void API_AllHeadersNull()
        {
            var headers = new NameValueCollection();
            foreach (var header in _dataSet.HttpHeaders)
            {
                headers.Add(header, null);
            }
            FetchAllProperties(_provider.Match(headers));
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
            FetchAllProperties(_provider.Match(headers));
        }

        [TestMethod]
        public void API_DuplicateHeadersNull()
        {
            var headers = new NameValueCollection();
            for (var i = 0; i < 5; i++)
            {
                foreach (var header in _dataSet.HttpHeaders)
                {
                    headers.Add(header, null);
                }
            }
            FetchAllProperties(_provider.Match(headers));
        }

        [TestMethod]
        public void FetchProfiles() 
        {
            int lastProfileId = GetHighestProfileId();
            for (int i = 0; i <= lastProfileId; i++) {
                Profile profile = _dataSet.FindProfile(i);
                if (profile != null) {
                    Assert.IsTrue(profile.ProfileId == i);
                    FetchAllProperties(profile);
                }
            }
        }

        [TestMethod(), TestCategory("API")]
        public void FetchValidDeviceIds()
        {
            var random = new Random();
            for (int i = 0; i <= 100; i++)
            {
                var deviceId = _dataSet.Components.Select(c => 
                    c.Profiles.Skip(random.Next(c.Profiles.Length)).First().ProfileId).ToArray();
                var deviceIdString = String.Join("-", deviceId);
                var deviceIdArray = deviceId.SelectMany(p => BitConverter.GetBytes(p)).ToArray();
                var matchDeviceId = _provider.MatchForDeviceId(deviceId);
                var matchDeviceIdString = _provider.MatchForDeviceId(deviceIdString);
                var matchDeviceIdArray = _provider.MatchForDeviceId(deviceIdArray);
                Assert.IsTrue(matchDeviceId.DeviceId.Equals(deviceIdString));
                Assert.IsTrue(matchDeviceIdString.DeviceId.Equals(deviceIdString));
                Assert.IsTrue(matchDeviceIdArray.DeviceId.Equals(deviceIdString));
                Assert.IsTrue(matchDeviceId.DeviceIdAsByteArray.SequenceEqual(deviceIdArray));
                Assert.IsTrue(matchDeviceIdString.DeviceIdAsByteArray.SequenceEqual(deviceIdArray));
                Assert.IsTrue(matchDeviceIdArray.DeviceIdAsByteArray.SequenceEqual(deviceIdArray));
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
                _provider.Dispose();
                _dataSet.Dispose();
            }
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
            Console.WriteLine("Checksum: {0}", checkSum);
        }
        
        private int GetHighestProfileId()
        {
            int lastProfileId = 0;
            foreach (Profile profile in _dataSet.Components.SelectMany(i =>
                i.Profiles))
            {
                if (profile.ProfileId > lastProfileId)
                {
                    lastProfileId = profile.ProfileId;
                }
            }
            return lastProfileId;
        }

        private void FetchAllProperties(Profile profile)
        {
            var checkSum = 0;
            foreach (Property property in profile.Properties)
            {
                Console.WriteLine("Property: {0} with value {1}",
                    property.Name,
                    profile[property]);
                checkSum += profile[property.Name].ToString().GetHashCode();
            }
            Console.WriteLine("Checksum: {0}", checkSum);
        }
    }
}
