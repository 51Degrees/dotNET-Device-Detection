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
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FiftyOne.Foundation.UI.Web
{
    /// <summary>
    /// Template used to display a heading.
    /// </summary>
    public class HeaderTemplate : ITemplate
    {
        private int _level;
        private string _text;

        /// <summary>
        /// Constructs the template setting parameters that control the heading created.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="level"></param>
        internal HeaderTemplate(string text, int level)
        {
            _text = text;
            _level = level;
        }

        /// <summary>
        /// Adds requirement controls to the container.
        /// </summary>
        /// <param name="container">Container the template is being displayed in.</param>
        public void InstantiateIn(Control container)
        {
            var open = new Literal();
            var close = new Literal();
            var label = new Label();

            open.Text = String.Format("<h{0}>", _level);
            label.ID = "Heading";
            label.Text = _text;
            close.Text = String.Format("</h{0}>", _level);

            container.Controls.Add(open);
            container.Controls.Add(label);
            container.Controls.Add(close);
        }
    }
}
