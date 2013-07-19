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
 * Mobile Experts Limited are Copyright (C) 2009 - 2013. All Rights Reserved.
 * 
 * Contributor(s):
 *     James Rosewell <james@51degrees.mobi>
 * 
 * ********************************************************************* */

using System;
using System.Web;
using System.Web.UI.WebControls;

namespace Detector
{
    public partial class DeviceProperties : System.Web.UI.UserControl
    {
        /// <summary>
        /// Sets the device Id to the current device.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit(EventArgs e)
        {
            Current.DeviceID = Request.Browser["Id"];
            HttpCookie cookie;
            try
            {
                cookie = Request.Cookies["tab"];
            }
            catch
            {
                cookie = null;
            }
            if (cookie != null)
                SetTab(cookie.Value);
            else
                SetTab(DictionaryView.ID);
            base.OnLoad(e);
        }

        protected override void OnPreRender(EventArgs e)
        {
            CurrentButton.CssClass = String.Join(" ", new[] {
                "tab", Tabs.GetActiveView() == CurrentView ? "active" : String.Empty });
            ExplorerButton.CssClass = String.Join(" ", new[] {
                "tab", Tabs.GetActiveView() == ExplorerView ? "active" : String.Empty });
            TopButton.CssClass = String.Join(" ", new[] {
                "tab", Tabs.GetActiveView() == TopView ? "active" : String.Empty });
            DictionaryButton.CssClass = String.Join(" ", new[] {
                "tab", Tabs.GetActiveView() == DictionaryView ? "active" : String.Empty });
            UserAgentTesterButton.CssClass = String.Join(" ", new[] {
                "tab", Tabs.GetActiveView() == UserAgentTesterView ? "active" : String.Empty });
            RedirectButton.CssClass = String.Join(" ", new[] {
                "tab", Tabs.GetActiveView() == RedirectView ? "active" : String.Empty });
            DetectionButton.CssClass = String.Join(" ", new[] {
                "tab", Tabs.GetActiveView() == DetectionView ? "active" : String.Empty });
            StandardPropertiesButton.CssClass = String.Join(" ", new[] {
                "tab", Tabs.GetActiveView() == StandardPropertiesView ? "active" : String.Empty });

            HttpCookie cookie = Request.Cookies["tab"];
            if (cookie == null)
                cookie = new HttpCookie("tab");
            cookie.Value = Tabs.GetActiveView().ID;
            Response.Cookies.Add(cookie);

            base.OnPreRender(e);
        }

        protected void TabChange(object sender, CommandEventArgs e)
        {
            SetTab(e.CommandArgument as string);
        }

        private void SetTab(string name)
        {
            switch (name)
            {
                default:
                case "CurrentView":
                    Tabs.SetActiveView(CurrentView);
                    CurrentButton.CssClass = "active";
                    break;
                case "ExplorerView":
                    Tabs.SetActiveView(ExplorerView);
                    ExplorerButton.CssClass = "active";
                    break;
                case "TopView":
                    {
                        if (TopDevices.SelectedDevice != null)
                            goto case "ExplorerView";
                        Tabs.SetActiveView(TopView);
                        TopButton.CssClass = "active";
                        break;
                    }
                case "DictionaryView":
                    Tabs.SetActiveView(DictionaryView);
                    DictionaryButton.CssClass = "active";
                    break;
                case "UserAgentTesterView":
                    Tabs.SetActiveView(UserAgentTesterView);
                    UserAgentTesterButton.CssClass = "active";
                    break;
                case "RedirectView":
                    Tabs.SetActiveView(RedirectView);
                    RedirectButton.CssClass = "active";
                    break;
                case "DetectionView":
                    Tabs.SetActiveView(DetectionView);
                    DetectionButton.CssClass = "active";
                    break;
                case "StandardPropertiesView":
                    Tabs.SetActiveView(StandardPropertiesView);
                    StandardPropertiesButton.CssClass = "active";
                    break;
            }
        }
    }
}