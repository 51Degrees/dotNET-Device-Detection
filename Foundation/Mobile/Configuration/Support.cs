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
 * Mobile Experts Limited are Copyright (C) 2009 - 2012. All Rights Reserved.
 * 
 * Contributor(s):
 *     James Rosewell <james@51degrees.mobi>
 * 
 * ********************************************************************* */

#region Usings

using System;
using System.Configuration;
using System.IO;
using System.Security;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;
using FiftyOne.Foundation.Mobile.Detection.Xml;
using FiftyOne.Foundation.Mobile;

#endregion

namespace FiftyOne.Foundation.Mobile.Configuration
{
    /// <summary>
    /// Utility methods for handling common configuration tasks such as converting virutal
    /// to physical paths, or retrieving configuration sections.
    /// </summary>
    /// <remarks>
    /// This class should not be used in developers code.
    /// </remarks>
    public static class Support
    {
        #region Methods

        internal static string GetFilePath(string pathSetOnWebConfig)
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

        private static ConfigurationSection GetConfigurationSectionFromAltConfig(string sectionName, bool isMandatory)
        {
            string configFileName = GetFilePath(Constants.CONFIG_FILENAME);
            System.Configuration.Configuration fiftyOneConfig = null;

            // Checking for the existence of the configured alternate config file
            if (File.Exists(configFileName))
            {
                // Define cofniguration file(s) mapping for loading the alternate configuration.
                ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap();
                configFileMap.ExeConfigFilename = configFileName;
                configFileMap.MachineConfigFilename = GetFilePath("~/Web.config");

                // Load the alternate configuration file using the configuration file map definition.
                fiftyOneConfig = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);

                // If alternate configuration file loaded successfully go ahead and retrieve requested 
                // confguration section.
                if (fiftyOneConfig != null && fiftyOneConfig.HasFile == true)
                {
                    EventLog.Debug(string.Format("Getting '{0}' configuration from file '{1}'.", sectionName, configFileName));
                    ConfigurationSection section = fiftyOneConfig.GetSection(sectionName);
                    if (section != null)
                        return section;
                }
            }

            // If the configuration section is mandatory and failed to load the alternate configuration throw the exception.
            if (isMandatory)
                throw new MobileException(string.Format("Could not retrieve '{0}' section from configuration file '{1}'.", sectionName, configFileName));

            return null;
        }

        #endregion
    }
}