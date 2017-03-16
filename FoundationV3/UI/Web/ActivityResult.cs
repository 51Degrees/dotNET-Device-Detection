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

namespace FiftyOne.Foundation.UI.Web
{
    /// <summary>
    /// Used to communicate to the users of the control the
    /// result of an upload or activate process.
    /// </summary>
    public class ActivityResult
    {
        #region Fields

        private readonly string _html = null;
        private readonly bool _success = false;

        #endregion

        #region Properties

        /// <summary>
        /// Html to be displayed as the result of the upload.
        /// </summary>
        public string Html
        {
            get { return _html; }
        }

        /// <summary>
        /// True if the upload was successfull, otherwise false.
        /// </summary>
        public bool Success
        {
            get { return _success; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new instance of ActivityResult.
        /// </summary>
        /// <param name="html">Html to be displayed as the result of the upload.</param>
        /// <param name="success">True is the upload was successfull, otherwise false.</param>
        internal ActivityResult(string html, bool success)
        {
            _html = html;
            _success = success;
        }

        /// <summary>
        /// Creates a new instance of ActivityResult.
        /// </summary>
        /// <param name="html">Html to be displayed as the result of the upload.</param>
        internal ActivityResult(string html)
        {
            _html = html;
            _success = false;
        }

        #endregion
    }
}
