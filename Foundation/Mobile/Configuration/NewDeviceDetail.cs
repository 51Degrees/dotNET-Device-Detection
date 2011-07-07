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
 * Mobile Experts Limited are Copyright (C) 2009 - 2011. All Rights Reserved.
 * 
 * Contributor(s):
 *     James Rosewell <james@51degrees.mobi>
 * 
 * ********************************************************************* */

namespace FiftyOne.Foundation.Mobile.Configuration
{
    /// <summary>
    /// Enumeration containing the different levels of request
    /// details that can be sent to 51Degrees.mobi. Cookie headers
    /// are never sent to 51Degrees.mobi. 
    /// </summary>
    public enum NewDeviceDetail
    {
        /// <summary>
        /// Sends only UserAgent and UAProf header fields.
        /// </summary>
        Minimum = 0,
        /// <summary>
        /// Sends all headers except cookies.
        /// </summary>
        Maximum = 1
    }
}