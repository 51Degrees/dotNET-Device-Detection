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

#if AZURE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace FiftyOne.Foundation.Mobile.Redirection.Azure
{
    /// <summary>
    /// A sub class which breaks the unique key of the request record into a partition
    /// key and a row key.
    /// </summary>
    internal class RequestRecord : Redirection.RequestRecord
    {
        /// <summary>
        /// If the IP address is IPv4 (4 bytes) then use the ip address as the high order
        /// bytes of the value and the hashcode as the low order bytes.
        /// If the IP address is IPv6 (8 bytes) then covert the bytes to a 64 bit
        /// integer. 
        /// If anything else which we can't imagine use a hashcode of the string value.
        /// </summary>
        /// <param name="request"></param>
        protected internal RequestRecord(HttpRequest request)
            : base (request)
        {
        }

        /// <summary>
        /// Returns the first byte as a string.
        /// </summary>
        internal string PartitionKey
        {
            get { return BitConverter.GetBytes(base.Key)[0].ToString(); }
        }

        /// <summary>
        /// Returns the full 8 bytes as a string.
        /// </summary>
        internal string RowKey
        {
            get { return Key.ToString(); }
        }
    }
}

#endif