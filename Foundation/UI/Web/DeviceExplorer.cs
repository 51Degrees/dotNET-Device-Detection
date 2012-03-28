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

using System;
using System.Web.UI.WebControls;
using System.Reflection;
using System.IO;
using FiftyOne.Foundation.Mobile.Detection;
using System.Xml;
using System.Collections.Specialized;

#if VER4 || VER35

using System.Linq;

#endif

namespace FiftyOne.Foundation.UI.Web
{
    /// <summary>
    /// Used to explore the devices contained in the database. Won't work with Lite data.
    /// </summary>
    public class DeviceExplorer : BaseUserControl
    {
        #region Fields

        private Repeater _vendors;
        private DataList _models;
        private DataList _related;
        private Panel _device;

        private string _vendor;
        private string _model;
        private string _deviceID;

        private string _backButtonCssClass = "back";
        private bool _navigation = true;

        private Literal _literalInstruction;

        private string _deviceExplorerDeviceHtml = Resources.DeviceExplorerDeviceInstructionsHtml;
        private string _deviceExplorerVendorsHtml = Resources.DeviceExplorerVendorsHtml;
        private string _deviceExplorerModelsHtml = Resources.DeviceExplorerModelsInstructionsHtml;
        private string _backButtonDevicesText = Resources.BackButtonDevicesText;
        private string _backButtonDeviceText = Resources.BackButtonDeviceText;
        private string _devicesVendorHeading = Resources.DevicesVendorHeading;
        private string _relatedDevicesHeading = Resources.RelatedDevicesHeading;

        private HyperLink _linkBackTop;
        private HyperLink _linkBackBottom;
        private Panel _panelBackTop;
        private Panel _panelBackBottom;

        #endregion

        #region Properties

        /// <summary>
        /// The heading used to indicate a list of related devices.
        /// </summary>
        public string RelatedDevicesHeading
        {
            get { return _relatedDevicesHeading; }
            set { _relatedDevicesHeading = value; }
        }

        /// <summary>
        /// The heading showing the list of devices for the vendor. The {0}
        /// tag is replaced with the vendor name.
        /// </summary>
        public string DevicesVendorHeading
        {
            get { return _devicesVendorHeading; }
            set { _devicesVendorHeading = value; }
        }

        /// <summary>
        /// The message shown on the back button when displayed on the screen showing
        /// the properties of a specific device. The {0} tag of the string is
        /// replaced with the vendor name.
        /// </summary>
        public string BackButtonDeviceText
        {
            get { return _backButtonDeviceText; }
            set { _backButtonDeviceText = value; }
        }

        /// <summary>
        /// The message shown on the back button when displayed on the screen showing
        /// models enabling the user to return to the list of vendors.
        /// </summary>
        public string BackButtonDevicesText
        {
            get { return _backButtonDevicesText; }
            set { _backButtonDevicesText = value; }
        }

        /// <summary>
        /// The message shown at the top of the control when models are displayed.
        /// </summary>
        public string DeviceExplorerModelsHtml
        {
            get { return _deviceExplorerModelsHtml; }
            set { _deviceExplorerModelsHtml = value; }
        }

        /// <summary>
        /// The message shown at the top of the control when vendors are displayed.
        /// </summary>
        public string DeviceExplorerVendorsHtml
        {
            get { return _deviceExplorerVendorsHtml; }
            set { _deviceExplorerVendorsHtml = value; }
        }

        /// <summary>
        /// The message shown at the top of the control when device details are displayed.
        /// </summary>
        public string DeviceExplorerDeviceHtml
        {
            get { return _deviceExplorerDeviceHtml; }
            set { _deviceExplorerDeviceHtml = value; }
        }

        /// <summary>
        /// Controls if navigating away from the set page is enabled.
        /// </summary>
        public bool Navigation
        {
            get { return _navigation; }
            set { _navigation = value; }
        }

        /// <summary>
        /// The CssClass to be used by the back button.
        /// </summary>
        public string BackButtonCssClass
        {
            get { return _backButtonCssClass; }
            set { _backButtonCssClass = value; }
        }

        /// <summary>
        /// Returns the XML resource with the headings for each property.
        /// </summary>
        private static Stream Headings
        {
            get
            {
                return Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream("FiftyOne.Foundation.UI.Headings.xml");
            }
        }

