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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.IO;
using FiftyOne.Foundation.Mobile.Detection.Entities.Stream;
using System.Collections.Specialized;

namespace FiftyOne.Foundation.Mobile.Detection
{
    /// <summary>
    /// Decision trie data structure provider.
    /// </summary>
    public abstract class TrieProvider : IDisposable
    {
        #region Enumerations

        /// <summary>
        /// The type of integers used to represent the offset to the children.
        /// </summary>
        public enum OffsetType
        {
            /// <summary>
            /// The offsets in the node are 16 bit integers.
            /// </summary>
            Bits16 = 0,
            /// <summary>
            /// The offsets in the node are 32 bit integers.
            /// </summary>
            Bits32 = 1,
            /// <summary>
            /// The offsets in the node are 64 bit integers.
            /// </summary>
            Bits64 = 2
        }

        #endregion

        #region Fields

        /// <summary>
        /// The copy right notice associated with the data file.
        /// </summary>
        public readonly string Copyright;

        /// <summary>
        /// Byte array of the strings available.
        /// </summary>
        private readonly byte[] _strings;

        /// <summary>
        /// Byte array of the available properties.
        /// </summary>
        protected readonly byte[] _properties;

        /// <summary>
        /// Byte array of the devices list.
        /// </summary>
        private readonly byte[] _devices;

        /// <summary>
        /// Byte array of the look up list loaded into memory.
        /// </summary>
        private readonly byte[] _lookupList;

        /// <summary>
        /// A pool of readers that can be used in multi threaded operation.
        /// </summary>
        private readonly Pool _pool;

        /// <summary>
        /// The position in the source data file of the nodes.
        /// </summary>
        private readonly long _nodesOffset;

        /// <summary>
        /// Dictionary of property names to indexes.
        /// </summary>
        protected readonly Dictionary<string, int> _propertyIndex = new Dictionary<string, int>();

        /// <summary>
        /// List of the available property names.
        /// </summary>
        protected readonly List<string> _propertyNames = new List<string>();

        /// <summary>
        /// List of Http headers for each property index.
        /// </summary>
        protected readonly List<string[]> _propertyHttpHeaders = new List<string[]>();

        #endregion

        #region Public Properties

        /// <summary>
        /// A list of Http headers that the provider can use
        /// for device detection.
        /// </summary>
        public List<string> HttpHeaders
        {
            get
            {
                if (_httpHeaders == null)
                {
                    lock(this)
                    {
                        if (_httpHeaders == null)
                        {
                            _httpHeaders = _propertyHttpHeaders.SelectMany(i =>
                                i).Distinct().OrderBy(i => i).ToList();
                        }
                    }
                }
                return _httpHeaders;
            }
        }
        private List<string> _httpHeaders;

