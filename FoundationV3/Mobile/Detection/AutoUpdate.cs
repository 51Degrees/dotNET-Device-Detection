/* *********************************************************************
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited. 
 * Copyright © 2014 51Degrees Mobile Experts Limited, 5 Charlotte Close,
 * Caversham, Reading, Berkshire, United Kingdom RG4 7BY
 * 
 * This Source Code Form is the subject of the following patent 
 * applications, owned by 51Degrees Mobile Experts Limited of 5 Charlotte
 * Close, Caversham, Reading, Berkshire, United Kingdom RG4 7BY: 
 * European Patent Application No. 13192291.6; and
 * United States Patent Application Nos. 14/085,223 and 14/085,301.
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
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web.Hosting;
using FiftyOne.Foundation.Mobile.Detection.Configuration;
using System.Web;
using FiftyOne.Foundation.Mobile.Detection.Entities;
using FiftyOne.Foundation.Mobile.Detection.Factories;
using System.IO.Compression;
using System.Globalization;
using System.Security;

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

        /// <summary>
        /// Used to signal between multiple instances of the update
        /// threads.
        /// </summary>
        private static AutoResetEvent _autoUpdateSignal = new AutoResetEvent(true);

        private static AutoResetEvent _autoDownloadUpdateSignal = new AutoResetEvent(true);

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
                    _binaryFile = DataOnDisk();
                }
                return _binaryFile;
            }
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Gets FileInfo on the data file currently on disk. Returns null if no file was found.
        /// </summary>
        /// <returns></returns>
        private static FileInfo DataOnDisk()
        {
            if (String.IsNullOrEmpty(Manager.BinaryFilePath) == false)
                return new FileInfo(Manager.BinaryFilePath);
            return null;
        }

        private static string GetMd5Hash(Stream stream)
        {
            using (MD5 md5Hash = MD5.Create())
                return GetMd5Hash(md5Hash, stream);
        }

        private static string GetMd5Hash(MD5 md5Hash, Stream stream)
        {
            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(stream);

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
        /// <param name="client">Used to make HTTP request to check for new data</param>
        /// <param name="stream">Open stream to the data to be validated</param>
        internal static void ValidateMD5(WebClient client, Stream stream)
        {
            string mdHash = client.ResponseHeaders["Content-MD5"];
            if (mdHash != GetMd5Hash(stream))
            {
                throw new MobileException(String.Format(
                    "MD5 hash '{0}' validation failure with data downloaded from update URL '{1}'.",
                    mdHash,
                    client.BaseAddress));
            }
        }
        
        /// <summary>
        /// Used to get the url for data download.
        /// </summary>
        /// <param name="licences">An array of licences to try.</param>
        /// <returns>The full url including all parameters needed to download
        /// the device data file.</returns>
        internal static string FullUrl(string[] licences)
        {
            List<string> parameters = new List<string>();
            parameters.Add(String.Format("LicenseKeys={0}", String.Join("|", licences)));
            parameters.Add(String.Format("Download={0}", bool.TrueString));
            parameters.Add("Type=BinaryV3");

            return String.Format("{0}?{1}",
                Constants.AutoUpdateUrl,
                String.Join("&", parameters.ToArray()));
        }

        #endregion

        #region Thread Methods
        
        /// <summary>
        /// Checks if a new data file is available for download and if it is newer than
        /// the current data on disk, if enough time has passed between now and the write 
        /// time of the current data in memory. See Constants.AutoUpdateWait. This method 
        /// is designed to be used in a seperate thread from the one performing detection.
        /// </summary>
        internal static void CheckForUpdate(object state)
        {
            try
            {
                // If licence keys are available auto update.
                if (LicenceKey.Keys.Length > 0)
                {
                    // Check that there is a binary file, and that either the active provider
                    // is not available indicating no source data file, or if is available
                    // that the next update is in the past, or that the device data being used
                    // is the free lite version.
                    if (BinaryFile != null &&
                        (WebProvider.ActiveProvider == null ||
                        WebProvider.ActiveProvider.DataSet.NextUpdate < DateTime.UtcNow ||
                        WebProvider.ActiveProvider.DataSet.Name == "Lite"))
                    {
                        if (Download(LicenceKey.Keys) == LicenceKeyResults.Success)
                        {
                            WebProvider.Refresh();
                        }
                    }
                }
            }
            catch (ThreadAbortException ex)
            {
                EventLog.Warn(new MobileException(
                    "Auto update download thread aborted",
                    ex));
            }
            catch (Exception ex)
            {
                if (BinaryFile != null && BinaryFile.FullName != null)
                {
                    EventLog.Warn(new MobileException(String.Format(
                        "Exception auto update download binary data file '{0}'.",
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
        internal static LicenceKeyResults Download(string[] licenceKeys)
        {
            var status = LicenceKeyResults.InProgress;
            WebClient client = new WebClient();

            var compressedTempFile = WebProvider.GetTempFileName();
            var uncompressedTempFile = String.Format("{0}.new", Manager.BinaryFilePath);
            try
            {
                // Wait until any other threads have finished executing.
                _autoDownloadUpdateSignal.WaitOne();

                // Download the data file to the compressed temporary file.
                status = DownloadFile(client, Manager.BinaryFilePath, compressedTempFile, licenceKeys);

                // Validate the MD5 hash of the download.
                if (status == LicenceKeyResults.InProgress)
                {
                    status = CheckedDownloadedFileMD5(client, compressedTempFile);
                }

                // Decompress the data file ready to create the data set.
                if (status == LicenceKeyResults.InProgress)
                {
                    status = Decompress(compressedTempFile, uncompressedTempFile);
                }

                // Validate that the data file can be used to create a provider.
                if (status == LicenceKeyResults.InProgress)
                {
                    status = ValidateDownloadedFile(uncompressedTempFile);
                }

                // Activate the data file downloaded for future use.
                if (status == LicenceKeyResults.InProgress)
                {
                    status = ActivateDownloadedFile(client, uncompressedTempFile);
                }
            }
            catch(Exception)
            {
                status = LicenceKeyResults.GenericFailure;
            }
            finally
            {
                File.Delete(compressedTempFile);
                File.Delete(uncompressedTempFile);
                client.Dispose();
                _autoDownloadUpdateSignal.Set();
            }
            return status;
        }

        private static LicenceKeyResults ActivateDownloadedFile(WebClient client, string uncompressedTempFile)
        {
            var status = LicenceKeyResults.Success;

            // Rename the current master file to a temp file so enable the new
            // master file to take it's place and to rollback if there's a problem.
            var tempCopyofCurrentMaster = String.Format("{0}.tmp", BinaryFile.FullName);
            try
            {
                // Both the MD5 hash was good and the provider was created.
                // Save the data and force the factory to reload.

                if (BinaryFile.Exists)
                {
                    // Keep a copy of the old data in case we need to go back to it.
                    File.Move(BinaryFile.FullName, tempCopyofCurrentMaster);
                }

                // Copy the new file to the master file.
                File.Move(uncompressedTempFile, BinaryFile.FullName);

                // Get the published date from the new data file.
                var publishedDate = WebProvider.GetDataFileDate(BinaryFile.FullName);

                // Sets the last modified time of the file downloaded to the one
                // provided in the HTTP header, or if not valid then the published
                // date of the data set.
                DateTime lastModified;
                if (DateTime.TryParseExact(
                    client.ResponseHeaders[HttpResponseHeader.LastModified],
                    "R",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal,
                    out lastModified) == false)
                {
                    lastModified = publishedDate.Value;
                }
                BinaryFile.LastWriteTimeUtc = lastModified.ToUniversalTime();

                EventLog.Info(String.Format(
                    "Automatically updated binary data file '{0}' with version " +
                    "published on the '{1:d}'.",
                    BinaryFile.FullName,
                    publishedDate));
            }
            catch (Exception ex)
            {
                if (BinaryFile.Exists == false)
                {
                    File.Move(tempCopyofCurrentMaster, BinaryFile.FullName);
                }
                EventLog.Warn(ex);
                status = LicenceKeyResults.WriteDataFile;
            }
            finally
            {
                File.Delete(tempCopyofCurrentMaster);
            }
            
            return status;
        }

        private static LicenceKeyResults ValidateDownloadedFile(string uncompressedTempFile)
        {
            LicenceKeyResults status = LicenceKeyResults.InProgress;
            try
            {
                using (var dataSet = StreamFactory.Create(uncompressedTempFile))
                {
                    var currentProvider = WebProvider.ActiveProvider;
                    status = currentProvider == null ||
                        dataSet.Published != currentProvider.DataSet.Published ||
                        dataSet.Properties.Count != currentProvider.DataSet.Properties.Count ?
                        LicenceKeyResults.InProgress : LicenceKeyResults.UpdateNotNeeded;
                }
            }
            catch (MobileException ex)
            {
                EventLog.Warn(ex);
                return LicenceKeyResults.Invalid;
            }
            return status;
        }

        private static LicenceKeyResults CheckedDownloadedFileMD5(WebClient client, string downloadedFile)
        {
            var status = LicenceKeyResults.InProgress;
            try
            {
                using (var stream = File.OpenRead(downloadedFile))
                {
                    ValidateMD5(client, stream);
                }
            }
            catch (MobileException ex)
            {
                EventLog.Warn(ex);
                status = LicenceKeyResults.Invalid;
            }
            return status;
        }

        /// <summary>
        /// Downloads the data associated with 
        /// </summary>
        /// <param name="client">Connect to make HTTP request for the data file</param>
        /// <param name="currentFile">Path to the current active data file</param>
        /// <param name="destination">Destination of the file to download</param>
        /// <param name="licenceKeys">Array of licence keys to use to authenticate</param>
        /// <returns>The current status of the overall activity</returns>
        private static LicenceKeyResults DownloadFile(WebClient client, string currentFile, string destination, string[] licenceKeys)
        {
            var status = LicenceKeyResults.InProgress;

            // Only set the last modified data if we're not dealing with Lite data
            // at the moment.
            if (BinaryFile.Exists)
            {
                client.Headers.Add(
                    HttpRequestHeader.LastModified,
                    BinaryFile.LastWriteTimeUtc.ToString("R"));
            }

            try
            {
                // Download the data to the temporary file.
                client.DownloadFile(FullUrl(licenceKeys), destination);
            }
            catch (SecurityException ex)
            {
                EventLog.Warn(ex);
                status = LicenceKeyResults.Https;
            }
            catch (WebException ex)
            {
                // Use the server response text if available. Otherwise the exception
                // message being handled.
                string responseText;
                try
                {
                    responseText = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
                    if (String.IsNullOrEmpty(responseText))
                    {
                        responseText = ex.Message;
                    }
                }
                catch (Exception)
                {
                    responseText = ex.Message;
                }
                EventLog.Info(String.Format(
                    "No device data was returned, probably because no newer data is available. " +
                    "The server responded with the message '{0}'.",
                    responseText));
                EventLog.Debug(ex);
                status = LicenceKeyResults.Https;
            }
            catch (MobileException ex)
            {
                EventLog.Warn(ex);
                status = LicenceKeyResults.Https;
            }

            return status;
        }

        private static LicenceKeyResults Decompress(string source, string destination)
        {
            var status = LicenceKeyResults.InProgress;
            try
            {
                using (var fs = File.Create(destination))
                {
                    using (var gs = new GZipStream(
                        File.OpenRead(source), CompressionMode.Decompress))
                    {
                        gs.CopyTo(fs);
                    }
                }
            }
            catch (Exception ex)
            {
                EventLog.Warn(ex);
                status = LicenceKeyResults.WriteDataFile;
            }
            return status;
        }
        
        #endregion
    }
}
