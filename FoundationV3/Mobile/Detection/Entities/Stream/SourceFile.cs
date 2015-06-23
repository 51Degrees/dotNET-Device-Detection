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

namespace FiftyOne.Foundation.Mobile.Detection.Entities.Stream
{
    /// <summary>
    /// Encapsulates either a file containing the uncompressed
    /// data structures used by the data set.
    /// </summary>
    internal class SourceFile : SourceFileBase
    {
        #region Constructors

        /// <summary>
        /// Creates the source from the file provided.
        /// </summary>
        /// <param name="fileName">File source of the data</param>
        internal SourceFile(string fileName)
            : base(fileName)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a new stream from the data source.
        /// </summary>
        /// <returns>A freshly opened stream to the data source</returns>
        internal override System.IO.Stream CreateStream()
        {
            return _fileInfo.OpenRead();
        }

        /// <summary>
        /// Closes any file references and then checks
        /// to delete the file.
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            DeleteFile();
        }

        #endregion
    }
}
