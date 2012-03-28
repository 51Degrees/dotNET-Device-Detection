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
#if VER4
            using (XmlReader reader = XmlReader.Create(new StringReader(xml), GetXmlReaderSettings()))
#else 
            xml = Regex.Replace(xml, "<!DOCTYPE.+>", "");
            using (XmlReader reader = XmlReader.Create(new StringReader(xml)))
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
                    reader.IsStartElement(Constants.HandlerElementName))
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
                        case Constants.CanHandleElementName:
                            handler.CanHandleRegex.AddRange(ProcessRegex(reader.ReadSubtree()));
                            break;
                        case Constants.CantHandleElementName:
                            handler.CantHandleRegex.AddRange(ProcessRegex(reader.ReadSubtree()));
                            break;
                        case Constants.RegexSegmentsElementName:
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
                    string pattern = reader.GetAttribute(Constants.PatternAttributeName);
                    int weight = 0;
                    if (String.IsNullOrEmpty(pattern) == false &&
                        int.TryParse(reader.GetAttribute(Constants.WeightAttributeName), out weight))
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
                if (reader.Depth > 0 && reader.IsStartElement(Constants.RegexPrefix))
                {
                    HandleRegex regex = new HandleRegex(reader.GetAttribute(Constants.PatternAttributeName));
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
            string name = reader.GetAttribute(Constants.NameAttributeName);
            string type = reader.GetAttribute(Constants.TypeAttributeName);
            bool.TryParse(reader.GetAttribute(Constants.CheckUserAgentProfileAttibuteName), out checkUAProf);
            byte.TryParse(reader.GetAttribute(Constants.ConfidenceAttributeName), out confidence);

            switch (type)
            {
                case "editDistance":
                    return new Handlers.EditDistanceHandler(
                        provider, name, String.Empty, confidence, checkUAProf);
                case "reducedInitialString":
                    return new Handlers.ReducedInitialStringHandler(
                        provider, name, String.Empty, confidence,
                        checkUAProf, reader.GetAttribute(Constants.ToleranceAttributeName));
                case "regexSegment":
                    return new Handlers.RegexSegmentHandler(
                        provider, name, String.Empty, confidence, checkUAProf);
            }

            throw new XmlException(String.Format("Type '{0}' is invalid.", type));
        }
    }
}
