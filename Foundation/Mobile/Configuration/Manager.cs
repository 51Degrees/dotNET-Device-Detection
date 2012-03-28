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

#region Usings

using System.Web.Configuration;
using System.Configuration;

#endregion

namespace FiftyOne.Foundation.Mobile.Configuration
{
    internal static class Manager
    {
        #region Fields

        internal static LogSection Log;
        internal static RedirectSection Redirect;

        #endregion

        #region Constructor

        static Manager()
        {
            Log = (LogSection)Support.GetWebApplicationSection("fiftyOne/log", false);
            Redirect = (RedirectSection)Support.GetWebApplicationSection("fiftyOne/redirect", false);

            if (Redirect == null)
                Redirect = new RedirectSection();
        }

        #endregion

        #region Manager
        
        /// <summary>
        /// Creates a new configuration instance checking for
        /// fresh data.
        /// </summary>
        internal static void Refresh()
        {
            // Ensure the managers detection section is refreshed in case the
            // process is not going to restart as a result of the change.
            ConfigurationManager.RefreshSection("fiftyOne/redirect");

            Redirect = Support.GetWebApplicationSection("fiftyOne/redirect", false) as RedirectSection;
        }

        #endregion
    }
}