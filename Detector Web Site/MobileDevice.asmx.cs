using System.Collections.Generic;
using System.Web.Services;

namespace Detector
{
    ///
    /// Example web server for returning WURFL capabilities.
    ///

    [WebService(Namespace = "http://mobiledevice.51degrees.mobi/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    public class MobileDevice : System.Web.Services.WebService
    {
        [WebMethod]
        public string GetCapability(string capability)
        {
            // Get the sorted list of wurfl capabilities.
            var wurfl = Context.Request.Browser.Capabilities["WurflCapabilities"] as SortedList<string, string>;
            string value;
            // If the list exists find the capability requested.
            if (wurfl != null &&
                wurfl.TryGetValue(capability, out value))
                return value;
            return null;
        }
    }
}