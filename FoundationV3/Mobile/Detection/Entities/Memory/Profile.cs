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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FiftyOne.Foundation.Mobile.Detection.Entities.Memory
{
    /// <summary>
    /// All data is loaded into memory when the entity is constructed.
    /// </summary>
    public class Profile : Entities.Profile
    {
        #region Constructors

        /// <summary>
        /// Constructs a new instance of the <see cref="Profile"/>
        /// </summary>
        /// <param name="dataSet">
        /// The data set whose profile list the profile will be contained within
        /// </param>
        /// <param name="offset">
        /// The offset position in the data structure to the profile</param>
        /// <param name="reader">
        /// Reader connected to the source data structure and positioned to start reading
        /// </param>
        internal Profile(
            DataSet dataSet,
            int offset,
            BinaryReader reader)
            : base(dataSet, offset, reader)
        {
            var valueIndexesCount = reader.ReadInt32();
            var signatureIndexesCount = reader.ReadInt32();
            _valueIndexes = BaseEntity.ReadIntegerArray(reader, valueIndexesCount);
            _signatureIndexes = BaseEntity.ReadIntegerArray(reader, signatureIndexesCount);
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Array of value indexes associated with the profile.
        /// </summary>
        protected internal override int[] ValueIndexes
        {
            get { return _valueIndexes; }
        }
        private readonly int[] _valueIndexes;

        /// <summary>
        /// Array of signature indexes associated with the profile.
        /// </summary>
        protected internal override int[] SignatureIndexes
        {
            get { return _signatureIndexes; }
        }
        private readonly int[] _signatureIndexes;

        #endregion
    }
}
