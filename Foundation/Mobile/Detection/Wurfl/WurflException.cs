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
 * Mobile Experts Limited are Copyright (C) 2009 - 2010. All Rights Reserved.
 * 
 * Contributor(s):
 *     James Rosewell <james@51degrees.mobi>
 * 
 * ********************************************************************* */

#region Usings

using System;
using System.Runtime.Serialization;

#endregion

namespace FiftyOne.Foundation.Mobile.Detection.Wurfl
{
    /// <summary>
    /// Represents Wurfl exceptions.
    /// </summary>
    [Serializable]
    public class WurflException : Exception
    {
        /// <summary>
        /// Initializes a new instance of <typeparamref name="WurflException"/>
        /// </summary>
        public WurflException()
        {
        }

        /// <summary>
        /// Initializes a new instance of <typeparamref name="WurflException"/>
        /// </summary>
        /// <param name="message">The human readable message explaining the exception.</param>
        public WurflException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of <typeparamref name="WurflException"/>
        /// </summary>
        /// <param name="message">The human readable message explaining the exception.</param>
        /// <param name="innerException">The exception that caused the new one.</param>
        public WurflException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of <typeparamref name="WurflException"/>
        /// </summary>
        protected WurflException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}