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
            _dataSet = StreamFactory.Create(File.ReadAllBytes(DataFile));
            _provider = new Provider(_dataSet);
            Console.WriteLine("Dataset: {0}", _dataSet.Name);
            Console.WriteLine("Format: {0}", _dataSet.Format);
        }

        [TestMethod]
        [TestCategory("API")]
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
        [TestCategory("API")]
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
        [TestCategory("API")]
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
        [TestCategory("API")]
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

        private readonly static string[] UNKNOWN_USER_AGENTS = new string[] {
            "!abc",
            "!123"
        };

        [TestMethod]
        [TestCategory("API")]
        public void API_UnknownUserAgent()
        {
            foreach (var userAgent in UNKNOWN_USER_AGENTS)
            {
                var match = _provider.Match(userAgent);
                FetchAllProperties(match);
                Assert.IsTrue(match.Method == MatchMethods.None);
            }
        }

        [TestMethod]
        [TestCategory("API")]
        public void API_ShortBadUserAgent()
        {
            string userAgent = null;
            var checkSum = 0;
            var chars = Enumerable.Range(
                _provider.DataSet.LowestCharacter, 
                _provider.DataSet.HighestCharacter).Select(i => 
                    (char)i).ToArray();
            var random = new Random(0);
            var match = _provider.CreateMatch();
            try
            {
                for (int i = 0; i <= 100000; i++)
                {
                    int numberOfCharacters = random.Next(chars.Length);
                    userAgent = new String(
                        Enumerable.Repeat(chars, numberOfCharacters).Select(s =>
                        s[random.Next(chars.Length)]).ToArray());
                    _provider.Match(userAgent, match);
                    foreach (var property in match.DataSet.Properties)
                    {
                        checkSum += match[property.Name].ToString().GetHashCode();
                    }
                }
            }
            catch(Exception ex)
            {
                Assert.Fail("User-Agent '{0}' caused '{1}'",
                    userAgent,
                    ex.Message);
            }
            Console.WriteLine("Checksum: {0}", checkSum);
        }

        [TestMethod]
        [TestCategory("API")]
        public void API_SingleCharacterUserAgents()
        {
            for (var c = 0; c < 256; c++)
            {
                FetchAllProperties(_provider.Match(c.ToString()));
            }
        }

        [TestMethod]
        [TestCategory("API")]
        public void API_NullUserAgent()
        {
            FetchAllProperties(_provider.Match((string)null));
        }

        [TestMethod]
        [TestCategory("API")]
        public void API_EmptyUserAgent()
        {
            FetchAllProperties(_provider.Match(String.Empty));
        }

        [TestMethod]
        [TestCategory("API")]
        public void API_LongUserAgent()
        {
            var userAgent = String.Join(" ", UserAgentGenerator.GetEnumerable(10, 10));
            FetchAllProperties(_provider.Match(userAgent));
        }

        [TestMethod]
        [TestCategory("API")]
        public void API_NullHeaders()
        {
            FetchAllProperties(_provider.Match((NameValueCollection)null));
        }

        [TestMethod]
        [TestCategory("API")]
        public void API_EmptyHeaders()
        {
            var headers = new NameValueCollection();
            FetchAllProperties(_provider.Match(headers));
        }

        [TestMethod]
        [TestCategory("API")]
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
        [TestCategory("API")]
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
        [TestCategory("API")]
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
        [TestCategory("API")]
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
        [TestCategory("API")]
        public void API_FetchProfiles() 
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

        [TestMethod()]
        [TestCategory("API")]
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

        private void FetchAllProperties(Match match)
        {
            var checkSum = 0;
            foreach(var profile in match.Profiles)
            {
                Console.WriteLine("Component: {0} has profile Id {1}",
                    profile.Component.ComponentId,
                    profile.ProfileId);
                checkSum += profile.ProfileId;
            }
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
