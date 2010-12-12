using System.Collections.Generic;
using System.Web.Services;

namespace Detector
{
    ///
    /// Example web server for returning WURFL capabilities. Session is disabled.
    ///

    [WebService(Namespace = "http://mobiledevice.51degrees.mobi/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    public class MobileDevice : System.Web.Services.WebService
    {
        [WebMethod(false)]
        public string GetCapability(string capability)
        {
            return Context.Request.Browser[capability];
        }
    }
}