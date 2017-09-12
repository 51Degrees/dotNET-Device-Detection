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
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FiftyOne.Foundation.Mobile.Detection.Entities;
using System.Threading;
using FiftyOne.Foundation.Mobile.Detection.Caching;

namespace FiftyOne.Foundation.Mobile.Detection
{
    /// <summary>
    /// Methods used to interface with device data from SQL server.
    /// </summary>
    public partial class UserDefinedFunctions
    {
        #region Constants

        /// <summary>
        /// Characters used to seperate the profile Ids in the device id.
        /// </summary>
        private static readonly string[] ProfileSeperator = new string[] {
            FiftyOne.Foundation.Mobile.Detection.Constants.ProfileSeperator };

        #endregion

        #region Classes

        /// <summary>
        /// Loads byte array results into the cache.
        /// </summary>
        class ByteArrayIdCacheLoad : IValueLoader<byte[], InternalResult>
        {
            public InternalResult Load(byte[] id)
            {
                return GetMatch(IterateProfileIds(id));
            }
        }

        /// <summary>
        /// Loads missed string Id result into the cache.
        /// </summary>
        class StringIdCacheLoader : IValueLoader<string, InternalResult>
        {
            public InternalResult Load(string id)
            {
                return GetMatch(IterateProfileIds(id));
            }
        }

