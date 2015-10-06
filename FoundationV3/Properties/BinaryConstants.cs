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

using System;
using System.Collections.Generic;

namespace FiftyOne.Foundation.Mobile.Detection
{
    /// <summary>
    /// Constants used by the binary file format.
    /// </summary>
    public class BinaryConstants
    {
        /// <summary>
        /// Different format versions in an enumeration for quick reference.
        /// </summary>
        public enum FormatVersions
        {
            /// <summary>
            /// First released in May 2014 for version 3 device deteciton.
            /// </summary>
            PatternV31,
            /// <summary>
            /// Contains the same data as V3.1 but organises the information
            /// more efficiently to reduce data file size and improve performance.
            /// </summary>
            PatternV32,
            /// <summary>
            /// The binary data file format used with Trie version 3 device 
            /// detection.
            /// </summary>
            TrieV30,
            /// <summary>
            /// The binary data file format used with Trie version 3.2 device 
            /// detection.
            /// </summary>
            TrieV32
        }

        /// <summary>
        /// An array of pattern format versions that this API will support.
        /// </summary>
        public static readonly KeyValuePair<FormatVersions, Version>[] SupportedPatternFormatVersions = new KeyValuePair<FormatVersions, Version>[] {
            new KeyValuePair<FormatVersions, Version>(FormatVersions.PatternV31, new Version(3, 1, 0, 0)),
            new KeyValuePair<FormatVersions, Version>(FormatVersions.PatternV32, new Version(3, 2, 0, 0))
        };

        /// <summary>
        /// An array of trie format versions that this API will support.
        /// </summary>
        public static readonly KeyValuePair<FormatVersions, Version>[] SupportedTrieFormatVersions = new KeyValuePair<FormatVersions, Version>[] {
            new KeyValuePair<FormatVersions, Version>(FormatVersions.TrieV30, new Version(3, 0, 0, 0)),
            new KeyValuePair<FormatVersions, Version>(FormatVersions.TrieV32, new Version(3, 2, 0, 0))
        };

        /// <summary>
        /// The format version of the binary data contained in the file header. 
        /// This much match with the data file for the file to be read.
        /// </summary>
        [Obsolete("As multiple versions can now be supported us SupportedFormatVersions array instead.")]
        public static readonly Version FormatVersion = new Version(3, 1, 0, 0);
    }
}
