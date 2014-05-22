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

using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using System.Linq;

namespace FiftyOne.Foundation.UI.Web
{
    internal static class DeviceImages
    {
        #region Constants

        const string UNKNOWN_IMAGE_URL = "http://download.51Degrees.mobi/DeviceImages/UnknownPhone.png";

        #endregion
        
        /// <summary>
        /// Gets a panel with all the image elements from a device within it.
        /// </summary>
        /// <param name="images"></param>
        /// <returns></returns>
        internal static Panel GetImagesPanel(KeyValuePair<string, Uri>[] images)
        {
            if (images != null)
            {
                Panel panel = new Panel();
                foreach (var hardwareImage in images)
                {
                    Panel item = new Panel();
                    item.Style.Add("float", "left");

                    Panel imagePanel = new Panel();

                    System.Web.UI.WebControls.Image image = new System.Web.UI.WebControls.Image();
                    image.ImageUrl = hardwareImage.Value.ToString();
                    image.Height = 128;
                    image.Width = 128;
                    imagePanel.Controls.Add(image);


                    Literal caption = new Literal();

                    caption.Text = String.Format("<h4 class=\"deviceImageCaption\">{0}</h4>", hardwareImage.Key);

                    item.Controls.Add(imagePanel);
                    item.Controls.Add(caption);

                    panel.Controls.Add(item);
                }
                return panel;
            }
            else
                return null;
        }
        
        /// <summary>
        /// Gets an image control with mouse events required to rotate the image cycle on mouse over. Returns null if
        /// no image is available.
        /// </summary>
        /// <param name="deviceURL"></param>
        /// <param name="images"></param>
        /// <returns></returns>
        internal static System.Web.UI.WebControls.WebControl GetDeviceImageRotater(KeyValuePair<string, Uri>[] images, string deviceURL)
        {
            HyperLink deviceLink = new HyperLink();
            deviceLink.NavigateUrl = deviceURL;
            if (images != null)
            {
                System.Web.UI.WebControls.Image deviceImage = new System.Web.UI.WebControls.Image();

                if (images.Length > 0)
                {
                    deviceImage.ImageUrl = images[0].Value.ToString();

                    // Hover events are only useful if there are extra images to cycle to
                    if (images.Length > 1)
                    {
                        string[] imageUrls = images.Select(i => String.Format("'{0}'", i.Value)).ToArray();

                        // Create onmouseover event. It creates an array of url strings that should be cycled in order
                        string mouseOver = String.Format("ImageHovered(this, new Array({0}))",
                            String.Join(",", imageUrls)
                            );

                        // Create onmouseout event. It is passed a single url string that should be loaded when the cursor leaves the image
                        string mouseOff = String.Format("ImageUnHovered(this, '{0}')", images[0].Value.ToString());

                        deviceImage.Attributes.Add("onmouseover", mouseOver.Replace("\\", "\\\\"));
                        deviceImage.Attributes.Add("onmouseout", mouseOff.Replace("\\", "\\\\"));
                    }
                }
                else // there are no images availble, so use the unknown one
                    deviceImage.ImageUrl = UNKNOWN_IMAGE_URL;

                deviceImage.Height = Unit.Pixel(128);
                deviceImage.Width = Unit.Pixel(128);

                deviceLink.Controls.Add(deviceImage);
                return deviceLink;
            }
            else
                return null;
        }
    }
}
