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
using System.IO;
using System.Security.Principal;
using System.Web.Hosting;
using FiftyOne.Foundation.Mobile.Detection;

namespace FiftyOne.Foundation.UI.Web
{
    /// <summary>
    /// Base control used to include common methods shared between activation
    /// with a licence key and uploading a data file.
    /// </summary>
    public class BaseDataControl : BaseUserControl
    {
        #region Fields

        #region Css

        private string _buttonCssClass = "button";
        private string _successCssClass = "success";
        private string _errorCssClass = "error";

        #endregion

        #region Messages

        private string _activationDataInvalidHtml = Resources.ActivationDataInvalidHtml;
        private string _activationFailureCouldNotUpdateConfigHtml = Resources.ActivationFailureCouldNotUpdateConfigHtml;
        private string _activationFailureCouldNotWriteDataFileHtml = Resources.ActivationFailureCouldNotWriteDataFileHtml;
        private string _activationFailureCouldNotWriteLicenceFileHtml = Resources.ActivationFailureCouldNotWriteLicenceFileHtml;
        private string _activationFailureGenericHtml = Resources.ActivationFailureGenericHtml;
        private string _activationFailureHttpHtml = Resources.ActivationFailureHttpHtml;
        private string _activationFailureInvalidHtml = Resources.ActivationFailureInvalidHtml;
        private string _activationStreamFailureHtml = Resources.ActivationStreamFailureHtml;
        private string _activationSuccessHtml = Resources.ActivationSuccessHtml;
        private bool _instructionsEnabled = true;

        #endregion

        #endregion

        #region Properties

        #region Messages

        /// <summary>
        /// Error HTML displayed if the premium data is invalid.
        /// </summary>
        public virtual string ActivationDataInvalidHtml
        {
            get
            {
                return _activationDataInvalidHtml;
            }
            set
            {
                _activationDataInvalidHtml = value;
            }
        }

        /// <summary>
        /// Error HTML dispalyed in the configuration files can't be changed.
        /// </summary>
        public virtual string ActivationFailureCouldNotUpdateConfigHtml
        {
            get
            {
                return _activationFailureCouldNotUpdateConfigHtml;
            }
            set
            {
                _activationFailureCouldNotUpdateConfigHtml = value;
            }
        }

        /// <summary>
        /// Error HTML displayed if the premium data file downloaded can't be written.
        /// </summary>
        public virtual string ActivationFailureCouldNotWriteDataFileHtml
        {
            get
            {
                return _activationFailureCouldNotWriteDataFileHtml;
            }
            set
            {
                _activationFailureCouldNotWriteDataFileHtml = value;
            }
        }

        /// <summary>
        /// Error HTML displayed if the premium data licence file can't be written.
        /// </summary>
        public virtual string ActivationFailureCouldNotWriteLicenceFileHtml
        {
            get
            {
                return _activationFailureCouldNotWriteLicenceFileHtml;
            }
            set
            {
                _activationFailureCouldNotWriteLicenceFileHtml = value;
            }
        }

        /// <summary>
        /// Error HTML displayed if there is a unknown problem with activation.
        /// </summary>
        public virtual string ActivationFailureGenericHtml
        {
            get
            {
                return _activationFailureGenericHtml;
            }
            set
            {
                _activationFailureGenericHtml = value;
            }
        }

        /// <summary>
        /// Error HTML displayed if the HTTP connection to get premium data can not be established.
        /// </summary>
        public virtual string ActivationFailureHttpHtml
        {
            get
            {
                return _activationFailureHttpHtml;
            }
            set
            {
                _activationFailureHttpHtml = value;
            }
        }

        /// <summary>
        /// Error HTML displayed if the licence key is invalid.
        /// </summary>
        public virtual string ActivationFailureInvalidHtml
        {
            get
            {
                return _activationFailureInvalidHtml;
            }
            set
            {
                _activationFailureInvalidHtml = value;
            }
        }

        /// <summary>
        /// Error HTML displayed if the returned stream is not valid during activation.
        /// </summary>
        public virtual string ActivationStreamFailureHtml
        {
            get
            {
                return _activationStreamFailureHtml;
            }
            set
            {
                _activationStreamFailureHtml = value;
            }
        }