        /// <summary>
        /// The vendor the user has selected.
        /// </summary>
        public string Vendor
        {
            get
            {
                if (_vendor == null)
                {
                    _vendor = Request.QueryString["Vendor"];
                }
                return _vendor;
            }
            set
            {
                _vendor = value;
            }
        }

        /// <summary>
        /// The device id to be displayed.
        /// </summary>
        public string DeviceID
        {
            get
            {
                if (_deviceID == null)
                {
                    _deviceID = Request.QueryString["DeviceID"];
                }
                return _deviceID;
            }
            set
            {
                _deviceID = value;
            }
        }

        /// <summary>
        /// Model the user has selected.
        /// </summary>
        public string Model
        {
            get
            {
                if (_model == null)
                {
                    _model = Request.QueryString["Model"];
                }
                return _model;
            }
            set
            {
                _model = value;
            }
        }

        #endregion
        
        #region Events

        /// <summary>
        /// Set the visibility of the data lists.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            var clearTop = new Literal();
            var clearBottom = new Literal();
            clearTop.Text = clearBottom.Text = "<div style=\"clear: both\"></div>";
            _literalInstruction = new Literal();
            _linkBackTop = new HyperLink();
            _panelBackTop = new Panel();
            _vendors = CreateVendorRepeater();
            _models = CreateDeviceDataList();
            _related = CreateRelatedDeviceDataList();
            _device = new Panel();
            _panelBackBottom = new Panel();
            _linkBackBottom = new HyperLink();
            _panelBackTop.Visible = _panelBackBottom.Visible = false;
            _panelBackTop.Controls.Add(_linkBackTop);
            _container.Controls.Add(_panelBackTop);
            _container.Controls.Add(_literalInstruction);
            _container.Controls.Add(clearTop);
            _container.Controls.Add(_vendors);
            _container.Controls.Add(_models);
            _container.Controls.Add(_device);
            _container.Controls.Add(_related);
            _container.Controls.Add(clearBottom);
            _panelBackBottom.Controls.Add(_linkBackBottom);
            _container.Controls.Add(_panelBackBottom);
        }

        /// <summary>
        /// PreRenders one of the three available controls.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            _models.HeaderTemplate = new HeaderTemplate(String.Format(DevicesVendorHeading, Vendor), 1);
            _related.HeaderTemplate = new HeaderTemplate(RelatedDevicesHeading, 1);

            _device.Visible = 
                (String.IsNullOrEmpty(Vendor) == false && String.IsNullOrEmpty(Model) == false) || 
                String.IsNullOrEmpty(DeviceID) == false;

            _models.Visible = _device.Visible == false &&
                String.IsNullOrEmpty(Vendor) == false;

            _vendors.Visible = _device.Visible == false && _models.Visible == false;

            if (_vendors.Visible && DataProvider.IsPremium)
            {
                _literalInstruction.Text = DeviceExplorerVendorsHtml;
                _vendors.DataSource = DataProvider.Vendors.Keys;
                _vendors.DataBind();
                _container.CssClass = VendorsCssClass;
            }

            if (_models.Visible && DataProvider.IsPremium)
            {
                Value key = null;
#if VER4 || VER35
                key = DataProvider.Vendors.Keys.FirstOrDefault(i =>
                    i.Name == Vendor);
#else
                foreach (var i in DataProvider.Vendors.Keys)
                {
                    if (i.Name == Vendor)
                    {
                        key = i;
                        break;
                    }
                }
#endif
                if (key != null)
                {
                    _literalInstruction.Text = DeviceExplorerModelsHtml;
                    _models.DataSource = DataProvider.Vendors[key];
                    _models.DataBind();
                    _container.CssClass = DevicesCssClass;

                    var parameters = new NameValueCollection(Request.QueryString);
                    parameters.Remove("Vendor");
                    _panelBackBottom.Visible = _panelBackTop.Visible = _navigation;
                    _linkBackBottom.Text = _linkBackTop.Text = String.Format(BackButtonDevicesText);
                    _linkBackBottom.NavigateUrl = _linkBackTop.NavigateUrl = GetNewUrl(parameters, null, null);
                }
            }

