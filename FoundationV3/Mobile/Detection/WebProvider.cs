/* *********************************************************************
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited. 
 * Copyright © 2015 51Degrees Mobile Experts Limited, 5 Charlotte Close,
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
using System.Reflection;

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

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a new instance of the provider with the data set 
        /// supplied.
        /// </summary>
        /// <param name="dataSet"></param>
        private WebProvider(DataSet dataSet)
            : base(dataSet, true, Constants.UserAgentCacheSize)
        {
        }

        #endregion

        #region Destructor

        /// <summary>
        /// Disposes of the DataSet if one is available.
        /// </summary>
        /// <param name="disposing">
        /// True if the calling method is Dispose, false for the finaliser.
        /// </param>        
        protected override void Dispose(bool disposing)
        {
            if (DataSet != null)
            {
                DataSet.Dispose();
            }
            base.Dispose(disposing);
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
        internal static bool _activeProviderCreated = false;

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
            var directoryInfo = Directory.CreateDirectory(
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
                using (var reader = new BinaryReader(File.OpenRead(fileName)))
                {
                    using (var dataSet = new DataSet(File.GetLastWriteTimeUtc(fileName), DataSet.Modes.File))
                    {
                        CommonFactory.LoadHeader(dataSet, reader);
                        return dataSet.Published;
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

                // Loop through the temporary files to see if we can use any of them.
                foreach (var temporaryFile in directory.GetFiles("*.tmp").Where(i =>
                        i.Exists &&
                        i.FullName.Equals(masterFile.FullName) == false &&
                        i.Name.StartsWith(masterFile.Name) &&
                        masterDate.Equals(GetDataFileDate(i.FullName))))
                {
                    // Use the existing temporary file.
                    EventLog.Debug(
                        "Trying to use existing temporary data file '{0}' with published date '{1}' as data source.",
                        temporaryFile.FullName,
                        masterDate.HasValue ? masterDate.Value.ToShortDateString() : "null");
                    try
                    {
                        // Try and create the data set from the existing temporary file.
                        // If the file can't be used then record the exception in debug
                        // logging.
                        dataSet = StreamFactory.Create(temporaryFile.FullName, File.GetLastWriteTimeUtc(masterFile.FullName), true);

                        // The data set could be created so exit the loop.
                        break;
                    }
                    catch (Exception ex)
                    {
                        // This can happen if the data file is being used by another process in
                        // exclusive mode and as yet hasn't been freed up.
                        EventLog.Debug(
                            "Could not use existing temporary data file '{0}' as data source",
                            temporaryFile.FullName);
                        EventLog.Debug(ex);
                        dataSet = null;
                    }
                }
                
                if (dataSet == null)
                {
                    // No suitable temp file was found, create one in the
                    // temporary file folder to enable the source file to be updated
                    // without stopping the web site.
                    dataSet = StreamFactory.Create(CreateNewTemporaryFile(masterFile).FullName, File.GetLastWriteTimeUtc(masterFile.FullName), true);
                }
                
            }
            return dataSet;
        }

        /// <summary>
        /// Create a new temporary file for use the stream factory.
        /// </summary>
        /// <param name="masterFile">Name of the master file to use as the source</param>
        /// <returns>Name of the file created.</returns>
        private static FileInfo CreateNewTemporaryFile(FileInfo masterFile)
        {
            var temporaryFile = new FileInfo(GetTempFileName());

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
            return temporaryFile;
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
                    // Log API version for diagnosis.
                    var assembly = Assembly.GetExecutingAssembly().GetName();
                    EventLog.Info(String.Format(
                        "Creating data set from '{0}' version '{1}'",
                        assembly.Name,
                        assembly.Version));
                    
                    if (File.Exists(Manager.BinaryFilePath))
                    {
                        if (Manager.MemoryMode)
                        {
                            EventLog.Info(String.Format(
                                "Creating memory byte array dataset and provider from binary data file '{0}'.",
                                Manager.BinaryFilePath));
                            provider = new WebProvider(StreamFactory.Create(File.ReadAllBytes(Manager.BinaryFilePath)));
                        }
                        else
                        {
                            provider = new WebProvider(GetTempFileDataSet());
                        }
                        EventLog.Info(String.Format(
                            "Created provider from version '{0}' format '{1}' data published on '{2:u}' in master file '{3}'.",
                            provider.DataSet.Version,
                            provider.DataSet.Name,
                            provider.DataSet.Published,
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
        /// Checks for a new version of the data file if licence keys are available.
        /// </summary>
        /// <returns>
        /// Update status indicating whether or not the update was successful.
        /// </returns>
        public static LicenceKeyResults Download()
        {
            return AutoUpdate.Download(LicenceKey.Keys);
        }

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
            try
            {
                if (ActiveProvider == null ||
                    CheckDataFileHasRefreshed(ActiveProvider.DataSet))
                {
                    EventLog.Info("Refreshing active provider due to change in underlying data source.");
                    Refresh();
                }
            }
            catch(Exception ex)
            {
                EventLog.Info(String.Format("Exception processing data file refresh check."));
                EventLog.Debug(ex);
            }
        }

        /// <summary>
        /// Returns true if a newer data set is now available.
        /// </summary>
        /// <param name="dataSet">The data set to be checked for changes to the master source</param>
        /// <returns>true is a newer data set is available</returns>
        private static bool CheckDataFileHasRefreshed(DataSet dataSet)
        {
            if (Manager.BinaryFilePath != null &&
                File.Exists(Manager.BinaryFilePath) &&
                dataSet.Published != null && dataSet.LastModified > DateTime.MinValue)
            {
                if (dataSet.LastModified != null && File.GetLastWriteTimeUtc(Manager.BinaryFilePath) > dataSet.LastModified)
                {
                    return true;
                }
            }
            return false;
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
                    if ((match == null || hasOverrides) && ActiveProvider != null)
                    {
                        // Get the match and store the list of properties and values 
                        // in the context and session.
                        match = ActiveProvider.Match(GetRequiredHeaders(request));
                            
                        // Allow other feature detection methods to override profiles.
                        Feature.ProfileOverride.Override(request, match);
                        if (items != null)
                        {
                            items[matchKey] = match;
                        }
                    }
                }
            }
            
            return match;
        }

        /// <summary>
        /// Gets the required headers. If the MVC method SetBrowserOverride has been used
        /// the User-Agent will be escapted and may contain + characters which need to 
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
                foreach (var componentHeader in ActiveProvider.DataSet.Components.SelectMany(i => i.HttpHeaders).Distinct())
                {
                    if (headers[componentHeader] != null)
                    {
                        headers[componentHeader] = escapedUA;
                        break;
                    }
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
