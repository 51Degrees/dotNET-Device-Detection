/* *********************************************************************
 * The contents of this file are subject to the Mozilla internal License 
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
 *     Andy Allan <andy.allan@mobgets.com>
 * 
 * ********************************************************************* */

#region Usings

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Web.Configuration;
using System.Web.Hosting;
using FiftyOne.Foundation.Mobile.Configuration;

#endregion

#if VER4
using System.Linq;
#endif

namespace FiftyOne.Foundation.Mobile.Detection.Wurfl.Configuration
{
    /// <summary>
    /// Returns all the settings from the Web.Config for this component.
    /// </summary>
    internal static class Manager
    {
        #region Fields

        private static readonly WurflSection _configurationSection;

        #endregion

        #region Constructors

        static Manager()
        {
            _configurationSection = (WurflSection)Support.GetWebApplicationSection("fiftyOne/wurfl", false);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Returns true if the <see cref="FiftyOne.Foundation.Mobile.Detection.Wurfl"/> namespace is activated
        /// through valid settings in the web.config file.
        /// </summary>
        internal static bool Enabled
        {
            get
            {
                if (_configurationSection != null && _configurationSection.ElementInformation.IsPresent)
                {
                    return String.IsNullOrEmpty(_configurationSection.WurflFilePath) == false;
                }
                return false;
            }
        }

        /// <summary>
        /// When set to true only Wurfl devices marked with the attribute "actual_device_root"
        /// are used to provide capabilities.
        /// </summary>
        internal static bool UseActualDeviceRoot
        {
            get { return _configurationSection.UseActualDeviceRoot; }
        }

        /// <summary>
        /// Gets the wurfl xml file path.
        /// </summary>
        internal static string WurflFilePath
        {
            get { return Support.GetFilePath(_configurationSection.WurflFilePath); }
        }

        /// <summary>
        /// Gets the URL to send new device information to.
        /// </summary>
        internal static string NewDevicesURL
        {
            get { return _configurationSection.NewDevicesURL; }
        }

        /// <summary>
        /// Returns the level of detail to provide.
        /// </summary>
        internal static NewDeviceDetail NewDeviceDetail
        {
            get
            {
                switch (_configurationSection.NewDeviceDetail)
                {
                    case "maximum":
                        return NewDeviceDetail.Maximum;
                    default:
                        return NewDeviceDetail.Minimum;
                }
            }
        }

        /// <summary>
        /// Gets a list containing the names of the capabilities to be used.
        /// If none, all capabilities will be loaded into the memory.
        /// </summary>
        internal static string[] CapabilitiesWhiteList
        {
            get
            {
                List<string> capabilitiesWhiteList = new List<string>();
                foreach (CapabilityElement capability in _configurationSection.CapabilitiesWhiteList)
                    capabilitiesWhiteList.Add(capability.CapabilityName);
                return capabilitiesWhiteList.ToArray();
            }
        }


        /// <summary>
        /// Gets a list containing the path of the wurfl patches to be applied.
        /// </summary>
        internal static string[] WurflPatchFiles
        {
            get
            {
#if VER4
                return  (from PatchConfigElement patch in _configurationSection.WurflPatches
                         where patch.Enabled
                         select Mobile.Configuration.Support.GetFilePath(patch.FilePath)).ToArray();
#elif VER2
                List<string> patchFiles = new List<string>();
                foreach (PatchConfigElement patch in _configurationSection.WurflPatches)
                    if (patch.Enabled)
                        patchFiles.Add(Support.GetFilePath(patch.FilePath));
                return patchFiles.ToArray();
#endif
            }
        }

        #endregion
    }
}