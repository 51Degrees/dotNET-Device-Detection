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

#region Usings

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Cache;
using System.Security;
using System.Text;
using System.Threading;
using System.Web;
using System.Xml;

#endregion

namespace FiftyOne.Foundation.Mobile.Detection
{
    /// <summary>
    /// Class used to record new device details.
    /// </summary>
    /// <remarks>
    /// A queue shared by all instances of Modules is populated with
    /// details of requests. When the queue is full, or a Module instance
    /// is disposed of, then the threadpool will be used to send the 
    /// contents of the queue to 51Degrees via an HTTP web request. If
    /// the request fails then the process of recording new device request
    /// details will be stopped until the worker process restarts.
    /// </remarks>
    internal static class NewDevice
    {
        #region Fields

        // Can be set to false if an exception occurs.
        private static bool _enabled = false;

        // The number of times a timeout occured.
        private static int _timeoutCount = 0;

        // The URL of the url used to recieve new device information.
        private static readonly Uri _newDevicesUrl;

        // The active queue of requests ready to be sent on batch. Replaced
        // by a new queue instance when the active queue is full or a Module
        // is disposed of forcing the queue to be emptied.
        private static Queue<byte[]> _queue;

        // Used to handle multi thread access to the queue.
        private static object _lock = new object();

        // True if a thread is currently running on a previous queue.
        private static bool _sending = false;

        #endregion

        #region Constructor

