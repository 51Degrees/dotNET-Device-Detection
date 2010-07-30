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

using System;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Collections.Specialized;
using System.IO.Compression;
using FiftyOne.Foundation.Mobile.Detection.Wurfl.Configuration;

namespace FiftyOne.Foundation.Mobile.Detection.Wurfl
{
    /// <summary>
    /// Constains all methods needed to process the WURFL xml file.
    /// </summary>
    internal static class WurflProcessor
    {
        #region Constants

        private static readonly string[] COMPRESSED = { ".gz" };

        #endregion

        #region Fields

        static bool _loadOnlyCapabilitiesWhiteListed;
        static StringCollection _capabilitiesWhiteListed = new StringCollection();

        #endregion

        #region Constructors

        static WurflProcessor()
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
            foreach (string extension in COMPRESSED)
            {
                if (file.Extension == extension)
                    return true;
            }
            return false;
        }

        private static void LoadXmlData(
            StringCollection availableCapabilities,
            Provider devices,
            string filePath,
            DateTime masterFileDate)
        {
            XmlReaderSettings settings = GetXmlReaderSettings();
            DeviceInfo device = null;
            FileInfo file = new FileInfo(filePath);
            Stream stream = null;
            GZipStream gzipStream = null;
            XmlReader reader = null;
            bool ignoreDevicesWithoutFallbacks = false;
            bool isActualDeviceRoot = false;

            if (file.Exists == true)
            {
                try
                {
                    // Open the reader using decompression if the file has an extension
                    // that indicates it's compressed.
                    if (IsCompressed(file) == true)
                    {
                        stream = File.OpenRead(filePath);
                        gzipStream = new GZipStream(stream, CompressionMode.Decompress);
                        reader = XmlReader.Create(gzipStream);
                    }
                    else
                    {
                        reader = XmlReader.Create(file.FullName, settings);
                    }

                    // Process the data file.
                    while (reader.Read())
                    {
                        switch (reader.Name)
                        {
                            // Remove the device if it's older than the current master file.
                            case Constants.CreatedDate:
                                // Tell subsequent processing to ignore devices that
                                // have no fallback devices already loaded.
                                ignoreDevicesWithoutFallbacks = true;

                                // If the element is a valid date element.
                                if (masterFileDate != null && device != null && reader.IsStartElement())
                                {
                                    // Get the date and time the current device was created in 
                                    // the wurfl patch file.
                                    DateTime createdDate = reader.ReadElementContentAsDateTime();
                                    if (createdDate < masterFileDate)
                                    {
                                        EventLog.Debug(String.Format("Device ID '{0}' has not been loaded as it was created prior to the current master file.",
                                            device.DeviceId));
                                        device = null;
                                    }
                                }
                                break;
                            // Load Device Data
                            case Constants.DeviceNodeName:
                                if (reader.IsStartElement())
                                {
                                    // If a device has already been created ensure it's saved.
                                    if (device != null)
                                    {
                                        devices.Set(device);
                                        device = null;
                                    }

                                    // Create the next device using the fallback device if available.
                                    string deviceId = reader.GetAttribute(Constants.IdAttributeName, string.Empty);
                                    string userAgent = reader.GetAttribute(Constants.UserAgentAttributeName, string.Empty);
                                    string fallbackDeviceId = reader.GetAttribute(Constants.FallbackAttributeName, string.Empty);
                                    
                                    // If the device already exists then use the previous one. This may happen
                                    // when an earlier device referenced a fallback device that had not yet
                                    // been created.
                                    device = devices.GetDeviceInfoFromID(deviceId);
                                    if (device == null)
                                    {
                                        // Create the new device.
                                        device = new DeviceInfo(devices, deviceId, userAgent);
                                    }
                                    else
                                    {
                                        // Ensure the correct UserAgent string is assigned to this device.
                                        device.SetUserAgent(userAgent);
                                    }

                                    // If the Actual Device Root attribute is specified then set the value 
                                    // for this device.
                                    if (bool.TryParse(reader.GetAttribute(Constants.ActualDeviceRoot, string.Empty), out isActualDeviceRoot) == true)
                                        device.IsActualDeviceRoot = isActualDeviceRoot;

                                    // Check the fallback device is different to the device being loaded.
                                    if (device.DeviceId != fallbackDeviceId && fallbackDeviceId != null)
                                    {
                                        // Does the fallback device already exist?
                                        device.FallbackDevice = devices.GetDeviceInfoFromID(fallbackDeviceId);
                                        if (device.FallbackDevice == null && ignoreDevicesWithoutFallbacks == false)
                                        {
                                            // No. So create new fallback device.
                                            device.FallbackDevice = new DeviceInfo(devices, fallbackDeviceId);
                                            // Add it to the available devices.
                                            devices.Set(device.FallbackDevice);
                                        }
                                    }

                                    // Check to see if we should ignore devices without fallbacks.
                                    if (ignoreDevicesWithoutFallbacks == true && device.FallbackDevice == null)
                                        device = null;
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
                    
                    // If a device is outstanding ensure it's added to the device dataset.
                    if (device != null)
                    {
                        devices.Set(device);
                        device = null;
                    }
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
                finally
                {
                    if (reader != null) { reader.Close(); }
                    if (gzipStream != null) { gzipStream.Close(); gzipStream.Dispose(); }
                    if (stream != null) { stream.Close(); stream.Dispose(); }
                }
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
            if (_loadOnlyCapabilitiesWhiteListed == true &&
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
                foreach (string capability in capabilitiesWhiteList)
                    if (!_capabilitiesWhiteListed.Contains(capability))
                        _capabilitiesWhiteListed.Add(capability);
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