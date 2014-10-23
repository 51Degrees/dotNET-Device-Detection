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
using System.IO;
using System.Web;
using FiftyOne.Foundation.Mobile.Detection.Configuration;
using FiftyOne.Foundation.Mobile.Detection.Entities;
using System.Collections.Generic;
using FiftyOne.Foundation.Mobile.Detection.Factories;
using System.Collections.Specialized;

namespace FiftyOne.Foundation.Mobile.Detection
{
    /// <summary>
    /// A provider with methods designed to be called from within 
    /// web applications where <see cref="HttpRequest"/> objects can
    /// be used to provide the necessary evidence for detection.
    /// </summary>
    public class WebProvider : Provider, IDisposable
    {
        #region Fields
        
        /// <summary>
        /// Used to create singleton instances.
        /// </summary>
        private readonly static object _lock = new object();

        private static DateTime? binaryFileLastModified = null;

        #endregion

        #region Constructor

        internal WebProvider() : base(Constants.CacheServiceInterval) { }

        internal WebProvider(DataSet dataSet)
            : base(dataSet, true, Constants.CacheServiceInterval)
        {
        }

        #endregion

        #region Static Properties

        /// <summary>
        /// A reference to the active provider.
        /// </summary>
        public static WebProvider ActiveProvider
        {
            get
            {
                if (_activeProvider == null)
                {
                    lock (_lock)
                    {
                        AppDomain.CurrentDomain.ProcessExit += ProcessExit;
                        _activeProvider = Create();
                    }
                }
                return _activeProvider;
            }
        }

        
        internal static WebProvider _activeProvider;

        /// <summary>
        /// A reference to the embedded provider.
        /// </summary>
        internal static WebProvider EmbeddedProvider
        {
            get
            {
                if (_embeddedProvider == null)
                {
                    lock (_lock)
                    {
                        _embeddedProvider = new WebProvider();
                    }
                }
                return _embeddedProvider;
            }
        }
        internal static WebProvider _embeddedProvider;

        /// <summary>
        /// Returns a temporary file name for the data file.
        /// </summary>
        /// <returns></returns>
        internal static string GetTempFileName()
        {
            var fileInfo = new FileInfo(Manager.BinaryFilePath);

            var fileName = String.Format(
                "{0}.{1}.tmp",
                fileInfo.Name,
                Guid.NewGuid());

            return Path.Combine(fileInfo.DirectoryName, fileName);
        }

        private static DateTime GetDataFileDate(FileInfo fileInfo)
        {
            var dataset = StreamFactory.Create(fileInfo.FullName);
            return dataset.Published;
        }

        /// <summary>
        /// Gets the file path of a temporary data file for use with a
        /// stream provider. This method will create a file if one does
        /// not already exist.
        /// </summary>
        /// <returns></returns>
        private static string GetTempWorkingFile()
        {
            string tempFileName = null;
            var masterFile = new FileInfo(Manager.BinaryFilePath);
            if (masterFile.Exists)
            {
                // Get the publish date of the master data file.
                var masterDate = GetDataFileDate(masterFile);

                // Check if there are any other tmp files.
                var fileNames = Directory.GetFiles(Mobile.Configuration.Support.GetFilePath("~/App_Data"));
                foreach (var fileName in fileNames)
                {
                    var file = new FileInfo(fileName);
                    if(file.FullName != masterFile.FullName &&
                    file.Name.StartsWith(masterFile.Name) &&
                        file.Extension == ".tmp")
                    {
                        // Check if temp file matches date of the master file.
                        try
                        {
                            var tempDate = GetDataFileDate(file);
                            if (tempDate == masterDate)
                            {
                                EventLog.Info("Using existing temp data file with published data {0} - \"{1}\"", tempDate.ToShortDateString(), file.FullName);
                                return fileName;
                            }
                        }
                        catch (Exception ex) // Exception may occur if file is not a 51Degrees file, no action is needed.
                        {
                            EventLog.Info("Error while reading temporary data file \"{0}\": {1}", file.FullName, ex.Message);
                        }
                    }
                }
                
                // No suitable temp file was found, create one in the
                //App_Data folder to enable the source file to be updated
                // without stopping the web site.
                tempFileName = GetTempFileName();

                // Copy the file to enable other processes to update it.
                try
                {
                    File.Copy(masterFile.FullName, tempFileName);
                    EventLog.Info("Created temp data file - \"{0}\"", tempFileName);
                }
                catch (IOException ex)
                {
                    throw new MobileException(String.Format("Could not create temporary file. Check worker process has write permissions to '{0}'. For medium trust environments ensure data file is located in App_Data.", tempFileName), ex);
                }
            }
            return tempFileName;
        }

