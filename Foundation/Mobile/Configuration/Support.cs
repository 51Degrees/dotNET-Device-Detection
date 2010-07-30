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
 * Mobile Experts Limited are Copyright (C) 2009 - 2010. All Rights Reserved.
 * 
 * Contributor(s):
 *     James Rosewell <james@51degrees.mobi>
 * 
 * ********************************************************************* */

using System.IO;
using System;
using System.Web;
using System.Security;
namespace FiftyOne.Foundation.Mobile.Configuration
{
    internal class Support
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
                if (DoesDirectoryOrFileExist(pathSetOnWebConfig) == true)
                    return pathSetOnWebConfig;
            }
            catch (SecurityException)
            {
                // Do nothing as we're not allowed to check for this type
                // of file.
            }

            // If this is a virtual path then remove the tilda, make absolute
            // and then add the base directory.
            if (pathSetOnWebConfig.StartsWith("~") == true)
                return MakeAbsolute(RemoveTilda(pathSetOnWebConfig));

            // Return the original path.
            return pathSetOnWebConfig;
        }

        private static bool DoesDirectoryOrFileExist(string pathSetOnWebConfig)
        {
            FileInfo info = new FileInfo(pathSetOnWebConfig);
            if (info.Exists == true || info.Directory.Exists == true)
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
            if (partialPath.StartsWith(Path.AltDirectorySeparatorChar.ToString()) || partialPath.StartsWith(Path.DirectorySeparatorChar.ToString()))
                partialPath = partialPath.Substring(1, partialPath.Length - 1);
            // Combing with the application root.
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, partialPath).Replace("/", "\\");
        }

        #endregion
    }
}
