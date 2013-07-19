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
using System.Xml;
using System.Xml.Schema;

#if VER4

using System.Linq;

#endif

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
            Provider provider = new Provider();
            Add(provider, streams);
            return provider;
        }

        /// <summary>
        /// Creates a new provider and adds the xml files specified to the provider.
        /// </summary>
        /// <param name="files">List of files to be processed.</param>
        public static Provider Create(IList<string> files)
        {
            Provider provider = new Provider();
            Add(provider, files);
            return provider;
        }

        /// <summary>
        /// Adds the xml files provided to the provider. If the file is compressed
        /// it will be uncompressed before being processed because the reader needs
        /// to be able to seek within the file which is not possible with compressed
        /// file access.
        /// </summary>
        /// <param name="provider">The provider to be modified.</param>
        /// <param name="files">List of files to be processed.</param>
        public static void Add(Provider provider, IList<string> files)
        {
            List<Stream> streams = new List<Stream>();
            try
            {
                foreach (string file in files)
                {
                    FileInfo fileInfo = new FileInfo(file);
                    if (fileInfo.Exists)
                    {
                        if (Detection.Constants.CompressedFileExtensions.Contains(fileInfo.Extension))
                        {
                            // This is a compressed file. It needs to be copied to a memory stream
                            // to enable multiple passes of the data.

                            // Create a 1mb buffer to hold data before writing to the memory stream.
                            byte[] buffer = new byte[0x100000];
                            
                            // Create a memory stream and read the data from the compressed file.
                            MemoryStream ms = new MemoryStream();
                            using (GZipStream gz = new GZipStream(
                                new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read),
                                CompressionMode.Decompress))
                                ReadStream(gz, buffer, ms);

                            // Add the memory stream to the list of streams for later processing.
                            streams.Add(ms);
                        }
                        else
                            // A raw file stream supports seeking so does not need to be copied to 
                            // a memory stream.
                            streams.Add(new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read));
                    }
                }
                Add(provider, streams);
            }
            finally
            {
                foreach (Stream stream in streams)
                    stream.Dispose();
            }

            // A lot of data has just been moved around. Force garbage collection including large objects.
            // See: http://msdn.microsoft.com/en-us/magazine/cc534993.aspx
            GC.Collect();
        }
        
        /// <summary>
        /// Reads the content of one stream into another.
        /// </summary>
        /// <param name="source">The forward only stream to read from.</param>
        /// <param name="buffer">The preallocated memory buffer to read from.</param>
        /// <param name="destination">The destination stream to write data to.</param>
        private static void ReadStream(Stream source, byte[] buffer, Stream destination)
        {
            int bytes = source.Read(buffer, 0, buffer.Length);
            while (bytes > 0)
            {
                destination.Write(buffer, 0, bytes);
                bytes = source.Read(buffer, 0, buffer.Length);
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
                foreach (Stream stream in streams)
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
                foreach (Stream stream in streams)
                {
                    // Ensure we're at the start of the stream before reading.
                    stream.Position = 0;
                    using (XmlReader reader = XmlReader.Create(stream, GetXmlReaderSettings()))
                    {
                        if (reader.ReadToDescendant(Constants.TopLevelElementName) &&
                            reader.ReadToDescendant(Constants.ProfilesElementName))
                        {
                            ProcessDevices(provider, reader.ReadSubtree());
                            break;
                        }
                    }
                }

                // Read the manifest properties from the file.
                foreach (Stream stream in streams)
                {
                    // Ensure we're at the start of the stream before reading.
                    stream.Position = 0;
                    using (XmlReader reader = XmlReader.Create(stream, GetXmlReaderSettings()))
                    {
                        if (reader.ReadToDescendant(Constants.TopLevelElementName) &&
                            reader.ReadToDescendant(Constants.PropertiesElementName))
                        {
                            ProcessManifest(provider, reader.ReadSubtree());
                            break;
                        }
                    }
                }

                // Read the date the files were created and the name of the dataset.
                foreach (Stream stream in streams)
                {
                    // Ensure we're at the start of the stream before reading.
                    stream.Position = 0;
                    using (XmlReader reader = XmlReader.Create(stream, GetXmlReaderSettings()))
                    {
                        if (reader.ReadToDescendant(Constants.TopLevelElementName) &&
                            reader.ReadToDescendant(Constants.HeaderElementName))
                        {
                            if (ProcessHeaders(provider, reader.ReadSubtree()))
                                break;
                        }
                    }
                }

                // Set the components properties relate to.
                provider.SetDefaultComponents();
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
                                provider.Properties.Add(property.NameStringIndex, property);
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
            provider.Properties.Add(property.NameStringIndex, property);
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
            int found = 0;
            while (reader.EOF == false && found < 2)
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {
                        case Constants.PublishedDateAttributeName:
                            string date = reader.ReadElementContentAsString();
                            DateTime.TryParse(date, out provider._publishedDate);
                            found++;
                            break;
                        case Constants.DataSetNameAttributeName:
                            provider._dataSetName = reader.ReadElementContentAsString();
                            found++;
                            break;
                        default:
                            reader.Read();
                            break;
                    }
                }
                else
                    reader.Read();
            }
            return found >= 2;
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
            BaseDeviceInfo device = provider.GetDeviceInfoByID(deviceId);
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