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