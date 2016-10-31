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

using System.Text;
using System.IO;

namespace FiftyOne.Foundation.Mobile.Detection.Entities
{
    /// <summary>
    /// ASCII format strings are the only ones used in the data set. Many
    /// native string formats use Unicode format using 2 bytes for every 
    /// character. This is inefficient when only ASCII values are being 
    /// stored. The <see cref="AsciiString{T}"/> class wraps a byte array of 
    /// ASCII characters and exposes them as a native string type when 
    /// required.
    /// </summary>
    /// <remarks>
    /// Strings stored as ASCII strings include, the relevant characters 
    /// from signatures, sub strings longer than 4 characters, property
    /// and value names, the descriptions and URLs associated with 
    /// properties and values.
    /// </remarks>
    /// <para>
    /// For more information see https://51degrees.com/Support/Documentation/Net
    /// </para>
    /// <typeparam name="T">
    /// The type of the shared data set the AsciiString relates to.
    /// </typeparam>
    internal class AsciiString<T> : BaseEntity<T>
    {
        #region Fields

        /// <summary>
        /// The value of the string in ASCII bytes.
        /// </summary>
        public readonly byte[] Value;

        #endregion
        
        #region Constructor

        /// <summary>
        /// Constructs a new instance of <see cref="AsciiString{T}"/>.
        /// </summary>
        /// <param name="dataSet">
        /// The data set whose strings list the string is contained within.
        /// </param>
        /// <param name="offset">
        /// The offset to the start of the string within the string data 
        /// structure.
        /// </param>
        /// <param name="reader">
        /// Binary reader positioned at the start of the AsciiString.
        /// </param>
        internal AsciiString(T dataSet, int offset, BinaryReader reader)
            : base(dataSet, offset)
        {
            // Read the length of the array minus 1 to remove the 
            // last null character which isn't used by .NET.
            Value = reader.ReadBytes(reader.ReadInt16() - 1);

            // Read and discard the null value to ensure the file
            // position is correct for the next read.
            reader.ReadByte();
        }

        #endregion

        #region Methods

        /// <summary>
        /// .NET string representation of the ASCII string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (_stringValue == null)
            {
                lock (this)
                {
                    if (_stringValue == null)
                    {
                        _stringValue = Encoding.ASCII.GetString(Value);
                    }
                }
            }
            return _stringValue;
        }
        private string _stringValue = null;

        #endregion
    }
}
