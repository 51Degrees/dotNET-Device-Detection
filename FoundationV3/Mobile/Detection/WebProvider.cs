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
using System.Linq;
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

        /// <summary>
        /// Constructs a new instance of the provider with the data set 
        /// supplied.
        /// </summary>
        /// <param name="dataSet"></param>
        private WebProvider(DataSet dataSet)
            : base(dataSet, false, Constants.UserAgentCacheSize)
        {
        }

        #endregion

        #region Static Properties

        /// <summary>
        /// A reference to the active provider.
        /// </summary>
        internal static WebProvider GetActiveProvider()
        {
            if (_activeProviderCreated == false)
            {
                lock (_lock)
                {
                    if (_activeProviderCreated == false)
                    {
                        _activeProvider = Create();

                        // The DetectorModule should set to listen for the 
                        // application reycling or shutting down. This is belt
                        // and braces in case thte HttpModule method fails to 
                        // fire the application end event.
                        AppDomain.CurrentDomain.ProcessExit += OnProcessExit;

                        _activeProviderCreated = true;
                    }
                }
            }
            return _activeProvider;
        }
        private static WebProvider _activeProvider;
        private static bool _activeProviderCreated = false;

        /// <summary>
        /// A reference to the active provider.
        /// </summary>
        public static WebProvider ActiveProvider
        {
            get
            {
                return GetActiveProvider();
            }
        }
        
        /// <summary>
        /// Returns a temporary file name for the data file.
        /// </summary>
        /// <returns>A temporary file in the App_Data folder</returns>
        internal static string GetTempFileName()
        {
            var directoryInfo = new DirectoryInfo(
                Mobile.Configuration.Support.GetFilePath(Constants.TemporaryFilePath));
            var fileInfo = new FileInfo(Manager.BinaryFilePath);
            return Path.Combine(
                directoryInfo.FullName,
                String.Format("{0}.{1}.tmp",
                fileInfo.Name,
                Guid.NewGuid()));
        }

        /// <summary>
        /// Creates a stream data set for the file provided and 
        /// returns the published data of the file. Throws an 
        /// exception if there's a problem accessing the file.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>The date time from the file, or null if the file is not a valid data file</returns>
        internal static DateTime? GetDataFileDate(string fileName)
        {
            try
            {
                using (var stream = File.OpenRead(fileName))
                {
                    using (var reader = new BinaryReader(stream))
                    {
                        using (var dataSet = new DataSet(reader))
                        {
                            return dataSet.Published;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                EventLog.Info(String.Format(
                    "Exception getting data file date from file '{0}'",
                    fileName));
                EventLog.Debug(ex);
                return null;
            }
        }

        /// <summary>
        /// Returns a data set initialised with a temporary data file.
        /// If an existing one is present that can be used then it will
        /// be favoured. If a new one is needed then one will be created.
        /// </summary>
        /// <returns></returns>
        private static DataSet GetTempFileDataSet()
        {
            DataSet dataSet = null;
            var masterFile = new FileInfo(Manager.BinaryFilePath);
            if (masterFile.Exists)
            {
                // Get the publish date of the master data file.
                var masterDate = GetDataFileDate(masterFile.FullName);

                // Make sure the temporary directory exists.
                var directory = new DirectoryInfo(
                    Mobile.Configuration.Support.GetFilePath(Constants.TemporaryFilePath));
                if (directory.Exists == false)
                {
                    directory.Create();
                }

                // Get the first matching temporary file if one exists.
                var temporaryFile = directory.GetFiles("*.tmp").FirstOrDefault(i =>
                        i.Exists &&
                        i.FullName.Equals(masterFile.FullName) == false &&
                        i.Name.StartsWith(masterFile.Name) &&
                        masterDate.Equals(GetDataFileDate(i.FullName)));
                
                if (temporaryFile != null)
                {
                    // Use the existing temporary file.
                    EventLog.Info(
                        "Using existing temp data file with published data {0} - \"{1}\"",
                        masterDate.HasValue ? masterDate.Value.ToShortDateString() : "null",
                        temporaryFile.FullName);
                }
                else
                {
                    // No suitable temp file was found, create one in the
                    // temporary file folder to enable the source file to be updated
                    // without stopping the web site.
                    temporaryFile = new FileInfo(GetTempFileName());

                    // Copy the file to enable other processes to update it.
                    try
                    {
                        File.Copy(masterFile.FullName, temporaryFile.FullName);
                        EventLog.Info("Created temp data file - \"{0}\"", temporaryFile.FullName);
                    }
                    catch (IOException ex)
                    {
                        throw new MobileException(String.Format(
                            "Could not create temporary file. Check worker process has " +
                            "write permissions to '{0}'. For medium trust environments " +
                            "ensure data file is located in App_Data.", 
                            temporaryFile.FullName), ex);
                    }
                }
                dataSet = StreamFactory.Create(temporaryFile.FullName);
            }
            return dataSet;
        }

        /// <summary>
        /// Cleans up any temporary files that remain from previous
        /// providers.
        /// </summary>
        private static void CleanTemporaryFiles()
        {
            try
            {
                var directory = new DirectoryInfo(
                                    Mobile.Configuration.Support.GetFilePath(Constants.TemporaryFilePath));
                foreach (var file in directory.GetFiles())
                {
                    try
                    {
                        file.Delete();
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
                            provider = new WebProvider(GetTempFileDataSet());
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
            }

            // Does the provider exist and has data been loaded?
            if (provider == null || provider.DataSet == null)
            {
                EventLog.Fatal("No data source available to create provider.");
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
            var provider = _activeProvider;
            _activeProvider = null;
            _activeProviderCreated = false;
            if (provider != null)
            {
                // Dispose of the old provider.
                provider.Dispose();
            }
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
            {
                DataSet.Dispose();
            }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// The application is being disposed of either due to a recycle event or 
        /// shutdown. Ensure the active provider is disposed of to release resources.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnProcessExit(object sender, EventArgs e)
        {
            try
            {
                var provider = _activeProvider;
                if (provider != null)
                {
                    provider.Dispose();
                }
            }
            finally
            {
                _activeProvider = null;
                _activeProviderCreated = false;
            }
        }

        /// <summary>
        /// Returns a collection that can be used to persist data across the request. If 
        /// using .NET 4 or above the HttpRequest context items collection is used. If lower
        /// versions then the current context, if it exists is used. If there is no collection
        /// available then null is returned and the requesting routine will need to work
        /// without the benefit of persisting the detection result across the request.
        /// </summary>
        /// <remarks>
        /// The implementation varies by .NET version.
        /// </remarks>
        /// <param name="request"></param>
        /// <returns>A collection for storing data across the request or null</returns>
        internal static System.Collections.IDictionary GetContextItems(HttpRequest request)
        {
            System.Collections.IDictionary items = null;
            if (request.RequestContext != null &&
                request.RequestContext.HttpContext !=null)
            {
                items = request.RequestContext.HttpContext.Items;
            }
            return items;
        }

        /// <summary>
        /// Returns the match results for the request, or creates one if one does not
        /// already exist. If the match has already been calculated it is fetched
        /// from the context items collection.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>The match for the request</returns>
        internal static Match GetMatch(HttpRequest request)
        {
            var matchKey = Constants.MatchKey + (request.UserAgent != null ? request.UserAgent.GetHashCode().ToString() : "");
            var hasOverrides = Feature.ProfileOverride.HasOverrides(request);
            
            // Get a collection to store data across the request. One may not be returned
            // depending of the configuration of the server or the current stage in the 
            // pipeline.
            var items = GetContextItems(request);

            // Get the results from the items collection if if exists already.
            var match = items != null ? items[matchKey] as Match : null;

            if (match == null || hasOverrides)
            {
                // A lock is needed in case 2 simultaneous operations are done on the
                // request at the sametime. This is a rare use case but ensures robustness.
                lock (request)
                {
                    match = items != null ? items[matchKey] as Match : null;
                    var provider = GetActiveProvider();
                    if ((match == null || hasOverrides) && provider != null)
                    {
                        // Create the match object ready to store the results.
                        match = provider.CreateMatch();
                        match.Timer.Reset();
                        match.Timer.Start();

                        // Get the match and store the list of properties and values 
                        // in the context and session.
                        match = provider.Match(GetRequiredHeaders(request), match);
                            
                        // Allow other feature detection methods to override profiles.
                        Feature.ProfileOverride.Override(request, match);
                        if (items != null)
                        {
                            items[matchKey] = match;
                        }

                        match.Timer.Stop();
                        match.Elapsed = new TimeSpan(match.Timer.ElapsedTicks);
                    }
                }
            }
            
            return match;
        }

        /// <summary>
        /// Gets the required headers. If the MVC method SetBrowserOverride has been used
        /// the user agent will be escapted and may contain + characters which need to 
        /// be removed.
        /// </summary>
        /// <remarks>
        /// Requests after SetOverrideBrowser are still expected to use the overriden alias.
        /// The override useragent is stored in cookie which ASP.NET unpacks automatically, however
        /// it is url encoded with pluses instead of spaces. This code transforms the useragent back,
        /// but a new NameValueCollection is needed as the original collection cannot be modified.
        /// </remarks>
        /// <param name="request"></param>
        /// <returns>A clean header collection ready for device detection</returns>
        private static NameValueCollection GetRequiredHeaders(HttpRequest request)
        {
            // A useragent might be url encoded if SetOverrideBrowser was used in a previous request.
            // This is checked by comparing the escaped string is different against the original.

            NameValueCollection headers;
            var escapedUA = request.UserAgent != null ?
                Uri.UnescapeDataString(request.UserAgent).Replace('+', ' ') :
                null;
            if (escapedUA != request.UserAgent)
            {
                // Requests after SetOverrideBrowser are still expected to use the overriden alias.
                // The override useragent is stored in cookie which ASP.NET unpacks automatically, however
                // it is url encoded with pluses instead of spaces. This code transforms the useragent back,
                // but a new NameValueCollection is needed as the original collection cannot be modified.
                headers = new NameValueCollection(request.Headers.Count, request.Headers);
                if (headers[Constants.UserAgentHeader] != null)
                {
                    headers[Constants.UserAgentHeader] = escapedUA;
                }
            }
            else
            {
                // No MVC defect so use the headers unaltered.
                headers = request.Headers;
            }
            return headers;
        }
        
        #endregion
    }
}
