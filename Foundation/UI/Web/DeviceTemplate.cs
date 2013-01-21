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

using System.Web.UI;
using System.Web.UI.WebControls;

namespace FiftyOne.Foundation.UI.Web
{
    /// <summary>
    /// Template used to display device properties.
    /// </summary>
    public class DeviceTemplate : ITemplate
    {
        /// <summary>
        /// Adds requirement controls to the container.
        /// </summary>
        /// <param name="container">Container the template is being displayed in.</param>
        public void InstantiateIn(Control container)
        {
            Panel device = new Panel();
            Panel model = new Panel();
            Panel image = new Panel();
            Panel name = new Panel();

            device.ID = "Device";
            model.ID = "Model";
            image.ID = "Image";
            name.ID = "Name";

            device.Controls.Add(model);
            device.Controls.Add(image);
            device.Controls.Add(name);

            container.Controls.Add(device);
        }
    }
}
