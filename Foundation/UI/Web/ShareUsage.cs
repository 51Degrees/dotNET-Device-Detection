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

namespace FiftyOne.Foundation.UI.Web
{
    /// <summary>
    /// EventHandler used to notify containers of a message to display 
    /// when the share usage value changes.
    /// </summary>
    /// <param name="sender">The control instance generating the event.</param>
    /// <param name="html">The results of the change.</param>
    public delegate void ShareUsageChangedEventHandler(object sender, string html);

    /// <summary>
    /// Control used to provide the UI for turning the share usage attribute
    /// on and off.
    /// </summary>
    public class ShareUsage : BaseDataControl
    {
        #region Constants

        private const string VALIDATION_LICENCE = "ValidateLicence";

        #endregion

        #region Fields

        #region Controls

        private Literal _literalShareUsageResult = null;
        private CheckBox _checkBoxShareUsage = null;
        private HyperLink _hyperLinkShareUsage = null;

        #endregion

        #region Css

        private string _checkBoxCssClass = "checkbox";
        private string _hyperLinkCssClass = "hyperlink";

        #endregion

        #region Messages

        private string _shareUsageText = Resources.ShareUsageText;
        private string _shareUsageTrueHtml = Resources.ShareUsageTrueHtml;
        private string _shareUsageFalseHtml = Resources.ShareUsageFalseHtml;
        private string _shareUsageErrorHtml = Resources.ShareUsageErrorHtml;
        private string _shareUsageLinkText = Resources.ShareUsageLinkText;
        private string _shareUsageUrl = Resources.ShareUsageUrl;

        #endregion

        #region Other

        private bool _showShareUsage = true;

        #endregion

        #endregion

        #region Properties

        #region Messages

        /// <summary>
        /// The url of the page that should be used to find out more about
        /// sharing usage information.
        /// </summary>
        public string ShareUsageUrl
        {
            get { return _shareUsageUrl; }
            set { _shareUsageUrl = value; }
        }

        /// <summary>
        /// The text that should appear on the hyper link next to the 
        /// share usage check box text.
        /// </summary>
        public string ShareUsageLinkText
        {
            get { return _shareUsageLinkText; }
            set { _shareUsageLinkText = value; }
        }

        /// <summary>
        /// Html used when the share usage value is set to true.
        /// </summary>
        public string ShareUsageTrueHtml
        {
            get { return _shareUsageTrueHtml; }
            set { _shareUsageTrueHtml = value; }
        }

        /// <summary>
        /// Html used when the share usage value is set to false.
        /// </summary>
        public string ShareUsageFalseHtml
        {
            get { return _shareUsageFalseHtml; }
            set { _shareUsageFalseHtml = value; }
        }

        /// <summary>
        /// Html used when an error is generated changing the share usage value.
        /// </summary>
        public string ShareUsageErrorHtml
        {
            get { return _shareUsageErrorHtml; }
            set { _shareUsageErrorHtml = value; }
        }

        /// <summary>
        /// Text used to inform user about sharing usage information.
        /// </summary>
        public string ShareUsageText
        {
            get { return _shareUsageText; }
            set { _shareUsageText = value; }
        }
        
        #endregion

        #region Css

        /// <summary>
        /// Gets or sets the hyper link css class for share usage.
        /// </summary>
        public string HyperLinkCssClass
        {
            get { return _hyperLinkCssClass; }
            set { _hyperLinkCssClass = value; }
        }

        /// <summary>
        /// Gets or sets the check box css class for share usaged check box.
        /// </summary>
        public string CheckBoxCssClass
        {
            get { return _checkBoxCssClass; }
            set { _checkBoxCssClass = value; }
        }

        #endregion

        #region Other

        /// <summary>
        /// Controls whether the share usage information is displayed.
        /// Defaults to true.
        /// </summary>
        public bool ShowShareUsage
        {
            get { return _showShareUsage; }
            set { _showShareUsage = value; }
        }

        #endregion

        #endregion

        #region Event Handlers

        /// <summary>
        /// Fired when the upload has completed. Used to inform other
        /// controls the result of the upload.
        /// </summary>
        public event ShareUsageChangedEventHandler ShareUsageChanged;

        #endregion

        #region Events

        /// <summary>
        /// Load the controls which will form the user interface.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // Create the required controls.
            _checkBoxShareUsage = new CheckBox();
            _hyperLinkShareUsage = new HyperLink();
            _literalShareUsageResult = new Literal();

            // Set the new controls properties.
            _checkBoxShareUsage.ID = "CheckBoxShareUsage";
            _checkBoxShareUsage.CausesValidation = false;
            _checkBoxShareUsage.AutoPostBack = true;
            _checkBoxShareUsage.Checked = FiftyOne.Foundation.Mobile.Detection.Configuration.Manager.ShareUsage;
            _checkBoxShareUsage.CheckedChanged += new EventHandler(_checkBoxShareUsage_CheckedChanged);
            _hyperLinkShareUsage.ID = "HyperLinkShareUsage";
            
            // Add the controls to the user control.
            _container.Controls.Add(_literalShareUsageResult);
            _container.Controls.Add(_checkBoxShareUsage);
            _container.Controls.Add(_hyperLinkShareUsage);
        }

        /// <summary>
        /// Sets the visible status of the instruction messages.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            _literalShareUsageResult.Visible = !String.IsNullOrEmpty(_literalShareUsageResult.Text);
            _checkBoxShareUsage.CssClass = CheckBoxCssClass;
            _checkBoxShareUsage.Text = ShareUsageText;
            _hyperLinkShareUsage.CssClass = HyperLinkCssClass;
            _hyperLinkShareUsage.Text = ShareUsageLinkText;
            _hyperLinkShareUsage.NavigateUrl = ShareUsageUrl;
            _hyperLinkShareUsage.Target = "_blank";

            _hyperLinkShareUsage.Visible = _checkBoxShareUsage.Visible = 
                _literalShareUsageResult.Visible = ShowShareUsage;
        }

        /// <summary>
        /// Fired when the user changes the share usage preference.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _checkBoxShareUsage_CheckedChanged(object sender, EventArgs e)
        {
            string message;

            try
            {
                FiftyOne.Foundation.Mobile.Detection.Configuration.Manager.ShareUsage = _checkBoxShareUsage.Checked;
                
                message = String.Format(
                    _checkBoxShareUsage.Checked ?
                    ShareUsageTrueHtml : ShareUsageFalseHtml, SuccessCssClass);
            }
            catch
            {
                message = String.Format(
                    ShareUsageErrorHtml, ErrorCssClass);
            }
            
            if (ShareUsageChanged == null)
                _literalShareUsageResult.Text = message;
            else
                ShareUsageChanged(this, message);
        }

        #endregion
    }
}
