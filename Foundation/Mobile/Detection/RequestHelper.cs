/* *********************************************************************
 * The contents of this file are subject to the Mozilla internal License 
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
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Web;
using System.Xml;

#endregion

#if VER4
using System.Linq;
#endif

namespace FiftyOne.Foundation.Mobile.Detection
{
    internal static class RequestHelper
    {
        #region Constants

        /// <summary>
        /// IP Addresses of local host device.
        /// </summary>
        private static readonly IPAddress[] LOCALHOSTS = new[]
                                                             {
                                                                 IPAddress.Parse("127.0.0.1"),
                                                                 IPAddress.Parse("::1")
                                                             };

        /// <summary>
        /// The content of fields in this array should not be included in the request information
        /// information sent to 51degrees.
        /// </summary>
        private static readonly string[] IgnoreHeaderFieldValues = new string[] { "Referer", "cookie", "AspFilterSessionId" };

        #endregion

        #region Internal Static Methods

        /// <summary>
        /// Provides the content as XML for the http request.
        /// </summary>
        /// <param name="request">HttpRequest containing the required information.</param>
        /// <param name="maximumDetail">The amount of detail to provided.</param>
        /// <returns>XML string of request content.</returns>
        internal static string GetContent(HttpRequest request, bool maximumDetail)
        {
            StringBuilder content = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(content, GetXmlSettings());
            try
            {
                writer.WriteStartElement("Device");
                writer.WriteElementString("DateSent", DateTime.UtcNow.ToString("s"));

                // Record details about the assembly for diagnosis purposes.
                WriteAssembly(writer);

                // Record either the IP address of the client if not local or the IP
                // address of the machine.
                if (request.IsLocal == false ||
                    IsLocalHost(IPAddress.Parse(request.UserHostAddress)) == false)
                {
                    writer.WriteElementString("ClientIP", request.UserHostAddress);
                }
                else
                {
                    WriteHostIP(writer);
                }

                foreach (string key in request.Headers.AllKeys)
                {
                    // Determine if the field should be treated as a blank.
                    bool blank = IsBlankField(key);

                    // Include all header values if maximumDetail is enabled, or
                    // header values related to the useragent or any header
                    // key containing profile.
                    if (maximumDetail ||
                        key == "User-Agent" ||
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
            finally
            {
                if (writer != null)
                {
                    writer.Close();
                }
            }
            writer.Close();
            return content.ToString();
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
#if VER4
            return LOCALHOSTS.Any(host => host.Equals(address));
#elif VER2
            foreach (IPAddress host in LOCALHOSTS)
            {
                if (host.Equals(address))
                    return true;
            }
            return false;
#endif
        }

        /// <summary>
        /// Writes details about the host IP address.
        /// </summary>
        /// <param name="writer"></param>
        private static void WriteHostIP(XmlWriter writer)
        {
            IPAddress[] addresses = Dns.GetHostAddresses(Dns.GetHostName());
#if VER4
            foreach (IPAddress address in
                addresses.Where(address => !IsLocalHost(address) && address.AddressFamily == AddressFamily.InterNetwork))
            {
                writer.WriteElementString("ClientIP", address.ToString());
                return;
            }

            foreach (IPAddress address in addresses.Where(address => !IsLocalHost(address)))
            {
                writer.WriteElementString("ClientIP", address.ToString());
                return;
            }
#elif VER2
            foreach (IPAddress address in addresses)
            {
                if (IsLocalHost(address) == false && address.AddressFamily == AddressFamily.InterNetwork)
                {
                    writer.WriteElementString("ClientIP", address.ToString());
                    return;
                }
            }
            foreach (IPAddress address in addresses)
            {
                if (IsLocalHost(address) == false)
                {
                    writer.WriteElementString("ClientIP", address.ToString());
                    return;
                }
            }
#endif
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

        #endregion
    }
}