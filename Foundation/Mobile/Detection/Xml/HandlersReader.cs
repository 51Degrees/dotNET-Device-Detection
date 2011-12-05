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

using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using FiftyOne.Foundation.Mobile.Detection.Handlers;
using System;
using System.Collections.Generic;

namespace FiftyOne.Foundation.Mobile.Detection.Xml
{
    /// <summary>
    /// Used to read the handlers information from the xml source.
    /// </summary>
    public static class HandlersReader
    {
        /// <summary>
        /// Passed an xml string and returns a list of handlers.
        /// </summary>
        public static IList<Handler> ProcessHandlers(string xml, BaseProvider provider)
        {
            // Use different code to handle the DTD in the Xml if present.
#if VER2
            xml = Regex.Replace(xml, "<!DOCTYPE.+>", "");
            using (XmlReader reader = XmlReader.Create(new StringReader(xml)))
#endif
#if VER4
            using (XmlReader reader = XmlReader.Create(new StringReader(xml), GetXmlReaderSettings()))
#endif
            {
                return ProcessHandlers(reader, provider);
            }
        }

        /// <summary>
        /// Passed an open XML reader and returns a list of handlers.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static IList<Handler> ProcessHandlers(XmlReader reader, BaseProvider provider)
        {
            var handlers = new List<Handler>();
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element &&
                    reader.IsStartElement("handler"))
                {
                    ProcessHandler(
                        handlers,
                        CreateHandler(reader, provider),
                        reader.ReadSubtree());
                }
            }
            return handlers;
        }

#if VER4
        /// <summary>
        /// Returns an XML reader settings object that will ignore DTD.
        /// </summary>
        /// <returns></returns>
        private static XmlReaderSettings GetXmlReaderSettings()
        {
            var settings = new XmlReaderSettings();
            settings.DtdProcessing = DtdProcessing.Ignore;
            return settings;
        }
#endif

        /// <summary>
        /// Processes the current handler and adds it to the list of handlers.
        /// </summary>
        /// <param name="handlers">The list of all handers to be added to.</param>
        /// <param name="handler">The current handler object.</param>
        /// <param name="reader">The XML stream reader.</param>
        private static void ProcessHandler(List<Handler> handlers, Handler handler, XmlReader reader)
        {
            while (reader.Read())
            {
                if (reader.Depth > 0 && reader.IsStartElement())
                {
                    switch (reader.Name)
                    {
                        case "canHandle":
                            handler.CanHandleRegex.AddRange(ProcessRegex(reader.ReadSubtree()));
                            break;
                        case "cantHandle":
                            handler.CantHandleRegex.AddRange(ProcessRegex(reader.ReadSubtree()));
                            break;
                        case "regexSegments":
                            if (handler is RegexSegmentHandler)
                                ProcessRegexSegments((RegexSegmentHandler)handler, reader.ReadSubtree());
                            break;
                    }
                }
            }
            handlers.Add(handler);
        }
        
        /// <summary>
        /// Adds the segments to the regular expression handler.
        /// </summary>
        /// <param name="handler">Regular expression handler.</param>
        /// <param name="reader">The XML stream reader.</param>
        private static void ProcessRegexSegments(RegexSegmentHandler handler, XmlReader reader)
        {
            while (reader.Read())
            {
                if (reader.Depth > 0)
                {
                    string pattern = reader.GetAttribute("pattern");
                    int weight = 0;
                    if (String.IsNullOrEmpty(pattern) == false &&
                        int.TryParse(reader.GetAttribute("weight"), out weight))
                        handler.AddSegment(pattern, weight);
                }
            }
        }

        /// <summary>
        /// Returns a list of regexs used to evaluate the handler to see if it can
        /// be used to handle the requested useragent.
        /// </summary>
        /// <param name="reader">The XML stream reader.</param>
        /// <returns>A list of regexes.</returns>
        private static List<HandleRegex> ProcessRegex(XmlReader reader)
        {
            List<HandleRegex> regexs = new List<HandleRegex>();
            while (reader.Read())
            {
                if (reader.Depth > 0 && reader.IsStartElement("regex"))
                {
                    HandleRegex regex = new HandleRegex(reader.GetAttribute("pattern"));
                    regex.Children.AddRange(ProcessRegex(reader.ReadSubtree()));
                    regexs.Add(regex);
                }
            }
            return regexs;
        }

        /// <summary>
        /// Creates a new handler based on the attributes of the current element.
        /// </summary>
        /// <param name="reader">The XML stream reader.</param>
        /// <param name="provider">The provider the handler will be associated with.</param>
        /// <returns>A new handler object.</returns>
        private static Handler CreateHandler(XmlReader reader, BaseProvider provider)
        {
            bool checkUAProf;
            byte confidence;
            string name = reader.GetAttribute("name");
            string defaultDeviceId = reader.GetAttribute("defaultDevice");
            string type = reader.GetAttribute("type");
            bool.TryParse(reader.GetAttribute("checkUAProf"), out checkUAProf);
            byte.TryParse(reader.GetAttribute("confidence"), out confidence);

            switch (type)
            {
                case "editDistance":
                    return new Handlers.EditDistanceHandler(
                        provider, name, defaultDeviceId, confidence, checkUAProf);
                case "reducedInitialString":
                    return new Handlers.ReducedInitialStringHandler(
                        provider, name, defaultDeviceId, confidence,
                        checkUAProf, reader.GetAttribute("tolerance"));
                case "regexSegment":
                    return new Handlers.RegexSegmentHandler(
                        provider, name, defaultDeviceId, confidence, checkUAProf);
            }

            throw new XmlException(String.Format("Type '{0}' is invalid.", type));
        }
    }
}
