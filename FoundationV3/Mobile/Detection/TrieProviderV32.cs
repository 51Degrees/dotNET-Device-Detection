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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.IO;
using FiftyOne.Foundation.Mobile.Detection.Entities.Stream;

namespace FiftyOne.Foundation.Mobile.Detection
{
    /// <summary>
    /// Decision trie data structure provider.
    /// </summary>
    public class TrieProviderV32 : TrieProvider
    {
        /// <summary>
        /// Length of each property index.
        /// </summary>
        private const int PROPERTY_LENGTH = sizeof(int) * 3;

        #region Constructor

        /// <summary>
        /// Constructs a new instance of a trie provider.
        /// </summary>
        /// <param name="copyright">The copyright notice for the data file.</param>
        /// <param name="strings">Array containing all strings in the output.</param>
        /// <param name="httpHeaders">Array of http headers.</param>
        /// <param name="properties">Array of properties.</param>
        /// <param name="devices">Array of devices.</param>
        /// <param name="lookupList">Lookups data array.</param>
        /// <param name="nodesLength">The length of the node data.</param>
        /// <param name="nodesOffset">The position of the start of the nodes in the file provided.</param>
        /// <param name="pool">Pool connected to the data source.</param>
        internal TrieProviderV32(string copyright, byte[] strings, byte[] httpHeaders, byte[] properties, byte[] devices,
            byte[] lookupList, long nodesLength, long nodesOffset, Pool pool)
            : base (copyright, strings, properties, devices, lookupList, nodesLength, nodesOffset, pool)
        {
            for (int i = 0; i < _properties.Length / PROPERTY_LENGTH; i++)
            {
                var value = GetStringValue(BitConverter.ToInt32(_properties, i * PROPERTY_LENGTH));
                var headerCount = BitConverter.ToInt32(_properties, (i * PROPERTY_LENGTH) + sizeof(int));
                var headerFirstIndex = BitConverter.ToInt32(_properties, (i * PROPERTY_LENGTH) + (sizeof(int) * 2));
                _propertyIndex.Add(value, i);
                _propertyNames.Add(value);
                _propertyHttpHeaders.Add(GetHeaders(httpHeaders, headerCount, headerFirstIndex));
            }
        }
        
        #endregion

        #region Private Methods

        private string[] GetHeaders(byte[] httpHeaders, int headerCount, int headerFirstIndex)
        {
            var headers = new List<string>();
            for(int i = 0; i < headerCount; i++)
            {
                headers.Add(GetStringValue(
                    BitConverter.ToInt32(
                        httpHeaders, 
                        (headerFirstIndex + i) * sizeof(int))));
            }
            return headers.ToArray();
        }

        #endregion
    }
}
