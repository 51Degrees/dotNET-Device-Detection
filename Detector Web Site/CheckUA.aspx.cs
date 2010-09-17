using System;
using System.Collections.Generic;
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
                bool isResponseMobile = false;
                string thisPage = ResolveClientUrl(Page.AppRelativeVirtualPath);
                string newPage = Request.Url.ToString().Replace(thisPage, ResolveClientUrl("~/Default.aspx"));
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(newPage);
                request.UserAgent = TextBoxUA.Text;
                request.Referer = Request.Url.ToString();
                request.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (bool.TryParse(response.Headers["IsMobileDevice"], out isResponseMobile) == true)
                {
                    if (isResponseMobile == true)
                        PanelBasic.Controls.Add(CreateResultLabel("Is Mobile Device", "Yes"));
                    else
                        PanelBasic.Controls.Add(CreateResultLabel("Is Mobile Device", "No"));
                }
                else
                {
                    PanelBasic.Controls.Add(CreateResultLabel("Is Mobile Device", "Unknown"));
                }

                PanelBasic.Controls.Add(CreateResultLabel("Manufacturer", response.Headers["MobileDeviceManufacturer"]));
                PanelBasic.Controls.Add(CreateResultLabel("Model", response.Headers["MobileDeviceModel"]));

                PanelAdvanced.Controls.Add(CreateResultLabel("WURFL Device Id", response.Headers["deviceid"]));
                PanelAdvanced.Controls.Add(CreateResultLabel("Screen Height", response.Headers["ScreenPixelsHeight"]));
                PanelAdvanced.Controls.Add(CreateResultLabel("Screen Width", response.Headers["ScreenPixelsWidth"]));
                PanelAdvanced.Controls.Add(CreateResultLabel("Pointing Method", response.Headers["PointingMethod"]));
                PanelAdvanced.Controls.Add(CreateResultLabel("Actual Device Root", response.Headers["ActualDeviceRoot"]));
                PanelAdvanced.Controls.Add(CreateResultLabel("Tablet", response.Headers["IsTabletDevice"]));
                PanelAdvanced.Controls.Add(CreateResultLabel("Version", response.Headers["Version"]));

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
