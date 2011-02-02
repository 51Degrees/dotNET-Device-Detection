/* *********************************************************************
 * The contents of this file are subject to the Mozilla Public License 
 * Version 1.1 (the "License"); you may not use this file except in 
 * compliance with the License. You may obtain a copy of the License at 
 * http://www.mozilla.org/MPL/
 * 
 * Software distributed under the License is distributed on an "AS IS" 
 * basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 * See the License for the specific language governing rights and 
 * limitations under the License.
 *
 * The Original Code is named .NET Mobile API, first released under 
 * this licence on 11th March 2009.
 * 
 * The Initial Developer of the Original Code is owned by 
 * 51 Degrees Mobile Experts Limited. Portions created by 51 Degrees 
 * Mobile Experts Limited are Copyright (C) 2009 - 2011. All Rights Reserved.
 * 
 * Contributor(s):
 *     James Rosewell <james@51degrees.mobi>
 * 
 * ********************************************************************* */

#region Usings

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;
using FiftyOne.Foundation.Mobile.Configuration;

#endregion

#if VER4
using System.Linq;
#endif

namespace FiftyOne.Foundation.Mobile.Redirection
{
    /// <summary>
    /// Class used to record details of devices that have already accessed the
    /// mobile web site to determine if the request is the 1st one or a 
    /// subsequent one.
    /// </summary>
    internal static class RequestHistory
    {
        #region Private Classes

        #region Nested type: PreviousDevices

        /// <summary>
        /// Contains details of the previous devices held in the request history.
        /// </summary>
        private static class PreviousDevices
        {
            internal static readonly SortedList<long, long> Devices = new SortedList<long, long>();
            internal static RequestRecord lastDevice = new RequestRecord();
        }

        #endregion

        #region Nested type: RequestRecord

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
        private class RequestRecord : IComparable
        {
            #region Constants

            // Headers used to create a hashcode based on their values when present.
            private static readonly string[] ADDITIONAL_HEADERS = new[]
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
            /// If anything else which we can't image use a hashcode of the string value.
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
                    foreach(byte current in address)
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
#if VER4
                foreach (string key in ADDITIONAL_HEADERS.Where(key => request.Headers[key] != null))
                {
                    headers.Append(key).Append(request.Headers[key]);
                }
#elif VER2
                foreach (string key in ADDITIONAL_HEADERS)
                {
                    if (request.Headers[key] != null)
                        headers.Append(key).Append(request.Headers[key]);
                }
#endif
                int hashCode = headers.ToString().GetHashCode();
                buffer[0] = (byte) (hashCode);
                buffer[1] = (byte) (hashCode >> 8);
                buffer[2] = (byte) (hashCode >> 16);
                buffer[3] = (byte) (hashCode >> 24);
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

        #endregion

        #endregion

        #region Constants

        // The length of each request history record.
        private const int RECORD_LENGTH = 16;

        // Number of ms to wait to open a file stream.
        private const int TIMEOUT = 10000;

        // The number of minutes to wait between trimming the
        // request history file.

        #endregion

        #region Fields

        // Stores the path for the devices synchronisation file.
        private static readonly string _syncFilePath;

        // The next time this process should service the sync file.
        private static DateTime _nextServiceTime = DateTime.MinValue;

        // The last time the sync file was modified.
        private static DateTime _lastWriteTime = DateTime.MinValue;
        
        /// <summary>
        /// The number of minutes that should elapse before the record of 
        /// previous access for the device should be removed from all
        /// possible storage mechanisims.
        /// </summary>
        private static readonly int _redirectTimeout = 0;

        #endregion

        #region Static Constructor

