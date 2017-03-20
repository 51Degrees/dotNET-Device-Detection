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
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using FiftyOne.Foundation.Mobile.Detection.Entities;
using FiftyOne.Foundation.Mobile.Detection;
using System.Linq;
using System.Xml;
using System.Text.RegularExpressions;

namespace FiftyOne.Foundation.UI.Web
{
    /// <summary>
    /// The base user control containing common methods shared across
    /// controls.
    /// </summary>
    public abstract class BaseUserControl : UserControl
    {
        #region Classes

        /// <summary>
        /// Used to represent a hardware image with all it's attributes.
        /// </summary>
        internal struct HardwareImage
        {
            /// <summary>
            /// Used as the alternative text for the image.
            /// </summary>
            internal readonly string Title;

            /// <summary>
            /// The key for the image from the dataset.
            /// </summary>
            internal readonly string Key;

            /// <summary>
            /// The URL of the image.
            /// </summary>
            internal readonly Uri ImageUrl;

            /// <summary>
            /// Creates a new structure.
            /// </summary>
            /// <param name="title"></param>
            /// <param name="key"></param>
            /// <param name="imageUrl"></param>
            internal HardwareImage (string title, string key, Uri imageUrl) 
            {
                Title = title;
                Key = key;
                ImageUrl = imageUrl;
            }
        }

        #endregion

        #region Fields
        
        /// <summary>
        /// Used to remove white space and other characters that aren't valid 
        /// ID attributes in HTML.
        /// </summary>
        protected static readonly Regex _removeBadCharacters = new Regex(@"[^\w\d-]", RegexOptions.Compiled);

        /// <summary>
        /// An array of the free data set names.
        /// </summary>
        private static readonly IList<string> FreeDataSetNames = new string[] {
            "Lite"
        };

        /// <summary>
        /// An array of ordered captions that dictate what order an image should appear in.
        /// </summary>
        private static readonly IList<string> Captions = new string[] { 
            "Front",
            "Posed",
            "Left",
            "Back",
            "Right"};

        /// <summary>
        /// Footer control used to display stats about the provider.
        /// </summary>
        protected Stats _footer = null;

        /// <summary>
        /// The container for the all controls in the user control.
        /// </summary>
        protected Panel _container;

        /// <summary>
        /// Used to control if the logo is displayed, or not.
        /// </summary>
        private bool _logoEnabled = true;

        /// <summary>
        /// Controls if the footer appears showing the date published
        /// and if Lite or Premium data is being used.
        /// </summary>
        private bool _footerEnabled = true; 

        // CSS class fields.

        private string _iconsCssClass = "icons";
        private string _dataSetsCssClass = "datasets";
        private string _propertyCssClass = "property";
        private string _valueCssClass = "value";
        private string _descriptionCssClass = "description";
        private string _premiumCssClass = "premium";
        private string _liteCssClass = "lite";
        private string _cmsCssClass = "premium";
        private string _vendorCssClass = "vendor";
        private string _modelCssClass = "model";
        private string _nameCssClass = "name";
        private string _pagerCssClass = "pager";
        private string _vendorsCssClass = "vendors";
        private string _vendorLettersCssClass = "vendorLetters";
        private string _devicesCssClass = "devices";
        private string _deviceCssClass = "device";
        private string _itemCssClass = "item";
        private string _imagesCssClass = "image";
        private string _wideValueCssClass = "wide";
        private string _cssClass = null;
        private string _footerCssClass = "footer";

        #endregion
 
        #region Properties

        /// <summary>
        /// Returns true if a paid for version of the data set is being used.
        /// </summary>
        internal protected bool IsPaidFor
        {
            get { return DataSet != null && FreeDataSetNames.Contains(DataSet.Name) == false; }
        }

        /// <summary>
        /// The current data set to use to get information for the controls.
        /// </summary>
        internal protected DataSet DataSet
        {
            get 
            {
                if (_dataSet == null)
                {
                    lock (this)
                    {
                        if (_dataSet == null)
                        {
                            if (WebProvider.ActiveProvider != null)
                            {
                                _dataSet = WebProvider.ActiveProvider.DataSet;
                            }
                        }
                    }
                }
                return _dataSet;
            }
        }
        private DataSet _dataSet;