        /// <summary>
        /// Error HTML displayed on successful activation.
        /// </summary>
        public virtual string ActivationSuccessHtml
        {
            get
            {
                return _activationSuccessHtml;
            }
            set
            {
                _activationSuccessHtml = value;
            }
        }

        /// <summary>
        /// Controls if instruction messages are displayed.
        /// </summary>
        public virtual bool InstructionsEnabled
        {
            get { return _instructionsEnabled; }
            set { _instructionsEnabled = value; }
        }

        #endregion

        #region Css

        /// <summary>
        /// Gets or sets the success css class for messages.
        /// </summary>
        public virtual string SuccessCssClass
        {
            get { return _successCssClass; }
            set { _successCssClass = value; }
        }

        /// <summary>
        /// Gets or sets the error css class for messages.
        /// </summary>
        public virtual string ErrorCssClass
        {
            get { return _errorCssClass; }
            set { _errorCssClass = value; }
        }

        /// <summary>
        /// Gets or sets the button css class for activation and refresh.
        /// </summary>
        public virtual string ButtonCssClass
        {
            get { return _buttonCssClass; }
            set { _buttonCssClass = value; }
        }

        #endregion

        #endregion

        #region Methods
                
        /// <summary>
        /// Activates the data stream.
        /// </summary>
        /// <param name="stream">Stream being uploaded.</param>
        /// <returns></returns>
        protected ActivityResult Execute(Stream stream)
        {
            return ProcessResult(FiftyOne.Foundation.Mobile.Detection.LicenceKey.Activate(stream));
        }
        
        /// <summary>
        /// Activates the licence key and updates the user interface 
        /// with the results.
        /// </summary>
        /// <param name="licenceKey">The licence key to try activating.</param>
        /// <returns></returns>
        protected ActivityResult Execute(string licenceKey)
        {
            return ProcessResult(FiftyOne.Foundation.Mobile.Detection.LicenceKey.Activate(licenceKey));
        }

        /// <summary>
        /// Creates the activity results object with the correct information
        /// for use by a UI.
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        private ActivityResult ProcessResult(LicenceKeyResults result)
        {
            switch (result)
            {
                case Mobile.Detection.LicenceKeyResults.Success:
                    return new ActivityResult(
                        String.Format(
                            ActivationSuccessHtml,
                            SuccessCssClass),
                            true);
                case Mobile.Detection.LicenceKeyResults.Config:
                    return new ActivityResult(String.Format(
                        ActivationFailureCouldNotUpdateConfigHtml,
                        ErrorCssClass,
                        String.Join(", ", FiftyOne.Foundation.Mobile.Constants.ConfigFileNames)));
                case Mobile.Detection.LicenceKeyResults.Https:
                    return new ActivityResult(String.Format(
                        ActivationFailureHttpHtml,
                        ErrorCssClass,
                        FiftyOne.Foundation.Mobile.Detection.LicenceKey.HostName));
                case Mobile.Detection.LicenceKeyResults.Invalid:
                    return new ActivityResult(String.Format(
                        ActivationFailureInvalidHtml,
                        ErrorCssClass));
                case Mobile.Detection.LicenceKeyResults.StreamFailure:
                    return new ActivityResult(String.Format(
                        ActivationStreamFailureHtml,
                        ErrorCssClass));
                case Mobile.Detection.LicenceKeyResults.DataInvalid:
                    return new ActivityResult(String.Format(
                        ActivationDataInvalidHtml,
                        ErrorCssClass));
                case Mobile.Detection.LicenceKeyResults.WriteLicenceFile:
                    return new ActivityResult(String.Format(
                        ActivationFailureCouldNotWriteLicenceFileHtml,
                        ErrorCssClass,
                        FiftyOne.Foundation.Mobile.Detection.LicenceKey.LicenceKeyFileName,
                        Path.Combine(HostingEnvironment.ApplicationPhysicalPath, "bin")));
                case Mobile.Detection.LicenceKeyResults.WriteDataFile:
                    return new ActivityResult(String.Format(
                        ErrorCssClass,
                        ActivationFailureCouldNotWriteDataFileHtml,
                        AutoUpdate.BinaryFile.FullName,
                        WindowsIdentity.GetCurrent().Name));
                case Mobile.Detection.LicenceKeyResults.GenericFailure:
                default:
                    return new ActivityResult(String.Format(
                        ActivationFailureGenericHtml,
                        ErrorCssClass));
            }
        }

        #endregion
    }
}
