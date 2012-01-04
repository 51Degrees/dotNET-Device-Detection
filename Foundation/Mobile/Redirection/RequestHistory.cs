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
 * Mobile Experts Limited are Copyright (C) 2009 - 2012. All Rights Reserved.
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
    internal class RequestHistory : IRequestHistory
    {
        #region Private Classes

        #region Nested type: PreviousRequests

        /// <summary>
        /// Contains details of the previous devices held in the request history.
        /// </summary>
        private class PreviousRequests
        {
            internal readonly SortedList<long, long> Requests = new SortedList<long, long>();
            internal RequestRecord lastRequest = new RequestRecord();
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
        private readonly string _syncFilePath;

        // Record the file exists to avoid a costly call to the file system.
        private bool _syncFileExists = false;

        // The next time this process should service the sync file.
        private DateTime _nextServiceTime = DateTime.MinValue;

        // The last time the sync file was modified.
        private DateTime _lastWriteTime = DateTime.MinValue;
        
        /// <summary>
        /// The number of minutes that should elapse before the record of 
        /// previous access for the device should be removed from all
        /// possible storage mechanisims.
        /// </summary>
        private readonly int _redirectTimeout = 0;

        /// <summary>
        /// Details about previous requests held in memory.
        /// </summary>
        private readonly PreviousRequests _previous = new PreviousRequests();

        #endregion

        #region Constructor

        internal RequestHistory()
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

        #endregion

        #region Internal Methods

        /// <summary>
        /// Checks to find out if the device associated with the HttpRequest
        /// has already been seen by the application. Always returns false
        /// if the sync file has not been specified.
        /// </summary>
        /// <param name="request">HttpRequest to be checked.</param>
        /// <returns>True if the device associated with the request has been seen.</returns>
        public bool IsPresent(HttpRequest request)
        {
            if (_syncFilePath != null)
            {
                RequestRecord record = new RequestRecord(request);

                // Check to see if new request data needs to be loaded.
                RefreshSyncFile();

                long expiryDateTime;
                if (_previous.Requests.TryGetValue(record.Key, out expiryDateTime))
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
        public void Set(HttpRequest request)
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
        public void Remove(HttpRequest request)
        {
            if (_syncFilePath != null)
            {
                RequestRecord record = new RequestRecord(request);

                // Get the latest data.
                RefreshSyncFile();

                // Does the device exist in the previous devices list?
                if (_previous.Requests.ContainsKey(record.Key))
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
        private void RefreshSyncFile()
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
        /// <param name="record">Record of the request to be set.</param>
        private void Add(object record)
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

        private void ProcessSyncFile()
        {
            // Record if the sync file exists to avoid repeated calls
            // to the operating system.
            if (_syncFileExists == false)
                _syncFileExists = File.Exists(_syncFilePath);

            if (_syncFileExists)
            {
                // Used to indicate if the process should be repeated.
                bool repeatProcess = false;

                // Lock the list of devices we're about to update to ensure they can't be
                // changed by subsequent requests to this callback.
                lock (_previous.Requests)
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
                                if (record.CompareTo(_previous.lastRequest) == 0)
                                    break;

                                // Update the memory version.
                                if (record.LastActiveDate == 0)
                                {
                                    // Remove from the device as the last active date is zero.
                                    _previous.Requests.Remove(record.Key);
                                }
                                else
                                {
                                    // Update or insert a new record.
                                    if (_previous.Requests.ContainsKey(record.Key))
                                        _previous.Requests[record.Key] = record.LastActiveDate;
                                    else
                                        _previous.Requests.Add(record.Key, record.LastActiveDate);
                                }

                                if (firstDevice == null)
                                    firstDevice = new RequestRecord(record);
                            }
                            // If the length of the file hasn't changed during the processing
                            // then update the last device record to limit the number of rows
                            // examined in future file changes.
                            if (length == stream.Length && firstDevice != null)
                                _previous.lastRequest = firstDevice;

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
        private FileStream OpenSyncFilePath(FileMode mode, FileAccess access, FileShare share)
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
        private void CheckIfServiceRequired()
        {
            if (_nextServiceTime < DateTime.UtcNow)
            {
                long purgeDate;

                // If the last device has no active date use the current time for the purge date.
                if (_previous.lastRequest.LastActiveDate == DateTime.MinValue.Ticks)
                    purgeDate = DateTime.UtcNow.Ticks - (TimeSpan.TicksPerMinute*_redirectTimeout);
                    // Otherwise use the last active devices active date for the purge date.
                else
                    purgeDate = _previous.lastRequest.LastActiveDate -
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
        private void ServiceRequestHistory(object purgeDate)
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
            lock (_previous.Requests)
            {
                int index = 0;
                while (index < _previous.Requests.Count)
                {
                    if (_previous.Requests.Values[index] <= (long)purgeDate)
                        _previous.Requests.RemoveAt(index);
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