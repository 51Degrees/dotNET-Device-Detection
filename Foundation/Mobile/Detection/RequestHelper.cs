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

using System;
using System.Collections.Generic;
using System.Web;
using System.Xml;
using System.Text;
using System.Reflection;
using System.Net;
using System.Net.Sockets;
using System.Security.Permissions;

namespace FiftyOne.Foundation.Mobile.Detection
{
    internal class RequestHelper
    {
        private static readonly IPAddress[] LOCALHOSTS = new IPAddress[] {
            IPAddress.Parse("127.0.0.1"),
            IPAddress.Parse("::1") };

        private static bool IsLocalHost(IPAddress address)
        {
            foreach(IPAddress host in LOCALHOSTS)
            {
                if (host.Equals(address) == true)
                    return true;
            }
            return false;
        }
        
        private static void WriteHostIP(XmlWriter writer)
        {
            IPAddress[] addresses = Dns.GetHostAddresses(Dns.GetHostName());
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
        }

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
                    bool blank = 
                        (key.IndexOf("cookie", 0, 
                            StringComparison.InvariantCultureIgnoreCase) >= 0) ||
                        key == "Referer";
                    // Include all header values if maximumDetail is enabled, or
                    // header values related to the useragent or any header
                    // key containing profile.
                    if (maximumDetail == true ||
                        key == "User-Agent" ||
                        key.Contains("profile") == true ||
                        blank == true)
                    {
                        // Record the header content if it's not a cookie header.
                        if (blank == true)
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
                if (writer != null) { writer.Close(); }
            }
            writer.Close();
            return content.ToString();
        }

        private static void WriteAssembly(XmlWriter writer)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            if (assembly != null)
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(false);
                foreach (object attribute in attributes)
                {
                    if (attribute is AssemblyFileVersionAttribute)
                        writer.WriteElementString("Version", ((AssemblyFileVersionAttribute)attribute).Version.ToString());
                    if (attribute is AssemblyTitleAttribute)
                        writer.WriteElementString("Product", ((AssemblyTitleAttribute)attribute).Title);
                }
            }
        }

        private static void WriteHeader(XmlWriter writer, string key)
        {
            writer.WriteStartElement("Header");
            writer.WriteAttributeString("Name", key);
            writer.WriteEndElement();
        }

        private static void WriteHeaderValue(XmlWriter writer, string key, string value)
        {
            writer.WriteStartElement("Header");
            writer.WriteAttributeString("Name", key);
            writer.WriteCData(value);
            writer.WriteEndElement();
        }

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
    }
}
