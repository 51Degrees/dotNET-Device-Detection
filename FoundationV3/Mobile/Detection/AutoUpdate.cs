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
                // Wait until any other threads have finished executing.
                _autoDownloadUpdateSignal.WaitOne();

                // If licence keys are available auto update.
                if (LicenceKey.Keys.Length > 0)
                {
                    // Check the Next Update date of the data set to determine if we should
                    // check for an update.
                    if (BinaryFile != null &&
                        (WebProvider.ActiveProvider.DataSet.NextUpdate < DateTime.UtcNow ||
                        WebProvider.ActiveProvider.DataSet.Name == "Lite"))
                    {
                        if (Download() == LicenceKeyResults.Success)
                            WebProvider.Refresh();
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
            finally
            {
                // Signal any waiting threads to start.
                _autoDownloadUpdateSignal.Set();
            }
        }

        /// <summary>
        /// Downloads and updates the premium data file.
        /// </summary>
        internal static LicenceKeyResults Download()
        {
            return Download(LicenceKey.Keys);
        }

        /// <summary>
        /// Downloads and updates the premium data file.
        /// </summary>
        internal static LicenceKeyResults Download(string[] licenceKeys)
        {
            // Download the latest data.
            WebClient client = new WebClient();

            // Only set the last modified data if we're not dealing with Lite data
            // at the moment.
            if (BinaryFile.Exists)
                client.Headers.Add(
                    HttpRequestHeader.LastModified,
                    BinaryFile.LastWriteTimeUtc.ToString("R"));
            
            byte[] data;

            try
            {
                data = client.DownloadData(FullUrl(licenceKeys));
            }
            catch (SecurityException ex)
            {
                EventLog.Warn(ex);
                return LicenceKeyResults.Https;
            }
            catch (WebException ex)
            {
                EventLog.Info("No device data was returned, probably because no newer data is available.");
                EventLog.Debug(ex);
                return LicenceKeyResults.Https;
            }
            catch (MobileException ex)
            {
                EventLog.Warn(ex);
                return LicenceKeyResults.Https;
            }

            // Validate the MD5 hash result.
            try
            {
                ValidateMD5(client, data);
            }
            catch (MobileException ex)
            {
                EventLog.Warn(ex);
                return LicenceKeyResults.Invalid;
            }
            
            // Validate the data can produce a data set.
            var tempFile = WebProvider.GetTempFileName();
            DataSet dataSet = null;
            try
            {
                // Copy the compressed data to a temporary file.
                try
                {
                    using (var fs = File.Create(tempFile))
                    {
                        using (var gs = new GZipStream(
                            new MemoryStream(data), CompressionMode.Decompress))
                        {
                            byte[] buffer = new byte[1024 ^ 2];
                            int count = 0;
                            while ((count = gs.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                fs.Write(buffer, 0, count);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    EventLog.Warn(ex);
                    return LicenceKeyResults.WriteDataFile;
                }

                // Use the temporary file to create new data set.
                try
                {
                    dataSet = StreamFactory.Create(tempFile);
                }
                catch (MobileException ex)
                {
                    EventLog.Warn(ex);
                    return LicenceKeyResults.Invalid;
                }

                // Check this is new data based on publish data and
                // number of available properties.
                if (dataSet.Published != WebProvider.ActiveProvider.DataSet.Published ||
                    dataSet.Properties.Count != WebProvider.ActiveProvider.DataSet.Properties.Count)
                {
                    // Rename the current master file to a temp file so enable the new
                    // master file to take it's place and to rollback if there's a problem.
                    var temp = String.Format("{0}.tmp", BinaryFile.FullName);
                    try
                    {
                        // Both the MD5 hash was good and the provider was created.
                        // Save the data and force the factory to reload.
                        if (BinaryFile.Exists)
                            File.Move(BinaryFile.FullName, temp);

                        File.Copy(tempFile, BinaryFile.FullName);

                        if (File.Exists(temp))
                            File.Delete(temp);
                    }
                    catch (Exception ex)
                    {
                        if (BinaryFile.Exists == false)
                            File.Move(temp, BinaryFile.FullName);
                        EventLog.Warn(ex);
                        return LicenceKeyResults.WriteDataFile;
                    }

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
                        lastModified = dataSet.Published;
                    }
                    BinaryFile.LastWriteTimeUtc = lastModified.ToUniversalTime();

                    EventLog.Info(String.Format(
                        "Automatically updated binary data file '{0}' with version " +
                        "published on the '{1:d}'.",
                        BinaryFile.FullName,
                        dataSet.Published));

                    return LicenceKeyResults.Success;
                }
            }
            catch (Exception ex)
            {
                throw new MobileException(
                    "Exception validating new data set",
                    ex);
            }
            finally
            {
                if (dataSet != null)
                    dataSet.Dispose();
                File.Delete(tempFile);
            }
            return LicenceKeyResults.GenericFailure;
        }
        
        #endregion
    }
}