        static RequestHistory()
        {
            if (Manager.Redirect.Enabled)
            {
                // Get the timeout used to remove devices.
                _redirectTimeout = Manager.Redirect.Timeout;

                // Get the request history file and set to null it
                // it's empty.
                _syncFilePath = Support.GetFilePath(Manager.Redirect.DevicesFile);
                if (_syncFilePath == String.Empty)
                    _syncFilePath = null;

                // Process the syncfile.
                if (_syncFilePath != null)
                    ProcessSyncFile();
            }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Checks to find out if the device associated with the HttpRequest
        /// has already been seen by the application. Always returns false
        /// if the sync file has not been specified.
        /// </summary>
        /// <param name="request">HttpRequest to be checked.</param>
        /// <returns>True if the device associated with the request has been seen.</returns>
        internal static bool IsPresent(HttpRequest request)
        {
            if (_syncFilePath != null)
            {
                RequestRecord record = new RequestRecord(request);

                // Check to see if new request data needs to be loaded.
                RefreshSyncFile();

                long expiryDateTime;
                if (PreviousDevices.Devices.TryGetValue(record.Key, out expiryDateTime))
                {
                    // If redirect timeout is zero then simply check to see if the
                    // device is present in the list of previous devices.
                    if (_redirectTimeout == 0)
                        return true;

                    // Is it still valid?
                    return (new DateTime(expiryDateTime).AddMinutes(_redirectTimeout)).Ticks >=
                        record.LastActiveDate;
                }
            }
            return false;
        }

        /// <summary>
        /// Adds this device request to the previous devices list.
        /// </summary>
        /// <param name="request">HttpRequest of the device.</param>
        internal static void Add(HttpRequest request)
        {
            if (_syncFilePath != null)
            {
                RequestRecord record = new RequestRecord(request);

                // Get the latest data.
                RefreshSyncFile();

                // Add this most recent request to the sync file.
                Add(record);

                // Check if the sync file needs to be serviced.
                CheckIfServiceRequired();
            }
        }

        /// <summary>
        /// Removes this device request from the previous devices list.
        /// </summary>
        /// <param name="request">HttpRequest of the device.</param>
        internal static void Remove(HttpRequest request)
        {
            if (_syncFilePath != null)
            {
                RequestRecord record = new RequestRecord(request);

                // Get the latest data.
                RefreshSyncFile();

                // Does the device exist in the previous devices list?
                if (PreviousDevices.Devices.ContainsKey(record.Key))
                {
                    // Set the last active date to zero so that it will be 
                    // removed when the sync file is serviced.
                    record.LastActiveDate = 0;

                    // Add this most recent request to the sync file.
                    Add(record);
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Checks to determine if new records need to be processed and if so
        /// loads them into memory.
        /// </summary>
        private static void RefreshSyncFile()
        {
            FileInfo info = new FileInfo(_syncFilePath);
            if (info != null && info.LastWriteTimeUtc > _lastWriteTime)
            {
                ProcessSyncFile();
                _lastWriteTime = info.LastAccessTimeUtc;
            }
        }

        /// <summary>
        /// Adds the current request record to the file containing details of
        /// all the available requests.
        /// </summary>
        /// <param name="record">Record of the request to be added.</param>
        private static void Add(object record)
        {
            if (record is RequestRecord)
            {
                using (FileStream stream = OpenSyncFilePath(FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                {
                    if (stream != null)
                    {
                        ((RequestRecord) record).Write(stream);
                        stream.Flush();
                        stream.Close();
                    }
                }
            }
        }

        private static void ProcessSyncFile()
        {
            if (File.Exists(_syncFilePath))
            {
                bool repeatProcess = false;

                // Lock the list of devices we're about to update to ensure they can't be
                // changed by subsequent requests to this callback.
                lock (PreviousDevices.Devices)
                {
                    // Open the sync file for read access ensuring it's disposed 
                    // as soon as possible.
                    using (FileStream stream = OpenSyncFilePath(FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        if (stream != null)
                        {
                            // Record the length of the file so that if it changes we can abandon this
                            // update and rely on a subsequent call to this methd to complete 
                            // processing.
                            long length = stream.Length;
                            BinaryReader reader = new BinaryReader(stream);
                            RequestRecord record = new RequestRecord();
                            RequestRecord firstDevice = null;
                            long rows = length/RECORD_LENGTH;
                            for (long row = 0; row < rows; row++)
                            {
                                // Read the current row in reverse order. Capture EOF exceptions
                                // in case the file length has changed since we started processing.
                                try
                                {
                                    stream.Position = ((rows - row)*RECORD_LENGTH) - RECORD_LENGTH;
                                    record.Read(reader);
                                }
                                catch (EndOfStreamException ex)
                                {
                                    // The file has been trimmed by another process. Break and
                                    // allow the resulting call to this event to complete
                                    // processing.
                                    EventLog.Debug(ex);
                                    break;
                                }

                                // If the current record is the same as the last one we got last time
                                // this method was called then stop processing more records.
                                if (record.CompareTo(PreviousDevices.lastDevice) == 0)
                                    break;

                                // Update the memory version.
                                if (record.LastActiveDate == 0)
                                {
                                    // Remove from the device as the last active date is zero.
                                    PreviousDevices.Devices.Remove(record.Key);
                                }
                                else
                                {
                                    // Update or insert a new record.
                                    if (PreviousDevices.Devices.ContainsKey(record.Key))
                                        PreviousDevices.Devices[record.Key] = record.LastActiveDate;
                                    else
                                        PreviousDevices.Devices.Add(record.Key, record.LastActiveDate);
                                }

                                if (firstDevice == null)
                                    firstDevice = new RequestRecord(record);
                            }
                            // If the length of the file hasn't changed during the processing
                            // then update the last device record to limit the number of rows
                            // examined in future file changes.
                            if (length == stream.Length && firstDevice != null)
                                PreviousDevices.lastDevice = firstDevice;

                            // Signal to all the method again if the length of the file
                            // has changed during processing.
                            repeatProcess = length != stream.Length;

                            reader.Close();
                            stream.Close();
                        }
                    }
                }
                // If the file was altered during the processing then call the method
                // again to capture any new records.
                if (repeatProcess)
                    ProcessSyncFile();
            }
        }

        /// <summary>
        /// Opens the file for read and if an exception is thrown will return null rather
        /// than the exception.
        /// </summary>
        /// <returns></returns>
        private static FileStream OpenSyncFilePath(FileMode mode, FileAccess access, FileShare share)
        {
            FileStream stream = null;
            Random rnd = null;
            if (_syncFilePath != null)
            {
                DateTime timeout = DateTime.UtcNow.AddMilliseconds(TIMEOUT);
                {
                    try
                    {
                        stream = File.Open(_syncFilePath, mode, access, share);
                    }
                    catch (IOException ex)
                    {
                        EventLog.Info(ex);
                        stream = null;
                        if (rnd == null)
                            rnd = new Random(_lastWriteTime.GetHashCode());
                        Thread.Sleep(rnd.Next(5));
                    }
                }
                while (stream == null && DateTime.UtcNow < timeout);
                if (stream == null)
                    throw new MobileException(
                        String.Format(
                            "Could not open request history file '{0}' in mode '{1}', with access '{2}' and share '{3}'.",
                            _syncFilePath, mode, access, share));
            }
            return stream;
        }

        /// <summary>
        /// If the last time the devices file was serviced to remove old entries
        /// is older than 1 minute start a thread to service the devices file and 
        /// remove old entries. If the redirect timeout is 0 indicating infinite
        /// then nothing should be purged.
        /// </summary>
        private static void CheckIfServiceRequired()
        {
            if (_nextServiceTime < DateTime.UtcNow)
            {
                long purgeDate;

                // If the last device has no active date use the current time for the purge date.
                if (PreviousDevices.lastDevice.LastActiveDate == DateTime.MinValue.Ticks)
                    purgeDate = DateTime.UtcNow.Ticks - (TimeSpan.TicksPerMinute*_redirectTimeout);
                    // Otherwise use the last active devices active date for the purge date.
                else
                    purgeDate = PreviousDevices.lastDevice.LastActiveDate -
                                (TimeSpan.TicksPerMinute*_redirectTimeout);

                ThreadPool.QueueUserWorkItem(
                    ServiceRequestHistory,
                    purgeDate);
            }
        }

        /// <summary>
        /// Removes entries from the memory version and sync file that
        /// are older than the purgeDate specified.
        /// </summary>
        /// <param name="purgeDate">
        /// Date as a long used to determine if a request history 
        /// record is old and can be removed.
        /// </param>
        private static void ServiceRequestHistory(object purgeDate)
        {
            using (FileStream stream = OpenSyncFilePath(FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                if (stream != null)
                {
                    // Trim the sync file if it needs trimming and it has not
                    // been changed since the service routine started.
                    long originalLength = stream.Length;
                    byte[] buffer = ReadRecords(stream, (long) purgeDate);
                    if (buffer != null && stream.Length == originalLength)
                    {
                        stream.Position = 0;
                        stream.Write(buffer, 0, buffer.Length);
                        stream.Flush();
                        stream.SetLength(buffer.Length);
                        stream.Flush();
                        EventLog.Info(String.Format("Trimmed request history file '{0}' by removing {1} bytes.",
                                                    _syncFilePath, originalLength - buffer.Length));
                    }
                    stream.Close();
                }
            }

            // Remove old records from the memory version.
            lock (PreviousDevices.Devices)
            {
                int index = 0;
                while (index < PreviousDevices.Devices.Count)
                {
                    if (PreviousDevices.Devices.Values[index] <= (long)purgeDate)
                        PreviousDevices.Devices.RemoveAt(index);
                    else
                        index++;
                }
            }

            // Set the next time to service the sync file using a random offset to 
            // attempt to avoid conflicts with other processes.
            _nextServiceTime = DateTime.UtcNow.AddMinutes(1).AddSeconds(new Random().Next(30));
         }

        /// <summary>
        /// Read the records that should be retained in the sync file.
        /// </summary>
        /// <param name="stream">Stream for the sync file.</param>
        /// <param name="purgeDate">Date before which records should be removed.</param>
        /// <returns></returns>
        private static byte[] ReadRecords(FileStream stream, long purgeDate)
        {
            byte[] buffer = null;
            long offset = 0;
            BinaryReader reader = new BinaryReader(stream);
            RequestRecord record = new RequestRecord();
            stream.Position = 0;
            for (offset = 0; offset < stream.Length; offset += RECORD_LENGTH)
            {
                record.Read(reader);
                // Check to see if the current record is newer than the purgeDate
                // and isn't equal to zero. Zero date indicates the record should be
                // removed from the history.
                if (record.LastActiveDate > purgeDate && record.LastActiveDate != 0)
                    break;
            }

            if (offset > 0 && offset < stream.Length)
            {
                int length = (int) (stream.Length - offset);
                buffer = new byte[length];
                stream.Position = offset;
                stream.Read(buffer, 0, length);
            }

            return buffer;
        }

        #endregion
    }
}