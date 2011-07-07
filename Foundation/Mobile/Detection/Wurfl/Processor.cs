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
using FiftyOne.Foundation.Mobile.Detection.Wurfl.Configuration;

#endregion

namespace FiftyOne.Foundation.Mobile.Detection.Wurfl
{
    /// <summary>
    /// Constains all methods needed to process the WURFL xml file.
    /// </summary>
    internal static class Processor
    {
        #region Constants

        private static readonly string[] COMPRESSED = {".gz"};

        #endregion

        #region Methods

        #region Xml Settings

        /// <summary>
        /// Returns XML settings that allows the wurfl.xml files to be processed without errors.
        /// </summary>
        /// <returns>XMLReaderSettings appropriate for wurfl.xml files</returns>
        private static XmlReaderSettings GetXmlReaderSettings()
        {
            XmlReaderSettings settings = new XmlReaderSettings();

            settings.IgnoreComments = true;
            settings.IgnoreWhitespace = true;
            settings.ConformanceLevel = ConformanceLevel.Auto;
            settings.ValidationType = ValidationType.Schema;
            settings.ValidationFlags = XmlSchemaValidationFlags.ProcessSchemaLocation;

            return settings;
        }

        #endregion

        #region Parse Wurfl Methods

        /// <summary>
        /// Load the Wurfl files and updates the devices object with data found.
        /// </summary>
        /// <param name="wurflFilePaths">Paths of the Wurfl xml and patch files.</param>
        /// <param name="devices">Collection of mobile devices.</param>
        /// <param name="capabilitiesWhiteList">A white list of capabilites to load, or null if everything should be loaded.</param>
        /// <remarks>If no files are found devices will remain unchanged.</remarks>
        private static void LoadWurflFiles(Provider devices, string[] wurflFilePaths, List<string> capabilitiesWhiteList)
        {
            StringCollection availableCapabilities = new StringCollection();

            foreach (string filePath in wurflFilePaths)
            {
                // Load the data from the XML file into the devices collection.
                LoadXmlData(availableCapabilities,
                            devices,
                            filePath,
                            capabilitiesWhiteList);
            }
        }

        private static bool IsCompressed(FileInfo file)
        {
#if VER4
            return COMPRESSED.Any(extension => file.Extension == extension);
#elif VER2
            foreach (string extension in COMPRESSED)
            {
                if (file.Extension == extension)
                    return true;
            }
            return false;
#endif
        }

        private static void LoadXmlData(
            StringCollection availableCapabilities,
            Provider provider,
            string filePath,
            List<string> capabilitiesWhiteList)
        {
            DeviceInfo device = null;
            FileInfo file = new FileInfo(filePath);

            if (file.Exists)
            {
                // Open the reader using decompression if the file has an extension
                // that indicates it's compressed.
                using (XmlReader reader = GetReader(file))
                {
                    try
                    {
                        // Process the data file.
                        while (reader.Read())
                        {
                            switch (reader.Name)
                            {
                                case Constants.DeviceNodeName:
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
                                    
                                case Constants.CapabilityNodeName:
                                    // Load the device capability.
                                    if (reader.IsStartElement())
                                    {
                                        LoadCapabilityData(
                                            reader,
                                            device,
                                            availableCapabilities,
                                            capabilitiesWhiteList);
                                    }
                                    break;
                            }
                        }

                        // If a device has not been written ensure it's added to the device dataset.
                        if (device != null)
                            provider.Set(device);
                    }
                    catch (XmlException ex)
                    {
                        throw new WurflException(
                            String.Format("XML exception processing wurfl file '{0}'.", filePath),
                            ex);
                    }
                    catch (IOException ex)
                    {
                        throw new WurflException(
                            String.Format("IO exception processing wurfl file '{0}'.", filePath),
                            ex);
                    }
                    catch (Exception ex)
                    {
                        throw new WurflException(
                            String.Format("Exception processing wurfl file '{0}'.", filePath),
                            ex);
                    }
                }
            }
        }

