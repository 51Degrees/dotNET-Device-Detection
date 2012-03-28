using System.Web.UI;
using System.Web.UI.WebControls;

namespace FiftyOne.Foundation.UI.Web
{
    /// <summary>
    /// Template used to display a vendor from the data set.
    /// </summary>
    public class VendorTemplate : ITemplate
    {
        /// <summary>
        /// Adds requirement controls to the container.
        /// </summary>
        /// <param name="container">Container the template is being displayed in.</param>
        public void InstantiateIn(Control container)
        {
            var panel = new Panel();
            var open = new Literal();
            var close = new Literal();
            var vendorLink = new HyperLink();
            var countLabel = new Label();

            vendorLink.ID = "Vendor";
            countLabel.ID = "Count";
            panel.ID = "Panel";

            open.Text = "&nbsp;(";
            close.Text = ")";

            panel.Controls.Add(vendorLink);
            panel.Controls.Add(open);
            panel.Controls.Add(countLabel);
            panel.Controls.Add(close);

            container.Controls.Add(panel);
        }
    }
}
