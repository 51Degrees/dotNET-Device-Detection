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
        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack == true && String.IsNullOrEmpty(TextBoxUA.Text) == false)
            {
                PropertiesDevice.UserAgentString = TextBoxUA.Text;
            }
        }
    }
}
