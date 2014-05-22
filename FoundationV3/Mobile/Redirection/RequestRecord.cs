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
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Linq;

namespace FiftyOne.Foundation.Mobile.Redirection
{
    /// <summary>
    /// Class used to convert a HttpRequest into a single long value that is
    /// almost unique for the requesting device. Two devices that share the same
    /// external IP address and HTTP headers will calculate to the same long value
    /// so it may not always be unique. The number of handsets that fall into this
    /// category should be sufficiently small enough for it not to present a 
    /// practical problem.
    /// The date and time the request was received is also recorded to enable 
    /// out of date records to be removed from the list.
    /// </summary>
    internal class RequestRecord : IComparable
    {
        #region Constants

        // Headers used to create a hashcode based on their values when present.
        private static readonly string[] ADDITIONAL_HEADERS = new string[]
                                                                      {
                                                                          // Common headers
                                                                          "Accept-Language",
                                                                          "Host",
                                                                          "Via",
                                                                          "UA", // Another user agent field

                                                                          // Common x headers
                                                                          "x-forwarded-for",
                                                                          // Originating IP of a client connection to the server
                                                                          "x-source-id",
                                                                          // Could be an internal MNO IP address
                                                                          "x-wap-profile",
                                                                          // A reference to the user-agent profile
                                                                          "x-forwarded-host", // Origination host name
                                                                          "x-forwarded-server",
                                                                          // Originating server name

                                                                          // OpenWave gateway headers:
                                                                          "x-up-calling-line-id",
                                                                          // End users phone number

                                                                          // Nokia gateway headers:
                                                                          "x-nokia-alias",
                                                                          //The end users phone number. encrypted.
                                                                          "x-nokia-msisdn",
                                                                          // The users phone number in plain text.
                                                                          "x-nokia-ipaddress", // Internal IP address
                                                                          "x-nokia-imsi", // Imsi value 

                                                                          // Other headers:
                                                                          "x-imsi",
                                                                          // The imsi number. Identifies the end user. 
                                                                          "x-msisdn", // The end users phone number  

                                                                          // AvantGo headers.
                                                                          "x-avantgo-userid"
                                                                          // Identifying the end user.
                                                                      };

        #endregion

        #region Fields

        // The key for this device record.
        private long _key;

        // The date and time the device was last active.
        private long _lastActiveDate;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new empty instance of <see cref="RequestRecord"/> class.
        /// </summary>
        protected internal RequestRecord()
        {
        }

        /// <summary>
        /// Constructs a new instance of <see cref="RequestRecord"/> class.
        /// Copies the values of the <see cref="RequestRecord"/> provided to
        /// the new instance.
        /// </summary>
        /// <param name="recordToCopy"></param>
        protected internal RequestRecord(RequestRecord recordToCopy)
        {
            _key = recordToCopy.Key;
            _lastActiveDate = recordToCopy.LastActiveDate;
        }

        /// <summary>
        /// If the IP address is IPv4 (4 bytes) then use the ip address as the high order
        /// bytes of the value and the hashcode as the low order bytes.
        /// If the IP address is IPv6 (8 bytes) then covert the bytes to a 64 bit
        /// integer. 
        /// If anything else which we can't imagine use a hashcode of the string value.
        /// </summary>
        /// <param name="request"></param>
        protected internal RequestRecord(HttpRequest request)
        {
            byte[] buffer = new byte[8];
            byte[] address = IPAddress.Parse(request.UserHostAddress).GetAddressBytes();

            // If 4 bytes use these as the high order bytes and a hashcode from the
            // HTTP header as the low order bytes.
            if (address.Length == 4)
            {
                for (int i = 0; i < 4; i++)
                    buffer[7 - i] = address[i];
                SetHashCode(buffer, request);
                _key = BitConverter.ToInt64(buffer, 0);
            }

            else if (address.Length == 8)
            {
                // Use the value unaltered as a 64 bit value.
                _key = BitConverter.ToInt64(address, 0);
            }

            else
            {
                // Create hashcode from the address.
                int hashcode = 0;
                foreach (byte current in address)
                    hashcode += current;

                // Merge the address hashcode and the request hashcode.
                byte[] hashcodeArray = BitConverter.GetBytes(hashcode);
                for (int i = 0; i < 4; i++)
                    buffer[4 + i] = hashcodeArray[i];
                SetHashCode(buffer, request);

                _key = BitConverter.ToInt64(buffer, 0);
            }

            // Set the last time this request was seen.
            _lastActiveDate = DateTime.UtcNow.Ticks;
        }

        protected internal RequestRecord(BinaryReader reader)
        {
            _key = reader.ReadInt64();
            _lastActiveDate = reader.ReadInt64();
        }

        #endregion

        #region Properties

        /// <summary>
        /// The unique key for the device.
        /// </summary>
        protected internal long Key
        {
            get { return _key; }
        }

        /// <summary>
        /// The date and device was last seen as active expressed 
        /// as a long value.
        /// </summary>
        protected internal long LastActiveDate
        {
            get { return _lastActiveDate; }
            set { _lastActiveDate = value; }
        }

        /// <summary>
        /// The date and device was last seen as active expressed 
        /// as a DateTime value.
        /// </summary>
        protected internal DateTime LastActiveDateAsDateTime
        {
            get { return new DateTime(_lastActiveDate); }
            set { _lastActiveDate = value.Ticks; }
        }

        #endregion

        #region Methods

        protected internal void Read(BinaryReader reader)
        {
            _key = reader.ReadInt64();
            _lastActiveDate = reader.ReadInt64();
        }

        protected internal void Write(Stream stream)
        {
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(_key);
            writer.Write(_lastActiveDate);
        }

        /// <summary>
        /// Adds the hashcode of the relevent header fields as the low order
        /// bytes of the array.
        /// </summary>
        /// <param name="buffer">Buffer used to set the bytes.</param>
        /// <param name="request">HttpRequest to calculate a hashcode from.</param>
        private static void SetHashCode(byte[] buffer, HttpRequest request)
        {
            StringBuilder headers = new StringBuilder();
            headers.Append(request.UserAgent);

            foreach (string key in ADDITIONAL_HEADERS.Where(key => request.Headers[key] != null))
            {
                headers.Append(key).Append(request.Headers[key]);
            }

            int hashCode = headers.ToString().GetHashCode();
            buffer[0] = (byte)(hashCode);
            buffer[1] = (byte)(hashCode >> 8);
            buffer[2] = (byte)(hashCode >> 16);
            buffer[3] = (byte)(hashCode >> 24);
        }

        #endregion

        #region IComparable Members

        /// <summary>
        /// Compares one request to another to determine if they are the same.
        /// </summary>
        /// <param name="obj">The object being compared.</param>
        /// <returns>If the object contains the same value as this instance.</returns>
        public int CompareTo(object obj)
        {
            RequestRecord candidate = obj as RequestRecord;
            if (candidate != null)
            {
                if (candidate.LastActiveDate < LastActiveDate)
                    return -1;
                if (candidate.LastActiveDate > LastActiveDate)
                    return 1;
                if (candidate.Key < Key)
                    return -1;
                if (candidate.Key > Key)
                    return 1;
                return 0;
            }
            throw new MobileException(
                String.Format(
                    "Can not compare object of type '{0}' with '{1}'.",
                    obj.GetType(),
                    GetType()));
        }

        #endregion
    }
}