        /// <summary>
        /// Cleans up any temporary files that remain from previous
        /// providers.
        /// </summary>
        private static void CleanTemporaryFiles()
        {
            try
            {
                if (String.IsNullOrEmpty(Manager.BinaryFilePath))
                    return;

                var fileInfo = new FileInfo(Manager.BinaryFilePath);

                var files = Directory.GetFiles(
                    fileInfo.DirectoryName,
                    String.Format("{0}.*.tmp", fileInfo.Name));
                foreach (var file in files)
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch (IOException ex)
                    {
                        EventLog.Debug(new MobileException(
                            String.Format(
                            "Exception deleting temporary file '{0}'",
                            file), ex));
                    }
                }
            }
            catch (Exception ex)
            {
                EventLog.Warn(new MobileException(
                    "Exception cleaning temporary files",
                    ex));
            }
        }

        /// <summary>
        /// Forces the provider to update current ActiveProvider with new data.
        /// </summary>
        private static WebProvider Create()
        {
            WebProvider provider = null;
            CleanTemporaryFiles();
            try
            {
                // Does a binary file exist?
                if (Manager.BinaryFilePath != null)
                {
                    if (File.Exists(Manager.BinaryFilePath))
                    {
                        binaryFileLastModified = new FileInfo(Manager.BinaryFilePath).LastWriteTimeUtc;
                        if (Manager.MemoryMode)
                        {
                            EventLog.Info(String.Format(
                                "Creating memory provider from binary data file '{0}'.",
                                Manager.BinaryFilePath));
                            provider = new WebProvider(MemoryFactory.Create(Manager.BinaryFilePath));
                        }
                        else
                        {
                            var tempFile = GetTempWorkingFile();
                            EventLog.Info(String.Format(
                                "Creating stream provider from binary data file '{0}'.",
                                tempFile));
                            provider = new WebProvider(StreamFactory.Create(tempFile));
                        }
                        EventLog.Info(String.Format(
                            "Created provider from binary data file '{0}'.",
                            Manager.BinaryFilePath));
                    }
                    else
                    {
                        EventLog.Info("Data file at '{0}' could not be found. Either it does not exist or " +
                            "there is insufficient permission to read it. Check the AppPool has read permissions " +
                            "and the application is not running in medium trust if the data file is not in the " +
                            "application directory.", Manager.BinaryFilePath);
                    }
                }
            }
            catch (Exception ex)
            {
                // Record the exception in the log file.
                EventLog.Fatal(
                    new MobileException(String.Format(
                        "Exception processing device data from binary file '{0}'. " +
                        "Enable debug level logging and try again to help identify cause.",
                        Manager.BinaryFilePath),
                        ex));
                // Reset the provider to enable it to be created from the embedded data.
                provider = null;
            }

            // Does the provider exist and has data been loaded?
            if (provider == null || provider.DataSet == null)
            {
                // No so initialise it with the embeddded binary data so at least we can do something.
                provider = EmbeddedProvider;
            }
            
            return provider;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Forces the application to reload the active provider on the next
        /// request.
        /// </summary>
        public static void Refresh()
        {
            _activeProvider = null;
        }

        /// <summary>
        /// Checks if the data file has been updated and refreshes if it has.
        /// </summary>
        /// <param name="state">Required for timer callback. This parameter is not used.</param>
        public static void CheckDataFileRefresh(object state)
        {
            if (Manager.BinaryFilePath != null &&
                File.Exists(Manager.BinaryFilePath))
            {
                var modifiedDate = new FileInfo(Manager.BinaryFilePath).LastWriteTimeUtc;
                if (binaryFileLastModified == null || modifiedDate > binaryFileLastModified)
                {
                    Refresh();
                }
            }
        }
        
        /// <summary>
        /// Disposes of the <see cref="DataSet"/> created by the 
        /// web provider.
        /// </summary>
        public void Dispose()
        {
            if (DataSet != null)
                DataSet.Dispose();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Close the data set elegantly.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void ProcessExit(object sender, EventArgs e)
        {
            var provider = _activeProvider;
            _activeProvider = null;
            if (provider != null &&
                provider != _embeddedProvider)
            {
                // Dispose of the data set to ensure any open file connections
                // are closed before the detector data set is freed for collection.
                provider.DataSet.Dispose();
            }
        }

        /// <summary>
        /// Returns the match results for the current context, or creates one if one
        /// does not already exist.
        /// </summary>
        /// <param name="context">Context needing to find the matching device</param>
        /// <returns></returns>
        internal static SortedList<string, string[]> GetResults(HttpContext context)
        {
            return GetResults(context, context.Request);
        }

        /// <summary>
        /// Returns the match results for the request, or creates one if one does not
        /// already exist. This method allows detection if the request is not related
        /// to the context, ie SetOverridenBrowser has been used.
        /// </summary>
        /// <param name="context">Context needing to find the matching device</param>
        /// <param name="request"></param>
        /// <returns></returns>
        internal static SortedList<string, string[]> GetResults(HttpContext context, HttpRequest request)
        {
            var matchKey = Constants.MatchKey + (request.UserAgent != null ? request.UserAgent.GetHashCode().ToString() : "");
            var hasOverrides = Feature.ProfileOverride.HasOverrides(context);
            var items = context.Items;
            var results = items[matchKey] as SortedList<string, string[]>;

            if (results == null || hasOverrides)
            {
                // A lock is needed in case 2 simultaneous operations are done on the
                // context at once. This is a rare use case but ensures robustness.
                lock (context)
                {
                    results = items[matchKey] as SortedList<string, string[]>;
                    if (results == null || hasOverrides)
                    {
                        // Get the match and store the list of properties and values 
                        // in the context and session.

                        var headers = GetRequiredHeaders(context, request);

                        var match = ActiveProvider.Match(headers);
                        if (match != null)
                        {
                            // Allow other feature detection methods to override profiles.
                            Feature.ProfileOverride.Override(context, match);

                            items[matchKey] = match.Results;
                            results = match.Results;
                        }
                    }
                }
            }
            return results;
        }

        /// <summary>
        /// Gets the required headers. Headers from HttpContext and HttpRequest may not be the same
        /// if the MVC method SetBrowserOverride has been use.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private static NameValueCollection GetRequiredHeaders(HttpContext context, HttpRequest request)
        {
            // A useragent might be url encoded if SetOverrideBrowser was used in a previous request.
            // A new header collection is required so that they can be modified.
            NameValueCollection headers;
            if (request.UserAgent != context.Request.UserAgent)
            {
                // Requests after SetOverrideBrowser are still expected to use the overriden alias.
                // The override useragent is stored in cookie which ASP.NET unpacks automatically, however
                // it is url encoded with pluses instead of spaces. This code transforms the useragent back,
                // but a new NameValueCollection is needed as the original collection cannot be modified.
                headers = new NameValueCollection(request.Headers.Count, request.Headers);
                if (headers[Constants.UserAgentHeader] != null)
                {
                    headers[Constants.UserAgentHeader] =
                        Uri.UnescapeDataString(headers[Constants.UserAgentHeader]).Replace('+', ' ');
                }
            }
            else
            {
                // Use the original headers as there's no override in place.
                headers = request.Headers;
            }
            return headers;
        }
        
        #endregion
    }
}
