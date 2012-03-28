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

                    // Ensure we're at the start of the stream before reading starts.
                    stream.Position = 0;
                    using (XmlReader reader = XmlReader.Create(stream, GetXmlReaderSettings()))
                    {
                        if (reader.ReadToDescendant(Constants.HandlersElementName) ||
                            (reader.ReadToDescendant(Constants.TopLevelElementName) &&
                            reader.ReadToDescendant(Constants.HandlersElementName)))
                            provider.Handlers.AddRange(
                                HandlersReader.ProcessHandlers(
                                    reader.ReadSubtree(), 
                                    provider));
                    }
                }

                // Parse the data for devices.
                foreach (var stream in streams)
                {
                    // Ensure we're at the start of the stream before reading.
                    stream.Position = 0;
                    using (XmlReader reader = XmlReader.Create(stream, GetXmlReaderSettings()))
                    {
                        if (reader.ReadToDescendant(Constants.TopLevelElementName) &&
                            reader.ReadToDescendant(Constants.ProfilesElementName))
                            ProcessDevices(provider, reader.ReadSubtree());
                    }
                }

                // Read the manifest properties from the file.
                foreach (var stream in streams)
                {
                    // Ensure we're at the start of the stream before reading.
                    stream.Position = 0;
                    using (XmlReader reader = XmlReader.Create(stream, GetXmlReaderSettings()))
                    {
                        if (reader.ReadToDescendant(Constants.TopLevelElementName) &&
                            reader.ReadToDescendant(Constants.PropertiesElementName))
                            ProcessManifest(provider, reader.ReadSubtree());
                    }
                }

                // Finally read the date the files were created.
                foreach (var stream in streams)
                {
                    // Ensure we're at the start of the stream before reading.
                    stream.Position = 0;
                    using (XmlReader reader = XmlReader.Create(stream, GetXmlReaderSettings()))
                    {
                        if (reader.ReadToDescendant(Constants.TopLevelElementName) &&
                            reader.ReadToDescendant(Constants.HeaderElementName))
                            if (ProcessHeaders(provider, reader))
                                break;
                    }
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

        #region Manifest Methods

        /// <summary>
        /// Processes the properties manifest section of the xml.
        /// </summary>
        /// <param name="provider">Provider to have data loaded into.</param>
        /// <param name="reader">XmlReader for the source data stream.</param>
        private static void ProcessManifest(Provider provider, XmlReader reader)
        {
            Property property = null;
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {
                        case Constants.PropertyElementName:
                            if (property != null)
                                provider.Properties.Add(property);
                            property = new Property(
                                provider,
                                reader.GetAttribute(Constants.NameAttributeName),
                                reader.GetAttribute(Constants.DescriptionAttributeName),
                                reader.GetAttribute(Constants.UrlAttributeName),
                                bool.Parse(reader.GetAttribute(Constants.MandatoryAttributeName)),
                                bool.Parse(reader.GetAttribute(Constants.ListAttributeName)),
                                bool.Parse(reader.GetAttribute(Constants.ShowValuesAttributeName)));
                            break;
                        case Constants.ValueElementName:
                            property.Values.Add(new Value(
                                property,
                                reader.GetAttribute(Constants.NameAttributeName),
                                reader.GetAttribute(Constants.DescriptionAttributeName),
                                reader.GetAttribute(Constants.UrlAttributeName)));
                            break;
                    }
                }
            }
            provider.Properties.Add(property);
        }

        #endregion

        #region Header Methods

        /// <summary>
        /// Processes the xml header section.
        /// </summary>
        /// <param name="provider">Provider to have data loaded into.</param>
        /// <param name="reader">XmlReader for the source data stream.</param>
        /// <returns>Returns true if the headers elements are processed correctly.</returns>
        private static bool ProcessHeaders(Provider provider, XmlReader reader)
        {
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {
                        case Constants.PublishedDateAttributeName:
                            string date = reader.ReadElementContentAsString();
                            DateTime.TryParse(date, out provider._publishedDate);
                            return true;
                    }
                }
            }
            return false;
        }

        #endregion

        #region Device Methods

        /// <summary>
        /// Uses the xml reader to load data into the provider.
        /// </summary>
        /// <param name="provider">Provider to have data loaded into.</param>
        /// <param name="reader">XmlReader for the source data stream.</param>
        private static void ProcessDevices(Provider provider, XmlReader reader)
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
                                    device);
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
            var device = provider.GetDeviceInfoByID(deviceId);
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
                device.Parent = provider.GetDeviceInfoByID(fallbackDeviceId);
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
            BaseDeviceInfo device)
        {
            string capabilityName = reader.GetAttribute(Constants.NameAttributeName, string.Empty);
            string capabilityValue = reader.GetAttribute(Constants.ValueAttributeName, string.Empty);

            // Ensure the capability is set to the current value.
            device.Properties.Set(capabilityName, capabilityValue.Split(
                new string[] { Detection.Constants.ValueSeperator }, 
                StringSplitOptions.RemoveEmptyEntries));
        }

        #endregion
    }
}