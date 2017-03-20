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
using System.Web.UI;
using System.Web.UI.WebControls;
using FiftyOne.Foundation.Mobile;
using FiftyOne.Foundation.Mobile.Detection;

namespace FiftyOne.Foundation.UI.Web
{
    /// <summary>
    /// Displays key stats about the active data provider.
    /// </summary>
    public class Stats : UserControl
    {
        #region Fields

        private string _cssClass = "footer";
        private Literal _literal = null;
        private bool _buttonVisible = true;
        private string _buttonText = Resources.StatsRefreshButtonText;
        private string _buttonCssClass = "button";
        private string _html = Resources.StatsHtml;
        private Button _buttonRefresh = null;

        #endregion

        #region Properties

        /// <summary>
        /// Returns true if the refresh button should be visible.
        /// </summary>
        public bool ButtonVisible
        {
            get { return _buttonVisible; }
            set { _buttonVisible = value; }
        }
        
        /// <summary>
        /// Sets the Html for the control with the following replacable sections.
        /// {0} = CssClass
        /// {1} = Data type Lite / Premium
        /// {2} = Published data
        /// {3} = Count of available properties
        /// {4} = Detection time
        /// </summary>
        public string Html
        {
            get { return _html; }
            set { _html = value; }
        }

        /// <summary>
        /// The text that appears on the CSS button.
        /// </summary>
        public string RefreshButtonText
        {
            get { return _buttonText; }
            set { _buttonText = value; }
        }

        /// <summary>
        /// The css class used for the refresh button.
        /// </summary>
        public string ButtonCssClass
        {
            get { return _buttonCssClass; }
            set { _buttonCssClass = value; }
        }

        /// <summary>
        /// The css class used for the statistics literal control.
        /// </summary>
        public string CssClass
        {
            get { return _cssClass; }
            set { _cssClass = value; }
        }

        #endregion

        #region Events

        /// <summary>
        /// Initialise the control.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit(EventArgs e)
        {
 	        base.OnInit(e);
            _buttonRefresh = new Button();
            _literal = new Literal();
            _buttonRefresh.ID = "ButtonRefresh";
            Controls.Add(_literal);
            Controls.Add(_buttonRefresh);
            _buttonRefresh.Click += new EventHandler(_buttonRefresh_Click);
            Page.PreRenderComplete += new EventHandler(Page_PreRenderComplete);
        }

        /// <summary>
        /// The refresh button has been clicked, refresh the data.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _buttonRefresh_Click(object sender, EventArgs e)
        {
            try
            {
                if (AutoUpdate.Download(LicenceKey.Keys) == LicenceKeyResults.Success)
                {
                    WebProvider.Refresh();
                }
            }
            catch (Exception ex)
            {
                EventLog.Warn(new MobileException("Exception refreshing data.", ex));
            }
        }

        /// <summary>
        /// Set the properties of the controls.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            _buttonRefresh.Text = RefreshButtonText;
            _buttonRefresh.CssClass = ButtonCssClass;
            
            // Only enable the refresh button if premium keys are available.
            _buttonRefresh.Visible = ButtonVisible && LicenceKey.Keys != null && LicenceKey.Keys.Length > 0;

            // Enable the button if there's a chance new data could be available.
            var provider = WebProvider.ActiveProvider;
            _buttonRefresh.Enabled = provider != null ? provider.DataSet.Published.Add(
                FiftyOne.Foundation.Mobile.Detection.Constants.AutoUpdateWait) < DateTime.UtcNow : true;
        }

        /// <summary>
        /// Displays the statistics about the provider.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_PreRenderComplete(object sender, EventArgs e)
        {
            var dataSet = WebProvider.ActiveProvider != null ? WebProvider.ActiveProvider.DataSet : null;
            _literal.Text = String.Format(
                Html,
                CssClass,
                dataSet != null ? dataSet.Name : "Not Present",
                dataSet != null ? dataSet.Published : DateTime.MinValue,
                dataSet != null ? dataSet.Properties.Count : 0,
                Request.Browser[FiftyOne.Foundation.Mobile.Detection.Constants.DetectionTimeProperty],
                Context.Items["51D_AverageResponseTime"] == null ? "NA" : Context.Items["51D_AverageResponseTime"],
                Context.Items["51D_AverageCompletionTime"] == null ? "NA" : Context.Items["51D_AverageCompletionTime"]);
        }

        #endregion
    }
}
