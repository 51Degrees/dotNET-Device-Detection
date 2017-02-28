/* *********************************************************************
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited. 
 * Copyright © 2015 51Degrees Mobile Experts Limited, 5 Charlotte Close,
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

namespace FiftyOne.Foundation.Mobile.Detection.Entities.Headers
{
    /// <summary>
    /// Every list contains a standard initial header. This class provides
    /// the basic properties needed to access lists irrespective of the 
    /// storage implementation.
    /// </summary>
    public class Header
    {
        #region Fields

        /// <summary>
        /// The number of items contain in the collection.
        /// </summary>
        internal readonly int Count;

        /// <summary>
        /// The position in the file where the data structure starts.
        /// </summary>
        internal readonly int StartPosition;

        /// <summary>
        /// The number of bytes consumed by the data structure.
        /// </summary>
        internal readonly int Length;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a new instance of <see cref="Header"/>
        /// </summary>
        /// <param name="reader">
        /// Reader connected to the source data structure and positioned to start reading
        /// </param>
        public Header(BinaryReader reader)
        {
            StartPosition = reader.ReadInt32();
            Length = reader.ReadInt32();
            Count = reader.ReadInt32();
        }

        #endregion
    }
}
