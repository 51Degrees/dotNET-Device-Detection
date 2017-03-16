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

using System;
using System.Configuration;
using System.IO;
using System.Security;
using System.Web.Configuration;

#endregion

namespace FiftyOne.Foundation.Mobile.Configuration
{
    /// <summary>
    /// Utility methods for handling common configuration tasks such as converting virutal
    /// to physical paths, or retrieving configuration sections.
    /// </summary>
    /// <remarks>
    /// This class should not be called as it is part of the internal logic.
    /// </remarks>
    public static class Support
    {
        #region Fields

        /// <summary>
        /// Lists of files to use as machine config file sources.
        /// </summary>
        private static readonly string _machineConfigFile = null;

        #endregion

        #region Constructor

        /// <summary>
        /// Gets the path to the machine config file. If medium trust is enabled the call
        /// will fail and web.config should be used.
        /// </summary>
        static Support()
        {
            // Try and access the machine configuration first. This may fail
            // if in medium trust mode.
            try
            {
                _machineConfigFile = InitMachineConfigFiles();
                return;
            }
            catch(Exception)
            {
                // Nothing we can do as there is a problem accessing 
                // the machine configuration. Likely to do with medium
                // trust.
            }

            // Use the first configuration file we can find which we 
            // do have read access to.
            foreach (string file in Constants.ConfigFileNames)
            {
                string configFileName = GetFilePath(file);
                // Checking for the existence of the configuration file.
                if (File.Exists(configFileName))
                {
                    _machineConfigFile = configFileName;
                    break;
                }
            }
        }

        /// <summary>
        /// Has to be a seperate method incase a medium trust security exception is generated.
        /// </summary>
        private static string InitMachineConfigFiles()
        {
            return ConfigurationManager.OpenMachineConfiguration().FilePath;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a real path from a virtiual path.
        /// </summary>
        /// <param name="pathSetOnWebConfig"></param>
        /// <returns></returns>
        public static string GetFilePath(string pathSetOnWebConfig)
        {
            if (string.IsNullOrEmpty(pathSetOnWebConfig))
                return string.Empty;

            // Use file info to determine the file path if security permissions
            // allow this.
            try
            {
                if (DoesDirectoryOrFileExist(pathSetOnWebConfig))
                    return pathSetOnWebConfig;
            }
            catch (SecurityException)
            {
                // Do nothing as we're not allowed to check for this type
                // of file.
            }

            // If this is a virtual path then remove the tilda, make absolute
            // and then add the base directory.
            if (pathSetOnWebConfig.StartsWith("~"))
                return MakeAbsolute(RemoveTilda(pathSetOnWebConfig));

            // Return the original path.
            return pathSetOnWebConfig;
        }
        
        /// <summary>
        /// Returns the configuration section relating to the name provided. If the section
        /// is present in the web.config file this location is used. If it's present in the
        /// alternative configuration file then it will be return from there.
        /// </summary>
        /// <param name="sectionName">The name of the section to be returned.</param>
        /// <param name="isManadatory">True if the section is mandatary.</param>
        /// <exception cref="MobileException">Thrown if the section does not exist and the section is mandatory.</exception>
        /// <returns>The configuration section requested.</returns>
        public static ConfigurationSection GetWebApplicationSection(string sectionName, bool isManadatory)
        {
            ConfigurationSection configurationSection = WebConfigurationManager.GetWebApplicationSection(sectionName) as ConfigurationSection;

            // If section is not present in default web.config try to get it from the 51Degrees.mobi.config file.
            if (configurationSection == null || configurationSection.ElementInformation.IsPresent == false)
                configurationSection = GetConfigurationSectionFromAltConfig(sectionName, isManadatory);

            else
                EventLog.Debug(string.Format("Getting '{0}' configuration from the web.config file.", sectionName));

            return configurationSection;
        }

        #endregion

        #region Internal and Private Methods

        private static bool DoesDirectoryOrFileExist(string pathSetOnWebConfig)
        {
            FileInfo info = new FileInfo(pathSetOnWebConfig);
            if (info.Exists || info.Directory.Exists)
                return true;
            return false;
        }

        internal static string RemoveTilda(string partialPath)
        {
            if (partialPath.StartsWith("~"))
                return partialPath.Substring(1, partialPath.Length - 1);
            return partialPath;
        }

        internal static string MakeAbsolute(string partialPath)
        {
            // Remove any leading directory markers.
            if (partialPath.StartsWith(Path.AltDirectorySeparatorChar.ToString()) ||
                partialPath.StartsWith(Path.DirectorySeparatorChar.ToString()))
                partialPath = partialPath.Substring(1, partialPath.Length - 1);
            // Combing with the application root.
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, partialPath).Replace("/", "\\");
        }

        private static System.Configuration.Configuration OpenConfigFileMap(string configFileName)
        {
            // Define configuration file(s) mapping for loading the alternate configuration.
            ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap();
            configFileMap.ExeConfigFilename = configFileName;
            configFileMap.MachineConfigFilename = _machineConfigFile;

            // Load the alternate configuration file using the configuration file map definition.
            return OpenConfigFileMap(configFileMap);
        }

        private static System.Configuration.Configuration OpenConfigFileMap(ExeConfigurationFileMap configFileMap)
        {
            try
            {
                return ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);
            }
            catch
            {
                return null;
            }
        }

