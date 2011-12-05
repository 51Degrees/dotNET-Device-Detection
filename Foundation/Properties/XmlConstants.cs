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
 *     Andy Allan <andy.allan@mobgets.com>
 * 
 * ********************************************************************* */

namespace FiftyOne.Foundation.Mobile.Detection.Xml
{
    /// <summary>
    /// Holds all constants and read only strings used throughout this library.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Element name used to especify a property inside the xml file.
        /// </summary>
        public const string PropertyElementName = "property";

        /// <summary>
        /// Element name used to specify all profiles inside the xml file.
        /// </summary>
        public const string ProfilesElementName = "profiles";

        /// <summary>
        /// Element name used to specify a profile inside the xml file.
        /// </summary>
        public const string ProfileElementName = "profile";

        /// <summary>
        /// Attribute name used to specify a fall back inside the xml file.
        /// </summary>
        public const string ParentAttributeName = "parent";

        /// <summary>
        /// Attribute name used to especify an id inside the xml file.
        /// </summary>
        public const string IdAttributeName = "id";

        /// <summary>
        /// Attribute name used to especify a name inside the xml file.
        /// </summary>
        public const string NameAttributeName = "name";

        /// <summary>
        /// Attribute name used to specify a user agent inside the xml file.
        /// </summary>
        public const string UserAgentAttributeName = "UA";

        /// <summary>
        /// Attribute name used to specify a value inside the xml file.
        /// </summary>
        public const string ValueAttributeName = "value";
    }
}