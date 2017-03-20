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
using System.Web.UI.WebControls;
using FiftyOne.Foundation.Mobile.Detection;

namespace FiftyOne.Foundation.UI.Web
{
    /// <summary>
    /// Class to test a User-Agent and display the resulting device.
    /// </summary>
    public class UserAgentTester : BaseUserControl
    {
        #region Fields

        private Literal _instructions;
        private TextBox _textBoxUserAgent;
        private Button _buttonTest;
        private HyperLink _userAgentLink;
        private DeviceExplorer _deviceExplorer;

        private string _textBoxCssClass = "textbox";
        private string _buttonCssClass = "button";
        private string _linkCssClass = "link";
        private string _userAgentTesterButton = Resources.UserAgentTesterButtonText;
        private string _userAgentTesterInstructions = Resources.UserAgentTesterInstructions;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the css class used for the link.
        /// </summary>
        public string LinkCssClass
        {
            get { return _linkCssClass; }
            set { _linkCssClass = value; }
        }

        /// <summary>
        /// Gets or sets the text box css class for User-Agent entry.
        /// </summary>
        public string TextBoxCssClass
        {
            get { return _textBoxCssClass; }
            set { _textBoxCssClass = value; }
        }

        /// <summary>
        /// Gets or sets the button css class for activation and refresh.
        /// </summary>
        public string ButtonCssClass
        {
            get { return _buttonCssClass; }
            set { _buttonCssClass = value; }
        }

        /// <summary>
        /// Instruction text to use with the control.
        /// </summary>
        public string UserAgentTesterInstructions
        {
            get { return _userAgentTesterInstructions; }
            set { _userAgentTesterInstructions = value; }
        }

        /// <summary>
        /// Button text to use with the control.
        /// </summary>
        public string UserAgentTesterButton
        {
            get { return _userAgentTesterButton; }
            set { _userAgentTesterButton = value; }
        }

        #endregion

        #region Events

        /// <summary>
        /// Creates the new controls used by the User-Agent tester.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (DataSet != null)
            {
                _instructions = new Literal();
                _textBoxUserAgent = new TextBox();
                _buttonTest = new Button();
                _userAgentLink = new HyperLink();
                _deviceExplorer = new DeviceExplorer();
                _textBoxUserAgent.MaxLength = 800;
                _buttonTest.Click += new EventHandler(ButtonTest_Click);
                _deviceExplorer.Navigation = false;
                _deviceExplorer.FooterEnabled = false;
                _deviceExplorer.LogoEnabled = false;
                _deviceExplorer.ImagesEnabled = true;
                _container.Controls.Add(_instructions);
                _container.Controls.Add(_textBoxUserAgent);
                _container.Controls.Add(_buttonTest);
                _container.Controls.Add(_userAgentLink);
                _container.Controls.Add(_deviceExplorer);
                _container.DefaultButton = _buttonTest.ID;
            }
        }

        /// <summary>
        /// Controls if the device explorer is visible.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (DataSet != null)
            {
                _deviceExplorer.Visible =
                    _deviceExplorer.DeviceID != null ||
                    String.IsNullOrEmpty(_deviceExplorer.UserAgent) == false;

                _textBoxUserAgent.Text = String.IsNullOrEmpty(_deviceExplorer.UserAgent) ?
                    Request.UserAgent : _deviceExplorer.UserAgent;

                _userAgentLink.NavigateUrl = base.GetNewUrl("useragent", _textBoxUserAgent.Text);

                _userAgentLink.CssClass = LinkCssClass;
                _textBoxUserAgent.CssClass = TextBoxCssClass;
                _buttonTest.CssClass = ButtonCssClass;
                _instructions.Text = UserAgentTesterInstructions;
                _buttonTest.Text = UserAgentTesterButton;
                _userAgentLink.Text = "Link";
            }
        }

        private void ButtonTest_Click(object sender, EventArgs e)
        {
            _deviceExplorer.UserAgent = _textBoxUserAgent.Text;
        }

        #endregion
    }
}
