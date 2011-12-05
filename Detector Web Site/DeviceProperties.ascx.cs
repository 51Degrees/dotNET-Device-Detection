using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Detector
{
    public partial class DeviceProperties : System.Web.UI.UserControl
    {
        /// <summary>
        /// A list of the basic properties to be displayed at the top of the list.
        /// </summary>
        private static readonly string[] LITE_PROPERTIES = new string[] {
            "HardwareVendor",
            "HardwareModel",
            "IsMobile",
            "LayoutEngine" };

        /// <summary>
        /// The user agent string whose properties should be found.
        /// </summary>
        public string UserAgentString = null;

        /// <summary>
        /// Creates a new label to display a property and it's values.
        /// </summary>
        /// <param name="capability"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private Panel CreateResultLabel(string capability, string value)
        {
            Panel pnl = new Panel();
            Label lbl = new Label();
            lbl.Text = String.Format("{0}: {1}", capability, value);
            pnl.Controls.Add(lbl);
            return pnl;

        }
        
        /// <summary>
        /// Finds the property values for the name provided.
        /// </summary>
        /// <param name="allProperties">List of all properties.</param>
        /// <param name="propertyName">Property name to be found.</param>
        /// <returns>A string array for the values found, or null if no values are found.</returns>
        private WebService.Property GetProperty(List<WebService.Property> allProperties, string propertyName)
        {
            foreach (var property in allProperties)
                if (property.Name == propertyName)
                    return property;
            return null;
        }

        /// <summary>
        /// Gets the device details for the user agent passed to the control.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_PreRender(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(UserAgentString) == false)
            {
                // Create the service setting the URL to the current application.
                var service = new WebService.MobileDevice
                {
                    UserAgent = UserAgentString,
                    Url = Request.Url.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped) + ResolveUrl("~/MobileDevice.asmx")
                };

                var allProperties = new List<WebService.Property>(service.GetAllProperties());

                // Add the lite properties.
                foreach (string name in LITE_PROPERTIES)
                {
                    var property = GetProperty(allProperties, name);
                    if (property != null)
                    {
                        PanelLite.Controls.Add(CreateResultLabel(
                            name, String.Join(", ", property.Values)));
                        allProperties.Remove(property);
                    }
                }

                // Add the premium properties.
                foreach (var property in allProperties)
                {
                    PanelPremium.Controls.Add(CreateResultLabel(
                        property.Name, String.Join(", ", property.Values)));
                }

                PanelLite.Visible = true;
                PanelPremium.Visible = true;
            }
            else
            {
                PanelLite.Visible = false;
                PanelPremium.Visible = false;
            }
        }
    }
}