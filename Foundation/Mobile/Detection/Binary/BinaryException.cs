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

namespace FiftyOne.Foundation.Mobile.Detection.Binary
{
    /// <summary>
    /// Thrown by the binary format detection classes.
    /// </summary>
    public class BinaryException : MobileException
    {
        /// <summary>
        /// Constructs a new instance of <see cref="BinaryException"/>.
        /// </summary>
        /// <param name="message"></param>
        internal BinaryException(string message) : base(message) { }
    }
}
