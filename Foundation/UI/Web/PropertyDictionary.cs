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

using System;
using System.Web.UI.WebControls;
using FiftyOne.Foundation.Mobile.Detection;

namespace FiftyOne.Foundation.UI.Web
{
    /// <summary>
    /// Displays a list of the available properties and values.
    /// </summary>
    public class PropertyDictionary : BaseUserControl
    {
        #region Fields

        private Literal _legend = new Literal();
        private Literal _instructions = new Literal();
        private DataList _hardware = null;
        private DataList _software = null;
        private DataList _browser = null;
        private DataList _content = null;

        #endregion

        #region Properties

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
        /// Renders the property to the data list control.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataList_ItemDataBound(object sender, DataListItemEventArgs e)
        {
            if (e.Item.DataItem != null &&
                e.Item.DataItem is Property)
            {
                var property = (Property)e.Item.DataItem;

                var propertyPanel = e.Item.FindControl("Property") as Panel;
                var descriptionPanel = e.Item.FindControl("Description") as Panel;
                
                AddLabel(propertyPanel, property.Name, null, property.Url, property.Name);
                if (property.IsList)
                    AddLabel(propertyPanel, " [L]", null, null, null);
                AddLabel(descriptionPanel, property.Description, null, null, null);

                if (property.ShowValues)
                {
                    var valuesPanel = AddPlus(descriptionPanel, "block");
                    valuesPanel.CssClass = ValueCssClass;

                    int count = 0;
                    foreach (var value in property.Values)
                    {
                        if (count > 0)
                        {
                            var comma = new Literal();
                            comma.Text = ", ";
                            valuesPanel.Controls.Add(comma);
                        }
                        AddLabel(valuesPanel, value.Name, value.Description, value.Url, null);
                        count++;
                    }
                }

                propertyPanel.CssClass = String.Format("{0} {1}",
                    PropertyCssClass,
                    property.IsPremium ? PremiumCssClass : LiteCssClass);
                descriptionPanel.CssClass = DescriptionCssClass;
            }
        }

        /// <summary>
        /// Creates the new literal control and adds it to the user control.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            _hardware = CreateDataList("Hardware");
            _software = CreateDataList("Software");
            _browser = CreateDataList("Browser");
            _content = CreateDataList("Content");
                        
            _container.Controls.Add(_legend);
            _container.Controls.Add(_instructions);
            _container.Controls.Add(_hardware);
            _container.Controls.Add(_software);
            _container.Controls.Add(_browser);
            _container.Controls.Add(_content);
        }

        /// <summary>
        /// Adds html to the control displaying the upgrade message.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {
            _instructions.Text = Resources.PropertyDictionaryInstructions;
            _legend.Text = ReplaceTags(Resources.PropertyDictionaryLegend);

            _hardware.DataSource = DataProvider.HardwareProperties;
            _software.DataSource = DataProvider.SoftwareProperties;
            _browser.DataSource = DataProvider.BrowserProperties;
            _content.DataSource = DataProvider.ContentProperties;

            _hardware.DataBind();
            _software.DataBind();
            _browser.DataBind();
            _content.DataBind();

            _hardware.CssClass = ItemCssClass;
            _software.CssClass = ItemCssClass;
            _browser.CssClass = ItemCssClass;
            _content.CssClass = ItemCssClass;

            base.OnPreRender(e);
        }

        #endregion

        #region Private Methods

        private DataList CreateDataList(string heading)
        {
            var dataList = new DataList();
            dataList.ShowHeader = true;
            dataList.ItemDataBound += new DataListItemEventHandler(DataList_ItemDataBound);
            dataList.HeaderTemplate = new HeaderTemplate(heading, 1);
            dataList.ItemTemplate = new PropertyTemplate();
            return dataList;
        }
        
        #endregion
    }
}
