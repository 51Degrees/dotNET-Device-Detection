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
 * Mobile Experts Limited are Copyright (C) 2009 - 2010. All Rights Reserved.
 * 
 * Contributor(s):
 *     James Rosewell <james@51degrees.mobi>
 * 
 * ********************************************************************* */

#region

using System;
using System.IO;
using System.Net;
using System.Security;
using System.Threading;
using System.Web;
using FiftyOne.Foundation.Mobile.Detection.Wurfl.Configuration;

#endregion

#if VER4
using System.Linq;
using System.Threading.Tasks;
#endif

namespace FiftyOne.Foundation.Mobile.Detection.Wurfl
{
    internal class WurflNewDevice
    {
        private static readonly Uri _newDevicesUrl;
        private static bool _enabled;

        /// <summary>
        /// Sets the enabled state of the class.
        /// </summary>
        static WurflNewDevice()
        {
            if (Uri.TryCreate(Manager.NewDevicesURL, UriKind.RelativeOrAbsolute, out _newDevicesUrl))
                _enabled = true;
        }

        /// <summary>
        /// Returns if the new device recording functionality is enabled.
        /// </summary>
        protected internal static bool Enabled
        {
            get { return _enabled; }
        }

        /// <summary>
        /// Record the client in the new devices server if specified to avoid
        /// needing to use this method to match the useragent to the device
        /// in the future. Create another thread with low priority to reduce
        /// any impacting of recording this on the active web request.
        /// </summary>
        /// <param name="request">The request used to indentify the new device.</param>
        internal static void RecordNewDevice(HttpRequest request)
        {
            // Get the new device details.
            NewDeviceData data = new NewDeviceData(request);
#if VER4
            if (!data.Ignore)
                Task.Factory.StartNew(() => ProcessNewDevice(data));
#elif VER2
            if (data.Ignore == false)
                ThreadPool.QueueUserWorkItem(ProcessNewDevice, data);
#endif
        }

        private static void ProcessNewDevice(object sender)
        {
            NewDeviceData newDevice = sender as NewDeviceData;
            if (newDevice != null)
            {
                try
                {
                    // Record to a URL if one has been provided and the new devices.
                    if (String.IsNullOrEmpty(Manager.NewDevicesURL) == false &&
                        String.IsNullOrEmpty(newDevice.Content) == false)
                        RecordToURL(newDevice);
                }
                catch (SecurityException)
                {
                    EventLog.Warn(String.Format("Insufficient permission to send new device information to '{0}'.",
                                                _newDevicesUrl));
                    _enabled = false;
                }
                catch (WebException ex)
                {
                    try
                    {
                        if (newDevice.IsLocal == false)
                        {
                            EventLog.Warn(
                                String.Format(
                                    "Could not write device information for useragent '{0}' to URL '{1}'. Exception '{2}'",
                                    newDevice.UserAgent, _newDevicesUrl, ex.Message));
                        }
                        else
                        {
                            EventLog.Debug(
                                String.Format(
                                    "Could not write device information for useragent '{0}' to URL '{1}'. Exception '{2}'",
                                    newDevice.UserAgent, _newDevicesUrl, ex.Message));
                        }
                    }
                    catch
                    {
                        // Do nothing as there is nothing we can do.
                    }
                }
            }
        }

        private static void RecordToURL(NewDeviceData newDevice)
        {
            HttpWebRequest request = WebRequest.Create(_newDevicesUrl) as HttpWebRequest;
            request.Timeout = Constants.NewUrlTimeOut;
            request.Method = "POST";

            StreamWriter writer = new StreamWriter(request.GetRequestStream());
            writer.Write(newDevice.Content);
            writer.Flush();
            writer.Close();

            request.GetResponse();
        }

        #region Nested type: NewDeviceData

        internal class NewDeviceData
        {
            private static readonly NewDeviceDetail _newDeviceDetail;
            private readonly string _content;
            private readonly bool _ignore;
            private readonly bool _isLocal;
            private readonly string _userAgent;

            static NewDeviceData()
            {
                _newDeviceDetail = Manager.NewDeviceDetail;
            }

            internal NewDeviceData(HttpRequest request)
            {
                _content = RequestHelper.GetContent(request,
                                                    _newDeviceDetail == NewDeviceDetail.Maximum);
                _userAgent = Provider.GetUserAgent(request);
                _isLocal = request.IsLocal;

                // If the headers contain 51D as a setting or the request is to a ]
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