        /// <summary>
        /// Processes the XML element containing the device attributes.
        /// </summary>
        /// <param name="provider">A list of loaded devices.</param>
        /// <param name="reader">The XML stream readers.</param>
        /// <returns>An empty device.</returns>
        private static DeviceInfo LoadDevice(Provider provider, XmlReader reader)
        {
            // Create the next device using the fallback device if available.
            string deviceId = reader.GetAttribute(Constants.IdAttributeName, string.Empty);
            string userAgent = reader.GetAttribute(Constants.UserAgentAttributeName, string.Empty);
            string fallbackDeviceId = reader.GetAttribute(Constants.FallbackAttributeName, string.Empty);

            // If the device already exists then use the previous one. This may happen
            // when an earlier device referenced a fallback device that had not yet
            // been created.
            DeviceInfo device = (DeviceInfo)provider.GetDeviceInfoFromID(deviceId);
            if (device == null)
            {
                // Create the new device.
                device = new DeviceInfo(provider, deviceId, userAgent ?? String.Empty);
            }
            else if (userAgent != null)
            {
                // Ensure the correct UserAgent string is assigned to this device.
                device.InternalUserAgent = userAgent;
                provider.Set(device);
            }

            // If the Actual Device Root attribute is specified then set the value 
            // for this device.
            bool isActualDeviceRoot;
            if (bool.TryParse(reader.GetAttribute(Constants.ActualDeviceRoot, string.Empty), out isActualDeviceRoot))
                device.IsActualDeviceRoot = isActualDeviceRoot;

            // Check the fallback device is different to the device being loaded.
            if (fallbackDeviceId != null && device.DeviceId != fallbackDeviceId)
            {
                // Does the fallback device already exist?
                device.FallbackDevice = (DeviceInfo)provider.GetDeviceInfoFromID(fallbackDeviceId);
                if (device.FallbackDevice == null)
                {
                    // No. So create new fallback device.
                    device.FallbackDevice = new DeviceInfo(provider, fallbackDeviceId);
                    // Add it to the available devices.
                    provider.Set(device.FallbackDevice);
                }
            }
            return device;
        }

        /// <summary>
        /// Opens the Xml reader after first checking if the file is compressed.
        /// </summary>
        /// <param name="file">Information about the file to be opened.</param>
        /// <returns>An open XmlReader.</returns>
        private static XmlReader GetReader(FileInfo file)
        {
            XmlReaderSettings settings = GetXmlReaderSettings();
            if (IsCompressed(file))
            {
                return XmlReader.Create(
                    new GZipStream(
                        File.OpenRead(file.FullName), CompressionMode.Decompress),
                    settings);
            }
            else
            {
                return XmlReader.Create(file.FullName, settings);
            }
        }

        private static void LoadCapabilityData(
            XmlReader reader,
            DeviceInfo device,
            StringCollection availableCapabilities,
            List<string> capabilitiesWhiteList)
        {
            string capabilityName = reader.GetAttribute(Constants.NameAttributeName, string.Empty);

            // If it is not white listed, do not load into the memory.
            // This is to keep memory consumption down.
            if (capabilitiesWhiteList != null &&
                capabilitiesWhiteList.Contains(capabilityName) == false)
                return;

            string capabilityValue = reader.GetAttribute(Constants.ValueAttributeName, string.Empty);

            // Store all the capabilities names, that's used to make sure
            // all devices has all capabilities associated to it.
            if (availableCapabilities.Contains(capabilityName) == false)
                availableCapabilities.Add(capabilityName);

            // Ensure the capability is set to the current value.
            device.Capabilities.Set(capabilityName, capabilityValue);
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Parses the wurfl file into a instance of WurflFile.
        /// </summary>
        /// <param name="devices">Instance of Devices to store data.</param>
        /// <param name="capabilitiesWhiteList">List of capabilities to be used. If none, all capabilities will be loaded into the memory.</param>
        /// <param name="wurflFilePaths">Array of WURFL format files to load.</param>
        /// <returns>Returns an instance of WurflFile. 
        /// <remarks>If none file is found a null value will be returned.</remarks>
        /// </returns>
        /// <exception cref="System.IO.FileNotFoundException">Thrown if the parameter <paramref name="wurflFilePaths"/> 
        /// referes to a file that does not exists.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if the parameter <paramref name="wurflFilePaths"/> 
        /// is an empty string or a null value.</exception>
        internal static void ParseWurflFiles(
            Provider devices,
            string[] wurflFilePaths,
            string[] capabilitiesWhiteList)
        {
            // Check all the patch files exist.
            foreach (string wurflFilePath in wurflFilePaths)
            {
                if (string.IsNullOrEmpty(wurflFilePath))
                    throw new ArgumentNullException("wurflFilePath");

                if (!File.Exists(wurflFilePath))
                    throw new FileNotFoundException(Constants.WurflFileNotFound, wurflFilePath);
            }

            // Create the final white list of capabilites if provided.
            List<string> finalCapabilitiesWhiteList = null;
            if (capabilitiesWhiteList != null && capabilitiesWhiteList.Length > 0)
            {
                finalCapabilitiesWhiteList = new List<string>(Constants.DefaultUsedCapabilities);
                foreach (string capability in capabilitiesWhiteList)
                    if (finalCapabilitiesWhiteList.Contains(capability) == false)
                        finalCapabilitiesWhiteList.Add(capability);
            }

            LoadWurflFiles(devices, wurflFilePaths, finalCapabilitiesWhiteList);
        }

        #endregion

        #endregion
    }
}