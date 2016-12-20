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

#region Usings

using System;
using System.Runtime.Serialization;

#endregion

namespace FiftyOne.Foundation.Mobile
{
    /// <summary>
    /// <para>
    /// The generic Exception class for all exceptions generated from the Mobile
    /// Toolkit.
    /// </para>
    /// </summary>
    [Serializable]
    public class MobileException : Exception
    {
        /// <summary>
        /// Initializes a new instance of <see cref="MobileException"/>.
        /// </summary>
        internal MobileException()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="MobileException"/>.
        /// </summary>
        /// <param name="message">The human readable message explaining the exception.</param>
        public MobileException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="MobileException"/>.
        /// </summary>
        /// <param name="message">The human readable message explaining the exception.</param>
        /// <param name="innerException">The exception that caused the new one.</param>
        public MobileException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        #if NET40
        /// <summary>
        /// Initializes a new instance of <see cref="MobileException"/>.
        /// </summary>
        protected internal MobileException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
        #endif
    }
}