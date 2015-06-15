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
        #region Fields

        #region Controls

        private Literal _literalResult = null;
        private CheckBox _checkBoxShareUsage = null;
        private CheckBox _checkBoxDeviceDetection = null;
        private CheckBox _checkBoxAutoUpdate = null;
        private CheckBox _checkBoxImageOptimiser = null;
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

        private string _deviceDetectionText = Resources.DeviceDetectionText;
        private string _deviceDetectionTrueHtml = Resources.DeviceDetectionTrueHtml;
        private string _deviceDetectionFalseHtml = Resources.DeviceDetectionFalseHtml;
        private string _deviceDetectionErrorHtml = Resources.DeviceDetectionErrorHtml;

        private string _autoUpdateText = Resources.AutoUpdateText;
        private string _autoUpdateTrueHtml = Resources.AutoUpdateTrueHtml;
        private string _autoUpdateFalseHtml = Resources.AutoUpdateFalseHtml;
        private string _autoUpdateErrorHtml = Resources.AutoUpdateErrorHtml;

        private string _imageOptimiserText = Resources.ImageOptimiserText;
        private string _imageOptimiserTrueHtml = Resources.ImageOptimiserTrueHtml;
        private string _imageOptimiserFalseHtml = Resources.ImageOptimiserFalseHtml;
        private string _imageOptimiserErrorHtml = Resources.ImageOptimiserErrorHtml;

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

        /// <summary>
        /// Text used to inform user about sharing usage information.
        /// </summary>
        public string DeviceDetectionText
        {
            get { return _deviceDetectionText; }
            set { _deviceDetectionText = value; }
        }

        /// <summary>
        /// Html used when the share usage value is set to true.
        /// </summary>
        public string DeviceDetectionTrueHtml
        {
            get { return _deviceDetectionTrueHtml; }
            set { _deviceDetectionTrueHtml = value; }
        }

        /// <summary>
        /// Html used when the share usage value is set to false.
        /// </summary>
        public string DeviceDetectionFalseHtml
        {
            get { return _deviceDetectionFalseHtml; }
            set { _deviceDetectionFalseHtml = value; }
        }

        /// <summary>
        /// Html used when an error is generated changing the share usage value.
        /// </summary>
        public string DeviceDetectionErrorHtml
        {
            get { return _deviceDetectionErrorHtml; }
            set { _deviceDetectionErrorHtml = value; }
        }

        /// <summary>
        /// Text used to inform user about sharing usage information.
        /// </summary>
        public string AutoUpdateText
        {
            get { return _autoUpdateText; }
            set { _autoUpdateText = value; }
        }

        /// <summary>
        /// Html used when the share usage value is set to true.
        /// </summary>
        public string AutoUpdateTrueHtml
        {
            get { return _autoUpdateTrueHtml; }
            set { _autoUpdateTrueHtml = value; }
        }

        /// <summary>
        /// Html used when the share usage value is set to false.
        /// </summary>
        public string AutoUpdateFalseHtml
        {
            get { return _autoUpdateFalseHtml; }
            set { _autoUpdateFalseHtml = value; }
        }

        /// <summary>
        /// Html used when an error is generated changing the share usage value.
        /// </summary>
        public string AutoUpdateErrorHtml
        {
            get { return _autoUpdateErrorHtml; }
            set { _autoUpdateErrorHtml = value; }
        }

        /// <summary>
        /// Text used to inform user about sharing usage information.
        /// </summary>
        public string ImageOptimiserText
        {
            get { return _imageOptimiserText; }
            set { _imageOptimiserText = value; }
        }

        /// <summary>
        /// Html used when the share usage value is set to true.
        /// </summary>
        public string ImageOptimiserTrueHtml
        {
            get { return _imageOptimiserTrueHtml; }
            set { _imageOptimiserTrueHtml = value; }
        }

        /// <summary>
        /// Html used when the share usage value is set to false.
        /// </summary>
        public string ImageOptimiserFalseHtml
        {
            get { return _imageOptimiserFalseHtml; }
            set { _imageOptimiserFalseHtml = value; }
        }

        /// <summary>
        /// Html used when an error is generated changing the share usage value.
        /// </summary>
        public string ImageOptimiserErrorHtml
        {
            get { return _imageOptimiserErrorHtml; }
            set { _imageOptimiserErrorHtml = value; }
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
        /// Fired when the check box is changed.
        /// </summary>
        public event ShareUsageChangedEventHandler ShareUsageChanged;

        /// <summary>
        /// Fired when the check box is changed.
        /// </summary>
        public event ShareUsageChangedEventHandler DeviceDetectionChanged;

        /// <summary>
        /// Fired when the check box is changed.
        /// </summary>
        public event ShareUsageChangedEventHandler AutoUpdateChanged;

        /// <summary>
        /// Fired when the check box is changed.
        /// </summary>
        public event ShareUsageChangedEventHandler ImageOptimiserChanged;

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
            _checkBoxImageOptimiser = new CheckBox();
            _checkBoxShareUsage = new CheckBox();
            _checkBoxAutoUpdate = new CheckBox();
            _checkBoxDeviceDetection = new CheckBox();
            _hyperLinkShareUsage = new HyperLink();
            _literalResult = new Literal();

            // Set the new controls properties.
            _checkBoxShareUsage.ID = "CheckBoxShareUsage";
            _checkBoxAutoUpdate.ID = "CheckBoxAutoUpdate";
            _checkBoxDeviceDetection.ID = "CheckBoxDeviceDetection";
            _checkBoxImageOptimiser.ID = "CheckBoxImageOptimiser";
            _checkBoxImageOptimiser.CausesValidation = false;
            _checkBoxShareUsage.CausesValidation = false;
            _checkBoxAutoUpdate.CausesValidation = false;
            _checkBoxDeviceDetection.CausesValidation = false;
            _checkBoxImageOptimiser.AutoPostBack = true;
            _checkBoxShareUsage.AutoPostBack = true;
            _checkBoxAutoUpdate.AutoPostBack = true;
            _checkBoxDeviceDetection.AutoPostBack = true;
            _checkBoxImageOptimiser.Checked = FiftyOne.Foundation.Mobile.Configuration.Manager.ImageOptimisation.Enabled;
            _checkBoxShareUsage.Checked = FiftyOne.Foundation.Mobile.Detection.Configuration.Manager.ShareUsage;
            _checkBoxDeviceDetection.Checked = FiftyOne.Foundation.Mobile.Detection.Configuration.Manager.Enabled;
            _checkBoxAutoUpdate.Checked = FiftyOne.Foundation.Mobile.Detection.Configuration.Manager.AutoUpdate;
            _checkBoxShareUsage.ViewStateMode = System.Web.UI.ViewStateMode.Enabled;
            _checkBoxAutoUpdate.ViewStateMode = System.Web.UI.ViewStateMode.Enabled;
            _checkBoxDeviceDetection.ViewStateMode = System.Web.UI.ViewStateMode.Enabled;
            _checkBoxImageOptimiser.ViewStateMode = System.Web.UI.ViewStateMode.Enabled;
            _checkBoxShareUsage.CheckedChanged += new EventHandler(_checkBoxShareUsage_CheckedChanged);
            _checkBoxAutoUpdate.CheckedChanged += new EventHandler(_checkBoxAutoUpdate_CheckedChanged);
            _checkBoxDeviceDetection.CheckedChanged += new EventHandler(_checkBoxDeviceDetection_CheckedChanged);
            _checkBoxImageOptimiser.CheckedChanged += new EventHandler(_checkBoxImageOptimiser_CheckedChanged);
            _hyperLinkShareUsage.ID = "HyperLinkShareUsage";
            
            // Add the controls to the user control.
            _container.Controls.Add(_literalResult);
            _container.Controls.Add(_checkBoxDeviceDetection);
            _container.Controls.Add(_checkBoxAutoUpdate);
            _container.Controls.Add(_checkBoxImageOptimiser);
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

            _literalResult.Visible = !String.IsNullOrEmpty(_literalResult.Text);

            _checkBoxShareUsage.CssClass = CheckBoxCssClass;
            _checkBoxAutoUpdate.CssClass = CheckBoxCssClass;
            _checkBoxDeviceDetection.CssClass = CheckBoxCssClass;
            _checkBoxImageOptimiser.CssClass = CheckBoxCssClass;
            _checkBoxShareUsage.Text = ShareUsageText;
            _checkBoxAutoUpdate.Text = AutoUpdateText;
            _checkBoxDeviceDetection.Text = DeviceDetectionText;
            _checkBoxImageOptimiser.Text = ImageOptimiserText;

            _hyperLinkShareUsage.CssClass = HyperLinkCssClass;
            _hyperLinkShareUsage.Text = ShareUsageLinkText;
            _hyperLinkShareUsage.NavigateUrl = ShareUsageUrl;
            _hyperLinkShareUsage.Target = "_blank";

            _hyperLinkShareUsage.Visible = _checkBoxShareUsage.Visible = 
                _literalResult.Visible = _checkBoxShareUsage.Visible =
                _checkBoxDeviceDetection.Visible = _checkBoxImageOptimiser.Visible = 
                _checkBoxAutoUpdate.Visible = ShowShareUsage;
        }

        /// <summary>
        /// Fired when the user changes the image optimiser preference.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _checkBoxImageOptimiser_CheckedChanged(object sender, EventArgs e)
        {
            string message;

            try
            {
                FiftyOne.Foundation.Mobile.Configuration.Manager.ImageOptimisation.Enabled = _checkBoxImageOptimiser.Checked;

                

                Context.Application["51D_ImageOptimiser"] = _checkBoxImageOptimiser.Checked;

                message = String.Format(
                    _checkBoxImageOptimiser.Checked ?
                    ImageOptimiserTrueHtml : ImageOptimiserFalseHtml, SuccessCssClass);
            }
            catch
            {
                message = String.Format(
                    ImageOptimiserErrorHtml, ErrorCssClass);
            }

            if (ImageOptimiserChanged == null)
                _literalResult.Text = message;
            else
                ImageOptimiserChanged(this, message);
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
                _literalResult.Text = message;
            else
                ShareUsageChanged(this, message);
        }

        /// <summary>
        /// Fired when the user changes the device detection preference.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _checkBoxDeviceDetection_CheckedChanged(object sender, EventArgs e)
        {
            string message;

            try
            {
                FiftyOne.Foundation.Mobile.Detection.Configuration.Manager.Enabled = _checkBoxDeviceDetection.Checked;

                message = String.Format(
                    _checkBoxDeviceDetection.Checked ?
                    DeviceDetectionTrueHtml : DeviceDetectionFalseHtml, SuccessCssClass);
            }
            catch
            {
                message = String.Format(
                    DeviceDetectionErrorHtml, ErrorCssClass);
            }

            if (DeviceDetectionChanged == null)
                _literalResult.Text = message;
            else
                DeviceDetectionChanged(this, message);
        }

        /// <summary>
        /// Fired when the user changes the auto update preference.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _checkBoxAutoUpdate_CheckedChanged(object sender, EventArgs e)
        {
            string message;

            try
            {
                FiftyOne.Foundation.Mobile.Detection.Configuration.Manager.AutoUpdate = _checkBoxAutoUpdate.Checked;
                
                message = String.Format(
                    _checkBoxAutoUpdate.Checked ?
                    AutoUpdateTrueHtml : AutoUpdateFalseHtml, SuccessCssClass);
            }
            catch
            {
                message = String.Format(
                    AutoUpdateErrorHtml, ErrorCssClass);
            }
            
            if (AutoUpdateChanged == null)
                _literalResult.Text = message;
            else
                AutoUpdateChanged(this, message);
        }

        #endregion
    }
}
