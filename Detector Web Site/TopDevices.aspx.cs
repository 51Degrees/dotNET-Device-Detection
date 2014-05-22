using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Detector
{
    public partial class TopDevices : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            topDevices.Description = string.Format("This page shows the {0} most popular new devices of the current data file.", topDevices.DeviceAmount);
            topDevices.DeviceUrl = "Devices.aspx";
        }
    }
}