        /// <summary>
        /// Event handler used to create a label control for the item provided.
        /// </summary>
        /// <param name="container">The container the control will be placed into.</param>
        /// <param name="text">The text that will appear in the label.</param>
        /// <param name="tooltip">The tooltip that will be displayed next to the label.</param>
        /// <param name="url">A url for more information.</param>
        /// <returns></returns>
        public delegate void CreateLabelEventHandler(WebControl container, string text, string tooltip, Uri url);

        /// <summary>
        /// Event handler used to enable the client to persist the parameters of a request
        /// into a URL format of its choosing.
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public delegate string CreateUrlEventHandler(List<string> parameters);

        /// <summary>
        /// Event handler that if present will be used by GetNewUrl to find the url.
        /// </summary>
        public event CreateUrlEventHandler CreateUrl;

        /// <summary>
        /// Event handler that if present will be used by GetNewLabel when creating a label.
        /// </summary>
        public event CreateLabelEventHandler CreateLabel;

        /// <summary>
        /// Controls whether the footer is displayed.
        /// </summary>
        public bool FooterEnabled
        {
            get { return _footerEnabled; }
            set { _footerEnabled = value; }
        }

        /// <summary>
        /// Used to determine if a 51Degrees.mobi logo is displayed in the top
        /// right hand corner of the control.
        /// </summary>
        public bool LogoEnabled
        {
            get { return _logoEnabled; }
            set { _logoEnabled = value; }
        }

        /// <summary>
        /// The css class used by each item in the control.
        /// </summary>
        public string ItemCssClass
        {
            get { return _itemCssClass; }
            set { _itemCssClass = value; }
        }

        /// <summary>
        /// The css class used by a device image.
        /// </summary>
        public string ImagesCssClass
        {
            get { return _imagesCssClass; }
            set { _imagesCssClass = value; }
        }

        /// <summary>
        /// The css class used to display long lists of values.
        /// </summary>
        public string WideCssClass
        {
            get { return _wideValueCssClass; }
            set { _wideValueCssClass = value; }
        }

        /// <summary>
        /// The css class used by the default container.
        /// </summary>
        public string CssClass
        {
            get { return _cssClass; }
            set { _cssClass = value; }
        }

        /// <summary>
        /// The css class used by the devices container.
        /// </summary>
        public string DevicesCssClass
        {
            get { return _devicesCssClass; }
            set { _devicesCssClass = value; }
        }
        
        /// <summary>
        /// The css class used by the device container.
        /// </summary>
        public string DeviceCssClass
        {
            get { return _deviceCssClass; }
            set { _deviceCssClass = value; }
        }

        /// <summary>
        /// The css class used to control the pager when multiple devices are shown.
        /// </summary>
        public string PagerCssClass
        {
            get { return _pagerCssClass; }
            set { _pagerCssClass = value; }
        }

        /// <summary>
        /// The css class used for the list of vendor letters.
        /// </summary>
        public string VendorLettersCssClass
        {
            get { return _vendorLettersCssClass; }
            set { _vendorLettersCssClass = value; }
        }

        /// <summary>
        /// The css class used by the vendors container.
        /// </summary>
        public string VendorsCssClass
        {
            get { return _vendorsCssClass; }
            set { _vendorsCssClass = value; }
        }

        /// <summary>
        /// The css class to use when displaying hardware vendor. 
        /// </summary>
        public string VendorCssClass
        {
            get { return _vendorCssClass; }
            set { _vendorCssClass = value; }
        }

        /// <summary>
        /// The css class to use when displaying hardware model. 
        /// </summary>
        public string ModelCssClass
        {
            get { return _modelCssClass; }
            set { _modelCssClass = value; }
        }

        /// <summary>
        /// The css class to use when displaying hardware name. 
        /// </summary>
        public string NameCssClass
        {
            get { return _nameCssClass; }
            set { _nameCssClass = value; }
        }

        /// <summary>
        /// The css class to use when displaying lite properties. 
        /// </summary>
        [Obsolete("Data set property differences are now dealt with via the Maps property of th Property")]
        public string LiteCssClass
        {
            get { return _liteCssClass; }
            set { _liteCssClass = value; }
        }

        /// <summary>
        /// The css class to use when displaying CMS properties. 
        /// </summary>
        [Obsolete("Different versions beyond paid and free are no longer differentiated")]
        public string CmsCssClass
        {
            get { return _cmsCssClass; }
            set { _cmsCssClass = value; }
        }

