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
using System.Text.RegularExpressions;
using FiftyOne.Foundation.Licence;

namespace FiftyOne.Foundation.Mobile.Detection
{
    /// <summary>
    /// Used to fetch new device data from 51Degrees if a Premium or 
    /// Enterprise. Requires a valid 51Degrees licence key and 
    /// read/write access to the file system folder where the downloaded
    /// file should be written.
    /// </summary>
    /// <para>
    /// Requires a valid 51Degrees licence key and read/write access to the file 
    /// system folder where the downloaded file should be written.
    /// Get a licence key: https://51degrees.com/compare-data-options
    /// </para>
    /// <para>
    /// Auto update can be enabled/disabled in the 51Degrees.config file.
    /// Licence key should be placed in a .lic file in the bin folder.
    /// </para>
    /// <para>
    /// For general information on how 51Degrees automatic updates work see: 
    /// https://51degrees.com/support/documentation/automatic-updates"/>
    /// </para>
    internal static class AutoUpdate
    {
        #region Classes & Enumerations

        public enum AutoUpdateStatus
        {
            /// <summary>
            /// Update completed successfully. 
            /// </summary>
            AUTO_UPDATE_SUCCESS,
            /// <summary>
            /// HTTPS connection could not be established. 
            /// </summary>
            AUTO_UPDATE_HTTPS_ERR,
            /// <summary>
            /// No need to perform update. 
            /// </summary>
            AUTO_UPDATE_NOT_NEEDED,
            /// <summary>
            /// Update currently under way. 
            /// </summary>
            AUTO_UPDATE_IN_PROGRESS,
            /// <summary>
            /// Path to master file is directory not file
            /// </summary>
            AUTO_UPDATE_MASTER_FILE_CANT_RENAME,
            /// <summary>
            /// 51Degrees server responded with 429: too many attempts. 
            /// /// </summary>
            AUTO_UPDATE_ERR_429_TOO_MANY_ATTEMPTS,
            /// <summary>
            /// 51Degrees server responded with 403 meaning key is blacklisted. 
            /// </summary>
            AUTO_UPDATE_ERR_403_FORBIDDEN,
            /// <summary>
            /// Used when IO oerations with input or output stream failed. 
            /// </summary>
            AUTO_UPDATE_ERR_READING_STREAM,
            /// <summary>
            /// MD5 validation failed 
            /// </summary>
            AUTO_UPDATE_ERR_MD5_VALIDATION_FAILED,
            /// <summary>
            /// The new data file can't be renamed to replace the previous one.
            /// </summary>
            AUTO_UPDATE_NEW_FILE_CANT_RENAME
        }

        /// <summary>
        /// Stores critical data set attributes used to determine if the 
        /// downloaded data should be used to replace the current data file. 
        /// Using this class avoids the need for two Stream generated data sets
        /// to be held at the same time reducing memory consumption.
        /// </summary>
        private class DataSetAttributes
        {

            /// <summary>
            /// Date the data set was published.
            /// </summary>
            internal readonly DateTime Published;

            /// <summary>
            /// Number of properties contained in the data set.
            /// </summary>
            internal readonly int PropertyCount;

            /// <summary>
            /// Constructs a new instance of CriticalDataSetAttributes using 
            /// the data set provided. Assumes the file passed to the 
            /// constructor exists.
            /// </summary>
            /// <param name="dataFile">
            /// The data file whose attributes should be copied.
            /// </param>
            internal DataSetAttributes(FileInfo dataFile)
            {
                DataSet dataSet = StreamFactory.Create(
                        dataFile.FullName, false);
                try
                {
                    Published = dataSet.Published;
                    PropertyCount = dataSet.Properties.Count;
                }
                finally
                {
                    dataSet.Dispose();
                }
            }
        }

        #endregion

        #region Fields

        /// <summary>
        /// Used to validate licence keys before using them for automatic
        /// downloads.
        /// </summary>
        private readonly static Regex LicenceKeyValidation = new Regex(
            Constants.LicenceKeyValidationRegex, RegexOptions.Compiled);

