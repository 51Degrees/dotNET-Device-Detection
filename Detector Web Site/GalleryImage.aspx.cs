/* *********************************************************************
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited. 
 * Copyright 2017 51Degrees Mobile Experts Limited, 5 Charlotte Close,
 * Caversham, Reading, Berkshire, United Kingdom RG4 7BY
 * 
 * This Source Code Form is the subject of the following patents and patent 
 * applications, owned by 51Degrees Mobile Experts Limited of 5 Charlotte
 * Close, Caversham, Reading, Berkshire, United Kingdom RG4 7BY: 
 * European Patent No. 2871816;
 * European Patent Application No. 17184134.9;
 * United States Patent Nos. 9,332,086 and 9,350,823; and
 * United States Patent Application No. 15/686,066.
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0.
 * 
 * If a copy of the MPL was not distributed with this file, You can obtain
 * one at http://mozilla.org/MPL/2.0/.
 * 
 * This Source Code Form is "Incompatible With Secondary Licenses", as
 * defined by the Mozilla Public License, v. 2.0.
 * ********************************************************************* */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using FiftyOne.Foundation.Mobile.Detection;

namespace Detector
{
    public partial class GalleryImage : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var image = Request.QueryString["Image"];
            if (image != null)
            {
                if (Request.Browser["JavascriptImageOptimiser"] is string)
                {
                    Image.Attributes.Add("src", ResolveUrl("~/Empty.gif"));
                    Image.Attributes.Add("data-src", String.Format("{0}?w=auto", image));
                }
                else
                {
                    var screenPixelWidth = Request.Browser["ScreenPixelsWidth"];
                    int pixelWidth = 0;
                    if (int.TryParse(screenPixelWidth, out pixelWidth) == false)
                        pixelWidth = 800;
                    Image.Attributes.Add("src", String.Format("{0}?w={1}", image, pixelWidth));
                }
            }
        }
    }
}