        /// <summary>
        /// The css class to use when displaying premium properties. 
        /// </summary>
        [Obsolete("Data set property differences are now dealt with via the Maps property of th Property")]
        public string PremiumCssClass
        {
            get { return _premiumCssClass; }
            set { _premiumCssClass = value; }
        }

        /// <summary>
        /// The css class to use when displaying properties. 
        /// </summary>
        public string PropertyCssClass
        {
            get { return _propertyCssClass; }
            set { _propertyCssClass = value; }
        }

        /// <summary>
        /// The css class to use when displaying the data sets the property is provided in.
        /// </summary>
        public string DataSetsCssClass
        {
            get { return _dataSetsCssClass; }
            set { _dataSetsCssClass = value; }
        }

        /// <summary>
        /// Style to apply to icons.
        /// </summary>
        public string IconsCssClass
        {
            get { return _iconsCssClass; }
            set { _iconsCssClass = value; }
        }
        
        /// <summary>
        /// The css class to use when displaying values. 
        /// </summary>
        public string ValueCssClass
        {
            get { return _valueCssClass; }
            set { _valueCssClass = value; }
        }

        /// <summary>
        /// The css class to use when displaying descriptions. 
        /// </summary>
        public string DescriptionCssClass
        {
            get { return _descriptionCssClass; }
            set { _descriptionCssClass = value; }
        }

        /// <summary>
        /// The css class used to display the footer statistics.
        /// </summary>
        public string FooterCssClass
        {
            get { return _footerCssClass; }
            set { _footerCssClass = value; }
        }

        #endregion

        #region Events

        /// <summary>
        /// Adds the legend to the list of controls.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit(EventArgs e)
        {
            EnableViewState = false;
            _container = new Panel();
            _footer = new Stats();
            Controls.Add(_container);
            Page.PreRenderComplete += new EventHandler(Page_PreRenderComplete);
            base.OnInit(e);
            Controls.Add(_footer);
        }

        /// <summary>
        /// Sets the CSS class of the control.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {
            _container.CssClass = _cssClass;
            _footer.CssClass = FooterCssClass;
            base.OnPreRender(e);
        }

        /// <summary>
        /// Displays the logo if requested and the footer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_PreRenderComplete(object sender, EventArgs e)
        {
            if (_logoEnabled)
            {
                System.Web.UI.WebControls.Image image = new System.Web.UI.WebControls.Image();
                image.AlternateText = Constants.LogoAltText;
                image.Style.Add("border", "none");
                image.Style.Add("margin", "5px");
                image.Style.Add("float", "right");
                image.ImageUrl = Constants.Logo;
                _container.Controls.AddAt(0, image);
            }

            _footer.Visible = _footerEnabled;
        }

        #endregion

        #region Methods
        
        /// <summary>
        /// Writes the hardware profile to the writer.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="profile"></param>
        /// <param name="deviceUrl"></param>
        protected void WriteDeviceProfile(XmlWriter writer, Profile profile, string deviceUrl)
        {
            writer.WriteStartElement("li");
            writer.WriteAttributeString("class", ItemCssClass);

            // Write the model of the device.
            writer.WriteStartElement("h2");
            writer.WriteAttributeString("class", ModelCssClass);
            WriteLinkStart(writer, profile, deviceUrl, profile["HardwareVendor"].ToString());
            writer.WriteEndElement();
            writer.WriteEndElement();

            // Write the image.
            writer.WriteStartElement("div");
            writer.WriteAttributeString("class", ImagesCssClass);
            writer.WriteStartElement("a");
            writer.WriteAttributeString("href", deviceUrl);
            writer.WriteAttributeString("title", profile.ToString());
            BuildHardwareImages(writer, profile);
            writer.WriteEndElement();
            writer.WriteEndElement();

            // Write the name of the device.
            writer.WriteStartElement("h3");
            writer.WriteAttributeString("class", NameCssClass);
            WriteLinkStart(writer, profile, deviceUrl, profile["HardwareModel"].ToString(), profile["HardwareName"].ToString());
            writer.WriteEndElement();
            writer.WriteEndElement();

            writer.WriteEndElement();
        }

