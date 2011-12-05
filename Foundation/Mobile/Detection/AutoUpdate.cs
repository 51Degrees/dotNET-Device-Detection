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
 * Mobile Experts Limited are Copyright (C) 2009 - 2011. All Rights Reserved.
 * 
 * Contributor(s):
 *     James Rosewell <james@51degrees.mobi>
 * 
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

        private static FileInfo _binaryFile = null;

        #endregion

        #region Properties

        /// <summary>
        /// Returns a list of the valid license keys available
        /// to the assembly.
        /// </summary>
        private static string[] LicenseKeys
        {
            get
            {
                var list = new List<string>();

                // See if a license key is included in the assembly.
                if (String.IsNullOrEmpty(Constants.PremiumLicenceKey) == false)
                    list.Add(Constants.PremiumLicenceKey);

                // Now try the bin folder for license key files.
                foreach (string fileName in Directory.GetFiles(
                    HostingEnvironment.ApplicationPhysicalPath + "bin", "*.lic"))
                {
                    var alltext = File.ReadAllText(fileName);
                    list.AddRange(alltext.Split(
                        new[] { "\r\n", "\r", "\n" },
                        StringSplitOptions.RemoveEmptyEntries));
                }

                return list.ToArray();
            }
        }

        /// <summary>
        /// Returns details of the binary file to be updated.
        /// </summary>
        private static FileInfo BinaryFile
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

        #endregion

        #region Thread Methods

        internal static void Run(object state)
        {
            try
            {
                if (Constants.AutoUpdateDelayedStart.TotalSeconds > 0 &&
                    LicenseKeys.Length > 0)
                {
                    // Pause for X seconds to all the worker process to complete starting up.
                    Thread.Sleep(Constants.AutoUpdateDelayedStart);

                    // Check the last accessed date of the binary file to determine
                    // if it should be updated.
                    if (BinaryFile != null &&
                        BinaryFile.LastWriteTimeUtc.Add(Constants.AutoUpdateWait) < DateTime.UtcNow)
                    {
                        // Download the latest data.
                        var parameters = new List<string>();
                        parameters.Add(String.Format("LicenseKeys={0}", String.Join("|", LicenseKeys)));
                        parameters.Add(String.Format("Download={0}", bool.TrueString));
                        parameters.Add("Type=Binary");

                        var client = new WebClient();
                        byte[] data = client.DownloadData(String.Format("{0}?{1}",
                            Constants.AutoUpdateUrl,
                            String.Join("&", parameters.ToArray())));

                        // Check the MD5 hash of the data downloaded.
                        string mdHash = client.ResponseHeaders["Content-MD5"];
                        if (mdHash != GetMd5Hash(data))
                            throw new MobileException("MD5 hash validation failure.");

                        // Create new provider with the data to ensure it is valid.
                        var provider = Binary.Reader.Create(data);
                        if (provider.AllDevices.Count == 0)
                            throw new MobileException("No devices found in downloaded data.");

                        // Both the MD5 hash was good and the provider was created.
                        // Save the data and force the factory to reload.
                        File.WriteAllBytes(BinaryFile.FullName, data);
                                                
                        // Sets the last modified time of the file downloaded.
                        DateTime lastModified = DateTime.MinValue;
                        if (DateTime.TryParse(
                            client.ResponseHeaders["Last-Modified"],
                            out lastModified))
                            BinaryFile.LastWriteTimeUtc = lastModified;

                        Factory._mobileCapabilities = null;

                        EventLog.Info(String.Format(
                            "Automatically updated binary data file '{0}' with new version " +
                            "dated the '{1:d}'.",
                            BinaryFile.FullName,
                            lastModified == DateTime.MinValue ? "Unknown" : lastModified.ToString("d")));
                    }
                }
            }
            catch(Exception ex)
            {
                EventLog.Warn(new MobileException(String.Format(
                    "Exception auto updating binary data file '{0}'.",
                    BinaryFile.FullName), ex));
            }
        }

        #endregion
    }
}
