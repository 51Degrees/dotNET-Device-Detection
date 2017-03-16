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
using System.IO;
using System.IO.Compression;

namespace FiftyOne.Foundation.UI.Web
{
    /// <summary>
    /// EventHandler used to notify containers that the upload has completed.
    /// </summary>
    /// <param name="sender">The control instance generating the event.</param>
    /// <param name="e">The results of the upload.</param>
    public delegate void UploadEventHandler(object sender, ActivityResult e);

    /// <summary>
    /// User Control to enable the uploading of a data file manually.
    /// </summary>
    public class Upload : BaseDataControl
    {
        #region Constants

        private const string VALIDATION_UPLOAD = "ValidateUpload";

        #endregion

        #region Fields

        #region Controls

        private FileUpload _fileUploadData = null;
        private Button _buttonUpload = null;
        private CustomValidator _validatorFile = null;
        private ValidationSummary _validationFileSummary = null;

        #endregion
        
        #region Messages

        private string _uploadButtonText = Resources.UploadButtonText;
        private string _validationFileErrorText = Resources.ValidationFileErrorText;

        #endregion

        #endregion

        #region Properties

        #region Messages

        /// <summary>
        /// Error message displayed if the file provided is invalid.
        /// </summary>
        public string ValidationFileErrorText
        {
            get
            {
                return _validationFileErrorText;
            }
            set
            {
                _validationFileErrorText = value;
            }
        }

        /// <summary>
        /// Text used on the upload file button.
        /// </summary>
        public string UploadButtonText
        {
            get
            {
                return _uploadButtonText;
            }
            set
            {
                _uploadButtonText = value;
            }
        }

        #endregion

        #endregion

        #region Event Handlers

        /// <summary>
        /// Fired when the upload has completed. Used to inform other
        /// controls the result of the upload.
        /// </summary>
        public event UploadEventHandler UploadComplete;

        #endregion

        #region Events

        /// <summary>
        /// Initialises the child controls in the upload control.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // Create the controls.
            _validatorFile = new CustomValidator();
            _buttonUpload = new Button();
            _fileUploadData = new FileUpload();
            _validationFileSummary = new ValidationSummary();

            // Set the new controls properties.
            _buttonUpload.ID = "ButtonUpload";
            _buttonUpload.Click += new EventHandler(_buttonUpload_Click);
            _buttonUpload.ValidationGroup = VALIDATION_UPLOAD;
            _fileUploadData.ID = "FileUpload";

            // Set the validators.
            _validatorFile.ID = "ValidatorFile";
            _validationFileSummary.Style.Clear();
            _validationFileSummary.ValidationGroup = VALIDATION_UPLOAD;
            _validatorFile.ValidationGroup = VALIDATION_UPLOAD;
            _validatorFile.ControlToValidate = _fileUploadData.ID;
            _validatorFile.ValidateEmptyText = true;
            _validatorFile.ServerValidate += new ServerValidateEventHandler(_validationFile_ServerValidate);
            _validationFileSummary.ID = "ValidationFileSummary";
            _validationFileSummary.DisplayMode = ValidationSummaryDisplayMode.SingleParagraph;
            _validatorFile.Display = ValidatorDisplay.None;
            _validationFileSummary.EnableClientScript = true;

            // Add the controls to the user control.
            _container.Controls.Add(_validationFileSummary);
            _container.Controls.Add(_fileUploadData);
            _container.Controls.Add(_buttonUpload);
        }

        /// <summary>
        /// Sets the visible status of the instruction messages.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            _buttonUpload.Text = UploadButtonText;
            _buttonUpload.CssClass = ButtonCssClass;
            _validationFileSummary.CssClass = ErrorCssClass;
            _validatorFile.ErrorMessage = ValidationFileErrorText;
        }

        /// <summary>
        /// Fired when the upload is completed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _buttonUpload_Click(object sender, EventArgs e)
        {
            if (Page.IsValid)
            {
                ActivityResult result;
                if (Path.GetExtension(_fileUploadData.PostedFile.FileName) == ".gz")
                {
                    using (var stream = new GZipStream(
                        _fileUploadData.PostedFile.InputStream,
                        CompressionMode.Decompress))
                    {
                        result = Execute(stream);
                    }
                }
                else
                {
                    result = Execute(_fileUploadData.PostedFile.InputStream);
                }
                if (UploadComplete != null)
                    UploadComplete(this, result);
            }
        }
        
        /// <summary>
        /// Validates a file has been selected.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="args"></param>
        private void _validationFile_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = _fileUploadData.HasFile;
        }

        #endregion
    }
}
