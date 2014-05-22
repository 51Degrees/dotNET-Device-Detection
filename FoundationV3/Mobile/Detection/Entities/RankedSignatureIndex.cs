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

namespace FiftyOne.Foundation.Mobile.Detection.Entities
{
    /// <summary>
    /// Maps a ranked signature index to the signature index.
    /// </summary>
    public class RankedSignatureIndex : BaseEntity
    {        
        #region Properties

        /// <summary>
        /// The index of the signature in the list of signatures.
        /// </summary>
        public int SignatureIndex
        {
            get { return _signatureIndex; }
        }
        internal int _signatureIndex;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a new instance of <see cref="RankedSignatureIndex"/>
        /// </summary>
        /// <param name="dataSet">
        /// The data set whose strings list the string is contained within
        /// </param>
        /// <param name="index">
        /// The index in the data structure to the ranked signature index.
        /// </param>
        /// <param name="reader">
        /// Binary reader positioned at the start of the AsciiString.
        /// </param>
        internal RankedSignatureIndex(DataSet dataSet, int index, BinaryReader reader)
            : base(dataSet, index)
        {
            _signatureIndex = reader.ReadInt32();
        }
        
        #endregion
    }
}