            if (_device.Visible)
            {
                Device device = null;
                if (DeviceID != null)
                    device = DataProvider.GetDeviceFromDeviceID(DeviceID);

                if (device == null && 
                    String.IsNullOrEmpty(Vendor) == false && 
                    String.IsNullOrEmpty(Model) == false)
                    device = DataProvider.GetDeviceFromModel(Vendor, Model);

                if (device != null)
                {
                    _literalInstruction.Text = DeviceExplorerDeviceHtml;
                    ConstructDeviceContent(device);
                    _container.CssClass = DeviceCssClass;

                    var relatedDevices = DataProvider.GetRelatedInfo(device);
                    if (relatedDevices.Count > 0)
                    {
                        _related.DataSource = DataProvider.GetRelatedInfo(device);
                        _related.DataBind();
                    }

                    var parameters = new NameValueCollection(Request.QueryString);
                    parameters.Remove("Model");
                    parameters.Remove("DeviceID");
                    _panelBackBottom.Visible = _panelBackTop.Visible = _navigation;
                    _linkBackBottom.Text = _linkBackTop.Text = String.Format(BackButtonDeviceText, device.HardwareVendor);
                    _linkBackBottom.NavigateUrl = _linkBackTop.NavigateUrl = GetNewUrl(parameters, "Vendor", device.HardwareVendor);
                }
            }

