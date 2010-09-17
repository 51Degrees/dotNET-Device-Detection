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
 *     Andy Allan <andy.allan@mobgets.com>
 * 
 * ********************************************************************* */

#if VER4 
using System.Linq;
#endif

#region

using System;
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

        #region Fields

        private static readonly StringCollection _capabilitiesWhiteListed = new StringCollection();
        private static bool _loadOnlyCapabilitiesWhiteListed;

        #endregion

        #region Constructors

        static Processor()
        {
            foreach (string capability in Constants.DefaultUsedCapabilities)
                _capabilitiesWhiteListed.Add(capability);
        }

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
        /// Parses the Wurfl files and updates the devices object with data found.
        /// </summary>
        /// <param name="wurflFilePaths">Paths of the Wurfl xml and patch files.</param>
        /// <param name="devices">Collection of mobile devices.</param>
        /// <param name="masterFileDate">If a device has a creation date element only include it in the
        /// device database if it was created after the master file's date and time.</param>
        /// <remarks>If no files are found devices will remain unchanged.</remarks>
        private static void ParseWurflFiles(Provider devices, StringCollection wurflFilePaths,
                                            DateTime masterFileDate)
        {
            StringCollection availableCapabilities = new StringCollection();

            foreach (string filePath in wurflFilePaths)
            {
                // Load the data from the XML file into the devices collection.
                LoadXmlData(availableCapabilities,
                            devices,
                            filePath,
                            masterFileDate);
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
            Provider devices,
            string filePath,
            DateTime masterFileDate)
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
                                    // Load Device Data
                                case Constants.DeviceNodeName:
                                    if (reader.IsStartElement())
                                    {
                                        // If a device has already been created ensure it's saved.
                                        if (device != null)
                                            devices.Set(device);

                                        // Create or get the device related to the current XML element.
                                        device = LoadDevice(devices, reader);
                                    }
                                    break;

                                    // Load the device capability.
                                case Constants.CapabilityNodeName:
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

                        // If a device has not been written ensure it's added to the device dataset.
                        if (device != null)
                            devices.Set(device);
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
        /// <param name="devices">A list of loaded devices.</param>
        /// <param name="reader">The XML stream readers.</param>
        /// <returns>An empty device.</returns>
        private static DeviceInfo LoadDevice(Provider devices, XmlReader reader)
        {
            // Create the next device using the fallback device if available.
            string deviceId = reader.GetAttribute(Constants.IdAttributeName, string.Empty);
            string userAgent = reader.GetAttribute(Constants.UserAgentAttributeName, string.Empty);
            string fallbackDeviceId = reader.GetAttribute(Constants.FallbackAttributeName, string.Empty);

            // If the device already exists then use the previous one. This may happen
            // when an earlier device referenced a fallback device that had not yet
            // been created.
            DeviceInfo device = devices.GetDeviceInfoFromID(deviceId);
            if (device == null)
            {
                // Create the new device.
                device = new DeviceInfo(devices, deviceId, userAgent ?? String.Empty);
            }
            else if (userAgent != null)
            {
                // Ensure the correct UserAgent string is assigned to this device.
                device.SetUserAgent(userAgent);
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
                device.FallbackDevice = devices.GetDeviceInfoFromID(fallbackDeviceId);
                if (device.FallbackDevice == null)
                {
                    // No. So create new fallback device.
                    device.FallbackDevice = new DeviceInfo(devices, fallbackDeviceId);
                    // Add it to the available devices.
                    devices.Set(device.FallbackDevice);
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
            StringCollection availableCapabilities)
        {
            string capabilityName = reader.GetAttribute(Constants.NameAttributeName, string.Empty);

            // If it is not white listed, do not load into the memory.
            // This is to keep memory consumption down.
            if (_loadOnlyCapabilitiesWhiteListed &&
                _capabilitiesWhiteListed.Contains(capabilityName) == false)
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

        #region Public Methods

        internal static void ParseWurflFiles(Provider devices)
        {
            // Parse the wurfl files.
            ParseWurflFiles(
                devices,
                Manager.WurflFilePath,
                Manager.CapabilitiesWhiteList,
                Manager.WurflPatchFiles);
        }

        /// <summary>
        /// Parses the wurfl file into a instance of Devices.
        /// </summary>
        /// <param name="devices">Instance of Devices to store data.</param>
        /// <param name="wurflFilePath">Wurfl file path.</param>
        /// <param name="wurflPatchFiles">Null, string or array of strings representing the wurfl patch files
        /// which must be applied against the main file.</param>
        /// <returns>Returns an instance of Devices. 
        /// <remarks>If none file is found a null value will be returned.</remarks>
        /// </returns>
        /// <exception cref="System.IO.FileNotFoundException">Thrown if the parameter <paramref name="wurflFilePath"/> 
        /// referes to a file that does not exists.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if the parameter <paramref name="wurflFilePath"/> 
        /// is an empty string or a null value.</exception>
        private static void ParseWurflFiles(
            Provider devices,
            string wurflFilePath,
            params string[] wurflPatchFiles)
        {
            ParseWurflFiles(
                devices,
                wurflFilePath,
                null,
                wurflPatchFiles);
        }

        /// <summary>
        /// Parses the wurfl file into a instance of WurflFile.
        /// </summary>
        /// <param name="devices">Instance of Devices to store data.</param>
        /// <param name="wurflFilePath">Wurfl file path.</param>
        /// <param name="capabilitiesWhiteList">List of capabilities to be used. If none, all capabilities will be loaded into the memory.</param>
        /// <param name="wurflPatchFiles">Null, string or array of strings representing the wurfl patch files
        /// which must be applied against the main file.</param>
        /// <returns>Returns an instance of WurflFile. 
        /// <remarks>If none file is found a null value will be returned.</remarks>
        /// </returns>
        /// <exception cref="System.IO.FileNotFoundException">Thrown if the parameter <paramref name="wurflFilePath"/> 
        /// referes to a file that does not exists.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if the parameter <paramref name="wurflFilePath"/> 
        /// is an empty string or a null value.</exception>
        private static void ParseWurflFiles(
            Provider devices,
            string wurflFilePath,
            StringCollection capabilitiesWhiteList,
            params string[] wurflPatchFiles)
        {
            if (string.IsNullOrEmpty(wurflFilePath))
                throw new ArgumentNullException("wurflFilePath");

            if (!File.Exists(wurflFilePath))
                throw new FileNotFoundException(Constants.WurflFileNotFound, wurflFilePath);

            // Load white listed capabilities
            if (capabilitiesWhiteList != null)
            {
                _loadOnlyCapabilitiesWhiteListed = capabilitiesWhiteList.Count > 0;
#if VER4
                foreach (string capability in
                    capabilitiesWhiteList.Cast<string>().Where(capability => !_capabilitiesWhiteListed.Contains(capability)))
                {
                    _capabilitiesWhiteListed.Add(capability);
                }
#elif VER2
                foreach (string capability in capabilitiesWhiteList)
                    if (!_capabilitiesWhiteListed.Contains(capability))
                        _capabilitiesWhiteListed.Add(capability);
#endif
            }

            StringCollection wurflFilePaths = new StringCollection();
            wurflFilePaths.Add(wurflFilePath);
            wurflFilePaths.AddRange(wurflPatchFiles);

            ParseWurflFiles(devices, wurflFilePaths, File.GetCreationTimeUtc(wurflFilePath));
        }

        #endregion

        #endregion
    }
}