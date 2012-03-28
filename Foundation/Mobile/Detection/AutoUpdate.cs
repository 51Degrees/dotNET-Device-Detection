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

using System.Collections.Generic;
using System;
using System.IO;
using System.Web.Hosting;
using System.Threading;
using FiftyOne.Foundation.Mobile.Detection.Configuration;
using System.Net;
using System.Collections.Specialized;
using System.Security.Cryptography;
using System.Text;

namespace FiftyOne.Foundation.Mobile.Detection
{
    /// <summary>
    /// Used to fetch new device data from 51Degrees.mobi if a premium
    /// licence has been installed.
    /// </summary>
    internal static class AutoUpdate
    {
        #region Fields

        /// <summary>
        /// Maps to the binary file path.
        /// </summary>
        private static FileInfo _binaryFile = null;

        #endregion

        #region Internal Properties

        /// <summary>
        /// Returns details of the binary file to be updated.
        /// </summary>
        internal static FileInfo BinaryFile
        {
            get
            {
                if (_binaryFile == null)
                {
                    if (String.IsNullOrEmpty(Manager.BinaryFilePath) == false)
                        _binaryFile = new FileInfo(Manager.BinaryFilePath);
                }
                return _binaryFile;
            }
        }

        #endregion

        #region Static Methods

        private static string GetMd5Hash(byte[] value)
        {
            using (MD5 md5Hash = MD5.Create())
                return GetMd5Hash(md5Hash, value);
        }

        private static string GetMd5Hash(MD5 md5Hash, byte[] value)
        {
            if (value == null)
                return String.Empty;

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(value);

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        /// <summary>
        /// Checks the MD5 hash of the data against the expected value.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="data"></param>
        internal static void ValidateMD5(WebClient client, byte[] data)
        {
            // Check the MD5 hash of the data downloaded.
            string mdHash = client.ResponseHeaders["Content-MD5"];
            if (mdHash != GetMd5Hash(data))
                throw new MobileException(String.Format(
                    "MD5 hash '{0}' validation failure with data downloaded from update URL '{1}'.",
                    mdHash,
                    client.BaseAddress));

        }

        /// <summary>
        /// Throws an exception if the data is not valid.
        /// </summary>
        /// <param name="data">The data to use to create the provider.</param>
        /// <returns>A newly created provider.</returns>
        internal static Provider CreateProvider(byte[] data)
        {
            try
            {
                // Create new provider with the data to ensure it is valid.
                var provider = Binary.Reader.Create(data);
                if (provider.AllDevices.Count == 0)
                    throw new MobileException("No devices found in downloaded data.");
                return provider;
            }
            catch (Exception ex)
            {
                throw new MobileException("Exception validating data array", ex);
            }
        }

        /// <summary>
        /// Used to get the url for data download.
        /// </summary>
        /// <returns>The full url including all parameters needed to download
        /// the device data file.</returns>
        internal static string FullUrl()
        {
            return FullUrl(LicenceKey.Keys);
        }

        /// <summary>
        /// Used to get the url for data download.
        /// </summary>
        /// <param name="licences">An array of licences to try.</param>
        /// <returns>The full url including all parameters needed to download
        /// the device data file.</returns>
        internal static string FullUrl(string[] licences)
        {
            var parameters = new List<string>();
            parameters.Add(String.Format("LicenseKeys={0}", String.Join("|", licences)));
            parameters.Add(String.Format("Download={0}", bool.TrueString));
            parameters.Add("Type=Binary");

            return String.Format("{0}?{1}",
                Constants.AutoUpdateUrl,
                String.Join("&", parameters.ToArray()));
        }

        #endregion

        #region Thread Methods

        internal static void Run(object state)
        {
            try
            {
                if (Constants.AutoUpdateDelayedStart.TotalSeconds > 0)
                {
                    // Pause for X seconds to allow the worker process to complete starting up.
                    Thread.Sleep(Constants.AutoUpdateDelayedStart);

                    // If licence keys are available auto update.
                    if (LicenceKey.Keys.Length > 0)
                    {
                        // Check the last accessed date of the binary file to determine
                        // if it should be updated.
                        if (BinaryFile != null &&
                            BinaryFile.LastWriteTimeUtc.Add(Constants.AutoUpdateWait) < DateTime.UtcNow)
                        {
                            Download();
                        }
                    }
                }
            }
            catch (ThreadAbortException ex)
            {
                EventLog.Warn(new MobileException(
                    "Auto update thread aborted",
                    ex));
            }
            catch (Exception ex)
            {
                if (BinaryFile != null && BinaryFile.FullName != null)
                {
                    EventLog.Warn(new MobileException(String.Format(
                        "Exception auto updating binary data file '{0}'.",
                        BinaryFile.FullName), ex));
                }
                else
                {
                    EventLog.Fatal(ex);
                }
            }
        }

        /// <summary>
        /// Downloads and updates the premium data file.
        /// </summary>
        internal static void Download()
        {
            // Download the latest data.
            var client = new WebClient();
            var data = client.DownloadData(FullUrl());

            // Validate the results.
            ValidateMD5(client, data);
            var provider = CreateProvider(data);

            // Check this is new data based on publish data and
            // number of available properties.
            if (provider.PublishedDate != Factory.ActiveProvider.PublishedDate ||
                provider.Properties.Count != Factory.ActiveProvider.Properties.Count)
            {
                // Both the MD5 hash was good and the provider was created.
                // Save the data and force the factory to reload.
                File.WriteAllBytes(BinaryFile.FullName, data);

                // Sets the last modified time of the file downloaded.
                BinaryFile.LastWriteTimeUtc = provider.PublishedDate;

                // Switch the instance of the data provider over.
                Factory.Reset();

                EventLog.Info(String.Format(
                    "Automatically updated binary data file '{0}' with version " +
                    "published on the '{1:d}'.",
                    BinaryFile.FullName,
                    Factory.ActiveProvider.PublishedDate));
            }
        }

        #endregion
    }
}