        private static ConfigurationSection GetConfigurationSectionFromAltConfig(string sectionName, bool isMandatory)
        {
            System.Configuration.Configuration fiftyOneConfig = null;

            foreach (string file in Constants.ConfigFileNames)
            {
                string configFileName = GetFilePath(file);

                // Checking for the existence of the configured alternate config file
                if (File.Exists(configFileName))
                {
                    // Load the alternate configuration file using the configuration file map definition.
                    fiftyOneConfig = OpenConfigFileMap(configFileName);

                    // If alternate configuration file loaded successfully go ahead and retrieve requested 
                    // confguration section.
                    if (fiftyOneConfig != null)
                    {
                        if (fiftyOneConfig.HasFile == true)
                        {
                            EventLog.Debug(String.Format("Getting '{0}' configuration from file '{1}'.", sectionName, configFileName));
                            ConfigurationSection section = fiftyOneConfig.GetSection(sectionName);
                            EventLog.Debug(String.Format("Got section '{0}'.", section));
                            if (section != null)
                                return section;
                            else
                                EventLog.Debug(String.Format("Null section for '{0}' configuration from file '{1}'.", sectionName, configFileName));
                        }
                    }
                }
            }

            // If the configuration section is mandatory and failed to load the alternate configuration throw the exception.
            if (isMandatory)
                throw new MobileException(string.Format(
                    "Could not retrieve '{0}' section from configuration files '{1}'.",
                    sectionName,
                    String.Join(", ", Constants.ConfigFileNames)));

            return null;
        }

        /// <summary>
        /// Sets the section name in the first valid configuration file to the value contained
        /// in the section.
        /// </summary>
        /// <param name="section"></param>
        internal static void SetWebApplicationSection(ConfigurationSection section)
        {
            try
            {
                try
                {
                    System.Configuration.Configuration configuration = WebConfigurationManager.OpenWebConfiguration(null);

                    // If the section is present then try setting it.
                    if (configuration != null)
                    {
                        ConfigurationSection existingSection = configuration.GetSection(section.SectionInformation.SectionName);
                        if (existingSection != null && existingSection.ElementInformation.IsPresent)
                        {
                            SetConfigurationSection(section, configuration);
                            return;
                        }
                    }
                }
                catch (ConfigurationErrorsException)
                {
                    // Can't get to the master configuration file. Do nothing and carry on.
                }

                // If section is not present in default web.config try to set it with the alternative files.
                SetConfigurationSectionFromAltConfig(section);
            }
            catch (Exception ex)
            {
                throw new ConfigurationErrorsException(String.Format(
                    "Could not set configuration name '{0}'.",
                    section.SectionInformation.SectionName), ex);
            }
        }

        internal static System.Configuration.Configuration GetConfigurationContainingSectionGroupName(string name)
        {
            foreach (string file in Constants.ConfigFileNames)
            {
                string configFileName = GetFilePath(file);

                // Checking for the existence of the configured alternate config file
                if (File.Exists(configFileName))
                {
                    // Load the alternate configuration file using the configuration file map definition.
                    System.Configuration.Configuration fiftyOneConfig = OpenConfigFileMap(configFileName);

                    // If alternate configuration file loaded successfully go ahead and retrieve requested 
                    // confguration section.
                    if (fiftyOneConfig != null && fiftyOneConfig.HasFile == true)
                    {
                        foreach (ConfigurationSectionGroup group in fiftyOneConfig.SectionGroups)
                            if (SectionGroupMatch(group, name))
                                return fiftyOneConfig;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Checks to determine if the group contains the section name requested.
        /// </summary>
        /// <param name="group"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private static bool SectionGroupMatch(ConfigurationSectionGroup group, string name)
        {
            foreach (ConfigurationSection section in group.Sections)
                if (section.SectionInformation.SectionName == name)
                    return true;

            foreach (ConfigurationSectionGroup child in group.SectionGroups)
                if (SectionGroupMatch(child, name))
                    return true;

            return false;
        }

        /// <summary>
        /// Sets the section name in the first valid alternative configuration file to the value contained
        /// in the section.
        /// </summary>
        /// <param name="section"></param>
        internal static void SetConfigurationSectionFromAltConfig(ConfigurationSection section)
        {
            Exception lastException = null;

            foreach (string file in Constants.ConfigFileNames)
            {
                try
                {
                    string configFileName = GetFilePath(file);

                    // Checking for the existence of the configured alternate config file
                    if (File.Exists(configFileName))
                    {
                        // Load the alternate configuration file using the configuration file map definition.
                        System.Configuration.Configuration fiftyOneConfig = OpenConfigFileMap(configFileName);

                        // If alternate configuration file loaded successfully go ahead and retrieve requested 
                        // confguration section.
                        if (fiftyOneConfig != null && fiftyOneConfig.HasFile == true &&
                            SetConfigurationSection(section, fiftyOneConfig))
                            return;
                    }
                }
                catch (SecurityException ex)
                {
                    // Ignore as could be because file permissions are denied. Move
                    // to the next file.
                    lastException = ex;
                }
            }

            throw new MobileException(String.Format(
                "Could not set section with name '{0}' in any configuration files.",
                section.SectionInformation.SectionName), lastException);
        }

        /// <summary>
        /// Replaces an existing section with the name provided with a new one.
        /// </summary>
        /// <param name="section">The definintion of the section.</param>
        /// <param name="configuration">The configuration it's being replaced within.</param>
        private static bool SetConfigurationSection(ConfigurationSection section, System.Configuration.Configuration configuration)
        {
            ConfigurationSection existingSection = configuration.GetSection(section.SectionInformation.SectionName);

            // Remove the existing section if it exists.
            if (existingSection != null &&
                existingSection.GetType() == section.GetType())
            {
                foreach (string key in existingSection.ElementInformation.Properties.Keys)
                    existingSection.ElementInformation.Properties[key].Value =
                        section.ElementInformation.Properties[key].Value;
                configuration.Save(ConfigurationSaveMode.Modified);
                return true;
            }
            return false;
        }

        #endregion
    }
}