        /// <summary>
        /// Sets the enabled state of the class.
        /// </summary>
        static NewDevice()
        {
            if (Configuration.Manager.ShareUsage &&
                Uri.TryCreate(Detection.Constants.NewDevicesUrl, UriKind.RelativeOrAbsolute, out _newDevicesUrl))
            {
                // Creates the initial instance of the queue if a valid URL
                // is present and share usage is enabled.
                _queue = new Queue<byte[]>();
                _enabled = true;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Returns true if the new device recording functionality is enabled.
        /// </summary>
        internal static bool Enabled
        {
            get { return _enabled; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds the request details to the queue for processing
        /// by the background thread if the background thread
        /// isn't currently sending previously stored data.
        /// </summary>
        /// <param name="request">The request used to indentify the new device.</param>
        internal static void RecordNewDevice(HttpRequest request)
        {
            // Get the new device details in a form that can be sent.
            byte[] data = GetContent(request);

            if (data != null && data.Length > 0)
            {
                // Use a lock object which is different to the _queue as we switch over
                // the instance of the queue pointed to by _queue during the lock.
                lock (_lock)
                {
                    // Add the data to the queue and check to see if the data should be sent.
                    _queue.Enqueue(data);

                    // If the active queue has sufficient items on it and a previous queue
                    // is not still being sent then use the threadpool to send the contents 
                    // of the current queue. Create a new queue to record any new device 
                    // information from new requests.
                    if (_queue.Count >= Constants.NewDeviceQueueLength &&
                        _sending == false)
                    {
                        ThreadPool.QueueUserWorkItem(Run, Interlocked.Exchange(ref _queue, new Queue<byte[]>()));
                    }
                }
            }
        }

        /// <summary>
        /// Used to send the existing queue and create a new one for new requests.
        /// </summary>
        internal static void Send()
        {
            Run(Interlocked.Exchange(ref _queue, new Queue<byte[]>()));
        }

        /// <summary>
        /// Provides the xml writer settings.
        /// </summary>
        /// <returns></returns>
        private static XmlWriterSettings GetXmlSettings()
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = false;
            settings.ConformanceLevel = ConformanceLevel.Document;
            settings.Encoding = Encoding.UTF8;
            settings.CheckCharacters = true;
            settings.NewLineHandling = NewLineHandling.None;
            settings.CloseOutput = true;
            return settings;
        }

        /// <summary>
        /// Sends all the data on the queue.
        /// </summary>
        private static void SendData(Stream stream, Queue<byte[]> queue)
        {
            using (XmlWriter writer = XmlWriter.Create(new GZipStream(stream, CompressionMode.Compress), GetXmlSettings()))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("Devices");
                while (queue.Count > 0)
                {
                    byte[] item = queue.Dequeue();
                    if (item != null && item.Length > 0)
                    {
                        writer.WriteRaw(
                            ASCIIEncoding.UTF8.GetString(
                                item));
                    }
                }
                writer.WriteEndElement();
                    
            }
        }

        /// <summary>
        /// Sends the content of the queue object passed into the state parameter.
        /// </summary>
        /// <param name="state">The queue to be sent</param>
        private static void Run(object state)
        {
            // Indicate that a thread is running sending the contents of a queue
            // and that another thread should not be started.
            _sending = true;

            // Get the queue to be sent.
            var queue = (Queue<byte[]>)state;
            try
            {
                // Check that the queue isn't empty. This can happen if a signle Module didn't
                // record any information before it was disposed of. This check prevents empty
                // requests being sent.
                if (queue.Count > 0)
                {
#if DEBUG
                    EventLog.Debug("Sending NewDevice Queue");
#endif
                    // Prepare the request including all currently queued items.
                    HttpWebRequest request = WebRequest.Create(_newDevicesUrl) as HttpWebRequest;
                    request.ReadWriteTimeout = Constants.NewUrlTimeOut;
                    request.Method = "POST";
                    request.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
                    request.ContentType = "text/xml";
                    request.Headers.Add(HttpRequestHeader.ContentEncoding, "gzip");

                    // Send the queue content by pushing the data into the request stream.
                    SendData(request.GetRequestStream(), queue);

                    // Get the response and record the content if it's valid. If it's
                    // not valid consider turning off the functionality.
                    HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                    if (response != null)
                    {
                        switch (response.StatusCode)
                        {
                            case HttpStatusCode.OK:
#if DEBUG
                                // Only in debug mode record the response in the log file. This is
                                // only happening in debug build as the log file could fill up rapidly
                                // in a production environment.
                                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                                {
                                    string content = reader.ReadToEnd();
                                    EventLog.Debug(content);
                                }
#endif

                                // Reset the timeout counter as we've not discovered a
                                // timeout is no longer happening.
                                _timeoutCount = 0;
                                break;
                            case HttpStatusCode.RequestTimeout:
                                HandleTimeout();
                                break;
                            default:
                                // Turn off functionality as an unhandled status code
                                // was returned.
                                EventLog.Debug(
                                    String.Format(
                                        "Stopping usage sharing as remote name '{0}' returned " +
                                        "status description '{1}'.",
                                        _newDevicesUrl, response.StatusDescription));
                                _enabled = false;
                                break;
                        }
                    }

                    // Release the HttpWebResponse
                    response.Close();
                }
            }
            catch (Exception ex) 
            {
                // Possibly disable the functionality depending on the exception thrown.
                HandleException(ex); 
            }
            finally 
            { 
                // Set sending to false to enable another thread to be started to process 
                // the currently active queue once it's full.
                _sending = false; 
            }
#if DEBUG
            EventLog.Debug("Finished NewDevice Thread");
#endif
        }

        /// <summary>
        /// Used to handle the recording and management of a timeout exception.
        /// </summary>
        private static void HandleTimeout()
        {
            // Could be temporary, increase the count.
            _timeoutCount++;

            // If more than X timeouts have occured disable the
            // feature.
            if (_timeoutCount > Constants.NewUrlMaxTimeouts)
            {
                EventLog.Debug(
                    String.Format(
                        "Stopping usage sharing as remote name '{0}' " +
                        "timeout count exceeded '{1}'.",
                        _newDevicesUrl,
                        Constants.NewUrlMaxTimeouts));
                _enabled = false;
            }
        }

        /// <summary>
        /// Handles an exception generate in the Run method. Could disbale functionality
        /// depending on the nature of the exception thrown.
        /// </summary>
        /// <param name="ex"></param>
        private static void HandleException(Exception ex)
        {
            if (ex is SecurityException)
            {
                // The worker process can't make HTTP requests so disable the functionality. We
                // can't know this until we try and make an HTTP request.
                EventLog.Debug(
                    String.Format(
                        "Stopping usage sharing as insufficient permission to send device information to '{0}'.",
                         _newDevicesUrl));
                _enabled = false;
            }
            else if (ex is WebException)
            {
                try
                {
                    if (ex.Message.StartsWith("The remote name could not be resolved:"))
                    {
                        // The URL to send information to can't be resolved so there's no way
                        // to send information. Disable the feature.
                        EventLog.Debug(
                            String.Format(
                                "Stopping usage sharing as remote name '{0}' could not be resolved.",
                                _newDevicesUrl));
                        _enabled = false;
                    }
                    else if (ex.Message.StartsWith("The remote server returned an error:"))
                    {
                        // The remote service is returning an error state so disable the feature.
                        EventLog.Debug(
                            String.Format(
                                "Stopping usage sharing as call to '{0}' returned exception '{1}'.",
                                _newDevicesUrl, ex.Message));
                        _enabled = false;
                    }
                    else if (ex.Message.IndexOf("timeout", StringComparison.InvariantCultureIgnoreCase) >= 0)
                    {
                        // A timeout occured which was not reported without throwing an exception. Handle
                        // the timeout.
                        HandleTimeout();
                    }
                    else
                    {
                        // Another unhandled error occured so just disable the feature.
                        EventLog.Debug(
                            String.Format(
                                "Could not write device information to URL '{0}'. Exception '{1}'",
                                _newDevicesUrl, ex.Message));
                        _enabled = false;
                    }
                }
                catch
                {
                    // Do nothing as there is nothing we can do.
                    _enabled = false;
                }
            }
            else
            {
                // Another type of exception was thrown so just disable
                // the feature.
                EventLog.Debug(ex);
                _enabled = false;
            }
        }

        /// <summary>
        /// Returns a byte array containing relavent details of the HTTP request.
        /// </summary>
        /// <param name="request">The current HTTP request</param>
        private static byte[] GetContent(HttpRequest request)
        {
            // If the headers contain 51D as a setting or the request is to a
            // web service then do not send the data.
            bool ignore = request.Headers["51D"] != null ||
                request.Url.Segments[request.Url.Segments.Length - 1].EndsWith("asmx");

            if (ignore == false)
            {
                return RequestHelper.GetContent(
                    request,
                    Detection.Constants.NewDeviceDetail == NewDeviceDetails.Maximum,
                    true);
            }

            return null;
        }

        #endregion
    }
}