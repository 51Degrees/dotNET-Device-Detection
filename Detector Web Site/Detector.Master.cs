/* *********************************************************************
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited. 
 * Copyright 2014 51Degrees Mobile Experts Limited, 5 Charlotte Close,
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
 * This Source Code Form is "Incompatible With Secondary Licenses", as
 * defined by the Mozilla Public License, v. 2.0.
 * ********************************************************************* */

using System;
using System.IO;
using System.Net;
using System.Web;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Net.Cache;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Detector
{
    public partial class Detector : System.Web.UI.MasterPage
    {
        /// <summary>
        /// List of client IPs addresses that should be ignored for google analytics.
        /// </summary>
        private static readonly string[] BARRED_IPS = new string[] {
            "109.153.230.184",
            "91.223.16.108",
            "195.234.10.118",
            "81.149.101.214"
        };

        /// <summary>
        /// Cookie to use to store client device id.
        /// </summary>
        private const string CLIENT_ID_COOKIE_NAME = "_51DCID";

        /// <summary>
        /// True if universal analytics should be enabled.
        /// </summary>
        private static bool _enabled = false;

        protected void Page_Init(object sender, EventArgs e)
        {
            if (_enabled && 
                Request.Browser.Crawler == false && 
                Request.IsLocal == false &&
                BARRED_IPS.Contains(Request.UserHostAddress) == false)
            {
                var parameters = new Dictionary<string, string>();

                // Add the basic header parameters.
                parameters.Add("v", "1");
                parameters.Add("t", "pageview");
                parameters.Add("tid", "INSERT GA CODE");
                parameters.Add("z", Convert.ToBase64String(Guid.NewGuid().ToByteArray()));

                // Add client information.
                if (Request.Cookies[CLIENT_ID_COOKIE_NAME] != null)
                {
                    // Cookie already exists so use that one.
                    parameters.Add("cid", Request.Cookies[CLIENT_ID_COOKIE_NAME].Value);
                }
                else
                {
                    // This is a new request so set the client Id.
                    var buffer = new List<byte>();

                    // Use the IP Address of the client as the first part of the CID.
                    IPAddress clientIP;
                    if (IPAddress.TryParse(Request.UserHostAddress, out clientIP))
                        buffer.AddRange(clientIP.GetAddressBytes());
                    else
                        buffer.AddRange(Encoding.ASCII.GetBytes(Request.UserHostAddress));

                    // Use the date as the second part.
                    buffer.AddRange(BitConverter.GetBytes((short)DateTime.UtcNow.Year));
                    buffer.AddRange(BitConverter.GetBytes((byte)DateTime.UtcNow.Month));
                    buffer.AddRange(BitConverter.GetBytes((byte)DateTime.UtcNow.Day));
                    buffer.AddRange(BitConverter.GetBytes((short)DateTime.UtcNow.TimeOfDay.TotalMinutes));

                    // Use a hash code of the user agent for the final part.
                    buffer.AddRange(BitConverter.GetBytes(Request.UserAgent.GetHashCode()));

                    var clientIdCookie = new HttpCookie(
                        CLIENT_ID_COOKIE_NAME,
                        Convert.ToBase64String(buffer.ToArray()));

                    clientIdCookie.HttpOnly = true;
                    clientIdCookie.Expires = DateTime.UtcNow.AddYears(2);
                    Response.Cookies.Add(clientIdCookie);
                    parameters.Add("cid", clientIdCookie.Value);
                }
                parameters.Add("uip", Request.UserHostAddress);

                // If the session is new indicate this to google.
                if (Session != null &&
                    Session.IsNewSession)
                {
                    parameters.Add("sc", "start");
                }

                // Add user agent and device information.
                parameters.Add("ua", Request.UserAgent);
                parameters.Add("je", Request.Browser.JavaScript ? "1" : "0");
                if (Request.Browser.IsMobileDevice)
                {
                    var screenSize = String.Format(
                        "{0}x{1}",
                        Request.Browser.ScreenPixelsWidth,
                        Request.Browser.ScreenPixelsHeight);
                    parameters.Add("sr", screenSize);
                    parameters.Add("vp", screenSize);
                }

                // Add language information.
                if (Request.UserLanguages != null &&
                    Request.UserLanguages.Length > 0)
                    parameters.Add("ul", Request.UserLanguages[0]);

                // Add page information.
                Uri page;
                if (Uri.TryCreate(Request.Url, Request.RawUrl, out page) == false)
                    page = Request.Url;
                parameters.Add("dp", page.AbsolutePath);
                if (String.IsNullOrEmpty(Page.Title) == false)
                    parameters.Add("dt", Page.Title);
                parameters.Add("dh", page.Host);
                parameters.Add("dl", page.ToString());
                parameters.Add("de", Response.ContentEncoding.WebName.ToUpperInvariant());

                // Add referrer information.
                if (Request.UrlReferrer != null)
                    parameters.Add("dr", Request.UrlReferrer.ToString());

                // Add campaign parameters.
                if (Request.QueryString["utm_campaign"] != null)
                    parameters.Add("cn", Request.QueryString["utm_campaign"]);
                if (Request.QueryString["utm_content"] != null)
                    parameters.Add("cc", Request.QueryString["utm_content"]);
                if (Request.QueryString["utm_term"] != null)
                    parameters.Add("ck", Request.QueryString["utm_term"]);
                if (Request.QueryString["utm_medium"] != null)
                    parameters.Add("cm", Request.QueryString["utm_medium"]);
                if (Request.QueryString["utm_source"] != null)
                    parameters.Add("cs", Request.QueryString["utm_term"]);

                // Add user id information if known.
                if (Page.User != null &&
                    String.IsNullOrEmpty(Page.User.Identity.Name) == false)
                {
                    parameters.Add("uid", Page.User.Identity.Name);
                }

                // Build the url for google.
                var url = String.Concat(
                    "https://ssl.google-analytics.com/collect?",
                    String.Join("&", parameters.Select(p =>
                        String.Format("{0}={1}",
                        p.Key,
                        HttpUtility.UrlEncode(p.Value, Encoding.UTF8))).ToArray()));

                // Send the request asynchronously.
                var request = HttpWebRequest.Create(url);
                request.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
                request.Method = "GET";
                request.Timeout = 20000;
                request.BeginGetResponse(SendGoogleAnalyticsComplete, request);
            }
        }

        private static void SendGoogleAnalyticsComplete(IAsyncResult result)
        {
            _enabled = _enabled && result.IsCompleted;
            if (result.IsCompleted)
            {
                var request = (HttpWebRequest)result.AsyncState;
                var response = (HttpWebResponse)request.EndGetResponse(result);
                var buffer = new byte[response.ContentLength];
                response.GetResponseStream().Read(buffer, 0, buffer.Length);
            }
        }

        protected void Menu_Load(object sender, EventArgs e)
        {
            Menu.DataSource = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.aspx");
            Menu.DataBind();
        }
    }
}
