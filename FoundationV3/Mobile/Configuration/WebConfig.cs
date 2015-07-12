using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web.Configuration;
using System.Xml;

namespace FiftyOne.Foundation.Mobile.Configuration
{
    internal static class WebConfig
    {
        /// <summary>
        /// Makes sure the necessary HTTP module remove element is present in the web.config.
        /// </summary>
        /// <param name="xml">Xml fragment for the system.webServer web.config section</param>
        /// <returns>True if a change was made, otherwise false.</returns>
        private static bool FixRemoveModule(XmlDocument xml)
        {
            var changed = false;
            var module = xml.SelectSingleNode("//modules/remove[@name='Detector']") as XmlElement;
            if (module == null)
            {
                var modules = xml.SelectSingleNode("//modules");
                var remove = xml.CreateElement("remove");
                remove.Attributes.Append(xml.CreateAttribute("name"));
                remove.Attributes["name"].Value = "Detector";
                modules.InsertBefore(remove, modules.FirstChild);
                changed = true;
            }
            return changed;
        }

        /// <summary>
        /// Makes sure the necessary HTTP module is present in the web.config file to support
        /// device detection and image optimisation.
        /// </summary>
        /// <param name="xml">Xml fragment for the system.webServer web.config section</param>
        /// <returns>True if a change was made, otherwise false.</returns>
        private static bool FixAddModule(XmlDocument xml)
        {
            var changed = false;
            var module = xml.SelectSingleNode("//modules/add[@type='FiftyOne.Foundation.Mobile.Detection.DetectorModule, FiftyOne.Foundation']") as XmlElement;
            if (module != null)
            {
                // If image optimisation is enabled and the preCondition attribute
                // is present then it'll need to be removed.
                if (module.Attributes["preCondition"] != null)
                {
                    module.Attributes.RemoveNamedItem("preCondition");
                    changed = true;
                }
                // Make sure the module entry is named "Detector".
                if ("Detector".Equals(module.GetAttribute("name")) == false)
                {
                    module.Attributes["name"].Value = "Detector";
                    changed = true;
                }
            }
            else
            {
                // The module entry is missing so add a new one.
                var modules = xml.SelectSingleNode("//modules");
                module = xml.CreateElement("add");
                module.Attributes.Append(xml.CreateAttribute("name"));
                module.Attributes["name"].Value = "Detector";
                module.Attributes.Append(xml.CreateAttribute("type"));
                module.Attributes["type"].Value = "FiftyOne.Foundation.Mobile.Detection.DetectorModule, FiftyOne.Foundation";
                modules.InsertAfter(module, modules.LastChild);
                changed = true;
            }
            return changed;
        }

        /// <summary>
        /// Makes sure a HTTP modules section exists in the xml.
        /// </summary>
        /// <param name="xml">Xml fragment for the system.webServer web.config section</param>
        /// <returns>True if a change was made, otherwise false.</returns>
        private static bool FixAddModules(XmlDocument xml)
        {
            var changed = false;
            var modules = xml.SelectSingleNode("//modules") as XmlElement;
            if (modules == null)
            {
                modules = xml.CreateElement("modules");
                xml.FirstChild.InsertBefore(modules, xml.FirstChild.FirstChild);
                changed = true;
            }
            return changed;
        }

        /// <summary>
        /// Ensures that the web.config file has the HTTP module for device
        /// detection and image optimisation configured correctly.
        /// </summary>
        internal static bool SetWebConfigurationModules()
        {
            var config = WebConfigurationManager.OpenWebConfiguration("~");
            if (config != null)
            {
                var section = config.GetSection("system.webServer") as ConfigurationSection;
                if (section != null)
                {
                    var changed = false;
                    var xml = new XmlDocument();
                    xml.LoadXml(section.SectionInformation.GetRawXml());
                    changed |= FixAddModules(xml);
                    changed |= FixRemoveModule(xml);
                    changed |= FixAddModule(xml);
                    if (changed)
                    {
                        section.SectionInformation.SetRawXml(xml.InnerXml);
                        config.Save();
                    }
                }
            }
            return true;
        }

    }
}
