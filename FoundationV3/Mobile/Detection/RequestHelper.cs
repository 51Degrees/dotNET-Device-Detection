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

#region Usings

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Web;
using System.Xml;
using System.Linq;

#endregion

namespace FiftyOne.Foundation.Mobile.Detection
{
    internal static class RequestHelper
    {
        #region Constants

        /// <summary>
        /// IP Addresses of local host device.
        /// </summary>
        private static readonly IPAddress[] LOCALHOSTS = new IPAddress[]
                                                             {
                                                                 IPAddress.Parse("127.0.0.1"),
                                                                 IPAddress.Parse("::1")
                                                             };

        /// <summary>
        /// The content of fields in this array should not be included in the request information
        /// information sent to 51degrees.
        /// </summary>
        private static readonly string[] IgnoreHeaderFieldValues = new string[] { 
            "Referer", 
            "cookie", 
            "AspFilterSessionId",
            "Akamai-Origin-Hop",
            "Cache-Control",
            "Cneonction",
            "Connection",
            "Content-Filter-Helper",
            "Content-Length",
            "Cookie",
            "Cookie2",
            "Date",
            "Etag",
            "If-Last-Modified",
            "If-Match",
            "If-Modified-Since",
            "If-None-Match",
            "If-Range",
            "If-Unmodified-Since",
            "IMof-dified-Since",
            "INof-ne-Match",
            "Keep-Alive",
            "Max-Forwards",
            "mmd5",
            "nnCoection",
            "Origin",
            "ORIGINAL-REQUEST",
            "Original-Url",
            "Pragma",
            "Proxy-Connection",
            "Range",
            "Referrer",
            "Script-Url",
            "Unless-Modified-Since",
            "URL",
            "UrlID",
            "URLSCAN-ORIGINAL-URL",
            "UVISS-Referer",
            "X-ARR-LOG-ID",
            "X-Cachebuster",
            "X-Discard",
            "X-dotDefender-first-line",
            "X-DRUTT-REQUEST-ID",
            "X-Initial-Url",
            "X-Original-URL",
            "X-PageView",
            "X-REQUEST-URI",
            "X-REWRITE-URL",
            "x-tag",
            "x-up-subno",
            "X-Varnish" };

        #endregion

        #region Internal Static Methods

        /// <summary>
        /// Provides the content as XML for the HttpRequest.
        /// </summary>
        /// <param name="request">HttpRequest containing the required information.</param>
        /// <param name="maximumDetail">The amount of detail to provided.</param>
        /// <returns>byte array in XML string form of request content.</returns>
        internal static byte[] GetContent(HttpRequest request, bool maximumDetail)
        {
            return GetContent(request, maximumDetail, false);
        }

