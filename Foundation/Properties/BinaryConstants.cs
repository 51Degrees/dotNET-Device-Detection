/* *********************************************************************
 * The contents of this file are subject to the Mozilla Public License 
 * Version 1.1 (the "License"); you may not use this file except in 
 * compliance with the License. You may obtain a copy of the License at 
 * http://www.mozilla.org/MPL/
 * 
 * Software distributed under the License is distributed on an "AS IS" 
 * basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 * See the License for the specific language governing rights and 
 * limitations under the License.
 *
 * The Original Code is named .NET Mobile API, first released under 
 * this licence on 11th March 2009.
 * 
 * The Initial Developer of the Original Code is owned by 
 * 51 Degrees Mobile Experts Limited. Portions created by 51 Degrees
 * Mobile Experts Limited are Copyright (C) 2009 - 2012. All Rights Reserved.
 * 
 * Contributor(s):
 *     James Rosewell <james@51degrees.mobi>
 * 
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
        /// The name of the embedded resource containing the device data.
        /// </summary>
        internal const string EmbeddedDataResourceName = "FiftyOne.Foundation.Mobile.Detection.Binary.Resources.51Degrees.mobi-Lite.dat";
    }
}
