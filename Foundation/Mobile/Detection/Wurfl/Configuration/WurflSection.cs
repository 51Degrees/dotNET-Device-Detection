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
 *     Andy Allan <andy.allan@mobgets.com>
 * 
 * ********************************************************************* */

#region Usings

using System.Configuration;
using FiftyOne.Foundation.Mobile.Configuration;

#endregion

namespace FiftyOne.Foundation.Mobile.Detection.Wurfl.Configuration
{
    /// <summary>
    /// Configures the wurfl section. This class cannot be inherited.
    /// </summary>
    /// <remarks>
    /// <para>
    /// All the configuration information necessary to access and control the WURFL device database 
    /// is held in this section of the <c>web.config</c> file.
    /// </para>
    /// <example>
    /// <para>
    /// The follow example will look for the core device database at <c>~/App_Data/wurfl.xml.gz</c>.
    /// An additional file is also provided. <c>~/App_Data/web_browsers_patch.xml.gz</c> contains
    /// details of non mobile web browsers.
    /// </para>
    /// <para>
    /// The toolkit will check the <c>http://51degrees.mobi/wurfl-latest.xml.gz</c> URL for updates
    /// to the core device database when the current database is older than 28 days. The URL
    /// is checked whenever the toolkit is initialised.
    /// </para>
    /// <code lang="xml">
    /// &lt;wurfl wurflFilePath="~/App_Data/wurfl.xml.gz"&gt;
    ///     &lt;wurflPatches&gt;
    ///         &lt;add name="browser_definitions" filePath="~/App_Data/web_browsers_patch.xml.gz" enabled="true" /&gt;
    ///     &lt;/wurflPatches&gt;
    ///     &lt;wurflUpdateUrls&gt;
    ///         &lt;add url="http://51degrees.mobi/wurfl-latest.xml.gz" days="28" /&gt;
    ///     &lt;/wurflUpdateUrls&gt;
    /// &lt;/wurfl>
    /// </code>
    /// </example>
    /// </remarks>
    public sealed class WurflSection : ConfigurationSection
    {
        #region Constructors

        #endregion

        #region Properties

        /// <summary>
        /// When set to true only Wurfl devices marked with the attribute "actual_device_root"
        /// are used to provide capabilities. Child devices will continue to be used to 
        /// for device matching but their capabilities will not be used. This is an advanced
        /// feature for those familar with WURFL. (Optional)
        /// </summary>
        [ConfigurationProperty("useActualDeviceRoot", IsRequired = false, DefaultValue = "false")]
        public bool UseActualDeviceRoot
        {
            get { return (bool) this["useActualDeviceRoot"]; }
        }

        /// <summary>
        /// Gets the path to access the WURFL XML file. Defaults to <c>~/bin/wurfl.xml.gz</c>.
        /// </summary>
        [ConfigurationProperty("wurflFilePath", IsRequired = false, DefaultValue = "~/bin/wurfl.xml.gz")]
        [StringValidator(InvalidCharacters = "!@#$%^&*()[]{};'\"|", MaxLength = 255)]
        public string WurflFilePath
        {
            get { return (string) this["wurflFilePath"]; }
        }

        /// <summary>
        /// Gets the collection of URLs to be used to find updates to the wurfl data file.
        /// </summary>
        [ConfigurationProperty("wurflUpdateUrls", IsRequired = false)]
        public UrlCollection WurflUpdateUrls
        {
            get { return (UrlCollection) this["wurflUpdateUrls"]; }
        }

        /// <summary>
        /// Gets the collection of patches to be applied to the original WurflFile.
        /// </summary>
        [ConfigurationProperty("wurflPatches", IsRequired = false)]
        public PatchesCollection WurflPatches
        {
            get { return (PatchesCollection) this["wurflPatches"]; }
        }

        /// <summary>
        /// Gets the URL to send new device information to.
        /// </summary>
        /// <remarks>
        /// If provided new device information will be sent to the URL.
        /// </remarks>
        [ConfigurationProperty("newDevicesURL", IsRequired = false)]
        [StringValidator(InvalidCharacters = "!@#$%^&*()[]{};'\"|", MaxLength = 255)]
        public string NewDevicesURL
        {
            get { return (string) this["newDevicesURL"]; }
        }

        /// <summary>
        /// Determines the level of detail recoreded for new devices and the 
        /// associated HTTP request. Valid values are:
        ///     minimum - only the wap profile and useragent are recorded.
        ///     maximum - all the HTTP headers are recorded.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        [ConfigurationProperty("newDeviceDetail", IsRequired = false, DefaultValue = "minimum")]
        [StringValidator(InvalidCharacters = "!@#$%^&*()[]{};'\"|", MaxLength = 7)]
        public string NewDeviceDetail
        {
            get { return (string) this["newDeviceDetail"]; }
        }

        /// <summary>
        /// Gets capabilities name that will be loaded from the WURFL files.
        /// If none is set, all the capabilities will be loaded what could have
        /// a significant cost in server memory.
        /// </summary>
        [ConfigurationProperty("capabilitiesWhiteList", IsRequired = false)]
        public CapabilityCollection CapabilitiesWhiteList
        {
            get { return (CapabilityCollection) this["capabilitiesWhiteList"]; }
        }

        #endregion
    }
}