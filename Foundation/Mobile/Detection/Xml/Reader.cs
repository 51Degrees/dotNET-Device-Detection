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
 *     Andy Allan <andy.allan@mobgets.com>
 * 
 * ********************************************************************* */

#if VER4 
using System.Linq;
#endif

#region Usings

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.IO.Compression;
using System.Xml;
using System.Xml.Schema;

#endregion

namespace FiftyOne.Foundation.Mobile.Detection.Xml
{
    /// <summary>
    /// Constains all methods needed to process the xmls file.
    /// </summary>
    public static class Reader
    {
        #region Static Public Methods

        /// <summary>
        /// Creates a new provider and adds the data from the streams to the provider.
        /// </summary>
        /// <param name="streams">Array of stream readers.</param>
        /// <returns></returns>
        public static Provider Create(IList<Stream> streams)
        {
            var provider = new Provider();
            Add(provider, streams);
            return provider;
        }

        /// <summary>
        /// Creates a new provider and adds the xml files specified to the provider.
        /// </summary>
        /// <param name="files">List of files to be processed.</param>
        public static Provider Create(IList<string> files)
        {
            var provider = new Provider();
            Add(provider, files);
            return provider;
        }

        /// <summary>
        /// Adds the xml files provided to the provider.
        /// </summary>
        /// <param name="provider">The provider to be modified.</param>
        /// <param name="files">List of files to be processed.</param>
        public static void Add(Provider provider, IList<string> files)
        {
            var streams = new List<Stream>();
            try
            {
                foreach (string file in files)
                {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.Exists)
                    {
                        if (Detection.Constants.CompressedFileExtensions.Contains(fileInfo.Extension))
                        {
                            // This is a compressed file. It needs to be copied to a memory stream
                            // to enable multiple passed of the data.
                            var ms = new MemoryStream();
                            var temp = new GZipStream(
                                new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read),
                                CompressionMode.Decompress);
                            
                            // Copy the data to the memory stream.
                            int value = temp.ReadByte();
                            while (value > 0)
                            {
                                ms.WriteByte((byte)value);
                                value = temp.ReadByte();
                            }

                            streams.Add(ms);
                        }
                        else
                            streams.Add(new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read));
                    }
                }
                Add(provider, streams);
            }
            finally
            {
                foreach (var stream in streams)
                    stream.Dispose();
            }
        }

        /// <summary>
        /// Adds the data from the streams to the provider.
        /// </summary>
        /// <param name="streams">Array of stream readers.</param>
        /// <param name="provider">The provider to be modified.</param>
        /// <returns></returns>
        public static void Add(Provider provider, IList<Stream> streams)
        {
            try
            {
                // Read the streams provided looking for handlers.
                foreach (var stream in streams)
                {
                    if (stream.CanSeek == false)
                        throw new XmlException(String.Format("Stream must support seek operations."));

                    // Ensure we're at the start of the stream before reading.
                    stream.Position = 0;
                    using (XmlReader reader = XmlReader.Create(stream, GetXmlReaderSettings()))
                        provider.Handlers.AddRange(HandlersReader.ProcessHandlers(reader, provider));
                }

                // Parse the data for devices.
                var availableCapabilities = new StringCollection();
                foreach (var stream in streams)
                {
                    // Ensure we're at the start of the stream before reading.
                    stream.Position = 0;
                    using (XmlReader reader = XmlReader.Create(stream, GetXmlReaderSettings()))
                        ProcessDevices(availableCapabilities, provider, reader);
                }
            } 
            catch (System.Xml.XmlException ex)
            {
                throw new XmlException(
                    String.Format("XML exception creating provider."),
                    ex);
            }
            catch (IOException ex)
            {
                throw new XmlException(
                    String.Format("IO exception creating provider."),
                    ex);
            }
            catch (Exception ex)
            {
                throw new XmlException(
                    String.Format("Exception creating provider."),
                    ex);
            }
        }

        /// <summary>
        /// Returns XML settings that allows the xml streams to be processed without errors.
        /// </summary>
        /// <returns>XMLReaderSettings</returns>
        private static XmlReaderSettings GetXmlReaderSettings()
        {
            XmlReaderSettings settings = new XmlReaderSettings();

            settings.IgnoreComments = true;
            settings.IgnoreWhitespace = true;
            settings.ConformanceLevel = ConformanceLevel.Document;
            settings.ValidationType = ValidationType.Schema;
            settings.ValidationFlags = XmlSchemaValidationFlags.ProcessSchemaLocation;

            return settings;
        }

        #endregion

        #region Device Methods
               
        /// <summary>
        /// Uses the xml reader to load data into the provider.
        /// </summary>
        /// <param name="availableCapabilities">List of capabilities that are available.</param>
        /// <param name="provider">Provider to have data loaded into.</param>
        /// <param name="reader">XmlReader for the source data stream.</param>
        private static void ProcessDevices(StringCollection availableCapabilities, Provider provider, XmlReader reader)
        {
            BaseDeviceInfo device = null;

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {
                        case Constants.ProfileElementName:
                            // Load Device Data
                            if (reader.IsStartElement())
                            {
                                // If a device has already been created ensure it's saved.
                                if (device != null)
                                    provider.Set(device);
                                // Create or get the device related to the current XML element.
                                device = LoadDevice(provider, reader);
                            }
                            break;

                        case Constants.PropertyElementName:
                            // Load the device capability.
                            if (reader.IsStartElement())
                            {
                                LoadCapabilityData(
                                    reader,
                                    device,
                                    availableCapabilities);
                            }
                            break;
                    }
                }
            }

            // If a device has not been written ensure it's added to the device dataset.
            if (device != null)
                provider.Set(device);
        }

        /// <summary>
        /// Processes the XML element containing the device attributes.
        /// </summary>
        /// <param name="provider">A list of loaded devices.</param>
        /// <param name="reader">The XML stream readers.</param>
        /// <returns>An empty device.</returns>
        private static BaseDeviceInfo LoadDevice(Provider provider, XmlReader reader)
        {
            // Create the next device using the fallback device if available.
            string deviceId = reader.GetAttribute(Constants.IdAttributeName, string.Empty);
            string userAgent = reader.GetAttribute(Constants.UserAgentAttributeName, string.Empty);
            string fallbackDeviceId = reader.GetAttribute(Constants.ParentAttributeName, string.Empty);

            // If the device already exists then use the previous one. This may happen
            // when an earlier device referenced a fallback device that had not yet
            // been created.
            var device = provider.GetDeviceInfoFromID(deviceId);
            if (device == null)
            {
                // Create the new device.
                device = new BaseDeviceInfo(provider, deviceId, userAgent ?? String.Empty);
            }
            else if (userAgent != null)
            {
                // Ensure the correct UserAgent string is assigned to this device.
                device.InternalUserAgent = userAgent;
                provider.Set(device);
            }

            // Check the fallback device is different to the device being loaded.
            if (fallbackDeviceId != null && device.DeviceId != fallbackDeviceId)
            {
                // Does the fallback device already exist?
                device.Parent = provider.GetDeviceInfoFromID(fallbackDeviceId);
                if (device.Parent == null)
                {
                    // No. So create new fallback device.
                    device.Parent = new BaseDeviceInfo(provider, fallbackDeviceId);
                    // Add it to the available devices.
                    provider.Set(device.Parent);
                }
            }
            return device;
        }

        private static void LoadCapabilityData(
            XmlReader reader,
            BaseDeviceInfo device,
            StringCollection availableCapabilities)
        {
            string capabilityName = reader.GetAttribute(Constants.NameAttributeName, string.Empty);
            string capabilityValue = reader.GetAttribute(Constants.ValueAttributeName, string.Empty);

            // Store all the capabilities names, that's used to make sure
            // all devices havae all capabilities associated to it.
            if (availableCapabilities.Contains(capabilityName) == false)
                availableCapabilities.Add(capabilityName);

            // Ensure the capability is set to the current value.
            device.Properties.Set(capabilityName, capabilityValue.Split(
                new string[] { Detection.Constants.ValueSeperator }, 
                StringSplitOptions.RemoveEmptyEntries));
        }

        #endregion
    }
}