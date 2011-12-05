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

namespace FiftyOne.Foundation.Mobile.Detection.Configuration
{
    /// <summary>
    /// Returns all the settings from the Web.Config for this component.
    /// </summary>
    internal static class Manager
    {
        #region Fields

        private static readonly DetectionSection _configurationSection;

        #endregion

        #region Constructors

        static Manager()
        {
            _configurationSection = Support.GetWebApplicationSection("fiftyOne/detection", false) as DetectionSection;
        }

        #endregion

        #region Properties

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

        /// <summary>
        /// Gets a list containing the path of the xml files to be applied.
        /// </summary>
        internal static string[] XmlFiles
        {
            get
            {
                if (_configurationSection == null)
                    return null;
#if VER4
                return  (from FileConfigElement patch in _configurationSection.XmlFiles
                         where patch.Enabled
                         select Mobile.Configuration.Support.GetFilePath(patch.FilePath)).ToArray();
#elif VER2
                List<string> patchFiles = new List<string>();
                foreach (FileConfigElement patch in _configurationSection.XmlFiles)
                    if (patch.Enabled)
                        patchFiles.Add(Support.GetFilePath(patch.FilePath));
                return patchFiles.ToArray();
#endif
            }
        }

        #endregion
    }
}