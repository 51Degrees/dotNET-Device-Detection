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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web.UI.WebControls;
using System.Linq;
using FiftyOne.Foundation.Mobile.Detection.Entities;
using System.Text;
using System.Xml;
using System.Collections;

namespace FiftyOne.Foundation.UI.Web
{
    /// <summary>
    /// Used to display most popular devices. Won't work with lite data.
    /// </summary>
    public class TopDevices : BaseDataControl
    {
        #region Classes

        /// <summary>
        /// Used to ensure only one model from each family is included in the results.
        /// </summary>
        private class ProfileDistinctEqualityComparer : IEqualityComparer<Profile>
        {
            public bool Equals(Profile x, Profile y)
            {
                if (x["HardwareFamily"] != null && y["HardwareFamily"] != null)
                {
                    return x["HardwareFamily"].ToString().Equals(y["HardwareFamily"].ToString());
                }
                return true;
            }

            public int GetHashCode(Profile obj)
            {
                return obj["HardwareFamily"].ToString().GetHashCode();
            }
        }

        /// <summary>
        /// Used to order the list of devices to determine which ones
        /// are top.
        /// </summary>
        private class ProfileComparer : IComparer<Profile>
        {
            public int Compare(Profile x, Profile y)
            {
                if (y.ReleaseDate == x.ReleaseDate)
                {
                    if (y["Popularity"] != null && x["Popularity"] != null)
                        return y["Popularity"].ToDouble().CompareTo(x["Popularity"].ToDouble());
                    return y.ProfileId.CompareTo(x.ProfileId);
                }
                return y.ReleaseDate.CompareTo(x.ReleaseDate);
            }
        }

        #endregion

        #region Fields

        private static ProfileComparer _profileComparer = new ProfileComparer();
        private static ProfileDistinctEqualityComparer _profileEqualityComparer = new ProfileDistinctEqualityComparer();
        private static object _lock = new object();
        private static List<Profile> _topModels = null;

        private bool _imagesEnabled = true;
        private int _deviceAmount = 5;
        private string _vendorKey = "Vendor";
        private string _modelKey = "Model";
        private string _deviceUrl;
        private Profile _selectedDevice = null;
        private string _description = Resources.TopDevicesText;

        private string _topDevicesCssClass = "topDevices";

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets what should be used for navigation url. If null
        /// the requesting url will be used.
        /// </summary>
        public string DeviceUrl
        {
            get { return _deviceUrl; }
            set { _deviceUrl = value; }
        }

        /// <summary>
        /// Gets or sets how many devices to show devices to show.
        /// </summary>
        public int DeviceAmount
        {
            get { return _deviceAmount; }
            set { _deviceAmount = value; }
        }

        /// <summary>
        /// Gets or sets if device images should be displayed. Defaults to true.
        /// </summary>
        public bool ImagesEnabled
        {
            get { return _imagesEnabled; }
            set { _imagesEnabled = value; }
        }

        private string selectedVendor
        {
            get { return Request.QueryString[_vendorKey]; }
        }

        private string selectedModel
        {
            get { return Request.QueryString[_modelKey]; }
        }

        /// <summary>
        /// Gets the device that has been selected.
        /// </summary>
        public Profile SelectedDevice
        {
            get
            {
                if (_selectedDevice == null)
                {
                    if (selectedVendor != null && selectedModel != null)
                    {
                        _selectedDevice = DataSet.Hardware.Profiles.FirstOrDefault(i => 
                            i["HardwareVendor"] != null && i["HardwareVendor"].Any(v => v.Name == selectedVendor) &&
                            i["HardwareModel"] != null && i["HardwareModel"].Any(v => v.Name == selectedModel));
                    }
                }
                return _selectedDevice;
            }
        }

        /// <summary>
        /// Gets or sets the description to be used at the top of the control.
        /// </summary>
        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        /// <summary>
        /// Gets or sets the css class that will be used by the top devices control.
        /// </summary>
        public string TopDevicesCssClass
        {
            get { return _topDevicesCssClass; }
            set { _topDevicesCssClass = value; }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the OnInit event.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            
            if(ImagesEnabled && DataSet != null)
            {
                if (Page.ClientScript.IsClientScriptBlockRegistered(GetType(), "imageRotator") == false)
                    Page.ClientScript.RegisterClientScriptBlock(
                        GetType(), 
                        "imageRotator", 
                        Resources.ImageRotationScript, 
                        true);
            }
        }

        /// <summary>
        /// Handles the OnPreRender event.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (DataSet != null)
            {
                var xml = new StringBuilder();
                using (var writer = XmlWriter.Create(xml, new XmlWriterSettings()
                {
                    OmitXmlDeclaration = true,
                    Encoding = Response.HeaderEncoding,
                    ConformanceLevel = ConformanceLevel.Fragment
                }))
                {
                    if (TopModels != null)
                    {
                        writer.WriteStartElement("ul");
                        foreach (var profile in TopModels)
                        {
                            WriteDeviceProfile(writer, profile, GetDeviceLink(profile));
                        }
                        writer.WriteEndElement();
                    }
                }
                _container.Controls.Add(new Literal() { Text = xml.ToString() });

                // Data info in footer is not required
                base.FooterEnabled = false;

                _container.CssClass = TopDevicesCssClass;
            }
        }
        
        #endregion

        #region Private Methods

        /// <summary>
        /// Gets a list of devices to show, ordering by popularity and returning only as many devices as necessary.
        /// </summary>
        private List<Profile> TopModels
        {
            get
            {
                if (_topModels == null)
                {
                    lock (_lock)
                    {
                        if (_topModels == null)
                        {
                            if (DataSet.GetProperty("HardwareImages") != null &&
                                DataSet.GetProperty("IsMobile") != null)
                            {
                                var list = DataSet.Hardware.Profiles.Where(i =>
                                    i["IsMobile"] != null &&
                                    i["IsMobile"].ToBool() == true &&
                                    i["HardwareVendor"] != null &&
                                    i["HardwareVendor"].Contains(DataSet.GetProperty("HardwareVendor").DefaultValue) == false &&
                                    i["HardwareFamily"] != null &&
                                    i["HardwareFamily"].Contains(DataSet.GetProperty("HardwareFamily").DefaultValue) == false &&
                                    i["HardwareImages"] != null &&
                                    i["HardwareImages"].Any(v => v.Name.StartsWith("Image Unavailable")) == false).Distinct(_profileEqualityComparer).ToList();
                                list.Sort(_profileComparer);
                                _topModels = list.Take(DeviceAmount).ToList();
                            }
                        }
                    }
                }
                return _topModels;
            }
        }
        
        /// <summary>
        /// Gets a URL for the device provided.
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        private string GetDeviceLink(Profile profile)
        {
            NameValueCollection queryStrings = new NameValueCollection();
            queryStrings.Add(_vendorKey, profile["HardwareVendor"].ToString());
            queryStrings.Add(_modelKey, profile["HardwareModel"].ToString());
            string baseUrl = DeviceUrl == null ? Request.Url.AbsolutePath : DeviceUrl;
            return GetNewUrl(baseUrl, queryStrings);
        }
        
        #endregion
    }
}