        /// <summary>
        /// Adds thee HTML for the rotating images for the profile to the writer.
        /// </summary>
        /// <param name="writer">Write producing the output XML</param>
        /// <param name="profile">Profile who's images should be added</param>
        protected void BuildHardwareImages(XmlWriter writer, Profile profile)
        {
            var images = GetHardwareImages(profile);

            if (images != null &&
                images.Length > 0)
            {
                // Get the image urls surrounded by quotes.
                string[] imageUrls = images.Select(i => String.Format("'{0}'", i.ImageUrl)).ToArray();

                writer.WriteStartElement("img");
                writer.WriteAttributeString("alt", images[0].Title);
                writer.WriteAttributeString("src", images[0].ImageUrl.ToString());

                if (imageUrls.Count() > 1)
                {
                    if (Page.ClientScript.IsClientScriptBlockRegistered(GetType(), "imageRotator") == false)
                        Page.ClientScript.RegisterClientScriptBlock(
                            GetType(),
                            "imageRotator",
                            Resources.ImageRotationScript,
                            true);

                    // Create onmouseover event. It creates an array of url strings that 
                    // should be cycled in order
                    string mouseOver = String.Format(
                        "ImageHovered(this, new Array({0}))",
                        String.Join(",", imageUrls));

                    // Create onmouseout event. It is passed a single url string that should be 
                    // loaded when the cursor leaves the image
                    string mouseOff = String.Format(
                        "ImageUnHovered(this, '{0}')",
                        images[0].ImageUrl.ToString());
                    
                    writer.WriteAttributeString("onmouseover", mouseOver.Replace("\\", "\\\\"));
                    writer.WriteAttributeString("onmouseout", mouseOff.Replace("\\", "\\\\"));
                }
                writer.WriteEndElement();
            }
        }

        /// <summary>
        /// Returns the hardware images for the profile as an array.
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        internal HardwareImage[] GetHardwareImages(Profile profile)
        {
            var hardwareImages = new List<HardwareImage>();

            if (profile["HardwareImages"] != null)
            {
                foreach (var hardwareImage in profile["HardwareImages"])
                {
                    var sections = hardwareImage.Name.Split(new[] { '\t' });
                    if (sections.Length == 2)
                    {
                        Uri imageUrl;
                        if (Uri.TryCreate(
                            Request.IsSecureConnection ? sections[1].Replace(
                                "http://images.51degrees.mobi",
                                "https://51degrees.cachefly.net") : sections[1], 
                            UriKind.Absolute, out imageUrl))
                        {
                            hardwareImages.Add(new HardwareImage(
                                String.Format("{0} - {1}", profile, sections[0]),
                                sections[0], 
                                imageUrl));
                        }
                    }
                }
            }

            return hardwareImages.OrderBy(i => 
                Captions.IndexOf(i.Key) >= 0 ? Captions.IndexOf(i.Key) : int.MaxValue).ToArray();
        }

        /// <summary>
        /// Replaces all the standard tags in the source text with the values defined
        /// for the user control.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        protected virtual string ReplaceTags(string source)
        {
            List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
            AddTag(list, "property", _propertyCssClass);
            AddTag(list, "premium", _premiumCssClass);
            AddTag(list, "lite", _liteCssClass);
            AddTag(list, "cms", _cmsCssClass);
            AddTag(list, "description", _descriptionCssClass);
            AddTag(list, "value", _valueCssClass);
            return ReplaceTags(source, list);
        }

        #endregion

        #region Static Methods


        private static void WriteLinkStart(XmlWriter writer, Profile profile, string deviceUrl, string text, string title = null)
        {
            writer.WriteStartElement("a");
            writer.WriteAttributeString("href", deviceUrl);
            writer.WriteAttributeString("title", title != null ? title : profile.ToString());
            writer.WriteValue(text);
        }

        /// <summary>
        /// Replaces the tag with the value in the text provided.
        /// </summary>
        /// <param name="source">The source string builder.</param>
        /// <param name="tags">Collection of tags and their new values.</param>
        /// <returns>A string containing the altered text.</returns>
        protected static string ReplaceTags(string source, List<KeyValuePair<string, string>> tags)
        {
            StringBuilder builder = new StringBuilder(source);
            foreach(KeyValuePair<string, string> pair in tags)
                builder = builder.Replace(String.Format("{{{0}}}", pair.Key), pair.Value);
            return builder.ToString();
        }

