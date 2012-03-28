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

        /// <summary>
        /// Initializes a new instance of <see cref="MobileException"/>.
        /// </summary>
        protected internal MobileException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}