        /// <summary>
        /// Provides the content as XML for the HttpRequest.
        /// </summary>
        /// <param name="request">HttpRequest containing the required information.</param>
        /// <param name="maximumDetail">The amount of detail to provided.</param>
        /// <param name="fragment">True if only a fragment should be returned, otherwise false for a document.</param>
        /// <returns>byte array in XML string form of request content.</returns>
        internal static byte[] GetContent(HttpRequest request, bool maximumDetail, bool fragment)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (XmlWriter writer = XmlWriter.Create(ms, GetXmlSettings(fragment)))
                {
                    writer.WriteStartElement("Device");
                    writer.WriteElementString("DateSent", DateTime.UtcNow.ToString("s"));

                    // Record details about the assembly for diagnosis purposes.
                    WriteAssembly(writer);

                    // Record information about the active provider.
                    if (WebProvider._activeProviderCreated)
                    {
                        WriteProvider(writer, WebProvider.ActiveProvider);
                    }

                    // Record either the IP address of the client if not local or the IP
                    // address of the machine.
                    if (request.IsLocal == false ||
                        IsLocalHost(IPAddress.Parse(request.UserHostAddress)) == false)
                    {
                        writer.WriteElementString("ClientIP", request.UserHostAddress);
                    }
                    else
                    {
                        WriteHostIP(writer, "ClientIP");
                    }

                    WriteHostIP(writer, "ServerIP");

                    foreach (string key in request.Headers.AllKeys)
                    {
                        // Determine if the field should be treated as a blank.
                        bool blank = IsBlankField(key);

                        // Include all header values if maximumDetail is enabled, or
                        // header values related to the useragent or any header
                        // key containing profile or information helpful to determining
                        // mobile devices.
                        if (maximumDetail ||
                            key == "User-Agent" ||
                            key == "Host" ||
                            key.Contains("profile") ||
                            blank)
                        {
                            // Record the header content if it's not a cookie header.
                            if (blank)
                                WriteHeader(writer, key);
                            else
                                WriteHeaderValue(writer, key, request.Headers[key]);
                        }
                    }
                    writer.WriteEndElement();
                    writer.Flush();
                }
                return ms.ToArray();
            }
        }

        #endregion

        #region Private Static Methods

        /// <summary>
        /// Returns true if the request is from the local host IP address.
        /// </summary>
        /// <param name="address">The IP address to be checked.</param>
        /// <returns>True if from the local host IP address.</returns>
        private static bool IsLocalHost(IPAddress address)
        {
            return LOCALHOSTS.Any(host => host.Equals(address));
        }

        /// <summary>
        /// Writes details about the host IP address.
        /// </summary>
        /// <param name="writer">The XML writer to be written to.</param>
        /// <param name="elementName">The name of the XML to write to.</param>
        private static void WriteHostIP(XmlWriter writer, string elementName)
        {
            IPAddress[] addresses = Dns.GetHostAddresses(Dns.GetHostName());

            foreach (IPAddress address in
                addresses.Where(address => !IsLocalHost(address) && address.AddressFamily == AddressFamily.InterNetwork))
            {
                writer.WriteElementString(elementName, address.ToString());
                return;
            }

            foreach (IPAddress address in addresses.Where(address => !IsLocalHost(address)))
            {
                writer.WriteElementString(elementName, address.ToString());
                return;
            }
        }

        /// <summary>
        /// Returns true if the field provided is one that should not have it's contents
        /// sent to 51degrees for consideration a device matching piece of information.
        /// </summary>
        /// <param name="field">The name of the Http header field.</param>
        /// <returns>True if the field should be passed as blank.</returns>
        private static bool IsBlankField(string field)
        {
            foreach (string key in IgnoreHeaderFieldValues)
            {
                if (field.IndexOf(key, 0, StringComparison.InvariantCultureIgnoreCase) >= 0)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Writes details about the assembly to the output stream.
        /// </summary>
        /// <param name="writer"></param>
        private static void WriteAssembly(XmlWriter writer)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            if (assembly != null)
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(false);
                foreach (object attribute in attributes)
                {
                    if (attribute is AssemblyFileVersionAttribute)
                        writer.WriteElementString("Version", ((AssemblyFileVersionAttribute) attribute).Version);
                    if (attribute is AssemblyTitleAttribute)
                        writer.WriteElementString("Product", ((AssemblyTitleAttribute) attribute).Title);
                }
            }
        }

        /// <summary>
        /// Writes information about the data set being used by the provider.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="provider"></param>
        private static void WriteProvider(XmlWriter writer, WebProvider provider)
        {
            if (provider != null)
            {
                writer.WriteStartElement("DataSet");
                writer.WriteElementString("Version", provider.DataSet.Version.ToString());
                writer.WriteElementString("Name", provider.DataSet.Name);
                writer.WriteEndElement();
            }
        }

        /// <summary>
        /// Writer an XML header element with no value content.
        /// </summary>
        /// <param name="writer">Writer for the output stream.</param>
        /// <param name="key">Name of the header.</param>
        private static void WriteHeader(XmlWriter writer, string key)
        {
            writer.WriteStartElement("Header");
            writer.WriteAttributeString("Name", key);
            writer.WriteEndElement();
        }

        /// <summary>
        /// Writes an XML header element with a value using CData.
        /// </summary>
        /// <param name="writer">Writer for the output stream.</param>
        /// <param name="key">Name of the header.</param>
        /// <param name="value">Value of the header.</param>
        private static void WriteHeaderValue(XmlWriter writer, string key, string value)
        {
            writer.WriteStartElement("Header");
            writer.WriteAttributeString("Name", key);
            writer.WriteCData(value);
            writer.WriteEndElement();
        }

        /// <summary>
        /// Provides the xml writer settings.
        /// </summary>
        /// <returns></returns>
        private static XmlWriterSettings GetXmlSettings(bool fragment)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = fragment;
            settings.ConformanceLevel = fragment ? ConformanceLevel.Fragment : ConformanceLevel.Document;
            settings.Encoding = Encoding.UTF8;
            settings.CheckCharacters = true;
            settings.NewLineHandling = NewLineHandling.None;
            settings.CloseOutput = true;
            return settings;
        }

        #endregion
    }
}