        /// <summary>
        /// Adds a hyperlink prefix to the XML.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="url">A URL to the external link</param>
        /// <param name="title">The title attribute for the external link</param>
        protected void BuildExternalLink(XmlWriter writer, Uri url, string title)
        {
            writer.WriteStartElement("a");
            writer.WriteAttributeString("title", title);
            writer.WriteAttributeString("href", url.ToString());
            writer.WriteAttributeString("rel", "nofollow");
            writer.WriteAttributeString("target", "_blank");
        }

        /// <summary>
        /// Adds the tag and replacement value to the list.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        protected static void AddTag(List<KeyValuePair<string, string>> list, string key, string value)
        {
            list.Add(new KeyValuePair<string, string>(key, value));
        }

        /// <summary>
        /// Adds a <br/> to the controls.
        /// </summary>
        protected void AddBreak()
        {
            Literal literalBreak = new Literal();
            literalBreak.Text = "<br/>";
            _container.Controls.Add(literalBreak);
        }

        /// <summary>
        /// Adds a label control to the panel provided, setting the label controls
        /// text to the value provided.
        /// </summary>
        /// <param name="panel">Control the label will be added to.</param>
        /// <param name="text">The text that will appear in the label.</param>
        /// <param name="tooltip">The tooltip that will be displayed next to the label.</param>
        /// <param name="url">A url for more information.</param>
        /// <param name="anchor">The name of the anchor.</param>
        protected virtual void AddLabel(WebControl panel, string text, string tooltip, Uri url, string anchor)
        {
            // Open the anchor.
            if (String.IsNullOrEmpty(anchor) == false)
            {
                Literal literalAnchorOpen = new Literal();
                literalAnchorOpen.Text = String.Format("<a name=\"{0}\" style=\"text-decoration: none;\">", anchor);
                panel.Controls.Add(literalAnchorOpen);
            }

            // Add the label.
            if (CreateLabel != null && 
                (String.IsNullOrEmpty(tooltip) == false ||
                url != null))
            {
                CreateLabel(panel, text, tooltip, url);
            }
            else
            {
                Label labelContent = new Label();
                labelContent.Text = HttpUtility.HtmlEncode(text);
                labelContent.ToolTip = tooltip;
                panel.Controls.Add(labelContent);
                AddLink(panel, tooltip, url);
            }

            // Close the anchor.
            if (String.IsNullOrEmpty(anchor) == false)
            {
                Literal literalAnchorClose = new Literal();
                literalAnchorClose.Text = "</a>";
                panel.Controls.Add(literalAnchorClose);
            }
        }

        /// <summary>
        /// Adds a link control to the output for the value provided.
        /// </summary>
        /// <param name="panel">Panel the link should be added to.</param>
        /// <param name="tooltip">The tooltip that will be displayed next to the label.</param>
        /// <param name="url">A url for more information.</param>
        private static void AddLink(WebControl panel, string tooltip, Uri url)
        {
            if (url != null || String.IsNullOrEmpty(tooltip) == false)
            {
                Label labelOpen = new Label();
                Label labelClose = new Label();

                labelOpen.Text = " (";
                panel.Controls.Add(labelOpen);

                if (url != null)
                {
                    HyperLink link = new HyperLink();
                    link.NavigateUrl = url.ToString();
                    link.Text = "?";
                    link.ToolTip = tooltip;
                    link.Target = "_blank";
                    panel.Controls.Add(link);
                }
                else
                {
                    Label label = new Label();
                    label.Text = "?";
                    label.ToolTip = tooltip;
                    panel.Controls.Add(label);
                }

                labelClose.Text = ")";
                panel.Controls.Add(labelClose);
            }
        }

        /// <summary>
        /// Adds a plus toggle to the control, and then returns the control added.
        /// </summary>
        /// <param name="control">The parent control.</param>
        /// <param name="style">The CSS display style to apply when expanding.</param>
        /// <returns>The control added.</returns>
        protected virtual Panel AddPlus(WebControl control, string style)
        {
            if (Page.ClientScript.IsClientScriptBlockRegistered(GetType(), "toggle") == false)
                Page.ClientScript.RegisterClientScriptBlock(GetType(), "toggle", Resources.JavaScriptToggle, true);

            Panel target = new Panel();
            Label labelOpen = new Label();
            Label labelClose = new Label();
            LinkButton button = new LinkButton();
            
            control.Controls.Add(labelOpen);
            control.Controls.Add(button);
            control.Controls.Add(labelClose);
            control.Controls.Add(target);

            target.Style.Add(HtmlTextWriterStyle.Display, "none");

            labelOpen.Text = "[";
            button.OnClientClick = String.Format(
                "javascript:toggle(this, '{0}', '{1}'); return false;",
                target.ClientID,
                style);
            button.ToolTip = Resources.ToggleToolTip;
            button.Text = "+";
            labelClose.Text = "]";
            
            return target;
        }

