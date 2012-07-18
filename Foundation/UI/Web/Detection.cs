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
    /// Display a user interface to enable the user to enter a Premium license key and upgrade Foundation
    /// to the premium product. If the fiftyOne/detection element does not exist in the configuration
    /// this element is added and the binaryFilePath set to "51Degrees-Premium.dat". The licence key will 
    /// be written to a file called 51Degrees.mobi.lic in the bin folder.
    /// If the site is running in medium trust mode the operation will fail and a message will be displayed
    /// to the user.
    /// The control also contains a check box to enable / disable the sharing of usage information with
    /// 51Degrees.mobi.
    /// </summary>
    public class Detection : BaseDataControl
    {
        #region Constants

        private const string VALIDATION_LICENCE = "ValidateLicence";
        
        #endregion

        #region Fields

        #region Controls

        private Literal _literalInstructions = null;
        private TextBox _textBoxLicenceKey = null;
        private Button _buttonActivate = null;
        private Button _buttonRefresh = null;
        private RequiredFieldValidator _validatorRequired = null;
        private RegularExpressionValidator _validatorRegEx = null;
        private ValidationSummary _validationLicenceSummary = null;
        private Literal _literalResult = null;
        private Upload _upload = new Upload();
        private Literal _literalUpload = null;
        private ShareUsage _shareUsage = new ShareUsage();

        #endregion

        #region Css

        private string _textBoxCssClass = "textbox";

        #endregion

        #region Messages

        private string _activateButtonText = Resources.ActivateButtonText;
        private string _activatedMessageHtml = Resources.ActivatedMessageHtml;
        private string _activateInstructionsHtml = Resources.ActivateInstructionsHtml;
        private string _uploadInstructionsHtml = Resources.UploadInstructionsHtml;
        private string _validationRequiredErrorText = Resources.ValidationRequiredErrorText;
        private string _refreshButtonText = Resources.RefreshButtonText;
        private string _validationRegExErrorText = Resources.ValidationRegExErrorText;

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
            get { return _shareUsage.ShareUsageUrl; }
            set { _shareUsage.ShareUsageUrl = value; }
        }

        /// <summary>
        /// The text that should appear on the hyper link next to the 
        /// share usage check box text.
        /// </summary>
        public string ShareUsageLinkText
        {
            get { return _shareUsage.ShareUsageLinkText; }
            set { _shareUsage.ShareUsageLinkText = value; }
        }

        /// <summary>
        /// Html used when the share usage value is set to true.
        /// </summary>
        public string ShareUsageTrueHtml
        {
            get { return _shareUsage.ShareUsageTrueHtml; }
            set { _shareUsage.ShareUsageTrueHtml = value; }
        }

        /// <summary>
        /// Html used when the share usage value is set to false.
        /// </summary>
        public string ShareUsageFalseHtml
        {
            get { return _shareUsage.ShareUsageFalseHtml; }
            set { _shareUsage.ShareUsageFalseHtml = value; }
        }

        /// <summary>
        /// Html used when an error is generated changing the share usage value.
        /// </summary>
        public string ShareUsageErrorHtml
        {
            get { return _shareUsage.ShareUsageErrorHtml; }
            set { _shareUsage.ShareUsageErrorHtml = value; }
        }

        /// <summary>
        /// Text used to inform user about sharing usage information.
        /// </summary>
        public string ShareUsageText
        {
            get { return _shareUsage.ShareUsageText; }
            set { _shareUsage.ShareUsageText = value; }
        }

        /// <summary>
        /// Error HTML displayed if the premium data is invalid.
        /// </summary>
        public override string ActivationDataInvalidHtml
        {
            get
            {
                return base.ActivationDataInvalidHtml;
            }
            set
            {
                base.ActivationDataInvalidHtml = value;
                _upload.ActivationDataInvalidHtml = value;
            }
        }

        /// <summary>
        /// Error HTML dispalyed in the configuration files can't be changed.
        /// </summary>
        public override string ActivationFailureCouldNotUpdateConfigHtml
        {
            get
            {
                return base.ActivationFailureCouldNotUpdateConfigHtml;
            }
            set
            {
                base.ActivationFailureCouldNotUpdateConfigHtml = value;
                _upload.ActivationFailureCouldNotUpdateConfigHtml = value;
            }
        }

        /// <summary>
        /// Error HTML displayed if the premium data file downloaded can't be written.
        /// </summary>
        public override string ActivationFailureCouldNotWriteDataFileHtml
        {
            get
            {
                return base.ActivationFailureCouldNotWriteDataFileHtml;
            }
            set
            {
                base.ActivationFailureCouldNotWriteDataFileHtml = value;
                _upload.ActivationFailureCouldNotWriteDataFileHtml = value;
            }
        }

        /// <summary>
        /// Error HTML displayed if the premium data licence file can't be written.
        /// </summary>
        public override string ActivationFailureCouldNotWriteLicenceFileHtml
        {
            get
            {
                return base.ActivationFailureCouldNotWriteLicenceFileHtml;
            }
            set
            {
                base.ActivationFailureCouldNotWriteLicenceFileHtml = value;
                _upload.ActivationFailureCouldNotWriteLicenceFileHtml = value;
            }
        }

        /// <summary>
        /// Error HTML displayed if there is a unknown problem with activation.
        /// </summary>
        public override string ActivationFailureGenericHtml
        {
            get
            {
                return base.ActivationFailureGenericHtml;
            }
            set
            {
                base.ActivationFailureGenericHtml = value;
                _upload.ActivationFailureGenericHtml = value;
            }
        }
        
        /// <summary>
        /// Error HTML displayed if the HTTP connection to get premium data can not be established.
        /// </summary>
        public override string ActivationFailureHttpHtml
        {
            get
            {
                return base.ActivationFailureHttpHtml;
            }
            set
            {
                base.ActivationFailureHttpHtml = value;
                _upload.ActivationFailureHttpHtml = value;
            }
        }

        /// <summary>
        /// Error HTML displayed if the licence key is invalid.
        /// </summary>
        public override string ActivationFailureInvalidHtml
        {
            get
            {
                return base.ActivationFailureInvalidHtml;
            }
            set
            {
                base.ActivationFailureInvalidHtml = value;
                _upload.ActivationFailureInvalidHtml = value;
            }
        }

        /// <summary>
        /// Error HTML displayed if the returned stream is not valid during activation.
        /// </summary>
        public override string ActivationStreamFailureHtml
        {
            get
            {
                return base.ActivationStreamFailureHtml;
            }
            set
            {
                base.ActivationStreamFailureHtml = value;
                _upload.ActivationStreamFailureHtml = value;
            }
        }

        /// <summary>
        /// Error HTML displayed on successful activation.
        /// </summary>
        public override string ActivationSuccessHtml
        {
            get
            {
                return base.ActivationSuccessHtml;
            }
            set
            {
                base.ActivationSuccessHtml = value;
                _upload.ActivationSuccessHtml = value;
            }
        }

        /// <summary>
        /// Validation regex error message.
        /// </summary>
        public string ValidationRegExErrorText
        {
            get { return _validationRegExErrorText; }
            set { _validationRegExErrorText = value; }
        }

        /// <summary>
        /// Activate button text.
        /// </summary>
        public string ActivateButtonText
        {
            get
            {
                return _activateButtonText;
            }
            set
            {
                _activateButtonText = value;
            }
        }

        /// <summary>
        /// HTML displayed when the premium data is activated.
        /// </summary>
        public string ActivatedMessageHtml
        {
            get
            {
                return _activatedMessageHtml;
            }
            set
            {
                _activatedMessageHtml = value;
            }
        }

        /// <summary>
        /// Instruction HTML to activate premium data. Appears at the top of the control.
        /// Suppressed if InstructionsEnabled is set to false.
        /// </summary>
        public string ActivateInstructionsHtml
        {
            get
            {
                return _activateInstructionsHtml;
            }
            set
            {
                _activateInstructionsHtml = value;
            }
        }

        /// <summary>
        /// Errror message displayed if the licence key is not provided.
        /// </summary>
        public string ValidationRequiredErrorText
        {
            get
            {
                return _validationRequiredErrorText;
            }
            set
            {
                _validationRequiredErrorText = value;
            }
        }

        /// <summary>
        /// Error message displayed if the file provided is invalid.
        /// </summary>
        public string ValidationFileErrorText
        {
            get
            {
                return _upload.ValidationFileErrorText;
            }
            set
            {
                _upload.ValidationFileErrorText = value;
            }
        }

        /// <summary>
        /// Text used on the refresh button displayed after activation.
        /// </summary>
        public string RefreshButtonText
        {
            get
            {
                return _refreshButtonText;
            }
            set
            {
                _refreshButtonText = value;
            }
        }

        /// <summary>
        /// Text used on the upload file button.
        /// </summary>
        public string UploadButtonText
        {
            get
            {
                return _upload.UploadButtonText;
            }
            set
            {
                _upload.UploadButtonText = value;
            }
        }

        /// <summary>
        /// Instruction HTML used in the middle of the control to explain how to upload data.
        /// </summary>
        public string UploadInstructionsHtml
        {
            get
            {
                return _uploadInstructionsHtml;
            }
            set
            {
                _uploadInstructionsHtml = value;
            }
        }

        #endregion

        #region Css

        /// <summary>
        /// Gets or sets the hyper link css class for share usage.
        /// </summary>
        public string HyperLinkCssClass
        {
            get { return _shareUsage.HyperLinkCssClass; }
            set { _shareUsage.HyperLinkCssClass = value; }
        }

        /// <summary>
        /// Gets or sets the check box css class for share usaged check box.
        /// </summary>
        public string CheckBoxCssClass
        {
            get { return _shareUsage.CheckBoxCssClass; }
            set { _shareUsage.CheckBoxCssClass = value; }
        }

        /// <summary>
        /// Gets or sets the text box css class for licence key entry.
        /// </summary>
        public string TextBoxCssClass
        {
            get { return _textBoxCssClass; }
            set { _textBoxCssClass = value; }
        }

        /// <summary>
        /// Gets or sets the button css class for activation and refresh.
        /// </summary>
        public override string ButtonCssClass
        {
            get { return base.ButtonCssClass; }
            set { base.ButtonCssClass = value; _upload.ButtonCssClass = value; }
        }

        /// <summary>
        /// Gets or sets the success css class for messages.
        /// </summary>
        public override string SuccessCssClass
        {
            get { return base.SuccessCssClass; }
            set { base.SuccessCssClass = value; _upload.SuccessCssClass = value; _shareUsage.SuccessCssClass = value; }
        }

        /// <summary>
        /// Gets or sets the error css class for messages.
        /// </summary>
        public override string ErrorCssClass
        {
            get { return base.ErrorCssClass; }
            set { base.ErrorCssClass = value; _upload.ErrorCssClass = value; _shareUsage.ErrorCssClass = value; }
        }

        #endregion

        #region Controls

        /// <summary>
        /// The textbox used to capture the licence key.
        /// </summary>
        public TextBox LicenceKeyTextBox
        {
            get { return _textBoxLicenceKey; }
        }

        /// <summary>
        /// The button used to activate the licence key.
        /// </summary>
        public Button ActivateButton
        {
            get { return _buttonActivate; }
        }

        /// <summary>
        /// The upload control used to upload data.
        /// </summary>
        public Upload ActivateFileUploadData
        {
            get { return _upload; }
        }

        #endregion

        #region Other

        /// <summary>
        /// Controls whether the share usage information is displayed.
        /// Defaults to true.
        /// </summary>
        public bool ShowShareUsage
        {
            get { return _shareUsage.ShowShareUsage; }
            set { _shareUsage.ShowShareUsage = value; }
        }

        /// <summary>
        /// Controls if instruction messages are displayed.
        /// </summary>
        public override bool InstructionsEnabled
        {
            get { return base.InstructionsEnabled; }
            set 
            { 
                base.InstructionsEnabled = value; 
                _upload.InstructionsEnabled = value; 
            }
        }

        #endregion

        #endregion

        #region Events

        /// <summary>
        /// Load the controls which will form the user interface.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // Set the literal control.
            _literalUpload = new Literal();
            _literalInstructions = new Literal();

            // Create the required controls.
            _textBoxLicenceKey = new TextBox();
            _buttonActivate = new Button();
            _buttonRefresh = new Button();
            _validatorRequired = new RequiredFieldValidator();
            _validatorRegEx = new RegularExpressionValidator();
            _validationLicenceSummary = new ValidationSummary();
            _literalResult = new Literal();

            // Set the new controls properties.
            _buttonActivate.ID = "ButtonActivate";
            _buttonActivate.Click += new EventHandler(_buttonActivate_Click);
            _buttonActivate.ValidationGroup = VALIDATION_LICENCE;
            _buttonRefresh.ID = "ButtonRefresh";
            _buttonRefresh.Click += new EventHandler(_buttonActivate_Click);
            _buttonRefresh.Visible = false;
            _textBoxLicenceKey.ID = "TextBoxLicenceKey";
            _textBoxLicenceKey.ValidationGroup = VALIDATION_LICENCE;
                        
            // Set the validators.
            _validatorRequired.ID = "ValidatorRequired";
            _validatorRegEx.ID = "ValidatorRegEx";
            _validationLicenceSummary.ID = "ValidationLicenceSummary";
            _validatorRequired.ControlToValidate = _validatorRegEx.ControlToValidate = _textBoxLicenceKey.ID;
            _validatorRegEx.ValidationGroup = _validatorRequired.ValidationGroup = VALIDATION_LICENCE;
            _validatorRegEx.ValidationExpression = FiftyOne.Foundation.Mobile.Detection.Constants.LicenceKeyValidationRegex;
            _validationLicenceSummary.DisplayMode = ValidationSummaryDisplayMode.SingleParagraph;
            _validationLicenceSummary.Style.Clear();
            _validationLicenceSummary.ValidationGroup = VALIDATION_LICENCE;
            _validatorRequired.Display = _validatorRegEx.Display = ValidatorDisplay.None;
            _validatorRegEx.EnableClientScript = _validatorRequired.EnableClientScript =
                _validationLicenceSummary.EnableClientScript = true;

            // Set the child controls.
            _upload.FooterEnabled = false;
            _upload.LogoEnabled = false;
            _upload.UploadComplete += new UploadEventHandler(_upload_UploadComplete);
            _shareUsage.LogoEnabled = false;
            _shareUsage.FooterEnabled = false;
            _shareUsage.ShareUsageChanged += new ShareUsageChangedEventHandler(_shareUsage_ShareUsageChanged);

            // Add the controls to the user control.
            if (DataProvider.IsPremium == false)
            {
                _container.Controls.Add(_literalResult);
                _container.Controls.Add(_validationLicenceSummary);
            }
            _container.Controls.Add(_literalInstructions);
            if (DataProvider.IsPremium == false)
            {
                _container.Controls.Add(_validatorRequired);
                _container.Controls.Add(_validatorRegEx);
                _container.Controls.Add(_textBoxLicenceKey);
                _container.Controls.Add(_buttonActivate);
                _container.Controls.Add(_buttonRefresh);
                _container.Controls.Add(_literalUpload);
                _container.Controls.Add(_upload);
            }

            _container.Controls.Add(_shareUsage);
        }

        /// <summary>
        /// Sets the visible status of the instruction messages.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            _literalInstructions.Visible = _literalInstructions.Visible & InstructionsEnabled;
            _literalUpload.Visible = _literalUpload.Visible & InstructionsEnabled;
            if (String.IsNullOrEmpty(_literalInstructions.Text))
                _literalInstructions.Text = DataProvider.IsPremium ?
                    String.Format(ActivatedMessageHtml, SuccessCssClass) :
                    ActivateInstructionsHtml;
            _literalUpload.Text = UploadInstructionsHtml;
            _buttonActivate.Text = ActivateButtonText;
            _buttonRefresh.Text = RefreshButtonText;
            _validatorRequired.ErrorMessage = ValidationRequiredErrorText;
            _buttonActivate.CssClass = ButtonCssClass;
            _buttonRefresh.CssClass = ButtonCssClass;
            _textBoxLicenceKey.CssClass = TextBoxCssClass;
            _validationLicenceSummary.CssClass = ErrorCssClass;
            _validatorRegEx.ErrorMessage = ValidationRegExErrorText;
        }

        /// <summary>
        /// Fired when the user presses the Activate button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _buttonActivate_Click(object sender, EventArgs e)
        {
            if (Page.IsValid)
            {
                ActivityResult result = Execute(_textBoxLicenceKey.Text);
                DisplayResults(result);
            }
        }
        
        /// <summary>
        /// Displays the results to the user.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _upload_UploadComplete(object sender, ActivityResult e)
        {
            DisplayResults(e);
        }
        
        /// <summary>
        /// Displays the result of the share usage change.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="html"></param>
        private void _shareUsage_ShareUsageChanged(object sender, string html)
        {
            _literalInstructions.Text = html;
        }

        /// <summary>
        /// Displays the results returned from an activation or an
        /// upload activity.
        /// </summary>
        /// <param name="result"></param>
        private void DisplayResults(ActivityResult result)
        {
            _literalResult.Text = result.Html;
            if (result.Success)
            {
                _buttonActivate.Visible = _textBoxLicenceKey.Visible = _literalInstructions.Visible =
                _validationLicenceSummary.Visible = _validatorRegEx.Visible = _validatorRequired.Visible =
                _literalUpload.Visible = _upload.Visible = false;
                _buttonRefresh.Visible = true;
                base._footer.ButtonVisible = false;
            }
        }

        /// <summary>
        /// Used to refresh the page after the activation has been successful.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _buttonRefresh_Click(object sender, EventArgs e)
        {
            // Do nothing.
        }

        #endregion
    }
}
