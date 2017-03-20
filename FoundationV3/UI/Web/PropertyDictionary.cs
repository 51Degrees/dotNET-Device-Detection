/* *********************************************************************
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited. 
 * Copyright © 2017 51Degrees Mobile Experts Limited, 5 Charlotte Close,
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

using System;
using System.Linq;
using System.Web.UI.WebControls;
using FiftyOne.Foundation.Mobile.Detection.Entities;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;

namespace FiftyOne.Foundation.UI.Web
{
    /// <summary>
    /// Displays a list of the available properties and values.
    /// </summary>
    public class PropertyDictionary : BaseUserControl
    {
        #region Fields

        /// <summary>
        /// The HTML for the legend at the top of the page.
        /// </summary>
        private readonly Literal _legend = new Literal();
        
        /// <summary>
        /// The HTML for the instructions on using the page.
        /// </summary>
        private readonly Literal _instructions = new Literal();

        /// <summary>
        /// CSS class used for the category.
        /// </summary>
        private string _categoryCssClass = "category";

        /// <summary>
        /// CSS class used for the type of property.
        /// </summary>
        private string _typeCssClass = "type";

        #endregion

        #region Properties

        /// <summary>
        /// The CssClass used to display the type of the property.
        /// </summary>
        public string TypeCssClass
        {
            get { return _typeCssClass; }
            set { _typeCssClass = value; }
        }

        /// <summary>
        /// The CssClass used to display the category properties are
        /// under when displaying details of a device.
        /// </summary>
        public string CategoryCssClass
        {
            get { return _categoryCssClass; }
            set { _categoryCssClass = value; }
        }

        /// <summary>
        /// Determines the visible status of the legend text.
        /// </summary>
        public bool DisplayLegend
        {
            get { return _legend.Visible; }
            set { _legend.Visible = value; }
        }
        
        #endregion

        #region Events
                
        /// <summary>
        /// Creates the new literal control and adds it to the user control.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (DataSet != null)
            {
                _container.Controls.Add(_legend);
                _container.Controls.Add(_instructions);
                _container.Controls.Add(BuildProperties());
            }
        }

        private Literal BuildProperties()
        {
            var xml = new StringBuilder();
            using (var writer = XmlWriter.Create(xml, new XmlWriterSettings()
            {
                OmitXmlDeclaration = true,
                Encoding = Response.HeaderEncoding
            }))
            {
                writer.WriteStartElement("ul");
                foreach (var component in DataSet.Components.OrderBy(i => i.ComponentId))
                {
                    BuildProperties(writer, component);
                }
                writer.WriteEndElement();
            }
            var literal = new Literal();
            literal.Text = xml.ToString();
            return literal;
        }

        private void BuildProperties(XmlWriter writer, Component component)
        {
            writer.WriteStartElement("li");
            writer.WriteAttributeString("id", component.Name);
            writer.WriteAttributeString("class", ItemCssClass);
            writer.WriteStartElement("h1");
            writer.WriteString(component.Name);
            writer.WriteEndElement();
            writer.WriteStartElement("ul");
            foreach (var category in component.Properties.Where(i => 
                String.IsNullOrEmpty(i.Category) == false).Select(i => 
                    i.Category).Distinct().OrderBy(i => i))
            {
                BuildProperties(writer, component, category);
            }

            var generalProperties = component.Properties.Where(i =>
                String.IsNullOrEmpty(i.Category)).OrderBy(i =>
                    i.Name);
            if (generalProperties.Count() > 0)
            {
                BuildGeneralProperties(writer, component, generalProperties);
            }

            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        private void BuildGeneralProperties(XmlWriter writer, Component component, IOrderedEnumerable<Property> generalProperties)
        {
            writer.WriteStartElement("li");
            writer.WriteAttributeString("id",
                _removeBadCharacters.Replace(String.Format("{0}_Miscellaneous", component.Name), ""));
            writer.WriteAttributeString("class", CategoryCssClass);
            writer.WriteStartElement("h2");
            writer.WriteString("Miscellaneous");
            writer.WriteEndElement();
            writer.WriteStartElement("ul");
            foreach (var property in generalProperties)
            {
                BuildProperties(writer, property);
            }
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        private void BuildProperties(XmlWriter writer, Component component, string category)
        {
            writer.WriteStartElement("li");
            writer.WriteAttributeString("id",
                _removeBadCharacters.Replace(String.Format("{0}_{1}", component.Name, category), ""));
            writer.WriteAttributeString("class", CategoryCssClass);
            writer.WriteStartElement("h2");
            writer.WriteString(category);
            writer.WriteEndElement();
            writer.WriteStartElement("ul");
            foreach (var property in component.Properties.Where(i => i.Category == category).OrderBy(i => i.Name))
            {
                BuildProperties(writer, property);
            }
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        private void BuildProperties(XmlWriter writer, Property property)
        {
            writer.WriteStartElement("li");
            writer.WriteAttributeString("class", PropertyCssClass);

            writer.WriteStartElement("div");

            writer.WriteStartElement("h3");
            writer.WriteAttributeString("id", _removeBadCharacters.Replace(property.Name, ""));
            writer.WriteAttributeString("class", NameCssClass);
            
            if (property.Url != null)
            {
                BuildExternalLink(writer, property.Url, property.Description);
            }
            writer.WriteString(property.Name);
            if (property.Url != null)
            {
                writer.WriteEndElement();
            }            
            writer.WriteEndElement();

            writer.WriteStartElement("div");
            writer.WriteAttributeString("class", TypeCssClass);
            writer.WriteString(property._valueType.ToString());
            writer.WriteEndElement();

            writer.WriteStartElement("div");
            writer.WriteAttributeString("class", DataSetsCssClass);
            writer.WriteString(String.Join(", ", property.Maps.ToArray()));
            writer.WriteEndElement();

            if (property.IsList || property.IsObsolete)
            {
                writer.WriteStartElement("div");
                writer.WriteAttributeString("class", IconsCssClass);
                if (property.IsList)
                {
                    writer.WriteStartElement("span");
                    writer.WriteString("[L]");
                    writer.WriteEndElement();
                }
                if (property.IsObsolete)
                {
                    writer.WriteStartElement("span");
                    writer.WriteString("[O]");
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }

            writer.WriteEndElement();

            writer.WriteStartElement("div");

            writer.WriteStartElement("div");
            writer.WriteAttributeString("class", DescriptionCssClass);
            writer.WriteString(property.Description);
            if (property.ShowValues)
            {
                BuildValues(writer, property);
            }
            writer.WriteEndElement();

            writer.WriteEndElement();

            writer.WriteEndElement();
        }

        private void BuildValues(XmlWriter writer, Property property)
        {
            if (Page.ClientScript.IsClientScriptBlockRegistered(GetType(), "toggle") == false)
                Page.ClientScript.RegisterClientScriptBlock(GetType(), "toggle", Resources.JavaScriptToggle, true);

            var valuesId = Regex.Replace(String.Format("{0}Values", property.Name), @"[^\w\d-]", "");

            writer.WriteStartElement("span");
            writer.WriteString("[");
            writer.WriteStartElement("a");
            writer.WriteAttributeString("onclick",
                String.Format("toggle(this, '{0}', 'block');", valuesId));
            writer.WriteString("+");
            writer.WriteEndElement();
            writer.WriteString("]");
            writer.WriteEndElement();

            writer.WriteStartElement("ul");
            writer.WriteAttributeString("id", valuesId);
            writer.WriteAttributeString("class", ValueCssClass);
            writer.WriteAttributeString("style", "display: none");
            foreach (var value in property.Values)
            {
                writer.WriteStartElement("li");
                if (value.Url != null)
                {
                    BuildExternalLink(writer, value.Url, 
                        String.IsNullOrEmpty(value.Description) ?
                        String.Format("External information about '{0}'", value.Name) :
                        value.Description);
                }
                writer.WriteString(value.Name);
                if (value.Url != null)
                {
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }
        
        /// <summary>
        /// Adds html to the control displaying the upgrade message.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {
            if (DataSet != null)
            {
                _instructions.Text = Resources.PropertyDictionaryInstructions;
                _legend.Text = ReplaceTags(Resources.PropertyDictionaryLegend);
            }

            base.OnPreRender(e);
        }

        #endregion
    }
}