        /// <summary>
        /// List of all property names for the provider.
        /// </summary>
        public IList<string> PropertyNames
        {
            get 
            {
                return _propertyNames; 
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a new instance of a tree provider.
        /// </summary>
        /// <param name="copyright">The copyright notice for the data file.</param>
        /// <param name="strings">Array containing all strings in the output.</param>
        /// <param name="properties">Array of properties.</param>
        /// <param name="devices">Array of devices.</param>
        /// <param name="lookupList">Lookups data array.</param>
        /// <param name="nodesLength">The length of the node data.</param>
        /// <param name="nodesOffset">The position of the start of the nodes in the file provided.</param>
        /// <param name="pool">Pool connected to the data source.</param>
        internal TrieProvider(string copyright, byte[] strings, byte[] properties, byte[] devices,
            byte[] lookupList, long nodesLength, long nodesOffset, Pool pool)
        {
            Copyright = copyright;
            _strings = strings;
            _properties = properties;
            _devices = devices;
            _lookupList = lookupList;
            _nodesOffset = nodesOffset;
            _pool = pool;
        }
        
        #endregion

        #region Public Methods

        /// <summary>
        /// Returns the user agent matched against the one provided.
        /// </summary>
        /// <param name="userAgent"></param>
        /// <returns></returns>
        public string GetUserAgent(string userAgent)
        {
            var matchedUserAgent = new StringBuilder();
            var reader = _pool.GetReader();
            reader.BaseStream.Position = _nodesOffset;
            GetDeviceIndex(
                reader,
                GetUserAgentByteArray(userAgent),
                0,
                0,
                matchedUserAgent);
            _pool.Release(reader);
            return matchedUserAgent.ToString();
        }

        /// <summary>
        /// Returns the index of the device associated with the given user agent. The
        /// index returned may vary across different versions of the source data file
        /// and should not be stored. The "Id" property will remain unique.
        /// </summary>
        /// <param name="userAgent"></param>
        /// <returns></returns>
        public int GetDeviceIndex(string userAgent)
        {
            int index;
            var reader = _pool.GetReader();
            try
            {
                reader.BaseStream.Position = _nodesOffset;
                index = GetDeviceIndex(
                    reader,
                    GetUserAgentByteArray(userAgent),
                    0,
                    0);
            }
            finally
            {
                _pool.Release(reader);
            }
            return index;
        }

        /// <summary>
        /// Returns a collection of device indexes for each of the relevent
        /// HTTP headers provided. Those headers which are unrelated to device
        /// detection are ignored.
        /// </summary>
        /// <param name="headers">Collection of HTTP headers and values</param>
        /// <returns>Collection of headers and device indexes for each one</returns>
        public IDictionary<string, int> GetDeviceIndexes(NameValueCollection headers)
        {
            IDictionary<string, int> indexes = new SortedDictionary<string, int>();
            if (headers != null)
            {
                var reader = _pool.GetReader();
                try
                {
                    foreach (string header in headers.Keys)
                    {
                        if (HttpHeaders.BinarySearch(header) >= 0)
                        {
                            indexes.Add(header, GetDeviceIndex(headers[header]));
                        }
                    }
                }
                finally
                {
                    _pool.Release(reader);
                }
            }
            return indexes;
        }

        /// <summary>
        /// Returns the device id matching the device index.
        /// </summary>
        /// <param name="deviceIndex">Index of the device whose Id should be returned</param>
        /// <returns></returns>
        public string GetDeviceId(int deviceIndex)
        {
            return GetPropertyValue(deviceIndex, "Id");
        }

        /// <summary>
        /// Returns the property value based on the device indexes provided.
        /// </summary>
        /// <param name="deviceIndexes">Http headers and their device index.</param>
        /// <param name="property">The name of the property required.</param>
        /// <returns>The value of the property for the given device index</returns>
        public string GetPropertyValue(IDictionary<string, int> deviceIndexes, string property)
        {
            return GetPropertyValue(deviceIndexes, _propertyIndex[property]);
        }

        /// <summary>
        /// Returns the property value based on the device index provided.
        /// </summary>
        /// <param name="deviceIndex">The index of the device whose property should be returned.</param>
        /// <param name="property">The name of the property required.</param>
        /// <returns>The value of the property for the given device index</returns>
        public string GetPropertyValue(int deviceIndex, string property)
        {
            return GetPropertyValue(deviceIndex, _propertyIndex[property]);
        }

        /// <summary>
        /// Returns the value of the property index provided from the device indexes provided.
        /// Matches the Http header to the property index.
        /// </summary>
        /// <param name="deviceIndexes">Indexes for the device.</param>
        /// <param name="propertyIndex">Index of the property required.</param>
        /// <returns>The value of the property for the given device indexes</returns>
        public string GetPropertyValue(IDictionary<string, int> deviceIndexes, int propertyIndex)
        {
            int deviceIndex;
            foreach(var header in _propertyHttpHeaders[propertyIndex])
            {
                if (deviceIndexes.TryGetValue(header, out deviceIndex))
                {
                    return GetPropertyValue(deviceIndex, propertyIndex);
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the value of the property index provided for the device index provided.
        /// </summary>
        /// <param name="deviceIndex">Index for the device.</param>
        /// <param name="propertyIndex">Index of the property required.</param>
        /// <returns>The value of the property index for the given device index</returns>
        public string GetPropertyValue(int deviceIndex, int propertyIndex)
        {
            var devicePosition = deviceIndex * _propertyNames.Count * sizeof(int);
            return GetStringValue(
                BitConverter.ToInt32(
                    _devices,
                    devicePosition + (propertyIndex * sizeof(int))));
        }

        /// <summary>
        /// Returns the value of the property for the user agent provided.
        /// </summary>
        /// <param name="userAgent">User agent of the request</param>
        /// <param name="propertyName">Name of the property required</param>
        /// <returns>The value of the property for the given user agent</returns>
        public string GetPropertyValue(string userAgent, string propertyName)
        {
            return GetPropertyValue(GetDeviceIndex(userAgent), propertyName);
        }

        /// <summary>
        /// Returns the value of the property for the user agent provided.
        /// </summary>
        /// <param name="headers">Collection of HTTP headers and values</param>
        /// <param name="propertyName">Name of the property required</param>
        /// <returns>The value of the property for the given user agent</returns>
        public string GetPropertyValue(NameValueCollection headers, string propertyName)
        {
            return GetPropertyValue(GetDeviceIndexes(headers), propertyName);
        }
        
        /// <summary>
        /// Disposes of the pool assigned to the provider.
        /// </summary>
        public void Dispose()
        {
            _pool.Dispose();
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Returns the string at the offset provided.
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        protected string GetStringValue(int offset)
        {
            var index = 0;
            var builder = new StringBuilder();
            var current = _strings[offset];
            while (current != 0)
            {
                builder.Append((char)current);
                index++;
                current = _strings[offset + index];
            }
            return builder.ToString();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Converts a user agent in to a null terminated byte array.
        /// </summary>
        /// <param name="userAgent">The useragent to be tested</param>
        /// <returns>A null terminated byte array</returns>
        private static byte[] GetUserAgentByteArray(string userAgent)
        {
            var result = new byte[userAgent != null ? userAgent.Length + 1 : 0];
            if (result.Length > 0)
            {
                for (int i = 0; i < userAgent.Length; i++)
                {
                    result[i] = userAgent[i] <= 0x7F ? (byte)userAgent[i] : (byte)' ';
                }
                result[result.Length - 1] = 0;
            }
            return result;
        }

        /// <summary>
        /// Returns the offset in the node for the current character.
        /// </summary>
        /// <param name="lookupOffset">The offset in the byte array</param>
        /// <param name="value">The value to be checked</param>
        /// <returns>The position to move to</returns>
        private byte GetChild(int lookupOffset, byte value)
        {
            try
            {
                var lowest = _lookupList[lookupOffset];
                var highest = _lookupList[lookupOffset + 1];
                if (value < lowest ||
                    value > highest)
                    return byte.MaxValue;
                
                 // Statement is equivalent to "(lookupOffset + value - lowest) + 2".
                return _lookupList[lookupOffset + value - lowest + 2];
            }
            catch (IndexOutOfRangeException ex)
            {
                throw new System.Exception("Exception in GetChild", ex);
            }
        }

        /// <summary>
        /// The number of bytes each offset takes.
        /// </summary>
        /// <param name="offsetType"></param>
        /// <returns></returns>
        public static int SizeOfOffsets(OffsetType offsetType)
        {
            switch (offsetType)
            {
                case OffsetType.Bits16:
                    return sizeof(ushort);
                case OffsetType.Bits32:
                    return sizeof(uint);
                default:
                    return sizeof(long);
            }
        }

        /// <summary>
        /// Returns the position in the nodes stream of the next node.
        /// </summary>
        /// <param name="reader">Reader with exclusive access to the underlying file</param>
        /// <param name="childIndex"></param>
        /// <param name="numberOfChildren"></param>
        /// <param name="offsetType"></param>
        /// <returns></returns>
        private void SetNextNodePosition(BinaryReader reader, byte childIndex, byte numberOfChildren, OffsetType offsetType)
        {
            if (childIndex == 0)
            {
                // Move past the children offset to the first child.
                reader.BaseStream.Position += (numberOfChildren - 1) * SizeOfOffsets(offsetType);
            }
            else
            {
                // Move to the bytes that represent the node position of the next node based on the child index.
                reader.BaseStream.Position += (childIndex - 1) * SizeOfOffsets(offsetType);
                switch (offsetType)
                {
                    case OffsetType.Bits16:
                        reader.BaseStream.Position += reader.ReadUInt16();
                        break;
                    case OffsetType.Bits32:
                        reader.BaseStream.Position += reader.ReadUInt32();
                        break;
                    default:
                        reader.BaseStream.Position += reader.ReadInt64();
                        break;
                }
            }
        }
              
        /// <summary>
        /// Returns the offset in the device byte array to the device matching
        /// the useragent provided.
        /// </summary>
        /// <param name="reader">Reader with exclusive access to the underlying file</param>
        /// <param name="userAgent">A null terminated byte array of the user agent to be tested</param>
        /// <param name="index">The index in the array of the current character</param>
        /// <param name="parentDeviceIndex">The device index of the parent node</param>
        /// <returns>The device id with the most number of matching characters</returns>
        private int GetDeviceIndex(BinaryReader reader, byte[] userAgent, int index, int parentDeviceIndex)
        {
            // Get the lookup list.
            var lookupListOffset = reader.ReadInt32();

            // If there are no more characters in the user agent return
            // the parent device index.
            if (index == userAgent.Length)
            {
                return parentDeviceIndex;
            }

            // Get the index of the child.
            var childIndex = GetChild(Math.Abs(lookupListOffset), userAgent[index]);

            // Get the index of the device.
            int deviceIndex;
            if (lookupListOffset >= 0)
            {
                // The lookup list is positive so the device index
                // is contained as the next 4 bytes.
                deviceIndex = reader.ReadInt32();
            }
            else
            {
                // The look list is negative so the device index
                // of this node is the same as the parent device index.
                deviceIndex = parentDeviceIndex;
            }
            
            // If the child index indicates no children then
            // return the current device.
            if (childIndex == byte.MaxValue)
                return deviceIndex;

            // Get the number of children and check we're still within
            // the range for this node.
            var numberOfChildren = reader.ReadByte();
            if (childIndex >= numberOfChildren)
                return deviceIndex;

            // If there's only 1 child then it will appear immediately after
            // this element. The position will already be set at that position.
            if (numberOfChildren == 1)
            {
                return GetDeviceIndex(
                    reader, 
                    userAgent,
                    index + 1,
                    deviceIndex);
            }

            // There's more than 1 child so find the integer type used for the
            // offset and then move to that position recognising the 1st child 
            // always appears at the position immediately after the list of children.
            SetNextNodePosition(reader, childIndex, numberOfChildren, (OffsetType)reader.ReadByte());
            return GetDeviceIndex(
                reader,
                userAgent,
                index + 1,
                deviceIndex);
        }

        /// <summary>
        /// Returns the offset in the device byte array to the device matching
        /// the useragent provided.
        /// </summary>
        /// <param name="reader">Reader with exclusive access to the underlying file</param>
        /// <param name="userAgent">A null terminated byte array of the user agent to be tested</param>
        /// <param name="index">The index in the array of the current character</param>
        /// <param name="parentDeviceIndex">The parent device index to be used if this node doesn't have a different one</param>
        /// <param name="matchedUserAgent">The characters of the user agent matched</param>
        /// <returns>The device id with the most number of matching characters</returns>
        private int GetDeviceIndex(BinaryReader reader, byte[] userAgent, int index, int parentDeviceIndex, StringBuilder matchedUserAgent)
        {
            // Add the character to the matched user agent.
            matchedUserAgent.Append((char)userAgent[index]);

            // Get the lookup list.
            var lookupListOffset = reader.ReadInt32();

            // Get the index of the child.
            var childIndex = GetChild(Math.Abs(lookupListOffset), userAgent[index]);

            // Get the index of the device.
            int deviceIndex;
            if (lookupListOffset >= 0)
                // The lookup list is positive so the device index
                // is contained as the next 4 bytes.
                deviceIndex = reader.ReadInt32();
            else
                // The look list is negative so the device index
                // of this node is the same as the parent device index.
                deviceIndex = parentDeviceIndex;

            // If the child index indicates no children then
            // return the current device.
            if (childIndex == byte.MaxValue)
                return deviceIndex;

            // Get the number of children and check we're still within
            // the range for this node.
            var numberOfChildren = reader.ReadByte();
            if (childIndex >= numberOfChildren)
                return deviceIndex;

            // If there's only 1 child then it will appear immediately after
            // this element. The position will already be set at that position.
            if (numberOfChildren == 1)
            {
                return GetDeviceIndex(
                    reader,
                    userAgent,
                    index + 1,
                    deviceIndex,
                    matchedUserAgent);
            }

            // There's more than 1 child so find the integer type used for the
            // offset and then move to that position recognising the 1st child 
            // always appears at the position immediately after the list of children.
            SetNextNodePosition(reader, childIndex, numberOfChildren, (OffsetType)reader.ReadByte());
            return GetDeviceIndex(
                reader,
                userAgent,
                index + 1,
                deviceIndex,
                matchedUserAgent);
        }

        #endregion
    }
}
