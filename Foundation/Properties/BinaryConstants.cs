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

namespace FiftyOne.Foundation.Mobile.Detection.Binary
{
    /// <summary>
    /// Constants used by the binary file format.
    /// </summary>
    public class BinaryConstants
    {
        /// <summary>
        /// The format version of the binary data contained in the file header. This much match with the data
        /// file for the file to be read.
        /// </summary>
        public static readonly Version FormatVersion = new Version(1, 0);

        /// <summary>
        /// The name of the embedded resource containing the Lite device data compiled into the assembly.
        /// </summary>
        public const string EmbeddedDataResourceName = "FiftyOne.Foundation.Mobile.Detection.Binary.Resources.51Degrees.mobi-Lite.dat";
    }
}
