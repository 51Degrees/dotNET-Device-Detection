/* *********************************************************************
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited. 
 * Copyright © 2014 51Degrees Mobile Experts Limited, 5 Charlotte Close,
 * Caversham, Reading, Berkshire, United Kingdom RG4 7BY
 * 
 * This Source Code Form is the subject of the following patent 
 * applications, owned by 51Degrees Mobile Experts Limited of 5 Charlotte
 * Close, Caversham, Reading, Berkshire, United Kingdom RG4 7BY: 
 * European Patent Application No. 13192291.6; and
 * United States Patent Application Nos. 14/085,223 and 14/085,301.
 *
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
