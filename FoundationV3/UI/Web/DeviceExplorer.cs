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

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using FiftyOne.Foundation.Mobile.Detection;
using FiftyOne.Foundation.Mobile.Detection.Entities;
using System.Linq;
using System.Web;
using System.Text;
using System.Text.RegularExpressions;

namespace FiftyOne.Foundation.UI.Web
{
    /// <summary>
    /// Used to explore the devices contained in the database. Won't work with Lite data.
    /// </summary>
    public class DeviceExplorer : BaseUserControl
    {
        #region Constants

        /// <summary>
        /// Image to use when no image is available in the dataset.
        /// </summary>
        private const string UNKNOWN_IMAGE_URL = "http://download.51Degrees.mobi/DeviceImages/UnknownPhone.png";

        /// <summary>
        /// List of properties to check when performing a search.
        /// </summary>
        private static readonly string[] SEARCH_PROPERTIES = new string[] {
            "HardwareVendor",
            "HardwareModel",
            "HardwareName",
            "HardwareFamily"
        };

        #endregion

        #region Fields

        private int _pageIndex = -1;
        private string _vendor;
        private string _model;
        private string _deviceID;
        private string _searchText;
        private string _userAgent;

        private string _backButtonCssClass = "back";
        private string _searchCssClass = "search";
        private string _searchTextCssClass = "searchText";
        private string _searchBoxCssClass = "searchBox";
        private string _searchButtonCssClass = "searchButton";
        private string _matchCssClass = "match";
        private string _categoryCssClass = "category";
        
        private bool _navigation = true;

        private string _deviceExplorerDeviceHtml = Resources.DeviceExplorerDeviceInstructionsHtml;
        private string _deviceExplorerVendorsHtml = Resources.DeviceExplorerVendorsHtml;
        private string _deviceExplorerModelsHtml = Resources.DeviceExplorerModelsInstructionsHtml;
        private string _backButtonDevicesText = Resources.BackButtonDevicesText;
        private string _backButtonDeviceText = Resources.BackButtonDeviceText;
        private string _devicesVendorHeading = Resources.DevicesVendorHeading;
        private string _relatedDevicesHeading = Resources.RelatedDevicesHeading;
        private string _searchInstructionsText = Resources.SearchBoxInstructionText;
        private string _searchButtonText = Resources.SearchBoxButtonText;
        private string _matchCaption = Resources.MatchCaption;

        private bool _imagesEnabled = false;
        private int _devicesLimit = 30;
        
        #endregion

        #region Properties

        /// <summary>
        /// The User-Agent of the device to be displayed.
        /// </summary>
        public string UserAgent
        {
            get 
            {
                if (_userAgent == null)
                {
                    _userAgent = Request.QueryString["UserAgent"];
                }
                return _userAgent; 
            }
            set { _userAgent = value; }
        }

        /// <summary>
        /// The caption of the results section if displayed.
        /// </summary>
        public string MatchCaption
        {
            get { return _matchCaption; }
            set { _matchCaption = value; }
        }

        /// <summary>
        /// The heading used to indicate a list of related devices.
        /// </summary>
        [Obsolete("Related devices are no longer supported")]
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
        /// The CssClass used to display the caption indicating the 
        /// results of the User-Agent test.
        /// </summary>
        public string MatchCssClass
        {
            get { return _matchCssClass; }
            set { _matchCssClass = value; }
        }

