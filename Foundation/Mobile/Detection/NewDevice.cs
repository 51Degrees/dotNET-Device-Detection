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
using System.IO;
using System.Net;
using System.Security;
using System.Web;
using System.Net.Cache;

#endregion

namespace FiftyOne.Foundation.Mobile.Detection
{
    /// <summary>
    /// Class used to record new device details.
    /// </summary>
    internal class NewDevice
    {
        #region Fields

        private readonly Uri _newDevicesUrl;
        private readonly NewDeviceDetails _newDeviceDetail;
        private bool _enabled;

        #endregion

        #region Constructor

        /// <summary>
        /// Sets the enabled state of the class.
        /// </summary>
        internal NewDevice(string newDevicesUrl, NewDeviceDetails newDeviceDetail)
        {
            if (Uri.TryCreate(newDevicesUrl, UriKind.RelativeOrAbsolute, out _newDevicesUrl))
                _enabled = true;
            _newDeviceDetail = newDeviceDetail;
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
        /// Record the client in the new devices server if specified to avoid
        /// needing to use this method to match the useragent to the device
        /// in the future. Create another thread with low priority to reduce
        /// any impacting of recording this on the active web request.
        /// </summary>
        /// <param name="request">The request used to indentify the new device.</param>
        internal void RecordNewDevice(HttpRequest request)
        {
            // Get the new device details.
            NewDeviceData data = new NewDeviceData(request, _newDeviceDetail);

            // Process the new data details.
            if (!data.Ignore)
                try { Start(data); }
                catch (Exception ex) { HandleException(ex); }
        }

        private void Start(NewDeviceData data)
        {
            try
            {
                HttpWebRequest request = WebRequest.Create(_newDevicesUrl) as HttpWebRequest;
                request.ReadWriteTimeout = Constants.NewUrlTimeOut;
                request.Method = "POST";
                request.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
                request.BeginGetRequestStream(
                    new AsyncCallback(GetRequestStreamCallback),
                    new object[] { request, data.Content });
            }
            catch (Exception ex) { HandleException(ex); }
        }

        private void GetRequestStreamCallback(IAsyncResult asynchronousResult)
        {
            try
            {
                var state = (object[])asynchronousResult.AsyncState;
                var request = (HttpWebRequest)state[0];
                var content = (string)state[1];

                Stream postStream = request.EndGetRequestStream(asynchronousResult);
                using (var writer = new StreamWriter(postStream))
                    writer.Write(content);

                request.BeginGetResponse(new AsyncCallback(GetResponseCallback), request);
            }
            catch (Exception ex) { HandleException(ex); }
        }

        private void GetResponseCallback(IAsyncResult asynchronousResult)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)asynchronousResult.AsyncState;

                // End the operation
                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(asynchronousResult);
                using (var reader = new StreamReader(response.GetResponseStream()))
                    EventLog.Debug(reader.ReadToEnd());

                // Release the HttpWebResponse
                response.Close();
            }
            catch (Exception ex) { HandleException(ex); }
        }

        private void HandleException(Exception ex)
        {
            if (ex is SecurityException)
            {
                EventLog.Debug(String.Format("Insufficient permission to send new device information to '{0}'.",
                                            _newDevicesUrl));
                _enabled = false;
            }
            else if (ex is WebException)
            {
                try
                {
                    EventLog.Debug(
                        String.Format(
                            "Could not write device information to URL '{0}'. Exception '{1}'",
                            _newDevicesUrl, ex.Message));
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

        #endregion

        #region Nested type: NewDeviceData

        internal class NewDeviceData
        {
            private readonly string _content;
            private readonly bool _ignore;
            private readonly bool _isLocal;
            private readonly string _userAgent;

            internal NewDeviceData(HttpRequest request, NewDeviceDetails newDeviceDetail)
            {
                _content = RequestHelper.GetContent(request,
                                                    newDeviceDetail == NewDeviceDetails.Maximum);
                _userAgent = request.UserAgent;
                _isLocal = request.IsLocal;

                // If the headers contain 51D as a setting or the request is to a
                // web service then do not send the data.
                _ignore = request.Headers["51D"] != null ||
                    request.Url.Segments[request.Url.Segments.Length - 1].EndsWith("asmx");
            }

            /// <summary>
            /// XML content containing details about the client device.
            /// </summary>
            internal string Content
            {
                get { return _content; }
            }

            /// <summary>
            /// Returns the useragent string of the requesting device.
            /// </summary>
            internal string UserAgent
            {
                get { return _userAgent; }
            }

            /// <summary>
            /// Returns true if the content relates to a local client.
            /// </summary>
            internal bool IsLocal
            {
                get { return _isLocal; }
            }

            /// <summary>
            /// Returns true if the device should be ignored based on 
            /// settings in the request header.
            /// </summary>
            internal bool Ignore
            {
                get { return _ignore; }
            }
        }

        #endregion
    }
}