/* *********************************************************************
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited. 
 * Copyright © 2017 51Degrees Mobile Experts Limited, 5 Charlotte Close,
 * Caversham, Reading, Berkshire, United Kingdom RG4 7BY
 * 
 * This Source Code Form is the subject of the following patents and patent 
 * applications, owned by 51Degrees Mobile Experts Limited of 5 Charlotte
 * Close, Caversham, Reading, Berkshire, United Kingdom RG4 7BY: 
 * European Patent No. 2871816;
 * European Patent Application No. 17184134.9;
 * United States Patent Nos. 9,332,086 and 9,350,823; and
 * United States Patent Application No. 15/686,066.
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security;
using System.Text.RegularExpressions;
using System.Web.Hosting;
using FiftyOne.Foundation.Mobile.Configuration;
using FiftyOne.Foundation.Mobile.Detection.Configuration;
using System.Linq;
using System.Configuration;
using FiftyOne.Foundation.Mobile.Detection.Entities;
using FiftyOne.Foundation.Mobile.Detection.Factories;

namespace FiftyOne.Foundation.Mobile.Detection
{
    /// <summary>
    /// Static class used to manage the activate licence keys.
    /// </summary>
    public static class LicenceKey
    {
        #region Fields

        /// <summary>
        /// Licence keys added dynamically via external assmeblies.
        /// </summary>
        private static List<string> _dynamicKeys = new List<string>();

        #endregion

        #region Internal Properties

        /// <summary>
        /// Returns a list of the valid license keys available
        /// to the assembly.
        /// </summary>
        internal static string[] Keys
        {
            get
            {
                // Initilaise the list with any dynamic keys.
                List<string> list = new List<string>(_dynamicKeys);

                // See if a license key is included in the assembly.
                if (String.IsNullOrEmpty(Constants.PremiumLicenceKey) == false &&
                    IsKeyFormatValid(Constants.PremiumLicenceKey))
                    list.Add(Constants.PremiumLicenceKey);

                // See if we can get licence keys from the 51Degrees preferred licence
                // key file.
                if (Directory.Exists(HostingEnvironment.ApplicationPhysicalPath + "bin"))
                {
                    foreach (string fileName in Directory.GetFiles(
                        HostingEnvironment.ApplicationPhysicalPath + "bin", Constants.LicenceKeyFileName))
                    {
                        AddKeysFromFile(list, fileName);
                    }

                    // If there are no licence keys found so far then now try the bin 
                    // folder for any license key files.
                    if (list.Count == 0)
                    {
                        foreach (string fileName in Directory.GetFiles(
                            HostingEnvironment.ApplicationPhysicalPath + "bin", "*.lic"))
                        {
                            AddKeysFromFile(list, fileName);
                        }
                    }
                }

                return list.ToArray();
            }
        }

        /// <summary>
        /// Adds the valid licence keys from the files provided.
        /// </summary>
        /// <param name="list">A list of the licence </param>
        /// <param name="fileName"></param>
        private static void AddKeysFromFile(List<string> list, string fileName)
        {
            string alltext = File.ReadAllText(fileName);
            foreach (string key in alltext.Split(
                new string[] { "\r\n", "\r", "\n" },
                StringSplitOptions.RemoveEmptyEntries))
                if (IsKeyFormatValid(key))
                    list.Add(key);
        }
        
        #endregion

        #region Public Properties

        /// <summary>
        /// Returns the host name of the web service used to provide new device data
        /// and also validate the licence key.
        /// </summary>
        public static string HostName
        {
            get { return new Uri(FiftyOne.Foundation.Mobile.Detection.Constants.AutoUpdateUrl).Host; }
        }

        /// <summary>
        /// The name of the licence key file in the bin folder.
        /// </summary>
        public static string LicenceKeyFileName
        {
            get { return Detection.Constants.LicenceKeyFileName; }
        }
        
        #endregion

        #region Public Methods

        /// <summary>
        /// Adds a licence key to the list of available licence keys at runtime.
        /// This method can be used by 3rd party assemblies to set licence keys.
        /// </summary>
        /// <param name="key">Valid licence key</param>
        public static void AddKey(string key)
        {
            if (IsKeyFormatValid(key) &&
                _dynamicKeys.Contains(key) == false)
                _dynamicKeys.Add(key);
        }

        /// <summary>
        /// Activates the data pointed to by the stream.
        /// </summary>
        /// <param name="stream">Stream to data to activate</param>
        public static LicenceKeyResults Activate(Stream stream)
        {
            byte[] data = null;
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    data = ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                EventLog.Warn(ex);
                return LicenceKeyResults.StreamFailure;
            }

            return Activate(data);
        }

        /// <summary>
        /// Activates the data array containing the premium data.
        /// </summary>
        /// <param name="data">Data to activate</param>
        public static LicenceKeyResults Activate(byte[] data)
        {
            try
            {
                DataSet dataSet = null;

                // Validate the data provided is correct.
                try
                {
                    dataSet = StreamFactory.Create(data);
                }
                catch (MobileException ex)
                {
                    EventLog.Warn(ex);
                    return LicenceKeyResults.DataInvalid;
                }

                // Check the configuration.
                try
                {
                    CheckConfig();
                }
                catch (Exception ex)
                {
                    EventLog.Warn(ex);
                    return LicenceKeyResults.Config;
                }
                
                // Write the file to the binary data path.
                try
                {
                    File.WriteAllBytes(Detection.Configuration.Manager.BinaryFilePath, data);
                    File.SetLastAccessTimeUtc(Detection.Configuration.Manager.BinaryFilePath, dataSet.Published);
                }
                catch (IOException ex)
                {
                    EventLog.Warn(ex);
                    return LicenceKeyResults.WriteDataFile;
                }

                // Switch in the new data to complete activation.
                WebProvider.Refresh();

                EventLog.Info(String.Format(
                    "Activated binary data file '{0}' with new version " +
                    "dated the '{1:d}'.",
                    AutoUpdate.MasterBinaryDataFile.FullName,
                    dataSet.Published));
            }
            catch (Exception ex)
            {
                EventLog.Warn(ex);
                return LicenceKeyResults.GenericFailure;
            }

            return LicenceKeyResults.Success;
        }

        /// <summary>
        /// Activates the licence key provided.
        /// </summary>
        /// <param name="licenceKey">Licence key</param>
        public static LicenceKeyResults Activate(string licenceKey)
        {
            try
            {
                try
                {
                    CheckConfig();
                }
                catch (Exception ex)
                {
                    EventLog.Warn(ex);
                    return LicenceKeyResults.Config;
                }

                // Download the new data file.
                var result = AutoUpdate.Download(new string[] { licenceKey });
                if (result != LicenceKeyResults.Success)
                    return result;

                // Write the license key to the bin folder.
                try
                {
                    File.WriteAllText(Path.Combine(
                        HostingEnvironment.ApplicationPhysicalPath,
                        Path.Combine("bin", Constants.LicenceKeyFileName)), licenceKey);
                }
                catch (Exception ex)
                {
                    EventLog.Warn(ex);
                    return LicenceKeyResults.WriteLicenceFile;
                }
            }
            catch (Exception ex)
            {
                EventLog.Warn(ex);
                return LicenceKeyResults.GenericFailure;
            }

            return LicenceKeyResults.Success;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Returns true if the key format is valid. i.e. it contains
        /// only upper case letters and numbers.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private static bool IsKeyFormatValid(string key)
        {
            return Regex.IsMatch(key, Constants.LicenceKeyValidationRegex);
        }

        /// <summary>
        /// Checks 
        /// </summary>
        private static void CheckConfig()
        {
            // Get the current section.
            DetectionSection section = Support.GetWebApplicationSection("fiftyOne/detection", false) as DetectionSection;

            // If the section is valid then do nothing.
            if (section != null &&
                String.IsNullOrEmpty(section.BinaryFilePath) == false)
                return;

            // If the section does not exist then create it.
            if (section == null)
                section = new DetectionSection();

            // Set the binary path to the default.
            section.BinaryFilePath = Detection.Constants.DefaultBinaryFilePath;

            // Add the section back to the configuration.
            FiftyOne.Foundation.Mobile.Configuration.Support.SetWebApplicationSection(section);

            // Refresh the configuration.
            Detection.Configuration.Manager.Refresh();
        }

        #endregion
    }
}
