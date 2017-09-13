/* *********************************************************************
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited. 
 * Copyright © 2017 51Degrees Mobile Experts Limited, 5 Charlotte Close,
 * Caversham, Reading, Berkshire, United Kingdom RG4 7BY
 * 
 * This Source Code Form is the subject of the following patents and patent 
 * applications, owned by 51Degrees Mobile Experts Limited of 5 Charlotte
 * Close, Caversham, Reading, Berkshire, United Kingdom RG4 7BY: 
 * European Patent No. 2871816;
 * European Patent Application No. 17184134.9;
 * United States Patent Nos. 9,332,086 and 9,350,823; and
 * United States Patent Application No. 15/686,066.
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
    /// Maps a profile id to its position in the data file.
    /// </summary>
    public class ProfileOffset : DeviceDetectionBaseEntity
    {        
        #region Properties

        /// <summary>
        /// The unique id for the profile.
        /// </summary>
        public int ProfileId
        {
            get { return _profileId; }
        }
        internal int _profileId;

        /// <summary>
        /// The position within the data file that the profile can be read from.
        /// </summary>
        public int Offset
        {
            get { return _offset; }
        }
        internal int _offset;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a new instance of <see cref="ProfileOffset"/>.
        /// </summary>
        /// <param name="dataSet">
        /// The data set whose strings list the string is contained within.
        /// </param>
        /// <param name="offset">
        /// The offset to the start of the profile within the profiles 
        /// data structure.
        /// </param>
        /// <param name="reader">
        /// Binary reader positioned at the start of the ProfileOffset.
        /// </param>
        internal ProfileOffset(DataSet dataSet, int offset, BinaryReader reader)
            : base(dataSet, offset)
        {
            _profileId = reader.ReadInt32();
            _offset = reader.ReadInt32();
        }
        
        #endregion
    }
}
