/* *********************************************************************
 * The contents of this file are subject to the Mozilla Public License 
 * Version 1.1 (the "License"); you may not use this file except in 
 * compliance with the License. You may obtain a copy of the License at 
 * http://www.mozilla.org/MPL/
 * 
 * Software distributed under the License is distributed on an "AS IS" 
 * basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 * See the License for the specific language governing rights and 
 * limitations under the License.
 *
 * The Original Code is named .NET Mobile API, first released under 
 * this licence on 11th March 2009.
 * 
 * The Initial Developer of the Original Code is owned by 
 * 51 Degrees Mobile Experts Limited. Portions created by 51 Degrees
 * Mobile Experts Limited are Copyright (C) 2009 - 2012. All Rights Reserved.
 * 
 * Contributor(s):
 *     James Rosewell <james@51degrees.mobi>
 * 
 * ********************************************************************* */

#if VER4
using System;
using System.Web.Configuration;
#endif

namespace Detector
{
    public class Global : System.Web.HttpApplication
    {
        // .NET v4 ONLY
        // Uncomment the following code to use Global.asax rather than web.config HttpModules
        // to invoke device detection and redirection functionality. Do not use both HttpModules
        // and the following uncommented code. Unpredictable results may be experienced.

//#if VER4
//        protected void Application_Start(object sender, EventArgs e)
//        {
//            // Enable the mobile detection provider.
//            HttpCapabilitiesBase.BrowserCapabilitiesProvider =
//                new FiftyOne.Foundation.Mobile.Detection.MobileCapabilitiesProvider();
//        }

//        protected void Application_AcquireRequestState(object sender, EventArgs e)
//        {
//            // Check if a redirection is needed.
//            FiftyOne.Foundation.Mobile.Redirection.RedirectModule.OnPostAcquireRequestState(sender, e);
//        }
//#endif
    }
}