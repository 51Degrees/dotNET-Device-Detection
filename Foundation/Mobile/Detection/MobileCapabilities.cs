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
 * Mobile Experts Limited are Copyright (C) 2009 - 2010. All Rights Reserved.
 * 
 * Contributor(s):
 *     James Rosewell <james@51degrees.mobi>
 * 
 * ********************************************************************* */

#if VER4
using System.Linq;
#endif

#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;

#endregion

namespace FiftyOne.Foundation.Mobile.Detection
{
    /// <summary>
    /// Enhanced mobile capabilities assigned to mobile devices.
    /// </summary>
    internal abstract class MobileCapabilities
    {
        #region Configuration Values

        /// <summary>
        /// When set to true tablet devices will be treated as mobile devices for
        /// the purposes of redirection.
        /// </summary>
        protected bool _treatTabletAsMobile = true;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs MobileCapabilities setting the treat tablet as mobile 
        /// configuration field if redirect is enabled.
        /// </summary>
        internal MobileCapabilities()
        {
            if (Configuration.Manager.Redirect != null)
                _treatTabletAsMobile = Configuration.Manager.Redirect.TreatTabletAsMobile;
        }

        #endregion

        #region Create Methods

        /// <summary>
        /// Creates a new set of capabilities based on the context provided.
        /// </summary>
        /// <param name="context">HttpContext of the device.</param>
        /// <returns>A dictionary of capabilities.</returns>
        internal virtual IDictionary Create(HttpContext context)
        {
            // Create the mobile capabilities hashtable.
            IDictionary capabilities = new Hashtable();

            // Get support for javascript from the Accept header.
            SetJavaScript(capabilities, GetJavascriptSupport(context));

            return capabilities;
        }

        /// <summary>
        /// Creates a new set of capabilities based on the userAgent string provided.
        /// </summary>
        /// <param name="userAgent">UserAgent string of the device.</param>
        /// <returns>A dictionary of capabilities.</returns>
        internal virtual IDictionary Create(string userAgent)
        {
            // Create the mobile capabilities hashtable.
            return new Hashtable();
        }

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Returns true if the requesting context is associated with a device
        /// that should be redirected.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        internal abstract bool IsRedirectDevice(HttpContext context);

        #endregion

        #region Virtual Methods

        /// <summary>
        /// Initialises the IDictionary of capabilities.
        /// </summary>
        protected virtual void Init(IDictionary capabilities)
        {
            // Set the tagwriter.
            capabilities["tagwriter"] = GetTagWriter(capabilities);
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Sets the javascript boolean string in the capabilities dictionary.
        /// </summary>
        /// <param name="capabilities">Capabilities dictionary.</param>
        /// <param name="javaScript">The value of the jaavscript keys.</param>
        protected static void SetJavaScript(IDictionary capabilities, bool javaScript)
        {
            SetValue(capabilities, "javascript", javaScript.ToString().ToLowerInvariant());
            SetValue(capabilities, "Javascript", javaScript.ToString().ToLowerInvariant());
        }

        /// <summary>
        /// Checks the Accepts header of the request to determine if javascript is supported.
        /// </summary>
        /// <param name="context">Context of the request.</param>
        /// <returns></returns>
        private static bool GetJavascriptSupport(HttpContext context)
        {
            // Check the headers to see if javascript is supported.
            if (context.Request.AcceptTypes != null)
            {
                List<string> acceptTypes = new List<string>(context.Request.AcceptTypes);
#if VER2
                foreach (string checkType in Constants.JAVASCRIPT_ACCEPTS)
                    foreach (string acceptType in acceptTypes)
                        if (acceptType.StartsWith(checkType, StringComparison.InvariantCultureIgnoreCase))
                            return true;
#elif VER4
                return (from checkType in Constants.JAVASCRIPT_ACCEPTS
                    from acceptType in acceptTypes
                    where acceptType.StartsWith(checkType, StringComparison.InvariantCultureIgnoreCase) == true
                    select checkType).Any();
#endif
            }
            return false;
        }

        /// <summary>
        /// Sets the key in the capabilities dictionary to the object provided. If the key 
        /// already exists the previous value is replaced. If not a new entry is added
        /// to the Dictionary.
        /// </summary>
        /// <param name="capabilities">Dictionary of capabilities to be changed.</param>
        /// <param name="key">Key to be changed or added.</param>
        /// <param name="value">New entry value.</param>
        internal static void SetValue(IDictionary capabilities, string key, object value)
        {
            // Ignore new values that are empty strings.
            if (value is string && String.IsNullOrEmpty(((string) value)))
                return;

            // Change or add the new capability.
            if (capabilities.Contains(key) == false)
            {
                capabilities.Add(key, value);
            }
            else
            {
                capabilities[key] = value;
            }

#if DEBUG
            EventLog.Debug(String.Format("Setting '{0}' to '{1}'.", key, value != null ? value.ToString() : "null"));
#endif
        }

        /// <summary>
        /// Returns the class to use as a text writer for the output stream.
        /// </summary>
        /// <param name="capabilities">Dictionary of device capabilities.</param>
        /// <returns>A string containing the text writer class name.</returns>
        private static string GetTagWriter(IDictionary capabilities)
        {
            switch (capabilities["preferredRenderingType"] as string)
            {
                case "xhtml-mp":
                case "xhtml-basic":
                    return "System.Web.UI.XhtmlTextWriter";

                case "chtml10":
                    return "System.Web.UI.ChtmlTextWriter";

                case "html4":
                    return "System.Web.UI.HtmlTextWriter";

                case "html32":
                    return "System.Web.UI.Html32TextWriter";

                default:
                    return "System.Web.UI.Html32TextWriter";
            }
        }

        #endregion
    }
}