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
        /// The top level element name used to enclose all other elements.
        /// </summary>
        public const string TopLevelElementName = "fiftyOneDegrees";

        /// <summary>
        /// The element name of the properties section.
        /// </summary>
        public const string PropertiesElementName = "properties";

        /// <summary>
        /// The element name of the header section.
        /// </summary>
        public const string HeaderElementName = "header";

        /// <summary>
        /// Element name used to especify a property inside the xml file.
        /// </summary>
        public const string PropertyElementName = "property";

        /// <summary>
        /// Element name used to specify a value inside the xml file.
        /// </summary>
        public const string ValueElementName = "value";

        /// <summary>
        /// Attribute name used to indicate if the property supports multiple values.
        /// </summary>
        public const string ListAttributeName = "list";

        /// <summary>
        /// Attribute name used to indicate if the property is mandatory.
        /// </summary>
        public const string MandatoryAttributeName = "mandatory";

        /// <summary>
        /// Attribute name used to indicate if the values are suitable to be shown to customers.
        /// </summary>
        public const string ShowValuesAttributeName = "showValues";

        /// <summary>
        /// Attribute name used to indicate the description.
        /// </summary>
        public const string DescriptionAttributeName = "description";

        /// <summary>
        /// Attribute name used to indicate the url.
        /// </summary>
        public const string UrlAttributeName = "url";

        /// <summary>
        /// Element name used to specify all profiles inside the xml file.
        /// </summary>
        public const string ProfilesElementName = "profiles";

        /// <summary>
        /// Element names used to specify all handlers inside the xml file.
        /// </summary>
        public const string HandlersElementName = "handlers";

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
        /// The type of handler.
        /// </summary>
        public const string TypeAttributeName = "type";
        
        /// <summary>
        /// The relative confidence the handler should be given
        /// compare to others.
        /// </summary>
        public const string ConfidenceAttributeName = "confidence";

        /// <summary>
        /// Attribute name used to determine if user agent profile should
        /// also be checked when matching HTTP headers.
        /// </summary>
        public const string CheckUserAgentProfileAttibuteName = "checkUAProf";

        /// <summary>
        /// Element name for a list of patterns which are supported by
        /// the handler.
        /// </summary>
        public const string CanHandleElementName = "canHandle";

        /// <summary>
        /// Element name for a list of patterns which indicate the handler
        /// can't be used.
        /// </summary>
        public const string CantHandleElementName = "cantHandle";

        /// <summary>
        /// The tolerance to be used for the initial string match.
        /// </summary>
        public const string ToleranceAttributeName = "tolerance";

        /// <summary>
        /// A collection os segment patterns and weights.
        /// </summary>
        public const string RegexSegmentsElementName = "regexSegments";
        
        /// <summary>
        /// A specific segment to be used with a segmented handler.
        /// </summary>
        public const string RegexSegmentElementName = "regexSegment";

        /// <summary>
        /// Initial string used to indicate a regex element.
        /// </summary>
        public const string RegexPrefix = "regex";

        /// <summary>
        /// Attribute name used to provide a regular expression.
        /// </summary>
        public const string PatternAttributeName = "pattern";

        /// <summary>
        /// Attribute name used to provide the weight of an edit distance result.
        /// </summary>
        public const string WeightAttributeName = "weight";

        /// <summary>
        /// Attribute name used to specify a user agent inside the xml file.
        /// </summary>
        public const string UserAgentAttributeName = "UA";

        /// <summary>
        /// Attribute name used to specify a value inside the xml file.
        /// </summary>
        public const string ValueAttributeName = "value";

        /// <summary>
        /// Attribute name used to provide the date the XML file was published.
        /// </summary>
        public const string PublishedDateAttributeName = "date";

        /// <summary>
        /// The name of the element used to store the version of the data.
        /// </summary>
        public const string VersionElementName = "version";

        /// <summary>
        /// The name of the element used to store the copyright or licence.
        /// </summary>
        public const string CopyrightElementName = "copyright";

        /// <summary>
        /// The name of the element used to store each handler.
        /// </summary>
        public const string HandlerElementName = "handler";
    }
}