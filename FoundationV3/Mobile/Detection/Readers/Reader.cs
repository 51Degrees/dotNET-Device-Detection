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

using System.Collections.Generic;
using System.IO;

namespace FiftyOne.Foundation.Mobile.Detection.Readers
{
    /// <summary>
    /// Used to provide extra features to the standard binary reader
    /// to reduce the number of objects created for garbage collection 
    /// to handle.
    /// </summary>
    /// <remarks>Not intended to be used directly by 3rd parties.</remarks>
    public class Reader : System.IO.BinaryReader
    {
        /// <summary>
        /// A list of integers used to create arrays when the number of elements
        /// are unknown prior to commencing reading.
        /// </summary>
        internal readonly List<int> List = new List<int>();

        /// <summary>
        /// Constructs a new instance of reader from the stream.
        /// </summary>
        /// <param name="stream"></param>
        public Reader(Stream stream) : base(stream) { }
    }
}
