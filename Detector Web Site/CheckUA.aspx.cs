using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;

namespace Detector
{
    public partial class CheckUA : System.Web.UI.Page
    {
        private Panel CreateResultLabel(string capability, string value)
        {
            Panel pnl = new Panel();
            Label lbl = new Label();
            lbl.Text = String.Format("{0}: {1}", capability, value);
            pnl.Controls.Add(lbl);
            return pnl;
            
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack == true && String.IsNullOrEmpty(TextBoxUA.Text) == false)
            {
                bool isResponseMobile;

                // Create the service setting the URL to the current application.
                var service = new WebService.MobileDevice {
                    UserAgent = TextBoxUA.Text, 
                    Url = Request.Url.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped) + ResolveUrl("~/MobileDevice.asmx") };
                
                if (bool.TryParse(service.GetCapability("is_wireless_device"), out isResponseMobile))
                {
                    if (isResponseMobile)
                        PanelBasic.Controls.Add(CreateResultLabel("Is Mobile Device", "Yes"));
                    else
                        PanelBasic.Controls.Add(CreateResultLabel("Is Mobile Device", "No"));
                }
                else
                {
                    PanelBasic.Controls.Add(CreateResultLabel("Is Mobile Device", "Unknown"));
                }

                PanelBasic.Controls.Add(CreateResultLabel("Brand Name", service.GetCapability("brand_name")));
                PanelBasic.Controls.Add(CreateResultLabel("Model Name", service.GetCapability("model_name")));

                PanelAdvanced.Controls.Add(CreateResultLabel("WURFL Device Id", service.GetCapability("deviceid")));
                PanelAdvanced.Controls.Add(CreateResultLabel("Screen Height Pixels", service.GetCapability("resolution_height")));
                PanelAdvanced.Controls.Add(CreateResultLabel("Screen Width Pixels", service.GetCapability("resolution_width")));
                PanelAdvanced.Controls.Add(CreateResultLabel("Physical Screen Height", service.GetCapability("physical_screen_height")));
                PanelAdvanced.Controls.Add(CreateResultLabel("Physical Screen Width", service.GetCapability("physical_screen_width")));
                PanelAdvanced.Controls.Add(CreateResultLabel("Pointing Method", service.GetCapability("pointing_method")));
                PanelAdvanced.Controls.Add(CreateResultLabel("Actual Device Root", service.GetCapability("actual_device_root")));
                PanelAdvanced.Controls.Add(CreateResultLabel("Tablet", service.GetCapability("is_tablet")));
                PanelAdvanced.Controls.Add(CreateResultLabel("Version", service.GetCapability("mobile_browser_version")));
                PanelAdvanced.Controls.Add(CreateResultLabel("Device OS", service.GetCapability("device_os")));

                PanelBasic.Visible = true;
                PanelAdvanced.Visible = true;
            }
            else
            {
                PanelBasic.Visible = false;
                PanelAdvanced.Visible = false;
            }
        }
    }
}
