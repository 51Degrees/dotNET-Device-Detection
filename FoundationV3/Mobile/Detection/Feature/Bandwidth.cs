/* *********************************************************************
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited. 
 * Copyright © 2017 51Degrees Mobile Experts Limited, 5 Charlotte Close,
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
 * This Source Code Form is “Incompatible With Secondary Licenses”, as
 * defined by the Mozilla Public License, v. 2.0.
 * ********************************************************************* */

using FiftyOne.Foundation.Mobile.Detection.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FiftyOne.Foundation.Mobile.Detection.Feature
{
    internal static class Bandwidth
    {
        #region Classes

        private class Cookie 
        {
            // Used to split the values in the cookie.
            private static readonly char[] _split = new char[] { '|' };

            internal long Id;
            internal DateTime BrowserTimeSent;
            internal DateTime BrowserTimeRecieved;
            internal DateTime BrowserTimeCompleted;
            internal int ResponseLength;

            internal static Cookie Create(HttpContext context)
            {
                var cookie = context.Request.Cookies["51D_Bandwidth"];
                if (cookie != null)
                {
                    var values = HttpUtility.UrlDecode(cookie.Value).Split(_split);
                    if (values.Length == 5)
                    {
                        try
                        {
                            var newCookie = new Cookie();
                            newCookie.Id = BitConverter.ToInt64(Convert.FromBase64String(values[0]), 0);
                            newCookie.BrowserTimeRecieved = GetDateTime(values[1]);
                            newCookie.BrowserTimeSent = GetDateTime(values[2]);
                            newCookie.BrowserTimeCompleted = GetDateTime(values[3]);
                            int.TryParse(values[4], out newCookie.ResponseLength);
                            return newCookie;
                        }
                        catch
                        {
                            return null;
                        }
                    }
                }
                return null;
            }

            /// <summary>
            /// Gets the date time from the string provided.
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            private static DateTime GetDateTime(string value)
            {
                long longValue = 0;
                if (long.TryParse(value, out longValue))
                    return DateTime.FromBinary(longValue * TimeSpan.TicksPerMillisecond);
                return DateTime.MinValue;
            }
        }

        [Serializable]
        private class Stat : IComparable<Stat>
        {
            // Set when the server starts to send the headers.
            internal DateTime ServerTimeSent;

            // Set when the browser recieves the response.
            internal DateTime BrowserTimeRecieved;

            // Set when the browser finishes rendering the response.
            internal DateTime BrowserTimeCompleted;

            // Set when the browser sends the request.
            internal DateTime BrowserTimeSent;

            // Set when the stat is created.
            internal DateTime ServerTimeRecieved;

            // Length in bytes of the response.
            internal int ResponseLength;

            // Length in bytes of the request.
            internal int RequestLength;

            /// <summary>
            /// The unique Id of the stat for the session.
            /// </summary>
            internal long Id;

            /// <summary>
            /// The stat id encoded into a url and cookie safe string.
            /// </summary>
            internal string EncodedId
            {
                get
                {
                    return HttpUtility.UrlEncode(Convert.ToBase64String(BitConverter.GetBytes(Id)));
                }
            }

            internal Stat()
            {
                Id = DateTime.UtcNow.Ticks;
            }

            private Nullable<TimeSpan> ServerProcessingTime
            {
                get
                {
                    if (ServerTimeSent >= ServerTimeRecieved)
                        return ServerTimeSent - ServerTimeRecieved;
                    return null;
                }
            }

            /// <summary>
            /// The time it took the user to recieve a response once they
            /// requested something from the web site.
            /// </summary>
            internal Nullable<TimeSpan> ResponseTime
            {
                get
                {
                    if (BrowserTimeRecieved >= BrowserTimeSent)
                        return BrowserTimeRecieved - BrowserTimeSent;
                    return null;
                }
            }

            /// <summary>
            /// The time it took the user to recieve a completed page
            /// where all rendering has been completed.
            /// </summary>
            internal Nullable<TimeSpan> CompletionTime
            {
                get
                {
                    if (BrowserTimeCompleted >= BrowserTimeSent)
                        return BrowserTimeCompleted - BrowserTimeSent;
                    return null;
                }
            }

            internal double Bandwidth
            {
                get
                {
                    if (ResponseTime.HasValue &&
                        ServerProcessingTime.HasValue)
                        return (RequestLength + ResponseLength) /
                            (ResponseTime.Value - ServerProcessingTime.Value).TotalSeconds;
                    return 0;
                }
            }

            internal bool IsComplete
            {
                get
                {
                    return BrowserTimeSent > DateTime.MinValue &&
                        BrowserTimeRecieved >= BrowserTimeSent &&
                        BrowserTimeCompleted >= BrowserTimeSent &&
                        ServerTimeRecieved > DateTime.MinValue &&
                        ServerTimeSent >= ServerTimeRecieved;
                }
            }

            public int CompareTo(Stat other)
            {
                return Id.CompareTo(other.Id);
            }
        }

        [Serializable]
        private class Stats : List<Stat>
        {
            internal long LastId = 0;

            /// <summary>
            /// Returns the average response time as percieved by the
            /// user for the session.
            /// </summary>
            internal Nullable<TimeSpan> LastResponseTime
            {
                get
                {
                    var last = this.LastOrDefault(i => i.IsComplete);
                    if (last != null)
                    {
                        return last.ResponseTime;
                    }
                    return null;
                }
            }

            /// <summary>
            /// Returns the average response time as percieved by the
            /// user for the session.
            /// </summary>
            internal Nullable<TimeSpan> LastCompletionTime
            {
                get
                {
                    var last = this.LastOrDefault(i => i.IsComplete);
                    if (last != null)
                    {
                        return last.CompletionTime;
                    }
                    return null;
                }
            }

            /// <summary>
            /// Returns the average response time as percieved by the
            /// user for the session.
            /// </summary>
            internal Nullable<TimeSpan> AverageResponseTime
            {
                get
                {
                    var values = this.Where(i =>
                            i.IsComplete).ToArray();
                    if (values.Length > 0)
                    {
                        var average = values.Average(i =>
                                i.ResponseTime.Value.TotalMilliseconds);
                        return new TimeSpan((long)average * TimeSpan.TicksPerMillisecond);
                    }
                    return null;
                }
            }

            internal Nullable<TimeSpan> AverageCompletionTime
            {
                get
                {
                    var values = this.Where(i =>
                            i.IsComplete).ToArray();
                    if (values.Length > 0)
                    {
                        var average = values.Average(i =>
                                i.CompletionTime.Value.TotalMilliseconds);
                        return new TimeSpan((long)average * TimeSpan.TicksPerMillisecond);
                    }
                    return null;
                }
            }
            
            internal int AverageBandwidth
            {
                get
                {
                    var values = this.Where(i =>
                            i.IsComplete).ToArray();
                    if (values.Length > 0)
                    {
                        return (int)values.Average(i =>
                                i.Bandwidth);
                    }
                    return 0;
                }
            }
        }

        #endregion

        #region HttpModule Events

        /// <summary>
        /// Set a property in the application state to quickly determine if bandwidth monitoring
        /// is supported by the data set.
        /// </summary>
        /// <param name="application"></param>
        internal static void Init(HttpApplicationState application)
        {
            if (Configuration.Manager.BandwidthMonitoringEnabled == false ||
                WebProvider.ActiveProvider == null)
            {
                application["51D_Bandwidth"] = new bool?(false);
            }
            else
            {
                var property = WebProvider.ActiveProvider.DataSet.Properties["JavascriptBandwidth"];
                application["51D_Bandwidth"] = new bool?(property != null);
            }
            EventLog.Debug(String.Format("Bandwidth monitoring '{0}'", application["51D_Bandwidth"]));
        }

        /// <summary>
        /// Stores the data and time the request was recieved ready to
        /// be added to the stats counters once the session has been
        /// created for the request.
        /// </summary>
        /// <param name="context"></param>
        internal static void BeginRequest(System.Web.HttpContext context)
        {
            if (GetIsEnabled(context))
            {
                if (context.Items.Contains("51D_Stats_StartTime") == false)
                {
                    context.Items.Add("51D_Stats_StartTime", DateTime.UtcNow);
                }
            }
        }

        /// <summary>
        /// Adds the client javascript reference to the page.
        /// </summary>
        /// <param name="page"></param>
        internal static bool AddScript(System.Web.UI.Page page)
        {
            if (GetIsEnabled(HttpContext.Current))
            {
                page.ClientScript.RegisterClientScriptBlock(
                    typeof(Bandwidth),
                    "FODBW",
                    "<script type=\"text/javascript\">new FODBW();</script>");
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns the javascript for bandwidth monitoring for the
        /// requesting device.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        internal static string GetJavascript(HttpContext context)
        {
            var javascript = GetJavascriptValues(context.Request);
            return javascript != null && javascript.Count == 1 ?
                javascript[0].ToString() : null;
        }

        /// <summary>
        /// Called after the request handler has been executed to 
        /// </summary>
        /// <param name="context"></param>
        internal static void PostAcquireRequestState(HttpContext context)
        {
            if (GetIsEnabled(context))
            {
                var stats = GetStats(context);
                if (stats != null)
                {
                    // Clear any old stats for requests that aren't valid.
                    RemoveOldStats(stats);
                    
                    // If a cookie exists set the appropriate properties for the
                    // previous request.
                    var cookie = Cookie.Create(context);
                    if (cookie != null)
                    {
                        // Place the information from the browser into the stats record 
                        // for the previous page servered.
                        if (cookie.Id == stats.LastId)
                        {
                            var lastStat = stats.FirstOrDefault(i => i.Id == cookie.Id);
                            if (lastStat != null && !lastStat.IsComplete)
                            {
                                lastStat.BrowserTimeRecieved = cookie.BrowserTimeRecieved;
                                lastStat.BrowserTimeCompleted = cookie.BrowserTimeCompleted;
                                lastStat.ResponseLength = cookie.ResponseLength;
                            }
                        }
                        else
                        {
                            // The chain has been broken so remove all old
                            // stats as they're no longer valid.
                            stats.Clear();
                        }
                    }
                    
                    // Set the context items for use by the handler.
                    context.Items["51D_LastResponseTime"] = stats.LastResponseTime;
                    context.Items["51D_LastCompletionTime"] = stats.LastCompletionTime;
                    context.Items["51D_AverageResponseTime"] = stats.AverageResponseTime;
                    context.Items["51D_AverageCompletionTime"] = stats.AverageCompletionTime;
                    context.Items["51D_AverageBandwidth"] = stats.AverageBandwidth;
                }
            }
        }
        
        /// <summary>
        /// Adds the relevent information to the context of the request.
        /// </summary>
        /// <param name="context"></param>
        internal static void PostRequestHandlerExecute(HttpContext context)
        {
            if (GetIsEnabled(context))
            {
                var stats = GetStats(context);
                if (stats != null && context.Response.IsClientConnected)
                {
                    // Create the stats for this request.
                    var stat = new Stat()
                    {
                        ServerTimeRecieved = (DateTime)context.Items["51D_Stats_StartTime"],
                        RequestLength = context.Request.ContentLength
                    };

                    var cookie = Cookie.Create(context);
                    if (cookie != null)
                    {
                        // Record the time the browser sent this request.
                        stat.BrowserTimeSent = cookie.BrowserTimeSent;
                    }

                    // Add the stats for this request.
                    stats.Add(stat);

                    // Set the ID to be expected for the next request.
                    context.Response.Cookies.Add(
                        new HttpCookie(
                            "51D_Bandwidth",
                            stat.EncodedId) { HttpOnly = false, Path = "/" });
                    stats.LastId = stat.Id;

                    // Record the time that we started sending a response.
                    stat.ServerTimeSent = DateTime.UtcNow;
                }
            }
        }

        /// <summary>
        /// Determines if the feature is enabled based on the information
        /// written to the application when initialised.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        internal static bool GetIsEnabled(HttpContext context)
        {
            var enabled = context.Application["51D_Bandwidth"] as bool?;
            if (enabled.HasValue && enabled.Value)
            {
                // If the bandwidth javascript is present for this device then it's enabled.
                var javascript = GetJavascriptValues(context.Request);
                return javascript != null;
            }
            return false;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Returns the javascript for the feature.
        /// </summary>
        /// <param name="request">Request the javascript is needed for</param>
        /// <returns>Javascript to support the feature if present</returns>
        private static Values GetJavascriptValues(HttpRequest request)
        {
            Values values = null;
            var match = WebProvider.GetMatch(request);
            if (match != null)
            {
                var javascript = match["JavascriptBandwidth"];
                if (javascript != null && javascript.Count > 0)
                {
                    values = javascript;
                }
            }
            return values;
        }

        /// <summary>
        /// Retrieves the stats from the session.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private static Stats GetStats(HttpContext context)
        {
            Stats stats = null;
            if (context.Session != null)
            {
                stats = context.Session["51D_Stats"] as Stats;
                if (stats == null)
                {
                    stats = new Stats();
                    context.Session.Add("51D_Stats", stats);
                }
            }
            return stats;
        }

        /// <summary>
        /// Removes any stats information which isn't valid from the session.
        /// </summary>
        /// <param name="stats">List of states for the session.</param>
        private static void RemoveOldStats(Stats stats)
        {
            var expired = DateTime.UtcNow.AddMinutes(-Constants.RequestStatsValidityPeriod).Ticks;
            foreach (var stat in stats.Where(i =>
                i.Id <= expired).ToArray())
            {
                stats.Remove(stat);
            }
        }

        /// <summary>
        /// Looks at the cookies to find the unique id of the previous request.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private static long GetPreviousRequestId(HttpContext context)
        {
            long value = 0;
            var cookie = context.Request.Cookies["51D_Bandwidth"];
            if (long.TryParse(cookie.Value, out value))
                return value;
            return 0;
        }

        #endregion
    }
}