        /// <summary>
        /// Creates a new GET url containing the key and value parameters.
        /// </summary>
        /// <param name="key">The parameter key.</param>
        /// <param name="value">The parameter value.</param>
        /// <returns>A new url containing the additional key and value parameters.</returns>
        protected virtual string GetNewUrl(string key, string value)
        {
            return GetNewUrl(new NameValueCollection(Request.QueryString), key, value);
        }

        /// <summary>
        /// Returns a new url that will replace, or add a parameter
        /// to the one that is currently being used.
        /// </summary>
        /// <param name="parameters">Existing list of parameters to be altered.</param>
        /// <param name="key">The parameter key.</param>
        /// <param name="value">The parameter value.</param>
        /// <returns>A new url containing the parameters provided and the new key and value.</returns>
        protected virtual string GetNewUrl(NameValueCollection parameters, string key, string value)
        {
            return GetNewUrl(Request.Url.AbsolutePath, parameters, key, value);
        }

        /// <summary>
        /// Returns a new url that will replace, or add a parameter
        /// to the one that is currently being used.
        /// </summary>
        /// <param name="absoluteUrl">The root url to add parameters too.</param> 
        /// <param name="parameters">Existing list of parameters to be altered.</param>
        /// <returns>A new url containing the parameters provided and the new key and value.</returns>
        protected virtual string GetNewUrl(string absoluteUrl, NameValueCollection parameters)
        {
            return GetNewUrl(absoluteUrl, parameters, String.Empty);
        }

        /// <summary>
        /// Returns a new url that will replace, or add a parameter
        /// to the one that is currently being used.
        /// </summary>
        /// <param name="absoluteUrl">The root url to add parameters too.</param> 
        /// <param name="parameters">Existing list of parameters to be altered.</param>
        /// <param name="key">A parameter key not to be removed from the list of parameters.</param>
        /// <returns>A new url containing the parameters provided and the new key and value.</returns>
        protected virtual string GetNewUrl(string absoluteUrl, NameValueCollection parameters, string key)
        {
            List<string> list = new List<string>();

            // keys beginning with an _ will not be added to links, unless it was in the parameter.
            if (parameters != null)
            {
                foreach (string index in parameters.Keys)
                {
                    if (index != null &&
                        (index.StartsWith("_") == false || index.Equals(key)))
                    {
                        if (parameters[index] != null)
                        {
                            list.Add(String.Format("{0}={1}", index, HttpUtility.UrlEncode(parameters[index])));
                        }
                        else
                        {
                            list.Add(index);
                        }
                    }
                }
            }

            if (CreateUrl != null)
            {
                try
                {
                    // This is an external method. Trap for exceptions and
                    // use default behavior if there is a failure.
                    return CreateUrl(list);
                }
                catch(Exception ex)
                {
                    EventLog.Debug(ex);
                    EventLog.Warn("Exception calling CreateUrl method '{0}' with list '{1}'.",
                        CreateUrl.ToString(),
                        String.Join(", ", list));
                }
            }

            return String.Format("{0}?{1}",
                absoluteUrl,
                String.Join("&", list.ToArray()));
        }

        /// <summary>
        /// Returns a new url that will replace, or add a parameter
        /// to the one that was supplied.
        /// </summary>
        /// <param name="absoluteUrl">The root url to add parameters too.</param>
        /// <param name="parameters">Existing list of parameters to be altered.</param>
        /// <param name="key">The parameter key.</param>
        /// <param name="value">The parameter value.</param>
        /// <returns>A new url containing the parameters provided and the new key and value.</returns>
        protected virtual string GetNewUrl(string absoluteUrl, NameValueCollection parameters, string key, string value)
        {
            parameters.Remove(key);
            if (value != null)
                parameters.Add(key, value);
            return GetNewUrl(absoluteUrl, parameters, key);
        }

        #endregion
    }
}