        /// <summary>
        /// Loads a missed User-Agent result into the cache.
        /// </summary>
        class UserAgentCacheLoader : IValueLoader<string, InternalResult>
        {
            public InternalResult Load(string userAgent)
            {
                // The result wasn't in the cache so find it from the 51Degrees provider.
                var match = _provider.Match(userAgent);

                // Construct the results for the values and the profile Ids.
                var result = new InternalResult(
                    new SortedList<string, string>(_numberOfProperties),
                    match.ProfileIds.OrderBy(i =>
                        i.Key).SelectMany(i =>
                            BitConverter.GetBytes(i.Value)).ToArray());

                // Load the values into the results.
                foreach (var property in _requiredProperties.SelectMany(i =>
                    i.Value))
                {
                    var values = match[property];
                    if (values != null)
                    {
                        result.Values.Add(property.Name, values.ToString());
                    }
                }

                // Load the dynamic values into the results.
                foreach (var dynamicProperty in _dynamicProperties)
                {
                    // Add special properties for the detection.
                    switch (dynamicProperty)
                    {
                        case DynamicProperties.Id:
                            result.Values.Add("Id", String.Join(
                                Constants.ProfileSeperator,
                                match.ProfileIds.OrderBy(i =>
                                    i.Key).Select(i => i.Value.ToString())));
                            break;
                        case DynamicProperties.Difference:
                            result.Values.Add("Difference", match.Difference.ToString());
                            break;
                        case DynamicProperties.Method:
                            result.Values.Add("Method", match.Method.ToString());
                            break;
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// Used by the Id cache for the id byte array key field.
        /// </summary>
        class ByteArrayEqualityComparer : IEqualityComparer<byte[]>
        {
            public bool Equals(byte[] left, byte[] right)
            {
                if (left == null || right == null)
                {
                    return left == right;
                }
                return left.SequenceEqual(right);
            }

            public int GetHashCode(byte[] key)
            {
                if (key == null)
                    throw new ArgumentNullException("key");
                var hash = new byte[4];
                var index = 0;
                foreach (var v in key)
                {
                    hash[index] ^= v;
                    index++;
                    if (index >= hash.Length)
                        index = 0;
                }
                return BitConverter.ToInt32(hash, 0);
            }
        }

        /// <summary>
        /// Used to return the property value
        /// for a device.
        /// </summary>
        struct InternalDeviceProperty
        {
            public readonly string Name;
            public readonly string Value;

            internal InternalDeviceProperty(string name, string value)
            {
                this.Name = name;
                this.Value = value;
            }
        }

        struct InternalResult
        {
            /// <summary>
            /// The values associated with the result.
            /// </summary>
            internal readonly IDictionary<string, string> Values;

            /// <summary>
            /// The unique ID of the device the results are associated with. Can
            /// be stored as a unique key for the device and can be used with
            /// DevicePropertiesById method to return a table from the id.
            /// </summary>
            internal readonly byte[] Id;

            internal InternalResult(IDictionary<string, string> values, IList<int> profileIds)
            {
                this.Values = values;
                int index = 0;
                Id = new byte[profileIds.Count * sizeof(int)];
                for (int i = 0; i < profileIds.Count; i++)
                {
                    foreach (byte b in BitConverter.GetBytes(profileIds[i]))
                    {
                        Id[index++] = b;
                    }
                }
            }

            internal InternalResult(IDictionary<string, string> values, byte[] id)
            {
                this.Values = values;
                this.Id = id;
            }
        }

        /// <summary>
        /// Dynamic properties that can be requested.
        /// </summary>
        enum DynamicProperties
        {
            Id,
            Difference,
            Method
        }

        #endregion

        #region Fields

        /// <summary>
        /// The provider to be used for all detection requests.
        /// </summary>
        private static Provider _provider;

        /// <summary>
        /// List of comonent Ids, and their respective property Ids.
        /// </summary>
        private static readonly SortedList<Component, List<Property>> _requiredProperties =
            new SortedList<Component,List<Property>>();

        /// <summary>
        /// A list of the dynamic properties required.
        /// </summary>
        private static readonly List<DynamicProperties> _dynamicProperties = 
            new List<DynamicProperties>();

        /// <summary>
        /// The total number of properties to be returned.
        /// </summary>
        private static int _numberOfProperties = 0;

        /// <summary>
        /// Cache for User-Agent string keys to results.
        /// </summary>
        private static LruCache<string, InternalResult> _cacheUserAgent;

        /// <summary>
        /// Cache for Device ID string keys to results.
        /// </summary>
        private static LruCache<string, InternalResult> _cacheIdString;

        /// <summary>
        /// Cache for byte array device ID keys to results.
        /// </summary>
        private static LruCache<byte[], InternalResult> _cacheIdArray;

        #endregion

        #region Private Methods

        /// <summary>
        /// Iterates through the profile Ids that the device Id contains returning
        /// each id in turn.
        /// </summary>
        /// <param name="id">The device Id</param>
        /// <returns>Each integer profile Id contained in the device id</returns>
        private static IEnumerator<int> IterateProfileIds(string id)
        {
            int value;
            foreach (var profileId in id.Split(ProfileSeperator, StringSplitOptions.RemoveEmptyEntries))
            {
                if (int.TryParse(profileId, out value))
                {
                    yield return value;
                }
            }
        }

        /// <summary>
        /// Iterates through the profile Ids that the device Id contains returning
        /// each id in turn.
        /// </summary>
        /// <param name="id">The device Id</param>
        /// <returns>Each integer profile Id contained in the device id</returns>
        private static IEnumerator<int> IterateProfileIds(byte[] id)
        {
            var profileIds = new int[id.Length / sizeof(int)];
            for (int i = 0, b = 0; b < id.Length; i++, b += sizeof(int))
            {
                yield return BitConverter.ToInt32(id, b);
            }
        }

        /// <summary>
        /// Returns a match for the profile integers returned by the iterator.
        /// </summary>
        /// <param name="iterator"></param>
        /// <returns></returns>
        private static InternalResult GetMatch(IEnumerator<int> iterator)
        {
            List<Property> properties;
            var profileIds = new List<int>(_requiredProperties.Count);

            // Construct the results for the values and the profile Ids.
            var resultValues = new SortedList<string, string>(_numberOfProperties);

            while (iterator.MoveNext())
            {
                // Add the profile Id to the list of profile Ids.
                if (profileIds != null)
                {
                    profileIds.Add(iterator.Current);
                }

                // Get the profile based on the unique id.
                var profile = _provider.DataSet.FindProfile(iterator.Current);

                // Check that the profile Id exists and that we require 
                // properties from it.
                if (profile != null &&
                    _requiredProperties.TryGetValue(profile.Component, out properties))
                {
                    // Load the values into the results.
                    foreach (var property in properties)
                    {
                        var values = profile[property];
                        if (values != null)
                            resultValues.Add(property.Name, values.ToString());
                    }
                }
            }

            // Add the device Id as a string if required.
            if (_dynamicProperties.Contains(DynamicProperties.Id))
            {
                resultValues.Add("Id", String.Join(
                    Constants.ProfileSeperator,
                    profileIds.Select(i => i.ToString())));
            }

            return new InternalResult(resultValues, profileIds);
        }

        /// <summary>
        /// Gets the match from the cache for the id or if not
        /// present in the cache from the detection provider. Matched
        /// from the device id will not populate the Difference or Method
        /// values.
        /// </summary>
        /// <param name="id">The device id whose properties are needed</param>
        /// <returns>A results object for the device id</returns>
        private static InternalResult GetMatchById(string id)
        {
            return _cacheIdString[id];
        }

        /// <summary>
        /// Gets the match from the cache for the id or if not
        /// present in the cache from the detection provider. Matched
        /// from the device id will not populate the Difference or Method
        /// values.
        /// </summary>
        /// <param name="id">The device id whose properties are needed</param>
        /// <returns>A results object for the device id</returns>
        private static InternalResult GetMatchById(byte[] id)
        {
            return _cacheIdArray[id];
        }

        /// <summary>
        /// Gets the match from the cache for the useragent or if not
        /// present in the cache from the detection provider.
        /// </summary>
        /// <param name="userAgent">The user agent whose properties are needed</param>
        /// <returns>A results object for the user agent</returns>
        private static InternalResult GetMatchByUserAgent(string userAgent)
        {
            return _cacheUserAgent[userAgent];
        }

        /// <summary>
        /// Fills the SQL record with the values.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="Property"></param>
        /// <param name="Value"></param>
        private static void Fill(object source, out SqlString Property, out SqlString Value)
        {
            InternalDeviceProperty item = (InternalDeviceProperty)source;
            Property = item.Name;
            Value = item.Value;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initialises the device detection service with the filename provided and the
        /// list of properties to be returned.
        /// </summary>
        /// <param name="filename">The pattern format file containing 51Degrees data</param>
        /// <param name="properties">A space seperated list of properties to be returned 
        /// from calls to <see cref="GetDeviceProperties"/>GetDeviceProperties</see></param>
        /// <param name="expirySeconds">The number of seconds between cache services</param>
        /// <param name="memoryMode">True if the data set should be loaded into memory
        /// to improve performance</param>
        /// <returns>True if the provider was created correctly</returns>
        [Microsoft.SqlServer.Server.SqlFunction]
        public static SqlBoolean InitialiseDeviceDetection(SqlString filename, SqlString properties, SqlInt32 expirySeconds, SqlBoolean memoryMode)
        {
            if (filename.IsNull || File.Exists(filename.Value) == false)
            {
                throw new FileNotFoundException(String.Format(
                    "Device data file '{0}' could not be found",
                    filename.Value));
            }

            // Dispose of the old providers dataset if not null.
            if (_provider != null)
            {
                _provider.DataSet.Dispose();
            }

            // Create the provider using the file provided.
            try
            {
                _provider = new Provider(
                    memoryMode.IsNull || memoryMode.Value == false ?
                    Factories.StreamFactory.Create(filename.Value) :
                    Factories.MemoryFactory.Create(filename.Value));
            }
            catch(Exception ex)
            {
                throw new MobileException(String.Format(
                    "Could not create data set from file '{0}'. " +
                    "Check the file is uncompressed and in the correct format.",
                    filename.Value), ex);
            }

            if (_provider != null)
            {
                // Configure the caches.
                int cacheSize = 1000;
                _cacheUserAgent = new LruCache<string, InternalResult>(
                    cacheSize, 
                    new UserAgentCacheLoader());
                _cacheIdString = new LruCache<string, InternalResult>(
                    cacheSize,
                    new StringIdCacheLoader());
                _cacheIdArray = new LruCache<byte[], InternalResult>(
                    cacheSize,
                    new ByteArrayIdCacheLoad());

                // Set the properties that the functions should be returning.
                _numberOfProperties = 0;
                _requiredProperties.Clear();
                _dynamicProperties.Clear();
                if (properties.IsNull || String.IsNullOrEmpty(properties.Value))
                {
                    // Use all the available properties that the provider supports.
                    foreach(var component in _provider.DataSet.Components)
                    {
                        _requiredProperties.Add(component, component.Properties.ToList());
                        _numberOfProperties += component.Properties.Length;
                    }
                    _dynamicProperties.Add(DynamicProperties.Id);
                    _dynamicProperties.Add(DynamicProperties.Difference);
                    _dynamicProperties.Add(DynamicProperties.Method);
                }
                else
                {
                    // Get the array of properties that should be returned from the match where
                    // the property is contained in the providers data set.
                    foreach (var propertyName in properties.Value.Split(
                        new string[] { " ", ",", "|", "\t" },
                        StringSplitOptions.RemoveEmptyEntries).Select(i => i.Trim()).Distinct())
                    {
                        switch(propertyName)
                        { 
                            case "Id":
                                _dynamicProperties.Add(DynamicProperties.Id);
                                _numberOfProperties++;
                                break;
                            case "Method":
                                _dynamicProperties.Add(DynamicProperties.Method);
                                _numberOfProperties++;
                                break;
                            case "Difference":
                                _dynamicProperties.Add(DynamicProperties.Difference);
                                _numberOfProperties++;
                                break;
                            default:
                                var property = _provider.DataSet.Properties[propertyName];
                                if (property != null)
                                {
                                    if (_requiredProperties.ContainsKey(property.Component) == false)
                                    {
                                        _requiredProperties.Add(property.Component, new List<Property>());
                                    }
                                    _requiredProperties[property.Component].Add(property);
                                    _numberOfProperties++;
                                }
                                break;
                        }
                    }
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns the device ID as a byte array. The device id is unique across
        /// different versions of the data set and can be used to return properties
        /// at a later point in time.
        /// </summary>
        /// <param name="userAgent"></param>
        /// <returns></returns>
        [Microsoft.SqlServer.Server.SqlFunction]
        public static SqlBinary DeviceIdAsByteArray(SqlString userAgent)
        {
            return GetMatchByUserAgent(userAgent.Value).Id;
        }

        /// <summary>
        /// Returns the device ID in the form A-B-C-D for the given
        /// user agent. The ID is unique across different versions of the data set.
        /// </summary>
        /// <param name="userAgent">Target user agent</param>
        /// <returns>Device Id for the user agent.</returns>
        [Microsoft.SqlServer.Server.SqlFunction]
        public static SqlString DeviceIdAsString(SqlString userAgent)
        {
            return DevicePropertyByUserAgent(userAgent, "Id");
        }

        /// <summary>
        /// Returns the value as a string associated with the given user agent
        /// and property name.
        /// </summary>
        /// <param name="userAgent">Target user agent</param>
        /// <param name="propertyName">Property name to be returned</param>
        /// <returns></returns>
        [Microsoft.SqlServer.Server.SqlFunction]
        public static SqlString DevicePropertyByUserAgent(SqlString userAgent, SqlString propertyName)
        {
            var result = GetMatchByUserAgent(userAgent.Value);
            string value;
            if (result.Values.TryGetValue(propertyName.Value, out value))
                return value;
            return SqlString.Null;
        }

        /// <summary>
        /// Returns the enumeration where each record represents a required property
        /// provided in the intialiser. The key for the properties is a device id
        /// returned from a previous call to DeviceIdAsByteArray.
        /// </summary>
        /// <param name="id">Id of the device previously returned from DeviceIdAsByteArray</param>
        /// <returns>An enumerator of property names and values</returns>
        [SqlFunction(FillRowMethodName = "Fill", TableDefinition = "Property NVARCHAR(100), Value NVARCHAR(MAX)")]
        public static IEnumerable DevicePropertiesById(SqlBinary id)
        {
            var result = GetMatchById(id.Value);
            try
            {
                return result.Values.AsParallel().WithDegreeOfParallelism(Math.Min(System.Environment.ProcessorCount, 64)).Select(i =>
                    new InternalDeviceProperty(i.Key, String.Join(", ", i.Value)));
            }
            catch
            {
                return new InternalDeviceProperty[0];
            }
        }

        /// <summary>
        /// Returns the enumeration where each record represents a required property
        /// provided in the intialiser. The key for the properties is a device id
        /// in the string form A-B-C-D where each letter is a number.
        /// </summary>
        /// <param name="id">Id of the device in hyphen seperated string form</param>
        /// <returns>An enumerator of property names and values</returns>
        [SqlFunction(FillRowMethodName = "Fill", TableDefinition = "Property NVARCHAR(100), Value NVARCHAR(MAX)")]
        public static IEnumerable DevicePropertiesByStringId(SqlString id)
        {
            var result = GetMatchById(id.Value);
            try
            {
                return result.Values.AsParallel().WithDegreeOfParallelism(Math.Min(System.Environment.ProcessorCount, 64)).Select(i =>
                    new InternalDeviceProperty(i.Key, String.Join(", ", i.Value)));
            }
            catch
            {
                return new InternalDeviceProperty[0];
            }
        }
        
        /// <summary>
        /// Returns the en enumeration where each record represents a required property
        /// provided in the intialiser. The key is a user agent.
        /// </summary>
        /// <param name="userAgent">User agent whose properties are required</param>
        /// <returns>An enumerator of property names and values</returns>
        [SqlFunction(FillRowMethodName = "Fill", TableDefinition = "Property NVARCHAR(100), Value NVARCHAR(MAX)")]
        public static IEnumerable DevicePropertiesByUserAgent(SqlString userAgent)
        {
            var result = GetMatchByUserAgent(userAgent.Value);
            try
            {
                return result.Values.AsParallel().WithDegreeOfParallelism(Math.Min(System.Environment.ProcessorCount, 64)).Select(i =>
                    new InternalDeviceProperty(i.Key, String.Join(", ", i.Value)));
            }
            catch
            {
                return new InternalDeviceProperty[0];
            }
        }

        #endregion
    }
}