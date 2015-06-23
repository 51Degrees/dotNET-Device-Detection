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

using System.IO;

namespace FiftyOne.Foundation.Mobile.Detection.Entities.Stream
{
    /// <summary>
    /// Base class for file sources.
    /// </summary>
    internal abstract class SourceFileBase : SourceBase
    {
        #region Fields

        /// <summary>
        /// The file containing the source data.
        /// </summary>
        protected readonly FileInfo _fileInfo;

        #endregion

        #region Constructor

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName">File source of the data</param>
        internal SourceFileBase(string fileName)
        {
            _fileInfo = new FileInfo(fileName);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Delete the file if it's a temporary file and it 
        /// still exists.
        /// </summary>
        protected void DeleteFile()
        {
            if (".tmp".Equals(_fileInfo.Extension) &&
                _fileInfo.Exists)
            {
                try
                {
                    _fileInfo.Delete();
                }
                catch (IOException)
                {
                    // Do nothing as the cause is likely to be because the
                    // file is in use by another process.
                }
            }
        }

        #endregion
    }
}
