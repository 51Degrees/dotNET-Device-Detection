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
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;
using System.IO;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace FiftyOne.Foundation.Mobile.Detection
{
    /// <summary>
    /// Methods used to interface with device data from SQL server.
    /// </summary>
    public partial class UserDefinedFunctions
    {
        #region Constants

        /// <summary>
        /// The number of seconds to wait before stall items are removed from
        /// the cache.
        /// </summary>
        private const int DEFAULT_CACHE_EXPIRY_SECONDS = 60;

        #endregion

        #region Classes

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

            internal InternalResult(IDictionary<string, string> values, byte[] id)
            {
                this.Values = values;
                this.Id = id;
            }
        }

        #endregion

        #region Fields

        /// <summary>
        /// The provider to be used for all detection requests.
        /// </summary>
        private static Provider _provider;

        /// <summary>
        /// Array of property names to be returned for a table result.
        /// </summary>
        private static string[] _requiredProperties;

        /// <summary>
        /// The user agent cache.
        /// </summary>
        private static Cache<string, InternalResult> _cacheUserAgent;

        /// <summary>
        /// The device id cache.
        /// </summary>
        private static Cache<byte[], InternalResult> _cacheId;

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the match from the cache for the id or if not
        /// present in the cache from the detection provider. Matches 
        /// from the device id will not populate the Difference or Method
        /// values.
        /// </summary>
        /// <param name="id">The device id whose properties are needed</param>
        /// <returns>A results object for the device id</returns>
        private static InternalResult GetMatch(byte[] id)
        {
            InternalResult result;
            if (_cacheId.TryGetValue(id, out result) == false)
            {
                // Construct the results for the values and the profile Ids.
                result = new InternalResult(
                    new SortedList<string, string>(_requiredProperties.Length),
                    id);

                var profileIds = new int[id.Length / sizeof(int)];
                for (int i = 0, b = 0; b < id.Length; i++, b += sizeof(int))
                {
                    profileIds[i] = BitConverter.ToInt32(id, b);

                    // Get the profile based on the unique id.
                    var profile = _provider.DataSet.FindProfile(profileIds[i]);

                    // Load the values into the results.
                    foreach (var property in _requiredProperties)
                    {
                        var values = profile[property];
                        if (values != null)
                            result.Values.Add(property, values.ToString());
                    }
                }

                // Add the device Id as a string if required.
                if (_requiredProperties.Contains("Id"))
                {
                    result.Values.Add("Id", String.Join(
                        Constants.ProfileSeperator,
                        profileIds.Select(i => i.ToString())));
                }

                // Add the results to the cache.
                _cacheId.SetActive(id, result);
            }

            // Ensure the result is added to the background cache.
            _cacheId.SetBackground(id, result);

            return result;
        }

        /// <summary>
        /// Gets the match from the cache for the useragent or if not
        /// present in the cache from the detection provider.
        /// </summary>
        /// <param name="userAgent">The user agent whose properties are needed</param>
        /// <returns>A results object for the user agent</returns>
        private static InternalResult GetMatch(string userAgent)
        {
            InternalResult result;
            if (_cacheUserAgent.TryGetValue(userAgent, out result) == false)
            {
                // The result wasn't in the cache so find it from the 51Degrees provider.
                var match = _provider.Match(userAgent);

                // Construct the results for the values and the profile Ids.
                result = new InternalResult(
                    new SortedList<string, string>(_requiredProperties.Length),
                    match.ProfileIds.OrderBy(i =>
                        i.Key).SelectMany(i =>
                            BitConverter.GetBytes(i.Value)).ToArray());

                // Load the values into the results.
                foreach (var property in _requiredProperties)
                {
                    var values = match[property];
                    if (values != null)
                        result.Values.Add(property, values.ToString());
                    else
                    {
                        // Add special properties for the detection.
                        switch (property)
                        {
                            case "Id":
                                result.Values.Add("Id", String.Join(
                                    Constants.ProfileSeperator,
                                    match.ProfileIds.OrderBy(i =>
                                        i.Key).Select(i => i.Value.ToString())));
                                break;
                            case "Difference":
                                result.Values.Add("Difference", match.Difference.ToString());
                                break;
                            case "Method":
                                result.Values.Add("Method", match.Method.ToString());
                                break;
                        }
                    }
                }

                // Add the results to the cache.
                _cacheUserAgent.SetActive(userAgent, result);
            }

            // Ensure the result is added to the background cache.
            _cacheUserAgent.SetBackground(userAgent, result);

            return result;
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
        /// <param name="filename">The pattern formatted filename with 51Degrees data</param>
        /// <param name="properties">A space seperated list of properties to be returned 
        /// from calls to <see cref="GetDeviceProperties"/>GetDeviceProperties</see></param>
        /// <param name="expirySeconds">The number of seconds between cache services</param>
        /// <param name="memoryMode">True if the data set should be loaded into memory
        /// to improve performance</param>
        /// <returns>True if the provider was created correctly</returns>
        [Microsoft.SqlServer.Server.SqlFunction]
        public static SqlBoolean InitialiseDeviceDetection(SqlString filename, SqlString properties, SqlInt32 expirySeconds, SqlBoolean memoryMode)
        {
            if (File.Exists(filename.Value))
            {
                // Dispose of the old providers dataset if not null.
                if (_provider != null)
                    _provider.DataSet.Dispose();

                // Create the provider using the file provided.
                _provider = new Provider(memoryMode.Value ?
                    Factories.MemoryFactory.Create(filename.Value) :
                    Factories.StreamFactory.Create(filename.Value));

                if (_provider != null)
                {
                    // Clear the caches to flush out old results.
                    var serviceInternal = expirySeconds.IsNull ?
                                DEFAULT_CACHE_EXPIRY_SECONDS : expirySeconds.Value;
                    _cacheUserAgent = new Cache<string, InternalResult>(serviceInternal);
                    _cacheId = new Cache<byte[], InternalResult>(new ByteArrayEqualityComparer(), serviceInternal);

                    // Set the properties that the functions should be returning.
                    if (String.IsNullOrEmpty(properties.Value))
                    {
                        // Use all the available properties that the provider supports.
                        _requiredProperties = _provider.DataSet.Properties.Select(i =>
                            i.Name).ToArray();
                    }
                    else
                    {
                        // Get the array of properties that should be returned from the match where
                        // the property is contained in the providers data set.
                        _requiredProperties = properties.Value.Split(
                            new string[] { " ", ",", "|", "\t" },
                            StringSplitOptions.RemoveEmptyEntries).Distinct().ToArray();
                    }

                    return true;
                }
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
            return GetMatch(userAgent.Value).Id;
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
            var result = GetMatch(userAgent.Value);
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
            var result = GetMatch(id.Value);
            try
            {
                return result.Values.AsParallel().WithDegreeOfParallelism(Math.Min(System.Environment.ProcessorCount, 64)).Select(i =>
                    new InternalDeviceProperty(i.Key, String.Join(", ", i.Value)));
            }
            catch
            {
                return null;
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
            var result = GetMatch(userAgent.Value);
            try
            {
                return result.Values.AsParallel().WithDegreeOfParallelism(Math.Min(System.Environment.ProcessorCount, 64)).Select(i =>
                    new InternalDeviceProperty(i.Key, String.Join(", ", i.Value)));
            }
            catch
            {
                return null;
            }
        }

        #endregion
    }
}