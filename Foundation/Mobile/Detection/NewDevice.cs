/* *********************************************************************
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
    internal class NewDevice : IDisposable
    {
        #region Fields

        // Can be set to false if an exception occurs.
        private static bool _enabled = true;

        // Used to stop the thread.
        private bool _stop = false;

        private readonly Uri _newDevicesUrl;
        private readonly NewDeviceDetails _newDeviceDetail;
        private readonly Queue<byte[]> _queue;
        private readonly Thread _thread;
        private readonly AutoResetEvent _wait;
        
        #endregion

        #region Constructor

        /// <summary>
        /// Sets the enabled state of the class.
        /// </summary>
        internal NewDevice(string newDevicesUrl, NewDeviceDetails newDeviceDetail)
        {
#if DEBUG
            EventLog.Debug("Constructing NewDevice Instance");
#endif
            if (_enabled &&
                Configuration.Manager.ShareUsage &&
                Uri.TryCreate(newDevicesUrl, UriKind.RelativeOrAbsolute, out _newDevicesUrl))
            {
                _newDeviceDetail = newDeviceDetail;
                _queue = new Queue<byte[]>();
                _wait = new AutoResetEvent(false);
                _thread = new Thread(Run);
                _thread.Priority = ThreadPriority.Lowest;
                _thread.IsBackground = true;
                _thread.Start();
            }
            else
            {
#if DEBUG
                EventLog.Debug("Disabling NewDevice Instance");
#endif
                _enabled = false;
            }
        }

        #endregion

        #region Deconstructor

        /// <summary>
        /// Called when the object is deconstructed.
        /// </summary>
        ~NewDevice()
        {
            Dispose();
        }

        /// <summary>
        /// Disposes of the queue and ensures everything is shutdown.
        /// </summary>
        public void Dispose()
        {
            if (_thread != null && _thread.ThreadState != ThreadState.Stopped)
            {
#if DEBUG
                EventLog.Debug("Disposing NewDevice Instance");
#endif
                _stop = true;
                _wait.Set();
                _thread.Join();
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Returns if the new device recording functionality is enabled.
        /// </summary>
        internal bool Enabled
        {
            get { return _enabled; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds the request details to the queue for processing
        /// by the background thread.
        /// </summary>
        /// <param name="request">The request used to indentify the new device.</param>
        internal void RecordNewDevice(HttpRequest request)
        {
            // Get the new device details.
            byte[] data = GetContent(request, _newDeviceDetail);

            if (data != null && data.Length > 0)
            {
                // Add the new details to the queue for later processing.
                _queue.Enqueue(data);

                // Signal the background thread to check to see if it should
                // send queued data.
                _wait.Set();
            }
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
        private void SendData(Stream stream)
        {
            using (GZipStream compressed = new GZipStream(stream, CompressionMode.Compress))
            {
                using (XmlWriter writer = XmlWriter.Create(compressed, GetXmlSettings()))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("Devices");
                    while (_queue.Count > 0)
                    {
                        byte[] item = _queue.Dequeue();
                        if (item != null && item.Length > 0)
                            writer.WriteRaw(
                                ASCIIEncoding.UTF8.GetString(
                                    item));
                    }
                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }
            }
        }

        private void Run(object state)
        {
            do
            {
                try
                {
                    // Wait for something to happen.
                    _wait.WaitOne(-1);

                    // If there are enough items in the queue, or the thread is being
                    // stopped send the data.
                    if (_queue.Count >= Constants.NewDeviceQueueLength ||
                        (_stop == true && _queue.Count > 0))
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
                        SendData(request.GetRequestStream());

                        // Get the response and record the content if it's valid. If it's
                        // not valid consider turning off the functionality.
                        HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                        if (response != null)
                        {
                            switch (response.StatusCode)
                            {
                                case HttpStatusCode.OK:
                                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                                    {
                                        string content = reader.ReadToEnd();
#if DEBUG
                                        EventLog.Debug(content);
#endif
                                    }
                                    break;
                                case HttpStatusCode.RequestTimeout:
                                    // Could be temporary, do nothing.
                                    break;
                                default:
                                    // Turn off functionality.
                                    EventLog.Debug(
                                        String.Format(
                                            "Stopping usage sharing as remote name '{0}' returned status description '{1}'.",
                                            _newDevicesUrl, response.StatusDescription));
                                    _enabled = false;
                                    _stop = true;
                                    break;
                            }
                        }

                        // Release the HttpWebResponse
                        response.Close();
                    }
                }
                catch (Exception ex) { HandleException(ex); }
            } while (_stop == false);
#if DEBUG
            EventLog.Debug("Finished NewDevice Thread");
#endif
        }

        private void HandleException(Exception ex)
        {
            if (ex is SecurityException)
            {
                EventLog.Debug(
                    String.Format(
                        "Stopping usage sharing as insufficient permission to send device information to '{0}'.",
                         _newDevicesUrl));
                _enabled = false;
                _stop = true;
            }
            else if (ex is WebException)
            {
                try
                {
                    if (ex.Message.StartsWith("The remote name could not be resolved:"))
                    {
                        EventLog.Debug(
                            String.Format(
                                "Stopping usage sharing as remote name '{0}' could not be resolved.",
                                _newDevicesUrl));
                        _enabled = false;
                        _stop = true;
                    }
                    if (ex.Message.StartsWith("The remote server returned an error:"))
                    {
                        EventLog.Debug(
                            String.Format(
                                "Stopping usage sharing as call to '{0}' returned exception '{1}'.",
                                _newDevicesUrl, ex.Message));
                        _enabled = false;
                        _stop = true;
                    }
                    else
                    {
                        EventLog.Debug(
                            String.Format(
                                "Could not write device information to URL '{0}'. Exception '{1}'",
                                _newDevicesUrl, ex.Message));
                    }
                }
                catch
                {
                    // Do nothing as there is nothing we can do.
                }
            }
            else
            {
                EventLog.Debug(ex);
            }
        }

        /// <summary>
        /// Returns a byte array containing the content of the request.
        /// </summary>
        /// <param name="request">The current request.</param>
        /// <param name="newDeviceDetail">The level of detail to include.</param>
        private static byte[] GetContent(HttpRequest request, NewDeviceDetails newDeviceDetail)
        {
            // If the headers contain 51D as a setting or the request is to a
            // web service then do not send the data.
            bool ignore = request.Headers["51D"] != null ||
                request.Url.Segments[request.Url.Segments.Length - 1].EndsWith("asmx");

            if (ignore == false)
                return RequestHelper.GetContent(
                    request,
                    newDeviceDetail == NewDeviceDetails.Maximum,
                    true);

            return null;
        }

        #endregion
    }
}