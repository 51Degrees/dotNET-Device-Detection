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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using FiftyOne.Foundation.Mobile.Detection;

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
        private Panel _search;

        private string _vendor;
        private string _model;
        private string _deviceID;
        private string _searchText;

        private string _backButtonCssClass = "back";
        private string _pagerCssClass = "pager";
        private string _searchCssClass = "search";
        private string _searchTextCssClass = "searchText";
        private string _searchBoxCssClass = "searchBox";
        private string _searchButtonCssClass = "searchButton";
        

        private bool _navigation = true;

        private Literal _literalInstruction;

        private TextBox _searchBox;

        private string _deviceExplorerDeviceHtml = Resources.DeviceExplorerDeviceInstructionsHtml;
        private string _deviceExplorerVendorsHtml = Resources.DeviceExplorerVendorsHtml;
        private string _deviceExplorerModelsHtml = Resources.DeviceExplorerModelsInstructionsHtml;
        private string _backButtonDevicesText = Resources.BackButtonDevicesText;
        private string _backButtonDeviceText = Resources.BackButtonDeviceText;
        private string _devicesVendorHeading = Resources.DevicesVendorHeading;
        private string _relatedDevicesHeading = Resources.RelatedDevicesHeading;
        private string _searchInstructionsText = Resources.SearchBoxInstructionText;
        private string _searchBoxText = Resources.SearchBoxButtonText;

        private HyperLink _linkBackTop;
        private HyperLink _linkBackBottom;
        private Panel _panelBackTop;
        private Panel _panelBackBottom;

        private bool _imagesEnabled = false;
        private int _devicesLimit = 30;

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
        /// The CssClass used by the search box.
        /// </summary>
        public string SearchBoxCssClass
        {
            get { return _searchBoxCssClass; }
            set { _searchBoxCssClass = value; }
        }

        /// <summary>
        /// The CssClass used by the search button.
        /// </summary>
        public string SearchButtonCssClass
        {
            get { return _searchButtonCssClass; }
            set { _searchButtonCssClass = value; }
        }

        /// <summary>
        /// The CssClass used by the search div.
        /// </summary>
        public string SearchCssClass
        {
            get { return _searchCssClass; }
            set { _searchCssClass = value; }
        }

        /// <summary>
        /// The CssClass used by the search text.
        /// </summary>
        public string SearchTextCssClass
        {
            get{return _searchTextCssClass;}
            set{_searchTextCssClass = value;}
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
        /// The search term the user has created.
        /// </summary>
        public string SearchText
        {
            get
            {
                if (_searchText == null)
                {
                    _searchText = Request.QueryString["Search"];
                }
                return _searchText;
            }
            set
            {
                _searchText = value;
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

        /// <summary>
        /// Controls if the control will display device images.
        /// </summary>
        public bool ImagesEnabled
        {
            get { return _imagesEnabled; }
            set { _imagesEnabled = value; }
        }

        /// <summary>
        /// Gets or sets how many devices should be displayed at once.
        /// </summary>
        public int DevicesLimit
        {
            get { return _devicesLimit; }
            set { _devicesLimit = value; }
        }

        /// <summary>
        /// Gets or sets the instructions text to use with the device search.
        /// </summary>
        public string SearchInstructionsText
        {
            get { return _searchInstructionsText; }
            set { _searchInstructionsText = value; }
        }

        /// <summary>
        /// Gets or sets the text to use on the device search button.
        /// </summary>
        public string SearchButtonText
        {
            get { return _searchBoxText; }
            set { _searchBoxText = value; }
        }

        #endregion

        #region Events

        /// <summary>
        /// Set the visibility of the data lists.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit(EventArgs e)
        {

            base.OnInit(e); Literal clearTop = new Literal();
            Literal clearBottom = new Literal();
            clearTop.Text = clearBottom.Text = "<div style=\"clear: both\"></div>";
            _literalInstruction = new Literal();
            _linkBackTop = new HyperLink();
            _panelBackTop = new Panel();
            _vendors = CreateVendorRepeater();
            _models = CreateDeviceDataList();
            _related = CreateRelatedDeviceDataList();
            _search = CreateSearch();
            _device = new Panel();
            _panelBackBottom = new Panel();
            _linkBackBottom = new HyperLink();
            
            _panelBackTop.Visible = _panelBackBottom.Visible = false;
            _container.Controls.Add(_search);
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
            _container.Controls.Add(DeviceImages.GetImageRotationScript());
            
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
                (String.IsNullOrEmpty(Vendor) == false || String.IsNullOrEmpty(SearchText) == false);

            _vendors.Visible = _device.Visible == false && _models.Visible == false;

            _search.Visible = DataProvider.IsPremium && (_vendors.Visible || _models.Visible);

            if (_vendors.Visible && DataProvider.IsPremium)
            {
                _literalInstruction.Text = DeviceExplorerVendorsHtml;
                _vendors.DataSource = DataProvider.Vendors.Keys;
                _vendors.DataBind();
                _container.CssClass = VendorsCssClass;
            }

            if (_models.Visible && DataProvider.IsPremium)
            {
                List<Device> models = GetModels();
                _literalInstruction.Text = DeviceExplorerModelsHtml;

                int currentPage = 1;
                if (Request.QueryString["_Page"] != null)
                {
                    Int32.TryParse(Request.QueryString["_Page"], out currentPage);
                }

                int adjustedPage = currentPage - 1;
                int firstDevice = adjustedPage * _devicesLimit;
                int lastDevice = firstDevice + _devicesLimit;
                int offset = lastDevice < models.Count ? _devicesLimit : models.Count - firstDevice;
                int totalPages = (int)Math.Ceiling((double)(models.Count + 1) / (double)_devicesLimit);

                try
                {
                    _models.DataSource = models.GetRange(firstDevice, offset);
                    _models.DataBind();
                }
                catch { }

                // only add pager if there is more than one page
                if (totalPages > 1)
                    _container.Controls.Add(GetPagerFooter(currentPage, totalPages));

                _container.CssClass = DevicesCssClass;

                NameValueCollection parameters = new NameValueCollection(Request.QueryString);
                parameters.Remove("Vendor");
                _panelBackBottom.Visible = _panelBackTop.Visible = _navigation;
                _linkBackBottom.Text = _linkBackTop.Text = String.Format(BackButtonDevicesText);
                _linkBackBottom.NavigateUrl = _linkBackTop.NavigateUrl = GetNewUrl(parameters, null, null);
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
                    if (_imagesEnabled)
                    {
                        var imagesPanel = DeviceImages.GetImagesPanel(device);
                        if (imagesPanel != null)
                            _device.Controls.Add(imagesPanel);
                    }
                    ConstructDeviceContent(device);

                    _container.CssClass = DeviceCssClass;

                    List<Device> relatedDevices = DataProvider.GetRelatedInfo(device);
                    if (relatedDevices.Count > 0)
                    {
                        _related.DataSource = DataProvider.GetRelatedInfo(device);
                        _related.DataBind();
                    }

                    NameValueCollection parameters = new NameValueCollection(Request.QueryString);
                    parameters.Remove("Model");
                    parameters.Remove("DeviceID");
                    parameters.Remove("Search");
                    _panelBackBottom.Visible = _panelBackTop.Visible = _navigation;
                    _linkBackBottom.Text = _linkBackTop.Text = String.Format(BackButtonDeviceText, device.HardwareVendor);
                    _linkBackBottom.NavigateUrl = _linkBackTop.NavigateUrl = GetNewUrl(parameters, "Vendor", device.HardwareVendor);
                }
            }

            _panelBackTop.CssClass = _panelBackBottom.CssClass = BackButtonCssClass;
        }

        /// <summary>
        /// Gets models that safisfy search and vendor conditions.
        /// </summary>
        /// <returns></returns>
        protected virtual List<Device> GetModels()
        {
#if VER4 || VER35
            IEnumerable<Device> modelsEnum = DataProvider.Devices.AsEnumerable();

            // check if a vendor has been specified
            if (!String.IsNullOrEmpty(Vendor))
                modelsEnum = modelsEnum.Where(d => d.HardwareVendor == Vendor);

            // check if a search has been specified
            if (!String.IsNullOrEmpty(SearchText))
            {
                // gets seperate search terms by split search at spaces. Also removes empty entries and converts to lower case.
                string[] searchTerms =
                    SearchText.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.ToLowerInvariant()).ToArray();

                modelsEnum = modelsEnum.Where(d => MatchesSearch(searchTerms, d));
            }

            List<Device> models = modelsEnum.ToList();
#else
                List<Device> models = DataProvider.Devices;

                if (!String.IsNullOrEmpty(Vendor))
                {
                    List<Device> vendorModels = new List<Device>();
                    foreach (var model in models)
                    {
                        if (model.HardwareVendor == Vendor)
                            vendorModels.Add(model);
                    }
                    models = vendorModels;
                }

                if (!String.IsNullOrEmpty(SearchText))
                {
                    string[] searchTerms = SearchText.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < searchTerms.Length; i++)
                        searchTerms[i] = searchTerms[i].ToLowerInvariant();

                    List<Device> searchModels = new List<Device>();
                    foreach (var model in models)
                    {
                        if (MatchesSearch(searchTerms, model))
                            searchModels.Add(model);
                    }
                    models = searchModels;
                }
#endif
            return models;
        }

        private string GetPageUrl(int page)
        {
            return GetNewUrl("_Page", page.ToString());
        }

        private Control GetPagerFooter(int currentPage, int maxPage)
        {
            Panel panel = new Panel();
            panel.CssClass = _pagerCssClass;

            if (currentPage == 1)
            {
                Label prev = new Label();
                prev.Text = "< Prev ";
                panel.Controls.Add(prev);
            }
            else
            {
                HyperLink prev = new HyperLink();
                prev.Text = "< Prev ";
                prev.NavigateUrl = GetPageUrl(currentPage - 1);
                panel.Controls.Add(prev);
            }

            {
                Label spacer = new Label();
                spacer.Text = " | ";
                panel.Controls.Add(spacer);
            }

            for(int i = 1; i < maxPage; i++)
            {
                if (i != currentPage)
                {
                    HyperLink link = new HyperLink();
                    link.Text = i.ToString();
                    link.NavigateUrl = GetPageUrl(i);
                    panel.Controls.Add(link);
                }
                else
                {
                    Label label = new Label();
                    label.Text = i.ToString();
                    panel.Controls.Add(label);
                }
                Label spacer = new Label();
                spacer.Text = " | ";
                panel.Controls.Add(spacer);

            }

            if (currentPage == maxPage)
            {
                Label prev = new Label();
                prev.Text = " Next >";
                panel.Controls.Add(prev);
            }
            else
            {
                HyperLink prev = new HyperLink();
                prev.Text = " Next >";
                prev.NavigateUrl = GetPageUrl(currentPage + 1);
                panel.Controls.Add(prev);
            }

            return panel;
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
            Literal nameLiteral = new Literal();
            namePanel.Controls.Add(nameLiteral);

            // Add the name.
            nameLiteral.Text = property.Name;

            // Add the values.
            int count = 0;
            foreach (string value in device.Properties[property.Name])
            {
                if (count > 0)
                {
                    Literal comma = new Literal();
                    comma.Text = ", ";
                    valuePanel.Controls.Add(comma);
                }
                foreach (Value i in property.Values)
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
            using (XmlReader reader = XmlReader.Create(Headings))
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

                        Literal heading = CreateHeading(depth, caption);
                        parent.Controls.Add(heading);

                        reader.Read();
                        while (reader.Depth > depth)
                            added += AddNodes(parent, reader, device);

                        if (added == 0)
                            parent.Controls.Remove(heading);
                    }

                    if (device.Properties.ContainsKey(key) && device.Properties[key].Count > 0)
                    {
                        Property property = DataProvider.GetProperty(key);
                        if (property != null)
                        {
                            Panel itemPanel = new Panel();
                            Panel namePanel = new Panel();
                            Panel valuePanel = new Panel();

                            parent.Controls.Add(itemPanel);
                            itemPanel.Controls.Add(namePanel);
                            itemPanel.Controls.Add(valuePanel);

                            valuePanel.CssClass = ValueCssClass;
                            namePanel.CssClass = String.Join(" ", new string[] {
                                PropertyCssClass,
                                (property.IsCms ? CmsCssClass : (property.IsPremium ? PremiumCssClass : LiteCssClass)) });

                            string wide = reader.GetAttribute("wide");
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
            Literal heading = new Literal();
            heading.Text = String.Format("<h{0}>{1}</h{0}>", depth, caption);
            return heading;
        }

        private void VendorRepeater_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.DataItem != null &&
                e.Item.DataItem is Value)
            {
                Value vendor = (Value)e.Item.DataItem;
                HyperLink linkVendor = e.Item.FindControl("Vendor") as HyperLink;
                Label labelCount = e.Item.FindControl("Count") as Label;
                Panel panelContainer = e.Item.FindControl("Panel") as Panel;

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
                Device device = e.Item.DataItem as Device;
                Panel container = e.Item.FindControl("Device") as Panel;
                HyperLink link = e.Item.FindControl("Link") as HyperLink;

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
                Device device = e.Item.DataItem as Device;

                Panel panelContainer = e.Item.FindControl("Device") as Panel;
                Panel panelModel = e.Item.FindControl("Model") as Panel;
                Panel panelImage = e.Item.FindControl("Image") as Panel;
                Panel panelName = e.Item.FindControl("Name") as Panel;

                HyperLink linkModel = new HyperLink();
                HyperLink linkName = new HyperLink();

                linkModel.Text = device.HardwareModel;

                // if a device has been searched for the vendor query won't be there and has to be added now.
                // search will also be removed
                var queryStrings = new NameValueCollection(Request.QueryString);
                if (Vendor == null)
                    queryStrings.Add("Vendor", device.HardwareVendor);
                if (SearchText != null)
                    queryStrings.Remove("Search");
                linkModel.NavigateUrl = GetNewUrl(queryStrings, "Model", device.HardwareModel);

                if (String.IsNullOrEmpty(device.HardwareModel) == false)
                {
                    linkName.Text = device.HardwareName;
                    linkName.NavigateUrl = linkModel.NavigateUrl;
                }

                if (_imagesEnabled)
                {
                    var image = DeviceImages.GetDeviceImageRotater(device, linkModel.NavigateUrl);
                    if (image != null)
                        panelImage.Controls.Add(image);
                }

                panelName.Controls.Add(linkName);
                panelModel.Controls.Add(linkModel);


                panelModel.CssClass = ModelCssClass;
                panelName.CssClass = NameCssClass;
                panelImage.CssClass = ImagesCssClass;
                panelContainer.CssClass = ItemCssClass;
            }
        }

        /// <summary>
        /// Handles search box postback.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SearchBox_TextChanged(object sender, EventArgs e)
        {
            Response.Redirect(GetNewUrl("Search", _searchBox.Text));
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Creates a panel with search functionality, including text instructions, a text box and button
        /// </summary>
        /// <returns></returns>
        private Panel CreateSearch()
        {
            Panel searchPanel = new Panel();
            searchPanel.CssClass = SearchCssClass;

            // create instructions
            Panel instructions = new Panel();
            instructions.CssClass = SearchTextCssClass;
            Literal instructionText = new Literal();
            instructionText.Text = SearchInstructionsText;
            instructions.Controls.Add(instructionText);
            searchPanel.Controls.Add(instructions);

            // create search box
            _searchBox = new TextBox();
            // needed to preserve text during post back
            _searchBox.ID = "search_box";
            _searchBox.TextChanged += new EventHandler(SearchBox_TextChanged);
            _searchBox.Text = SearchText;
            _searchBox.CssClass = _searchBoxCssClass;
            Page.RegisterRequiresPostBack(_searchBox);
            searchPanel.Controls.Add(_searchBox);

            // create search button
            Button button = new Button();
            button.Text = _searchBoxText;
            button.CssClass = SearchButtonCssClass;
            searchPanel.Controls.Add(button);

            return searchPanel;
        }

        

        /// <summary>
        /// True if the given device's hardware matches the search terms.
        /// </summary>
        /// <param name="searchTerms">An array of sub string populated from the initial search string. Every string
        /// must have a match for the device to be matched.</param>
        /// <param name="d">The device being queried.</param>
        /// <returns>Returns true if the device is a match.</returns>
        private static bool MatchesSearch(string[] searchTerms, Device d)
        {
            foreach (var sub in searchTerms)
            {
                if ((MatchesSearchTerm(sub, d.HardwareVendor) ||
                                MatchesSearchTerm(sub, d.HardwareModel) ||
                                MatchesSearchTerm(sub, d.HardwareName)) == false)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Matches a search term against a device value.
        /// </summary>
        /// <param name="searchTerm">A substring from the search.</param>
        /// <param name="value">A value to match against.</param>
        /// <returns>True if a match is found.</returns>
        private static bool MatchesSearchTerm(string searchTerm, string value)
        {
            if (String.IsNullOrEmpty(value))
                return false;

            return value.ToLowerInvariant().Contains(searchTerm);
        }

        private Repeater CreateVendorRepeater()
        {
            Repeater repeater = new Repeater();
            repeater.ItemDataBound += new RepeaterItemEventHandler(VendorRepeater_ItemDataBound);
            repeater.ItemTemplate = new VendorTemplate();
            return repeater;
        }

        private DataList CreateDeviceDataList()
        {
            DataList dataList = new DataList();
            dataList.ItemDataBound += new DataListItemEventHandler(DeviceDataList_ItemDataBound);

            dataList.ItemTemplate = new DeviceTemplate();
            dataList.RepeatLayout = RepeatLayout.Flow;
            dataList.RepeatDirection = RepeatDirection.Horizontal;
            return dataList;
        }

        private DataList CreateRelatedDeviceDataList()
        {
            DataList dataList = new DataList();
            dataList.ItemDataBound += new DataListItemEventHandler(RelatedDeviceDataList_ItemDataBound);
            dataList.ItemTemplate = new RelatedDeviceTemplate();
            dataList.RepeatLayout = RepeatLayout.Flow;
            dataList.RepeatDirection = RepeatDirection.Horizontal;
            return dataList;
        }

        #endregion

        
    }
}
