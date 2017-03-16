/* *********************************************************************
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited. 
 * Copyright © 2017 51Degrees Mobile Experts Limited, 5 Charlotte Close,
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

#region Usings

using System.Configuration;
using FiftyOne.Foundation.Mobile.Configuration;
using System.Linq;
using System.IO;
using System;
using FiftyOne.Foundation.Mobile.Detection.Entities;

#endregion

namespace FiftyOne.Foundation.Mobile.Detection.Configuration
{
    /// <summary>
    /// Returns all the settings from the Web.Config for this component.
    /// </summary>
    public static class Manager
    {
        #region Fields

        private static DetectionSection _configurationSection;

        #endregion

        #region Constructors

        static Manager()
        {
            _configurationSection = Support.GetWebApplicationSection("fiftyOne/detection", false) as DetectionSection;
            if (_configurationSection == null)
                _configurationSection = new DetectionSection();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Determines if device data should automatically be updated when a
        /// licence key is provided.
        /// </summary>
        public static bool AutoUpdate
        {
            get
            {
                return _configurationSection.AutoUpdate;
            }
            set
            {
                SetAutoUpdate(value);
            }
        }

        /// <summary>
        /// Determines if device detection should be enabled.
        /// </summary>
        public static bool Enabled
        {
            get
            {
                return _configurationSection.Enabled;
            }
            set
            {
                SetDeviceDetection(value);
            }
        }

        /// <summary>
        /// Determines if image optimiser should be enabled or disabled.
        /// </summary>
        public static bool ImageOptimiserEnabled
        {
            get
            {
                return Mobile.Configuration.Manager.ImageOptimisation.Enabled;
            }
            set
            {
                SetImageOptimiserEnabled(value);
            }
        }

        /// <summary>
        /// Access to the memory mode setting. Returns true if the data should be
        /// loaded into memory increasing startup time but improving overall performance.
        /// </summary>
        /// <returns>
        /// True if the data should be loaded into memory</returns>
        public static bool MemoryMode
        {
            get
            {
                if (_configurationSection == null)
                    return false;
                return _configurationSection.MemoryMode;
            }
            set
            {
                SetMemoryMode(value);
            }
        }

        /// <summary>
        /// When set to true enables bandwidth monitoring. Also requires the
        /// data set specified in 
        /// the binaryFilePath attribute to support bandwidth monitoring. 
        /// </summary>
        public static bool BandwidthMonitoringEnabled
        {
            get
            {
                return _configurationSection.BandwidthMonitoringEnabled;
            }
        }

        /// <summary>
        /// When set to true enables feature detection. Also requires the
        /// data set specified in the
        /// binaryFilePath attribute to support feature detection. 
        /// </summary>
        public static bool FeatureDetectionEnabled
        {
            get
            {
                return _configurationSection.FeatureDetectionEnabled;
            }
        }

        /// <summary>
        /// Returns true or false depending on whether usage information
        /// should be shared with 51Degrees.mobi.
        /// </summary>
        /// <returns>
        /// True or false depending on whether usage sharing 
        /// is enabled</returns>
        public static bool ShareUsage
        {
            get
            {
                if (_configurationSection == null)
                    return true;
                return _configurationSection.ShareUsage;
            }
            set
            {
                SetShareUsage(value);
            }
        }

        /// <summary>
        /// Sets the device detection value.
        /// </summary>
        /// <param name="value"></param>
        private static void SetDeviceDetection(bool value)
        {
            DetectionSection element = GetDetectionElement();
            element.Enabled = value;
            Support.SetWebApplicationSection(element);
            WebConfig.SetWebConfigurationModules();
            Refresh();
        }

        /// <summary>
        /// Sets the image optimiser value.
        /// </summary>
        /// <param name="value"></param>
        private static void SetImageOptimiserEnabled(bool value)
        {
            var element = Mobile.Configuration.Manager.ImageOptimisation;
            element.Enabled = value;
            Support.SetWebApplicationSection(element);
            WebConfig.SetWebConfigurationModules();
            Refresh();
        }

        /// <summary>
        /// Sets the memory mode value.
        /// </summary>
        /// <param name="value"></param>
        private static void SetMemoryMode(bool value)
        {
            DetectionSection element = GetDetectionElement();
            element.MemoryMode = value;
            Support.SetWebApplicationSection(element);
            Refresh();
        }

        /// <summary>
        /// Sets the shared usage value.
        /// </summary>
        /// <param name="value"></param>
        private static void SetAutoUpdate(bool value)
        {
            DetectionSection element = GetDetectionElement();
            element.AutoUpdate = value;
            Support.SetWebApplicationSection(element);
            Refresh();
        }

        /// <summary>
        /// Sets the shared usage value.
        /// </summary>
        /// <param name="value"></param>
        private static void SetShareUsage(bool value)
        {
            DetectionSection element = GetDetectionElement();
            element.ShareUsage = value;
            Support.SetWebApplicationSection(element);
            Refresh();
        }

        /// <summary>
        /// Gets the detection element from a configuration source.
        /// </summary>
        /// <returns></returns>
        private static DetectionSection GetDetectionElement()
        {
            System.Configuration.Configuration configuration = Support.GetConfigurationContainingSectionGroupName("fiftyOne/detection");

            if (configuration == null)
                return null;

            return configuration.GetSection("fiftyOne/detection") as DetectionSection;
        }

        /// <summary>
        /// Returns the path to the binary file if provided.
        /// </summary>
        internal static string BinaryFilePath
        {
            get
            {
                if (_configurationSection == null)
                    return null;
                return Mobile.Configuration.Support.GetFilePath(_configurationSection.BinaryFilePath);
            }
        }
        
        #endregion

        #region Methods

        /// <summary>
        /// Creates a new configuration instance checking for
        /// fresh data.
        /// </summary>
        internal static void Refresh()
        {
            // Ensure the managers detection section is refreshed in case the
            // process is not going to restart as a result of the change.
            ConfigurationManager.RefreshSection("fiftyOne/detection");

            _configurationSection = Support.GetWebApplicationSection("fiftyOne/detection", false) as DetectionSection;
        }

        #endregion
    }
}