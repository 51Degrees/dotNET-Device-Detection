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
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FiftyOne.Foundation.UI.Web
{
    /// <summary>
    /// The base user control containing common methods shared across
    /// controls.
    /// </summary>
    public abstract class BaseUserControl : UserControl
    {
        #region Fields

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

        private string _propertyCssClass = "property";
        private string _valueCssClass = "value";
        private string _descriptionCssClass = "description";
        private string _premiumCssClass = "premium";
        private string _liteCssClass = "lite";
        private string _cmsCssClass = "premium";
        private string _vendorCssClass = "vendor";
        private string _modelCssClass = "model";
        private string _nameCssClass = "name";
        private string _vendorsCssClass = "deviceExplorerVendors";
        private string _devicesCssClass = "deviceExplorerDevices";
        private string _deviceCssClass = "deviceExplorerDevice";
        private string _itemCssClass = "item";
        private string _imagesCssClass = "image";
        private string _wideValueCssClass = "wide";
        private string _cssClass = null;
        private string _footerCssClass = "footer";

        #endregion
 
        #region Properties

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
        public string LiteCssClass
        {
            get { return _liteCssClass; }
            set { _liteCssClass = value; }
        }

        /// <summary>
        /// The css class to use when displaying CMS properties. 
        /// </summary>
        public string CmsCssClass
        {
            get { return _cmsCssClass; }
            set { _cmsCssClass = value; }
        }

        /// <summary>
        /// The css class to use when displaying premium properties. 
        /// </summary>
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

            // keys beginning with an _ will not be added to links, unless it was in the parameter
            foreach (string index in parameters.Keys)
                if (index.StartsWith("_") == false || index == key)
                    list.Add(String.Format("{0}={1}", index, HttpUtility.UrlEncode(parameters[index])));

            if (CreateUrl != null)
                return CreateUrl(list);

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
            return GetNewUrl(absoluteUrl, parameters);
        }

        #endregion
    }
}