        /// <summary>
        /// The CssClass used to display the category properties are
        /// under when displaying details of a device.
        /// </summary>
        public string CategoryCssClass
        {
            get { return _categoryCssClass; }
            set { _categoryCssClass = value; }
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
                    if (IsPostBack)
                    {
                        _searchText = Request.Params["deviceExplorerSearch"];
                    }
                    else
                    {
                        _searchText = Request.QueryString["search"];
                    }
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
                    _deviceID = Request.QueryString["deviceId"];
                }
                return _deviceID;
            }
            set
            {
                _deviceID = value;
            }
        }

        /// <summary>
        /// The current page in the model screen being viewed.
        /// </summary>
        public int PageIndex
        {
            get
            {
                if (_pageIndex < 0)
                {
                    int.TryParse(Request.QueryString["Page"], out _pageIndex);
                }
                return _pageIndex;
            }
            set
            {
                _pageIndex = value;
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
            get { return _searchButtonText; }
            set { _searchButtonText = value; }
        }

        #endregion

        #region Private Properties

        private static object _lock = new object();

        /// <summary>
        /// Date and time the data set used to populate the private properties
        /// was published.
        /// </summary>
        private static DateTime _dataSetPublishedDate = DateTime.MinValue;
                
        /// <summary>
        /// List of vendors held in the class instance to support rapid data
        /// retrieval when the cache is being used.
        /// </summary>
        private static Dictionary<string, Value> Vendors
        {
            get
            {
                if (_vendors == null ||
                    WebProvider.ActiveProvider.DataSet.Published != _dataSetPublishedDate)
                {
                    lock (_lock)
                    {
                        if (_vendors == null ||
                            WebProvider.ActiveProvider.DataSet.Published != _dataSetPublishedDate)
                        {
                            var vendor = WebProvider.ActiveProvider.DataSet.Properties["HardwareVendor"];
                            if (vendor != null)
                            {
                                _vendors = vendor.Values.ToDictionary(i => i.Name, i => i, StringComparer.InvariantCulture);
                            }
                            else
                            {
                                _vendors = new Dictionary<string, Value>();
                            }
                            _dataSetPublishedDate = WebProvider.ActiveProvider.DataSet.Published;
                        }
                    }
                }
                return _vendors;
            }
        }
        private static Dictionary<string, Value> _vendors;

        /// <summary>
        /// Returns the vendors grouped by the first letter.
        /// </summary>
        private static Dictionary<char, List<Value>> VendorsGrouped
        {
            get
            {
                if (_vendorsGrouped == null ||
                    WebProvider.ActiveProvider.DataSet.Published != _dataSetPublishedDate)
                {
                    lock (_lock)
                    {
                        if (_vendorsGrouped == null ||
                            WebProvider.ActiveProvider.DataSet.Published != _dataSetPublishedDate)
                        {
                            if (Vendors.Count == 0)
                            {
                                _vendorsGrouped = new Dictionary<char, List<Value>>();
                            }
                            else
                            {
                                var vendorsGrouped = Vendors.GroupBy(k =>
                                    StartCharacter(k.Key), v => v.Value).ToDictionary(g =>
                                        g.Key, g => g.ToList());
                                var tempValues = new Dictionary<Value, List<int>>();
                                foreach (var value in Vendors)
                                {
                                    tempValues.Add(value.Value, new List<int>());
                                }
                                foreach (Profile profile in WebProvider.ActiveProvider.DataSet.Hardware.Profiles)
                                {
                                    foreach (var value in profile["HardwareVendor"])
                                    {
                                        tempValues[value].Add(profile.Index);
                                    }
                                }
                                foreach (var value in tempValues)
                                {
                                    value.Key._profileIndexes = value.Value.ToArray();
                                }
                                _vendorsGrouped = vendorsGrouped;
                            }
                            _dataSetPublishedDate = WebProvider.ActiveProvider.DataSet.Published;
                        }
                    }
                }
                return _vendorsGrouped;
            }
        }
        private static Dictionary<char, List<Value>> _vendorsGrouped;

        #endregion

        #region Events

        /// <summary>
        /// Set the visibility of the data lists.
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
                    if (String.IsNullOrEmpty(UserAgent) == false)
                    {
                        BuildModelsForUserAgent(writer, UserAgent);
                    }
                    else if (String.IsNullOrEmpty(SearchText) == false)
                    {
                        if (IsPostBack)
                        {
                            BuildModelsForSearch(writer, SearchText);
                        }
                        else if (String.IsNullOrEmpty(DeviceID) == false)
                        {
                            BuildModelForSearch(writer, SearchText, DeviceID);
                        }
                        else
                        {
                            BuildModelsForSearch(writer, SearchText);
                        }
                    }
                    else if (String.IsNullOrEmpty(SearchText) == false)
                    {
                        BuildModelsForSearch(writer, SearchText);
                    }
                    else if (String.IsNullOrEmpty(Vendor) == false)
                    {
                        if (String.IsNullOrEmpty(Model) == false)
                        {
                            BuildModelForVendor(writer, Vendor, Model);
                        }
                        else
                        {
                            BuildModelsForVendor(writer, Vendor);
                        }
                    }
                    else if (String.IsNullOrEmpty(DeviceID) == false)
                    {
                        BuildModelForDeviceID(writer, DeviceID);
                    }
                    else
                    {
                        BuildVendors(writer);
                    }
                }
                var literal = new Literal();
                literal.Text = xml.ToString();
                _container.Controls.Add(literal);
            }
        }
        
        #endregion

        #region Private Methods

        private void BuildModelForDeviceID(XmlWriter writer, string deviceId)
        {
            int profileId;
            if (int.TryParse(deviceId, out profileId))
            {
                var profile = DataSet.FindProfile(profileId);
                if (profile != null)
                {
                    BuildModelDetail(writer, profile);
                }
            }
        }

        private void BuildModelsForUserAgent(XmlWriter writer, string userAgent)
        {
            var results = WebProvider.ActiveProvider.Match(userAgent);
            BuildModelsForResult(writer, results);
            foreach (var profile in results.Profiles)
                BuildModelDetail(writer, profile);
        }

        private void BuildModelsForResult(XmlWriter writer,
            FiftyOne.Foundation.Mobile.Detection.Match result)
        {
            writer.WriteStartElement("h1");
            writer.WriteString(_matchCaption);
            writer.WriteEndElement();

            writer.WriteStartElement("table");
            writer.WriteAttributeString("class", MatchCssClass);

            // Write the first row of the results.
            writer.WriteStartElement("tr");
            writer.WriteStartElement("th");
            writer.WriteString("Found");
            writer.WriteEndElement();

            writer.WriteStartElement("td");
            foreach (var profile in result.Profiles.Where(i =>
                i.Properties.Any(p => p.DisplayOrder > 0)))
            {
                writer.WriteStartElement("a");
                writer.WriteAttributeString("href", String.Format("#{0}", profile.ProfileId));
                writer.WriteAttributeString("title", profile.ToString());
                writer.WriteString(profile.ToString());
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.WriteEndElement();

            // Add the target User-Agent provided.
            BuildRowForResultFormatted(writer, "Target UserAgent", result.TargetUserAgent);

            // Add the signature found.
            if (result.UserAgent != null)
            {
                BuildRowForResultFormatted(writer, "Closest Sub Strings", result.UserAgent);
            }
            else
            {
                // Add the nodes found.
                BuildRowForResultFormatted(writer, "Relevant Sub Strings", result.ToString());
            }

            // Display the found signatures rank.
            if (result.Signature != null)
            {
                BuildRowForResult(writer, "Rank", result.Signature.Rank.ToString());
            }

            // Add the confidence of the match.
            BuildRowForResult(writer, "Difference", result.Difference.ToString());

            // Add the method used for the match.
            BuildRowForResult(writer, "Method", result.Method.ToString());

#if DEBUG
            // Add a header to indicate diagnostics information follows.
            writer.WriteStartElement("tr");
            writer.WriteStartElement("th");
            writer.WriteAttributeString("colspan", "2");
            writer.WriteString("Diagnostics Information");
            writer.WriteEndElement();
            writer.WriteEndElement();

            BuildRowForResult(writer, "Root Nodes Evaluated", result.RootNodesEvaluated.ToString());
            BuildRowForResult(writer, "Nodes Evaluated", result.NodesEvaluated.ToString());
            BuildRowForResult(writer, "Strings Read", result.StringsRead.ToString());
            BuildRowForResult(writer, "Signatures Read", result.SignaturesRead.ToString());
            BuildRowForResult(writer, "Signatures Compared", result.SignaturesCompared.ToString());
            BuildRowForResult(writer, "Closest Signatures", result.ClosestSignatures.ToString());
            BuildRowForResult(writer, "Elapsed Detection Time",
                String.Format("{0:0.000}ms", result.Elapsed.TotalMilliseconds));
            BuildRowForResultFormatted(writer, "Relevant Sub Strings", result.ToString());
#endif
            writer.WriteEndElement();
        }

        private void BuildRowForResultFormatted(XmlWriter writer, string title, string value)
        {
            writer.WriteStartElement("tr");
            writer.WriteStartElement("th");
            writer.WriteString(title);
            writer.WriteEndElement();
            writer.WriteStartElement("td");
            FormatUserAgent(writer, value);
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        private void BuildRowForResult(XmlWriter writer, string title, string value)
        {
            writer.WriteStartElement("tr");
            writer.WriteStartElement("th");
            writer.WriteString(title);
            writer.WriteEndElement();
            writer.WriteStartElement("td");
            writer.WriteString(value);
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        private void FormatUserAgent(XmlWriter writer, string value)
        {
            if (value != null)
            {
                var index = 0;
                while (index < value.Length)
                {
                    var length = Math.Min(
                         Constants.UserAgentCharactersPerLine,
                         value.Length - index);
                    writer.WriteRaw(value.Substring(index, length).Replace(" ", "&nbsp;"));
                    if (length == Constants.UserAgentCharactersPerLine &&
                        value.Length - index != Constants.UserAgentCharactersPerLine)
                    {
                        writer.WriteStartElement("br");
                        writer.WriteEndElement();
                    }
                    index += Constants.UserAgentCharactersPerLine;
                }
            }
        }

        /// <summary>
        /// Returns true if every regex matches the profile provided.
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="regexes"></param>
        /// <returns></returns>
        private static bool IsMatch(Profile profile, Regex[] regexes)
        {
            var remainingRegexes = new List<Regex>(regexes);
            foreach (var propertyName in SEARCH_PROPERTIES)
            {
                var values = profile[propertyName];
                if (values != null)
                {
                    ProcessRegexes(remainingRegexes, values);
                }
            }
            return remainingRegexes.Count == 0;
        }

        private static void ProcessRegexes(List<Regex> regexes, Values values)
        {
            foreach (var value in values)
            {
                for(int i = 0; i < regexes.Count; i++)
                {
                    if (regexes[i].IsMatch(value.ToString()))
                    {
                        regexes.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        private void BuildModelsForSearch(XmlWriter writer, string searchText)
        {
            var regexes = searchText.Split(
                    new string[] { " " },
                    StringSplitOptions.RemoveEmptyEntries).Select(i =>
                        new Regex(
                            String.Format(@"\b{0}\b", i),
                            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant)
                        ).ToArray();

            var profiles = DataSet.Hardware.Profiles.Where(i =>
                    IsMatch(i, regexes)).ToArray();

            BuildSearch(writer);

            BuildModels(writer, "search", searchText, profiles, String.Format("Search '{0}'", searchText));
        }

        private string MakeQueryString(string key, string value)
        {
            return base.GetNewUrl(key, value);
        }

        private string MakeQueryString(NameValueCollection parameters)
        {
            if (parameters == null)
                parameters = new NameValueCollection();
            return base.GetNewUrl(Request.Url.AbsolutePath, parameters);
        }

        private void BuildModelForSearch(XmlWriter writer, string searchText, string deviceId)
        {
            int profileId;
            if (int.TryParse(deviceId, out profileId))
            {
                var profile = DataSet.Hardware.Profiles.FirstOrDefault(i =>
                    i.ProfileId == profileId);
                if (profile != null)
                {
                    BuildBackButton(writer, "Back to Results", new NameValueCollection {
                            { "search", searchText }
                        }, "Back to Search Results");
                    BuildModelDetail(writer, profile);
                    BuildBackButton(writer, "Back to Results", new NameValueCollection {
                            { "search", searchText }
                        }, "Back to Search Results");
                }
            }
        }

        private void BuildModelForVendor(XmlWriter writer, string vendorName, string modelName)
        {
            var vendorProperty = DataSet.Properties["HardwareVendor"];
            var modelProperty = DataSet.Properties["HardwareModel"];
            if (vendorProperty != null &&
                modelProperty != null)
            {
                var profiles = vendorProperty.FindProfiles(vendorName, modelProperty.FindProfiles(modelName));
                foreach(var profile in profiles)
                {
                    DeviceID = profile.ProfileId.ToString();
                    BuildBackButton(writer, String.Format("Back to {0}", vendorName), new NameValueCollection {
                        { "vendor", vendorName }
                    }, String.Format("All {0} Models", vendorName));
                    BuildModelDetail(writer, profile);
                    BuildBackButton(writer, String.Format("Back to {0}", vendorName), new NameValueCollection {
                        { "vendor", vendorName }
                    }, String.Format("All {0} Models", vendorName));
                }
            }
        }

        private void BuildModelDetail(XmlWriter writer, Profile profile)
        {
            writer.WriteStartElement("div");
            writer.WriteAttributeString("class", DeviceCssClass);
            writer.WriteStartElement("ul");
            BuildModel(writer, profile);
            if (ImagesEnabled)
            {
                var images = GetHardwareImages(profile);
                if (images != null && images.Length > 0)
                {
                    // Sets the 1st item in the unordered list to be the images
                    // associated with the profile.
                    BuildModelImages(writer, images);
                }
            }
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        private void BuildModelImages(XmlWriter writer, HardwareImage[] images)
        {
            writer.WriteStartElement("li");
            writer.WriteAttributeString("class", ImagesCssClass);
            writer.WriteStartElement("ul");
            foreach (var image in images)
            {
                writer.WriteStartElement("li");
                writer.WriteAttributeString("title", image.Key);
                writer.WriteStartElement("img");
                writer.WriteAttributeString("alt", image.Title);
                writer.WriteAttributeString("src", image.ImageUrl.ToString());
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        private void BuildModel(XmlWriter writer, Profile profile)
        {
            writer.WriteStartElement("li");
            writer.WriteAttributeString("id", profile.ProfileId.ToString());
            writer.WriteStartElement("h1");
            writer.WriteString(profile.Component.Name);
            writer.WriteEndElement();

            writer.WriteStartElement("div");
            writer.WriteAttributeString("class", CategoryCssClass);
            writer.WriteStartElement("ul");
            foreach (var category in profile.Component.Properties.Where(i =>
                String.IsNullOrEmpty(i.Category) == false).Select(i =>
                    i.Category).Distinct().OrderBy(i => i))
            {
                BuildModel(writer, profile, category);
            }
            
            var generalProperties = profile.Component.Properties.Where(i =>
                String.IsNullOrEmpty(i.Category)).OrderBy(i =>
                    i.Name);
            if (generalProperties.Count() > 0)
            {
                BuildModelGeneralProperties(writer, profile, generalProperties);
            }

            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        private void BuildModelGeneralProperties(XmlWriter writer, Profile profile, IOrderedEnumerable<Property> generalProperties)
        {
            writer.WriteStartElement("li");
            writer.WriteAttributeString("id", String.Format("{0}Miscellaneous", profile.Component.Name));
            writer.WriteStartElement("h2");
            writer.WriteString("Miscellaneous");
            writer.WriteEndElement();
            writer.WriteStartElement("div");
            writer.WriteAttributeString("class", PropertyCssClass);
            writer.WriteStartElement("ul");
            foreach (var property in generalProperties)
            {
                BuildModel(writer, profile, property, profile.Values.Where(i => i.Property == property));
            }
            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        private void BuildModel(XmlWriter writer, Profile profile, string category)
        {
            var values = profile.Values.Where(i =>
                i.Property.Category == category &&
                i.IsDefault == false);
            var properties = values.Select(i => i.Property).Distinct().Where(i =>
                i._valueType != Property.PropertyValueType.JavaScript).OrderBy(i => i.Name);

            if (properties.Count() > 0)
            {
                writer.WriteStartElement("li");
                writer.WriteAttributeString("id", 
                    _removeBadCharacters.Replace(String.Format(
                        "{0}_{1}", 
                        profile.Component.Name, 
                        category), ""));
                writer.WriteStartElement("h2");
                writer.WriteString(category);
                writer.WriteEndElement();

                writer.WriteStartElement("div");
                writer.WriteAttributeString("class", PropertyCssClass);
                writer.WriteStartElement("ul");
                foreach (var property in properties)
                {
                    BuildModel(writer, profile, property, values.Where(i => i.Property == property));
                }
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
        }

        private void BuildModel(XmlWriter writer, Profile profile, Property property, IEnumerable<Value> values)
        {
            if (values.Count() > 0)
            {
                writer.WriteStartElement("li");
                writer.WriteAttributeString("id", _removeBadCharacters.Replace(property.Name, ""));
                writer.WriteStartElement("h3");
                writer.WriteAttributeString("title", property.Description.Trim());
                writer.WriteString(property.Name);
                writer.WriteEndElement();

                writer.WriteStartElement("ul");
                switch (property.Name)
                {
                    case "HardwareImages":
                        foreach (var value in values)
                        {
                            Uri imageUrl;
                            var segments = value.Name.Split(new char[] { '\t' });
                            if (segments.Length == 2 &&
                                Uri.TryCreate(segments[1], UriKind.Absolute, out imageUrl))
                            {
                                writer.WriteStartElement("li");
                                writer.WriteAttributeString("class", ValueCssClass);
                                BuildExternalLink(writer, imageUrl, String.Format("{0} - {1}",
                                    profile,
                                    segments[0]));
                                writer.WriteString(segments[0]);
                                writer.WriteEndElement();
                                writer.WriteEndElement();
                            }
                            else
                                BuildModelValue(writer, value);
                        }
                        break;
                    default:
                        foreach (var value in values)
                        {
                            BuildModelValue(writer, value);
                        }
                        break;
                }
                writer.WriteEndElement();

                writer.WriteEndElement();
            }
        }

        private void BuildModelValue(XmlWriter writer, Value value)
        {
            writer.WriteStartElement("li");
            writer.WriteAttributeString("class", ValueCssClass);
            if (value.Url != null)
            {
                BuildExternalLink(writer, value.Url,
                    String.IsNullOrEmpty(value.Description) ?
                    String.Format("External information about '{0}'", value.Name) :
                    value.Description);
            }
            writer.WriteString(value.Name);
            if (value.Url != null)
            {
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        private void BuildModelsForVendor(XmlWriter writer, string vendorName)
        {
            if (Vendors.Count > 0)
            {
                Value vendor;
                if (Vendors.TryGetValue(vendorName, out vendor))
                {
                    BuildInstructions(writer, DeviceExplorerModelsHtml);
                    BuildBackButton(writer, "All Vendors", null, "List all Vendors");
                    BuildModels(writer, "vendor", vendorName, vendor.Profiles, String.Format("Vendor '{0}'", vendorName));
                    BuildBackButton(writer, "All Vendors", null, "List all Vendors");
                }
            }
        }

        private void BuildModels(XmlWriter writer, string key, string value, Profile[] profiles, string title)
        {
            // Get the number of pages needed to display the models for the vendor.
            var pages = profiles.Length / DevicesLimit;
            if (profiles.Length % DevicesLimit != 0)
                pages++;

            if (pages > 1)
            {
                BuildModelsPager(writer, key, value, pages, PageIndex, title, "top");
            }

            writer.WriteStartElement("div");
            writer.WriteAttributeString("class", DevicesCssClass);
            writer.WriteStartElement("ul");

            // Display the profiles that relate to the selected page.
            foreach (var profile in profiles.Skip(PageIndex * DevicesLimit).Take(DevicesLimit))
            {
                BuildModelSummary(writer, key, value, profile);
            }

            writer.WriteEndElement();
            writer.WriteEndElement();

            if (pages > 1)
            {
                BuildModelsPager(writer, key, value, pages, PageIndex, title,"bottom");
            }
        }

        private void BuildModelsPager(XmlWriter writer, string key, string value, int pages, int page, string title, string idPrefix)
        {
            writer.WriteStartElement("div");
            writer.WriteAttributeString("class", PagerCssClass);
            writer.WriteStartElement("ul");

            BuildModelPagerLink(
                writer,
                key,
                value,
                page,
                0,
                "<<",
                String.Format("{0}_first", idPrefix),
                String.Format("{0} results page 1", title));
            BuildModelPagerLink(
                writer,
                key,
                value,
                page,
                page == 0 ? 0 : page - 1,
                "<",
                String.Format("{0}_previous", idPrefix),
                String.Format("{0} previous results", title));

            for (int index = 0; index < pages; index++)
            {
                BuildModelPagerLink(
                    writer,
                    key,
                    value,
                    page,
                    index,
                    String.Format("{0}", index + 1),
                    String.Format("{0}_pager{1}", idPrefix, index),
                    String.Format("{0} results {1} to {2}", 
                        title, 
                        (index * DevicesLimit) + 1, 
                        ((index + 1) * DevicesLimit)));
            }

            BuildModelPagerLink(
                writer,
                key,
                value,
                page, 
                page == pages - 1 ? pages - 1 : page + 1, ">",
                String.Format("{0}_next", idPrefix),
                String.Format("{0} next results", title));
            BuildModelPagerLink(
                writer,
                key,
                value,
                page,
                pages - 1, ">>",
                String.Format("{0}_last", idPrefix),
                String.Format("{0} last page", title));

            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        private void BuildModelPagerLink(XmlWriter writer, string key, string value, int Page, int index, string text, string id, string title)
        {
            writer.WriteStartElement("li");
            writer.WriteAttributeString("id", _removeBadCharacters.Replace(id, ""));
            if (index != Page)
            {
                writer.WriteStartElement("a");
                writer.WriteAttributeString("title", title);
                BuildModelsPagerLink(writer, key, value, index);
            }
            writer.WriteString(text);
            if (index != Page)
            {
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        private void BuildModelsPagerLink(XmlWriter writer, string key, string value, int index)
        {
            writer.WriteAttributeString("href", MakeQueryString(new NameValueCollection() {
                        { key, value },
                        { "page", index.ToString() }
                    }));
        }

        private void BuildBackButton(XmlWriter writer, string text, NameValueCollection parameters, string title)
        {
            var link = MakeQueryString(parameters);
            writer.WriteStartElement("div");
            writer.WriteAttributeString("class", BackButtonCssClass);
            writer.WriteStartElement("a");
            writer.WriteAttributeString("href", link);
            writer.WriteAttributeString("title", title);
            writer.WriteString(text);
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        private void BuildModelSummary(XmlWriter writer, string key, string value, Profile profile)
        {
            WriteDeviceProfile(writer, profile, MakeQueryString(new NameValueCollection() {
                { key, value },
                { "model", profile["HardwareModel"].ToString() },
                { "deviceId", profile.ProfileId.ToString() },
            }));
        }

        private static char StartCharacter(string value)
        {
            var c = value.ToUpperInvariant()[0];
            if (c >= 'A' && c <= 'Z')
            {
                return c;
            }
            return '1';
        }

        private void BuildVendors(XmlWriter writer)
        {
            if (VendorsGrouped.Count > 0)
            {
                BuildInstructions(writer, DeviceExplorerVendorsHtml);

                BuildSearch(writer);

                writer.WriteStartElement("div");
                writer.WriteAttributeString("class", VendorLettersCssClass);
                writer.WriteStartElement("ul");
                foreach (var g in VendorsGrouped)
                {
                    BuildVendorNavigation(writer, true, g.Key);
                }
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteStartElement("div");
                writer.WriteAttributeString("class", VendorsCssClass);
                writer.WriteStartElement("ul");
                foreach (var g in VendorsGrouped)
                {
                    BuildVendors(writer, g.Value, g.Key);
                }
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
        }

        private void BuildSearch(XmlWriter writer)
        {
            writer.WriteStartElement("div");
            writer.WriteAttributeString("class", SearchCssClass);
            writer.WriteStartElement("input");
            writer.WriteAttributeString("type", "text");
            writer.WriteAttributeString("autocomplete", "on");
            writer.WriteAttributeString("autofocus", "");
            writer.WriteAttributeString("maxlength", "50");
            writer.WriteAttributeString("placeholder", SearchInstructionsText);
            writer.WriteAttributeString("name", "deviceExplorerSearch");
            writer.WriteAttributeString("class", SearchTextCssClass);
            writer.WriteAttributeString("value", SearchText);
            writer.WriteEndElement();
            writer.WriteStartElement("input");
            writer.WriteAttributeString("type", "submit");
            writer.WriteAttributeString("class", SearchButtonCssClass);
            writer.WriteAttributeString("value", SearchButtonText);
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        private void BuildInstructions(XmlWriter writer, string html)
        {
            writer.WriteRaw(html);
        }

        private void BuildVendors(XmlWriter writer, IEnumerable<Value> vendors, char c)
        {
            writer.WriteStartElement("li");
            writer.WriteAttributeString("style", "display: none;");
            writer.WriteAttributeString("id", _removeBadCharacters.Replace(String.Format("{0}Values", c), ""));
            writer.WriteStartElement("ul");
            foreach (var vendor in vendors)
            {
                writer.WriteStartElement("li");
                writer.WriteStartElement("a");
                writer.WriteAttributeString("href", MakeQueryString("Vendor", vendor.Name));
                writer.WriteString(vendor.Name);
                writer.WriteEndElement();
                writer.WriteStartElement("span");
                writer.WriteString(String.Format("({0})", vendor.ProfileIndexes.Length));
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        private void BuildVendorNavigation(XmlWriter writer, bool hasValues, char c)
        {
            if (Page.ClientScript.IsClientScriptBlockRegistered(GetType(), "toggle") == false)
                Page.ClientScript.RegisterClientScriptBlock(GetType(), "toggle", Resources.JavaScriptToggle, true);

            writer.WriteStartElement("li");
            if (hasValues)
            {
                writer.WriteStartElement("a");
                writer.WriteAttributeString("title", String.Format(
                    "Vendors starting with '{0}'", c));
                writer.WriteAttributeString("href", String.Format(
                    "javascript:toggle(this, '{0}', 'block');",
                    String.Format("{0}Values", c)));
            }
            writer.WriteString(String.Format("{0}", c));
            if (hasValues)
            {
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        #endregion
    }
}
