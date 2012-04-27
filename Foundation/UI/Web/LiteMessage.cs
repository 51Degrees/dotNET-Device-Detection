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

using System;
using System.Web.UI.WebControls;

namespace FiftyOne.Foundation.UI.Web
{
    /// <summary>
    /// This class displays simple HTML explaining why any upgraded is needed. The text the control displays
    /// can be altered in the resources file. Retailers and affiliates should provide this own values for the
    /// RetailerUrl and RetailerName to direct users to their eCommerce web sites. Affiliates should ensure
    /// their affiliate ID is included in the URL if using the 51Degrees.mobi eCommerce web site.
    /// </summary>
    public class LiteMessage : BaseUserControl
    {
        #region Fields

        private Uri _retailerUrl = new Uri(RetailerConstants.RetailerUrl);
        private string _retailerName = RetailerConstants.RetailerName;
        private Literal _html;

        #endregion

        #region Properties

        /// <summary>
        /// The url of the retailers web site.
        /// </summary>
        public Uri RetailerUrl
        {
            get { return _retailerUrl; }
            set { _retailerUrl = value; }
        }

        /// <summary>
        /// The name of the retailer.
        /// </summary>
        public string RetailerName
        {
            get { return _retailerName; }
            set { _retailerName = value; }
        }

        #endregion
        
        #region Events

        /// <summary>
        /// Creates the new literal control and adds it to the user control.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _html = new Literal();
            _container.Controls.Add(_html);
        }

        /// <summary>
        /// Adds html to the control displaying the upgrade message.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {
            _html.Text = String.Format(Resources.UpgradeHtml,
                Resources.FiftyOneDegreesUrl,
                _retailerUrl,
                _retailerName);
            _html.Visible = DataProvider.IsPremium == false;
            base.OnPreRender(e);
        }

        #endregion
    }
}
