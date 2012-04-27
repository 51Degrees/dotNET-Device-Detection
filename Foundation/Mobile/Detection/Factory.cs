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

#region Usings

using System.Collections;
using System.Collections.Generic;
using System.Web;
using FiftyOne.Foundation.Mobile.Detection.Configuration;
using System.IO;
using System;
using System.Text;
using System.Threading;
using System.Collections.Specialized;

#endregion

namespace FiftyOne.Foundation.Mobile.Detection
{
    /// <summary>
    /// Used to create the Capabilities collection based on the input like user agent string.
    /// </summary>
    public static class Factory
    {
        #region Fields

        /// <summary>
        /// Cache for mobile capabilities. Items are removed approx. 60 minutes after the last
        /// time they were used.
        /// </summary>
        private static readonly Cache<IDictionary> _cache = new Cache<IDictionary>(60);

        /// <summary>
        /// Lock used when 
        /// </summary>
        private static readonly object _lock = new object();

        /// <summary>
        /// Used to obtain the mobile capabilities for the request or user agent string
        /// from the device data source provided.
        /// </summary>
        private static MobileCapabilities _instance;

        #endregion

        #region Private Properties

        /// <summary>
        /// Returns a single instance of the MobileCapabilities class used to provide
        /// capabilities to enhance the request.
        /// </summary>
        private static MobileCapabilities Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            Provider provider = null;

                            try
                            {
                                // Does a binary file exist?
                                if (Manager.BinaryFilePath != null &&
                                    File.Exists(Manager.BinaryFilePath))
                                {
                                    EventLog.Info(String.Format("Creating provider from binary data file '{0}'.",
                                        Manager.BinaryFilePath));
                                    provider = Binary.Reader.Create(Manager.BinaryFilePath);
                                    EventLog.Info(String.Format("Created provider from binary data file '{0}'.",
                                        Manager.BinaryFilePath));
                                }

                                // Do XML files exist?
                                if (Manager.XmlFiles != null &&
                                    Manager.XmlFiles.Length > 0)
                                {
                                    if (provider == null)
                                    {
                                        EventLog.Debug(String.Format("Creating provider from XML data files '{0}'.",
                                            String.Join(", ", Manager.XmlFiles)));
                                        provider = Xml.Reader.Create(Manager.XmlFiles);
                                        EventLog.Info(String.Format("Created provider from XML data files '{0}'.",
                                            String.Join(", ", Manager.XmlFiles)));
                                    }
                                    else
                                    {
                                        EventLog.Debug(String.Format("Adding to existing provider from XML data files '{0}'.",
                                            String.Join(", ", Manager.XmlFiles)));
                                        Xml.Reader.Add(provider, Manager.XmlFiles);
                                        EventLog.Info(String.Format("Added to existing provider from XML data files '{0}'.",
                                            String.Join(", ", Manager.XmlFiles)));
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                // Record the exception in the log file.
                                EventLog.Fatal(
                                    new MobileException(String.Format(
                                        "Exception processing device data from binary file '{0}', and XML files '{1}'. " +
                                        "Enable debug level logging and try again to help identify cause.",
                                        Manager.BinaryFilePath, String.Join(", ", Manager.XmlFiles)),
                                        ex));
                                // Reset the provider to enable it to be created from the embedded data.
                                provider = null;
                            }
                            finally
                            {
                                // Does the provider exist and has data been loaded?
                                if (provider == null || provider.Handlers.Count == 0)
                                {
                                    // No so initialise it with the embeddded binary data so at least we can do something.
                                    provider = Provider.EmbeddedProvider;

                                    // Do XML files exist?
                                    if (Manager.XmlFiles != null &&
                                        Manager.XmlFiles.Length > 0)
                                    {
                                        EventLog.Debug(String.Format("Adding to existing provider from XML data files '{0}'.",
                                            String.Join(", ", Manager.XmlFiles)));
                                        Xml.Reader.Add(provider, Manager.XmlFiles);
                                        EventLog.Info(String.Format("Added to existing provider from XML data files '{0}'.",
                                            String.Join(", ", Manager.XmlFiles)));
                                    }
                                }
                            }
                            _instance = new MobileCapabilities(provider);

                            // Start the auto update thread to check for new data files.
                            ThreadPool.QueueUserWorkItem(new WaitCallback(AutoUpdate.Run));
                        }
                    }
                }
                return _instance;
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Returns the <see cref="Provider"/> instance being
        /// used by the factory.
        /// </summary>
        public static Provider ActiveProvider
        {
            get { return Instance.Provider; }
        }

        #endregion

        #region Internal Static Methods

        /// <summary>
        /// Creates a new <see cref="MobileCapabilities"/> class based on the useragent
        /// string provided.
        /// </summary>
        /// <param name="userAgent">The useragent for the device.</param>
        /// <returns></returns>
        public static IDictionary Create(string userAgent)
        {
            IDictionary caps;

            // We can't do anything with empty user agent strings.
            if (userAgent == null)
                return null;

            if (_cache.GetTryParse(userAgent, out caps))
            {
                // Return these capabilities for adding to the existing ones.
                return caps;
            }

            // Create the new mobile capabilities and record the collection of
            // capabilities for quick creation in future requests.
            caps = Instance.Create(userAgent);
            _cache[userAgent] = caps;
            return caps;
        }

        /// <summary>
        /// Creates a new <see cref="MobileCapabilities"/> class based on the 
        /// HttpHeaders collection provided.
        /// </summary>
        /// <param name="headers">A collection of Http headers from the device.</param>
        /// <param name="currentCapabilities">Capabilities already determined by other sources.</param>
        /// <returns>A new mobile capabilities</returns>
        public static IDictionary Create(NameValueCollection headers, IDictionary currentCapabilities)
        {
            IDictionary caps;
            string ua = headers["User-Agent"] as string;

            // We can't do anything with empty user agent strings.
            if (ua == null)
                return null;

            if (_cache.GetTryParse(ua, out caps))
            {
                // Return these capabilities for adding to the existing ones.
                return caps;
            }

            // Create the new mobile capabilities and record the collection of
            // capabilities for quick creation in future requests.
            caps = Instance.Create(headers, currentCapabilities);
            _cache[ua] = caps;

            return caps;
        }

        /// <summary>
        /// Creates a new <see cref="MobileCapabilities"/> class based on the context
        /// of the requesting device.
        /// </summary>
        /// <param name="request">HttpRequest from the device.</param>
        /// <param name="currentCapabilities">Capabilities already determined by other sources.</param>
        /// <returns>A new mobile capabilities</returns>
        public static IDictionary Create(HttpRequest request, IDictionary currentCapabilities)
        {
            return Create(request.Headers, currentCapabilities);
        }

        #endregion
    }
}