        /// <summary>
        /// Used to signal between multiple instances of the update
        /// threads.
        /// </summary>
        private static AutoResetEvent _autoUpdateSignal = 
            new AutoResetEvent(true);

        #endregion

        #region Thread Methods

        /// <summary>
        /// Checks if a new data file is available for download and if it is
        /// newer than the current data on disk, if enough time has passed 
        /// between now and the write time of the current data in memory. See 
        /// Constants.AutoUpdateWait. This method is designed to be used in a 
        /// seperate thread from the one performing detection.
        /// </summary>
        internal static void CheckForUpdate(object state)
        {
            try
            {
                // If licence keys are available auto update.
                if (LicenceKey.Keys.Length > 0)
                {
                    // Check that there is a binary file, and that either the
                    // active provider is not available indicating no source 
                    // data file, or if is available that the next update is 
                    // in the past, or that the device data being used is the
                    // free lite version.
                    if (MasterBinaryDataFile != null &&
                        (WebProvider.ActiveProvider == null ||
                        WebProvider.ActiveProvider.DataSet.NextUpdate < 
                            DateTime.UtcNow ||
                        WebProvider.ActiveProvider.DataSet.Name == "Lite"))
                    {
                        if (Download(LicenceKey.Keys) == 
                            LicenceKeyResults.Success)
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
                if (MasterBinaryDataFile != null && MasterBinaryDataFile.FullName != null)
                {
                    EventLog.Warn(new MobileException(String.Format(
                        "Exception auto update download binary data file '{0}'.",
                        MasterBinaryDataFile.FullName), ex));
                }
                else
                {
                    EventLog.Fatal(ex);
                }
            }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Returns details of the master binary file. Refreshes the file
        /// info incase anything has changed since the instance was 
        /// created.
        /// </summary>
        internal static FileInfo MasterBinaryDataFile
        {
            get
            {
                if (_binaryFile == null)
                {
                    _binaryFile = new FileInfo(Manager.BinaryFilePath);
                }
                _binaryFile.Refresh();
                return _binaryFile;
            }
        }
        private static FileInfo _binaryFile = null;

        /// <summary>
        /// Uses the given license key to perform a device data update, writing
        /// the data to the file system and filling providers from this factory 
        /// instance with it.
        /// </summary>
        /// <param name="licenseKeys">
        /// The licence keys to submit to the server for authentication.
        /// </param>
        /// <returns>
        /// The result of the update to enable user reporting.
        /// </returns>
        internal static LicenceKeyResults Download(string[] licenseKeys)
        {
            switch (Update(licenseKeys, MasterBinaryDataFile))
            {
                case AutoUpdateStatus.AUTO_UPDATE_ERR_403_FORBIDDEN:
                case AutoUpdateStatus.AUTO_UPDATE_ERR_429_TOO_MANY_ATTEMPTS:
                case AutoUpdateStatus.AUTO_UPDATE_ERR_MD5_VALIDATION_FAILED:
                case AutoUpdateStatus.AUTO_UPDATE_ERR_READING_STREAM:
                case AutoUpdateStatus.AUTO_UPDATE_HTTPS_ERR:
                    return LicenceKeyResults.Https;
                case AutoUpdateStatus.AUTO_UPDATE_MASTER_FILE_CANT_RENAME:
                case AutoUpdateStatus.AUTO_UPDATE_NEW_FILE_CANT_RENAME:
                    return LicenceKeyResults.WriteDataFile;
                case AutoUpdateStatus.AUTO_UPDATE_NOT_NEEDED:
                    return LicenceKeyResults.UpdateNotNeeded;
                case AutoUpdateStatus.AUTO_UPDATE_SUCCESS:
                    return LicenceKeyResults.Success;
                default:
                    return LicenceKeyResults.GenericFailure;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Uses the given license key to perform a device data update, 
        /// writing the data to the file system and filling providers from 
        /// this factory instance with it.
        /// </summary>
        /// <param name="dataFile">
        /// Location of the data file to update or create.
        /// </param>
        /// <param name="licenseKey">
        /// The licence key to use for the update request.
        /// </param>
        /// <returns>
        /// The result of the update to enable user reporting.
        /// </returns>
        public static AutoUpdateStatus Update(
            string licenseKey, FileInfo dataFile)
        {
            return Update(new string[] { licenseKey }, dataFile);
        }

        /// <summary>
        /// Uses the given license keys to perform a device data update, 
        /// writing the data to the file system and filling providers from 
        /// this factory instance with it.
        /// </summary>
        /// <param name="dataFile">
        /// Location of the data file to update or create.
        /// </param>
        /// <param name="licenseKeys">
        /// The licence keys to use for the update request.
        /// </param>
        /// <returns>
        /// The result of the update to enable user reporting.
        /// </returns>
        public static AutoUpdateStatus Update(
            IList<string> licenseKeys, FileInfo dataFile)
        {
            if (licenseKeys == null || licenseKeys.Count == 0)
            {
                throw new ArgumentException(
                    "At least one valid licence key is required to update " +
                    "device data. See " +
                    "https://51degrees.com/compare-data-options " +
                    "to acquire valid licence keys.");
            }

            var validKeys = GetValidKeys(licenseKeys);
            if (validKeys.Count == 0)
            {
                throw new ArgumentException(
                    "The license key(s) provided were invalid. See " +
                    "https://51degrees.com/compare-data-options " +
                    "to acquire valid licence keys.");
            }
            return Download(validKeys, dataFile);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Downloads and updates the premium data file.
        /// </summary>
        /// <param name="licenceKeys">
        /// The licence key to use for the update request.
        /// </param>
        /// <param name="binaryFile">
        /// Location of the master data file.
        /// </param>
        /// <returns>
        /// The result of the download to enable user reporting.
        /// </returns>
        private static AutoUpdateStatus Download(
            IList<string> licenceKeys, FileInfo binaryFile)
        {
            AutoUpdateStatus result = AutoUpdateStatus.AUTO_UPDATE_IN_PROGRESS;

            // Set the three files needed to support the download, verification
            // and eventual activation.
            var compressedTempFile = GetTempFileName(binaryFile);
            var uncompressedTempFile = GetTempFileName(binaryFile);

            try
            {
                // Acquire a lock so that only one thread can enter this 
                // critical section at any given time. This is required to 
                // prevent multiple threads from performing the update 
                // simultaneously, i.e. if more than one thread is capable of 
                // invoking AutoUpdate.
                _autoUpdateSignal.WaitOne();

                // Download the device data, decompress, check validity and
                // finally replace the existing data file if all okay.
                var client = new WebClient();
                result = DownloadFile(
                    binaryFile,
                    compressedTempFile,
                    client,
                    FullUrl(licenceKeys));

                if (result == AutoUpdateStatus.AUTO_UPDATE_IN_PROGRESS)
                {
                    result = CheckedDownloadedFileMD5(
                        client,
                        compressedTempFile);
                }

                if (result == AutoUpdateStatus.AUTO_UPDATE_IN_PROGRESS)
                {
                    result = Decompress(
                        compressedTempFile, uncompressedTempFile);
                }

                if (result == AutoUpdateStatus.AUTO_UPDATE_IN_PROGRESS)
                {
                    result = ValidateDownloadedFile(
                        binaryFile, uncompressedTempFile);
                }

                if (result == AutoUpdateStatus.AUTO_UPDATE_IN_PROGRESS)
                {
                    result = ActivateDownloadedFile(
                            client, binaryFile, uncompressedTempFile);
                }
            }
            finally
            {
                try
                {
                    if (compressedTempFile.Exists)
                    {
                        compressedTempFile.Delete();
                    }
                    if (uncompressedTempFile.Exists)
                    {
                        uncompressedTempFile.Delete();
                    }
                }
                finally
                {
                    // No matter what, release the critical section lock.
                    _autoUpdateSignal.Set();
                }
            }
            return result;
        }

        /// <summary>
        /// Method performs the actual download by setting up and sending 
        /// request and processing the response.
        /// </summary>
        /// <param name="binaryFile">
        /// File reference for the current data file.
        /// </param>
        /// <param name="client">
        /// Web client used to download the URL provided.
        /// </param>
        /// <param name="compressedTempFile">
        /// File to write compressed downloaded.
        /// </param>
        /// <param name="fullUrl">
        /// URL to use to download the data file.
        /// </param>
        /// <returns>The current status of the overall process.</returns>
        private static AutoUpdateStatus DownloadFile(
                FileInfo binaryFile,
                FileInfo compressedTempFile,
                WebClient client,
                Uri fullUrl)
        {
            AutoUpdateStatus result = AutoUpdateStatus.AUTO_UPDATE_IN_PROGRESS;

            // Set the last modified header if available from the current
            // binary data file.
            if (binaryFile.Exists)
            {
                client.Headers.Add(
                    HttpRequestHeader.LastModified,
                    binaryFile.LastWriteTimeUtc.ToString("R"));
            }

            // If the response is okay then download the file to the temporary
            // compressed data file. If not then set the response code 
            // accordingly.
            try
            {
                client.DownloadFile(fullUrl, compressedTempFile.FullName);
            }
            catch (SecurityException)
            {
                result = AutoUpdateStatus.AUTO_UPDATE_HTTPS_ERR;
            }
            catch (WebException ex)
            {
                //Server response was not 200. Data download can not commence.
                var response = ex.Response as HttpWebResponse;
                if (response != null)
                {
                    switch (response.StatusCode)
                    {
                        // Note: needed because TooManyRequests is not available in
                        // earlier versions of the HttpStatusCode enum.
                        case ((HttpStatusCode)429):
                            result = AutoUpdateStatus.
                                AUTO_UPDATE_ERR_429_TOO_MANY_ATTEMPTS;
                            break;
                        case HttpStatusCode.NotModified:
                            result = AutoUpdateStatus.AUTO_UPDATE_NOT_NEEDED;
                            break;
                        case HttpStatusCode.Forbidden:
                            result = AutoUpdateStatus.AUTO_UPDATE_ERR_403_FORBIDDEN;
                            break;
                        default:
                            result = AutoUpdateStatus.AUTO_UPDATE_HTTPS_ERR;
                            break;
                    }
                }
                else
                {
                    result = AutoUpdateStatus.AUTO_UPDATE_HTTPS_ERR;
                }
            }

            return result;
        }

        /// <summary>
        /// Verifies that the data has been downloaded correctly by comparing 
        /// an MD5 hash off the downloaded data with one taken before the data
        /// was sent, which is stored in a response header.
        /// </summary>
        /// <param name="client">
        /// The Premium data download connection.
        /// </param>
        /// <param name="compressedTempFile">
        /// The path to compressed data file that has been downloaded.
        /// </param>
        /// <returns>True if the hashes match, otherwise false.</returns>
        private static AutoUpdateStatus CheckedDownloadedFileMD5(
                WebClient client, FileInfo compressedTempFile)
        {
            AutoUpdateStatus status = AutoUpdateStatus.AUTO_UPDATE_IN_PROGRESS;
            string serverHash = client.ResponseHeaders["Content-MD5"];
            string downloadHash = GetMd5Hash(compressedTempFile);
            if (serverHash == null ||
                serverHash.Equals(downloadHash) == false)
            {
                status = AutoUpdateStatus.AUTO_UPDATE_ERR_MD5_VALIDATION_FAILED;
            }
            return status;
        }

        /// <summary>
        /// Reads a source GZip file and writes the uncompressed data to 
        /// destination file.
        /// </summary>
        /// <param name="destinationPath">
        /// Path to GZip file to load from.
        /// </param>
        /// <param name="sourcePath">
        /// Path to file to write the uncompressed data to.
        /// </param>
        /// <returns>The current state of the update process.</returns>
        private static AutoUpdateStatus Decompress(
                FileInfo sourcePath, FileInfo destinationPath)
        {
            AutoUpdateStatus status = AutoUpdateStatus.AUTO_UPDATE_IN_PROGRESS;
            using (var fis = new GZipStream(
                sourcePath.OpenRead(), CompressionMode.Decompress))
            {
                using (var fos = destinationPath.Create())
                {
                    fis.CopyTo(fos);
                }
            }
            return status;
        }

        /// <summary>
        /// Method compares the downloaded data file to the existing data 
        /// file to check if the update is required. This will prevent file 
        /// switching if the data file was downloaded but is not newer than 
        /// the existing data file.
        /// </summary>
        /// <remarks>
        /// The following conditions must be met for the data file to be 
        /// considered newer than the current master data file:
        /// 1. Current master data file does not exist.
        /// 2. If the published dates are not the same.
        /// 3. If the number of properties is not the same.
        /// </remarks>
        /// <param name="binaryFile">
        /// Current file to compare against.
        /// </param>
        /// <param name="decompressedTempFile">
        /// Path to the decompressed downloaded file.
        /// </param>
        /// <returns>The current state of the update process.</returns>
        private static AutoUpdateStatus ValidateDownloadedFile(
                FileInfo binaryFile, FileInfo decompressedTempFile)
        {
            AutoUpdateStatus status = AutoUpdateStatus.AUTO_UPDATE_IN_PROGRESS;
            if (decompressedTempFile.Exists)
            {

                // This will throw an exception if the downloaded data file 
                // can't be used to get the required attributes. The exception 
                // is a key part of the validation process.
                DataSetAttributes tempAttrs = new DataSetAttributes(
                        decompressedTempFile);

                // If the current binary file exists then compare the two for
                // the same published date and same properties. If either value
                // is different then the data file should be accepted. If 
                // they're the same then the update is not needed.
                if (binaryFile.Exists)
                {
                    DataSetAttributes binaryAttrs = new DataSetAttributes(
                        binaryFile);
                    if (binaryAttrs.Published != tempAttrs.Published ||
                        binaryAttrs.PropertyCount != tempAttrs.PropertyCount)
                    {
                        status = AutoUpdateStatus.AUTO_UPDATE_IN_PROGRESS;
                    }
                    else
                    {
                        status = AutoUpdateStatus.AUTO_UPDATE_NOT_NEEDED;
                    }
                }
            }
            return status;
        }

        /// <summary>
        /// Method represents the final stage of the auto update process. The 
        /// uncompressed file is swapped in place of the existing master file.
        /// </summary>
        /// <param name="binaryFile">
        /// Path to a binary data that should be set to the downloaded data.
        /// </param>
        /// <param name="client">
        /// Used to get the Last-Modified HTTP header value.
        /// </param>
        /// <param name="uncompressedTempFile">
        /// File object containing the uncompressed version of the data file 
        /// downloaded from 51Degrees update server.
        /// </param>
        /// <returns>The current state of the update process.</returns>
        private static AutoUpdateStatus ActivateDownloadedFile(
                WebClient client,
                FileInfo binaryFile,
                FileInfo uncompressedTempFile)
        {
            AutoUpdateStatus status = AutoUpdateStatus.AUTO_UPDATE_IN_PROGRESS;
            bool backedUp = true;
            FileInfo tempCopyofCurrentMaster = new FileInfo(
                    binaryFile.FullName + ".replacing");
            try
            {
                // Keep a copy of the old data in case we need to go back to it.
                binaryFile.Refresh();
                if (binaryFile.Exists)
                {
                    try
                    {
                        File.Move(binaryFile.FullName, 
                            tempCopyofCurrentMaster.FullName);
                        backedUp = true;
                    }
                    catch
                    {
                        // The current master file can not be moved so the
                        // backup has not happened. Do not continue to try and 
                        // update the data file.
                        backedUp = false;
                    }
                }

                // If the backup of the master data file exists then switch the 
                // files.
                if (backedUp)
                {
                    try
                    {
                        // Copy the new file to the master file.
                        File.Move(uncompressedTempFile.FullName, 
                            binaryFile.FullName);
                        
                        // Set the binary file's last modified date to the one 
                        // provided from the web server with the download. This
                        // date will be used when checking for future updates
                        // to avoid downloading the file if there is no update.
                        binaryFile.LastWriteTimeUtc = GetLastModified(client);
                        status = AutoUpdateStatus.AUTO_UPDATE_SUCCESS;
                    }
                    catch
                    {
                        status = AutoUpdateStatus.AUTO_UPDATE_NEW_FILE_CANT_RENAME;
                    }
                }
                else
                {
                    status = AutoUpdateStatus.AUTO_UPDATE_MASTER_FILE_CANT_RENAME;
                }
            }
            catch (Exception ex)
            {
                binaryFile.Refresh();
                if (binaryFile.Exists == false &&
                    tempCopyofCurrentMaster.Exists == true)
                {
                    tempCopyofCurrentMaster.MoveTo(binaryFile.FullName);
                }
                throw ex;
            }
            finally
            {
                tempCopyofCurrentMaster.Refresh();
                if (tempCopyofCurrentMaster.Exists)
                {
                    tempCopyofCurrentMaster.Delete();
                }
            }
            return status;
        }

        /// <summary>
        /// Method initialises path to the a temporary file used during the
        /// auto update process.
        /// </summary>
        /// <remarks>
        /// The original data file does not have to exist, but the directory
        /// provided must exist and the path should not be a directory.
        /// </remarks>
        /// <param name="dataFile">
        /// Data file to use as the prefix of the temp file.
        /// </param>
        private static FileInfo GetTempFileName(FileInfo dataFile)
        {
            var sb = new StringBuilder();
            sb.Append(dataFile.FullName);
            sb.Append(".");
            sb.Append(Guid.NewGuid().ToString());
            sb.Append(".tmp");
            return new FileInfo(sb.ToString());
        }

        /// <summary>
        /// Returns the last modified date from the HTTP response.
        /// </summary>
        /// <param name="client">Webclient used to download the data.</param>
        /// <returns>
        /// The last modified date from the HTTP response, or min date if not 
        /// provided.
        /// </returns>
        private static DateTime GetLastModified(WebClient client)
        {
            DateTime lastModified;
            if (DateTime.TryParseExact(
                client.ResponseHeaders[HttpResponseHeader.LastModified],
                "R",
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal,
                out lastModified) == false)
            {
                lastModified = DateTime.MinValue;
            }
            return lastModified;
        }

        /// <summary>
        /// Validate the supplied keys to exclude keys from 3rd party products
        /// from being used.
        /// </summary>
        /// <param name="licenseKeys">
        /// An array of licence key strings to validate.
        /// </param>
        /// <returns>An array of valid licence keys.</returns>
        private static List<string> GetValidKeys(IList<string> licenseKeys)
        {
            List<string> validKeys = new List<string>();
            foreach (string key in licenseKeys)
            {
                if (LicenceKeyValidation.IsMatch(key) &&
                    new Key(key).IsValid)
                {
                    validKeys.Add(key);
                }
            }
            return validKeys;
        }

        /// <summary>
        /// Constructs the URL needed to download Enhanced device data.
        /// </summary>
        /// <param name="licenseKeys">Array of licence key strings.</param>
        /// <returns>The URL for the data download.</returns>
        private static Uri FullUrl(IList<string> licenseKeys)
        {
            string[] parameters = {
                "LicenseKeys=" + String.Join("|", licenseKeys),
                "Download=True",
                "Type=BinaryV32"};
            string url = String.Concat(
                Constants.AutoUpdateUrl,
                "?",
                String.Join("&", parameters));
            return new Uri(url);
        }

        /// <summary>
        /// Calculates the MD5 hash of the given data array.
        /// </summary>
        /// <param name="file">calculate MD5 of this file.</param>
        /// <returns>The MD5 hash of the given data.</returns>
        private static string GetMd5Hash(FileInfo file)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                using (var stream = file.OpenRead())
                {
                    return GetMd5Hash(md5Hash, stream);
                }
            }
        }

        /// <summary>
        /// Calculates the MD5 hash of the given data array.
        /// </summary>
        /// <param name="stream">calculate MD5 of this stream</param>
        /// <param name="md5Hash">instance of MD5 hash calculator</param>
        /// <returns>The MD5 hash of the given data.</returns>
        private static string GetMd5Hash(MD5 md5Hash, Stream stream)
        {
            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(stream);

            // Create a new stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sb = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sb.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sb.ToString();
        }

        #endregion
    }
}