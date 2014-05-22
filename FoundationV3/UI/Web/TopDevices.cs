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

namespace FiftyOne.Foundation.UI.Web
{
    /// <summary>
    /// Used to display most popular devices. Won't work with lite data.
    /// </summary>
    public class TopDevices : BaseDataControl
    {
        #region Classes
        
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

        private static object _lock = new object();
        private static List<Profile> _topModels = null;

        private DataList _models;
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
            
            if(ImagesEnabled)
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
            if (String.IsNullOrEmpty(Description) == false)
            {
                _container.Controls.Add(CreateHeaderPanel());
            }
            _models = CreateDeviceDataList();
            _container.Controls.Add(_models);
            // data info in footer is not required
            base.FooterEnabled = false;
            _models.DataSource = TopModels;
            _models.DataBind();

            _container.CssClass = TopDevicesCssClass;
        }

        #endregion

        #region Private Methods

        private DataList CreateDeviceDataList()
        {
            DataList dataList = new DataList();
            dataList.ItemDataBound += new DataListItemEventHandler(DeviceDataList_ItemDataBound);
            dataList.ItemTemplate = new DeviceTemplate();
            dataList.RepeatLayout = RepeatLayout.Flow;
            dataList.RepeatDirection = RepeatDirection.Horizontal;
            return dataList;
        }

        private Panel CreateHeaderPanel()
        {
            Panel panel = new Panel();
            panel.CssClass = DescriptionCssClass;
            panel.DefaultButton = null;

            Literal headerText = new Literal();
            headerText.Text = String.Format(Description, DeviceAmount);

            panel.Controls.Add(headerText);
            return panel;
        }

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
                                    i["HardwareModel"] != null &&
                                    i["HardwareModel"].Contains(DataSet.GetProperty("HardwareModel").DefaultValue) == false &&
                                    i["HardwareImages"] != null &&
                                    i["HardwareImages"].Any(v => v.Name.StartsWith("Image Unavailable")) == false).ToList();
                                list.Sort(new ProfileComparer());
                                _topModels = list.Take(DeviceAmount).ToList();
                            }
                        }
                    }
                }
                return _topModels;
            }
        }

        private static int _maxDate = DateTime.UtcNow.Year * 12 + DateTime.UtcNow.Month;

        private void DeviceDataList_ItemDataBound(object sender, DataListItemEventArgs e)
        {
            if (e.Item.DataItem != null &&
                e.Item.DataItem is Profile)
            {
                Profile profile = e.Item.DataItem as Profile;

                Panel panelContainer = e.Item.FindControl("Device") as Panel;
                Panel panelModel = e.Item.FindControl("Model") as Panel;
                Panel panelImage = e.Item.FindControl("Image") as Panel;
                Panel panelName = e.Item.FindControl("Name") as Panel;

                // the url of the page this device will link to
                string deviceUrl = GetDeviceLink(profile);

                HyperLink linkModel = new HyperLink();
                HyperLink linkName = new HyperLink();

                linkModel.Text = profile["HardwareModel"].ToString();

                linkModel.NavigateUrl = deviceUrl;
                linkName.NavigateUrl = deviceUrl;
                linkName.Text = profile["HardwareName"].ToString();


                if (_imagesEnabled)
                {
                    var xml = new StringBuilder();
                    using (var writer = XmlWriter.Create(xml, new XmlWriterSettings()
                    {
                        OmitXmlDeclaration = true,
                        Encoding = Response.HeaderEncoding,
                        ConformanceLevel = ConformanceLevel.Fragment
                    }))
                    {
                        writer.WriteStartElement("a");
                        writer.WriteAttributeString("href", ResolveClientUrl(deviceUrl));
                        BuildHardwareImages(writer, profile);
                        writer.WriteEndElement();
                    }
                    var literal = new Literal();
                    literal.Text = xml.ToString();
                    panelImage.Controls.Add(literal);
                }

                panelModel.Controls.Add(linkModel);
                panelName.Controls.Add(linkName);

                panelModel.CssClass = ModelCssClass;
                panelName.CssClass = NameCssClass;
                panelImage.CssClass = ImagesCssClass;
                panelContainer.CssClass = ItemCssClass;
            }
        }

        private string GetDeviceLink(Profile profile)
        {
            // add device query strings
            NameValueCollection queryStrings = new NameValueCollection();
            queryStrings.Add(_vendorKey, profile["HardwareVendor"].ToString());
            queryStrings.Add(_modelKey, profile["HardwareModel"].ToString());

            string baseUrl = DeviceUrl == null ? Request.Url.AbsolutePath : DeviceUrl;
            return GetNewUrl(baseUrl, queryStrings);
        }
        
        #endregion
    }
}