            _panelBackTop.CssClass = _panelBackBottom.CssClass = BackButtonCssClass;
        }

        private void ConstructDeviceContent(Device device)
        {
            AddNodes(device, 1);
        }
        
        /// <summary>
        /// Populates the panels with the information about the device.
        /// </summary>
        /// <param name="namePanel"></param>
        /// <param name="valuePanel"></param>
        /// <param name="device"></param>
        /// <param name="property"></param>
        protected void ItemDataBound(Panel namePanel, Panel valuePanel, Device device, Property property)
        {
            var nameLiteral = new Literal();
            namePanel.Controls.Add(nameLiteral);

            // Add the name.
            nameLiteral.Text = property.Name;

            // Add the values.
            int count = 0;
            foreach (var value in device.Properties[property.Name])
            {
                if (count > 0)
                {
                    var comma = new Literal();
                    comma.Text = ", ";
                    valuePanel.Controls.Add(comma);
                }
                foreach (var i in property.Values)
                {
                    if (value == i.Name)
                    {
                        AddLabel(valuePanel, i.Name, i.Description, i.Url, null);
                    }
                }
                count++;
            }
        }

        private void AddNodes(Device device, int headingLevel)
        {
            using (var reader = XmlReader.Create(Headings))
            {
                while (reader.EOF == false)
                {
                    AddNodes(_device, reader, device);
                }
            }
        }

        /// <summary>
        /// Adds new controls and returns the number of actual values added.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="reader"></param>
        /// <param name="device"></param>
        /// <returns></returns>
        private int AddNodes(WebControl parent, XmlReader reader, Device device)
        {
            int added = 0;
            if (reader.NodeType == XmlNodeType.Element)
            {
                string key = reader.GetAttribute("name");
                if (String.IsNullOrEmpty(key) == false)
                {
                    if (device.Properties.ContainsKey(key) == false)
                    {
                        int depth = reader.Depth;
                        string caption;
                        switch (key)
                        {
                            case "Hardware": caption = String.Format("Hardware: {0}", device.HardwareCaption); break;
                            case "Software": caption = String.Format("Software: {0}", device.SoftwareCaption); break;
                            case "Browser": caption = String.Format("Browser: {0}", device.BrowserCaption); break;
                            case "Content": caption = "Content & Streaming"; break;
                            default: caption = key; break;
                        }

                        var heading = CreateHeading(depth, caption);
                        parent.Controls.Add(heading);

                        reader.Read();
                        while (reader.Depth > depth)
                            added += AddNodes(parent, reader, device);

                        if (added == 0)
                            parent.Controls.Remove(heading);
                    }

                    if (device.Properties.ContainsKey(key) && device.Properties[key].Count > 0)
                    {
                        var property = DataProvider.GetProperty(key);

                        if (property != null)
                        {
                            var itemPanel = new Panel();
                            var namePanel = new Panel();
                            var valuePanel = new Panel();

                            parent.Controls.Add(itemPanel);
                            itemPanel.Controls.Add(namePanel);
                            itemPanel.Controls.Add(valuePanel);

                            valuePanel.CssClass = ValueCssClass;
                            namePanel.CssClass = String.Join(" ", new[] {
                            PropertyCssClass,
                            (property.IsPremium ? PremiumCssClass : LiteCssClass) });

                            var wide = reader.GetAttribute("wide");
                            itemPanel.CssClass = String.IsNullOrEmpty(wide) ? ItemCssClass : WideCssClass;

                            ItemDataBound(namePanel, valuePanel, device, property);

                            added++;
                        }
                    }
                }
            }
            reader.Read();
            return added;
        }

        /// <summary>
        /// Returns a literal with the heading.
        /// </summary>
        /// <param name="depth"></param>
        /// <param name="caption"></param>
        private Literal CreateHeading(int depth, string caption)
        {
            var heading = new Literal();
            heading.Text = String.Format("<h{0}>{1}</h{0}>", depth, caption);
            return heading;
        }
        
        private void VendorRepeater_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.DataItem != null &&
                e.Item.DataItem is Value)
            {
                var vendor = (Value)e.Item.DataItem;
                var linkVendor = e.Item.FindControl("Vendor") as HyperLink;
                var labelCount = e.Item.FindControl("Count") as Label;
                var panelContainer = e.Item.FindControl("Panel") as Panel;

                linkVendor.Text = vendor.Name;
                linkVendor.NavigateUrl = GetNewUrl("Vendor", vendor.Name);

                labelCount.Text = DataProvider.Vendors[vendor].Count.ToString();

                panelContainer.CssClass = ItemCssClass;
            }
        }

        private void RelatedDeviceDataList_ItemDataBound(object sender, DataListItemEventArgs e)
        {
            if (e.Item.DataItem != null &&
                e.Item.DataItem is Device)
            {
                var device = e.Item.DataItem as Device;
                var container = e.Item.FindControl("Device") as Panel;
                var link = e.Item.FindControl("Link") as HyperLink;

                link.NavigateUrl = GetNewUrl("DeviceID", device.DeviceID);
                link.Text = device.SoftwareBrowserCaption;

                container.CssClass = ItemCssClass;
            }
        }

        private void DeviceDataList_ItemDataBound(object sender, DataListItemEventArgs e)
        {
            if (e.Item.DataItem != null &&
                e.Item.DataItem is Device)
            {
                var device = e.Item.DataItem as Device;

                var panelContainer = e.Item.FindControl("Device") as Panel;
                var panelModel = e.Item.FindControl("Model") as Panel;
                var panelImage = e.Item.FindControl("Image") as Panel;
                var panelName = e.Item.FindControl("Name") as Panel;

                var linkModel = new HyperLink();
                var linkName = new HyperLink();

                linkModel.Text = device.HardwareModel;
                linkModel.NavigateUrl = GetNewUrl("Model", device.HardwareModel);
                

                if (String.IsNullOrEmpty(device.HardwareModel) == false)
                {
                    linkName.Text = device.HardwareName;
                    linkName.NavigateUrl = linkModel.NavigateUrl;
                }

                panelName.Controls.Add(linkName);
                panelModel.Controls.Add(linkModel);

                panelModel.CssClass = ModelCssClass;
                panelName.CssClass = NameCssClass;
                panelContainer.CssClass = ItemCssClass;
            }
        }
        
        #endregion

        #region Private Methods

        private Repeater CreateVendorRepeater()
        {
            var repeater = new Repeater();
            repeater.ItemDataBound += new RepeaterItemEventHandler(VendorRepeater_ItemDataBound);
            repeater.ItemTemplate = new VendorTemplate();
            return repeater;
        }

        private DataList CreateDeviceDataList()
        {
            var dataList = new DataList();
            dataList.ItemDataBound += new DataListItemEventHandler(DeviceDataList_ItemDataBound);
            
            dataList.ItemTemplate = new DeviceTemplate();
            dataList.RepeatLayout = RepeatLayout.Flow;
            dataList.RepeatDirection = RepeatDirection.Horizontal;
            return dataList;
        }

        private DataList CreateRelatedDeviceDataList()
        {
            var dataList = new DataList();
            dataList.ItemDataBound += new DataListItemEventHandler(RelatedDeviceDataList_ItemDataBound);
            dataList.ItemTemplate = new RelatedDeviceTemplate();
            dataList.RepeatLayout = RepeatLayout.Flow;
            dataList.RepeatDirection = RepeatDirection.Horizontal;
            return dataList;
        }

        #endregion
    }
}
