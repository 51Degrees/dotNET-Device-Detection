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

using System.Web;
using System.Collections.Generic;
using System.Collections;
using System;
using FiftyOne.Foundation.Mobile.Detection;
using FiftyOne.Foundation.Mobile.Detection.Wurfl;
using System.Web.Caching;
using System.Reflection;
using System.Web.UI;
using System.IO;
using System.Security.Permissions;
using System.Web.UI.WebControls;

namespace FiftyOne.Foundation.Mobile.Detection
{
    #region Enumerations

    /// <summary>
    /// Provides information about support for dual orientation.
    /// </summary>
    public enum DualOrientation
    {
        /// <summary>
        /// It's not been possible to determine if dual orientation
        /// is supported or noe.
        /// </summary>
        Unknown,
        /// <summary>
        /// Dual orientation is supported.
        /// </summary>
        True,
        /// <summary>
        /// Dual orientation is not supported.
        /// </summary>
        False
    }

    /// <summary>
    /// Pointing methods used if by mobile devices.
    /// </summary>
    public enum PointingMethods
    {
        /// <summary>
        /// Device uses a stylus as a pointing method.
        /// </summary>
        Stylus,
        /// <summary>
        /// Device uses a joystick as a pointing method.
        /// </summary>
        Joystick,
        /// <summary>
        /// Devices uses a click wheel as a pointing method.
        /// </summary>
        Clickwheel,
        /// <summary>
        /// Device uses the old fashioned finger as a pointing method.
        /// </summary>
        Touchscreen,
        /// <summary>
        /// The device does not have a pointing method or it's unknown
        /// which method is supported.
        /// </summary>
        Unknown
    }

    /// <summary>
    /// Audio formats detected by the API.
    /// </summary>
    public enum AudioFormats
    {
        /// <summary>
        /// Supports the rmf sound format (Beatnik format).
        /// </summary>
        Rmf,
        /// <summary>
        /// Supports the Qualcomm Code Excited Linear Predictive waveform format.
        /// </summary>
        Qcelp,
        /// <summary>
        /// AMR wide band standard sound format.
        /// </summary>
        Awb,
        /// <summary>
        /// Supports the smf (Standard MIDI File) sound format
        /// </summary>
        Smf,
        /// <summary>
        /// Supports the .wav (Waveform) sound format.
        /// </summary>
        Wav,
        /// <summary>
        /// Supports the Nokia ringing tone sound format.
        /// </summary>
        NokiaRingtone,
        /// <summary>
        /// AAC standard sound format.
        /// </summary>
        Aac,
        /// <summary>
        /// A compact polyphonic sound format developed by the Digiplug company.
        /// </summary>
        Digiplug,
        /// <summary>
        /// Supports the Scalable Polyphonic MIDI sound format.
        /// </summary>
        SpMidi,
        /// <summary>
        /// Supports the Compact MIDI sound format (a Faith Inc. format).
        /// </summary>
        CompactMidi,
        /// <summary>
        /// Supports the mp3 sound format.
        /// </summary>
        Mp3,
        /// <summary>
        /// An iMode sound format.
        /// </summary>
        Mld,
        /// <summary>
        /// Supports the Enhanced Variable Rate Codec waveform format.
        /// </summary>
        Evrc,
        /// <summary>
        /// AMR standard sound format.
        /// </summary>
        Amr,
        /// <summary>
        /// Supports the XMF sound format (Beatnik format).
        /// </summary>
        Xmf,
        /// <summary>
        /// Supports the MMF (a Yamaha format) sound format.
        /// </summary>
        Mmf,
        /// <summary>
        /// A standard file format for melodies, also adopted as the ringtone format by the 4 companies developing the EMS standard.
        /// </summary>
        IMelody,
        /// <summary>
        /// Supports the midi (Musical Instrument Digital Interface) monophonic sound format.
        /// </summary>
        MidiMonophonic,
        /// <summary>
        /// Also called the uLaw, NeXT, or Sun Audio format.
        /// </summary>
        Au,
        /// <summary>
        /// Supports the midi (Musical Instrument Digital Interface) polyphonic sound format.
        /// </summary>
        MidiPolyphonic
    }

    /// <summary>
    /// WURFL CSS capabilities
    /// </summary>
    public enum CssGroup
    {
        /// <summary>
        /// No value set 
        /// </summary>
        None,
        /// <summary>
        /// Device supports CSS3 standards
        /// </summary>
        CSS3,
        /// <summary>
        /// Device supports Webkit browser engine
        /// </summary>
        WebKit,
        /// <summary>
        /// Device supports Mozilla browser
        /// </summary>
        Mozilla,
        /// <summary>
        /// Device supports Opera browser
        /// </summary>
        Opera
    }

    #endregion

    /// <summary>
    /// Enhanced mobile capabilities assigned to mobile devices.
    /// </summary>
    public abstract class MobileCapabilities : System.Web.Mobile.MobileCapabilities
    {
        #region Constants

        /// <summary>
        /// Key in the capabilities collection to identify the bool value
        /// indicating if the mobile device supports dual orientation.
        /// </summary>
        protected const string DUAL_ORIENTATION = "dualOrientation";

        /// <summary>
        /// Key in the capabilities collection to return the 
        /// <see cref="PointingMethods"/> supported by the device.
        /// </summary>
        protected const string POINTING_METHOD = "pointingMethod";
        
        #endregion

        #region Constructor

        /// <summary>
        /// Creates an instance of the <see cref="MobileCapabilities"/> class using
        /// the list of capabilities provided.
        /// </summary>
        internal MobileCapabilities(IDictionary capabilities)
            : base()
        {
            Capabilities = new Hashtable(capabilities);
        }

        /// <summary>
        /// Creates an instance of the <see cref="MobileCapabilities"/> class using
        /// the useragent to determine the capabilities.
        /// </summary>
        internal MobileCapabilities(string userAgent)
            : base()
        {
            Capabilities = new Hashtable();

            // Assume javascript support is true and rely on the implementing
            // inheritor to provide confirmation.
            SetJavaScript(Capabilities, true);
        }

        /// <summary>
        /// Creates an instance of the <see cref="MobileCapabilities"/> class
        /// initialising all the capabilities to the values of the context provided.
        /// </summary>
        internal MobileCapabilities(HttpContext context)
            : base()
        {
            Capabilities = new Hashtable(context.Request.Browser.Capabilities);
            
            // Get support for javascript from the Accept header.
            SetJavaScript(Capabilities, GetJavascriptSupport(context));
        }

        #endregion

        #region Properties

        /// <summary>
        /// Provides the maximum image width in pixels as an integer.
        /// </summary>
        public abstract int MaxImagePixelsWidth { get; }

        /// <summary>
        /// Provides the maximum image height in pixels as an integer.
        /// </summary>
        public abstract int MaxImagePixelsHeight { get; }

        /// <summary>
        /// Provides details of device support for dual orientation.
        /// </summary>
        public abstract DualOrientation DualOrientation { get; }

        /// <summary>
        /// Returns true if the browser supports the manipulation of
        /// css properties via javascript.
        /// </summary>
        public abstract bool CssManipulation { get; }

        /// <summary>
        /// Returns the physical width of the screen in milimeters.
        /// </summary>
        public abstract int PhysicalScreenWidth { get; }

        /// <summary>
        /// Returns the physical height of the screen in milimeters.
        /// </summary>
        public abstract int PhysicalScreenHeight { get; }

        /// <summary>
        /// Provides the pointing method used by the mobile device.
        /// </summary>
        public abstract PointingMethods PointingMethod { get; }

        /// <summary>
        /// Is the 'Viewport' META tag supported
        /// </summary>
        public abstract bool MetaViewportSupported { get; }

        /// <summary>
        /// Checks the META tag
        /// Check if the device prevent the browser from trying to adapt the page to fit the mobile screen
        /// </summary>
        public abstract bool MetaMobileOptimizedSupported { get; }

        /// <summary>
        /// Checks the META tag
        /// Check if the device prevent the browser from trying to adapt the page to fit the mobile screen
        /// </summary>
        public abstract bool MetaHandHeldFriendlySupported { get; }

        /// <summary>
        /// Checks if the device work when CSS defined as percentage
        /// </summary>
        public abstract bool CssSupportsWidthAsPercentage { get; }

        /// <summary>
        /// Check if the device can refer to pictures and use them in different circimstances as backgounds.
        /// </summary>
        public abstract bool CssSpriting { get; }

        /// <summary>
        /// Check which Css group provides gradient support for Css
        /// </summary>
        public abstract CssGroup CssGradientSupport { get; }

        /// <summary>
        /// Check which Css group provides border image support for Css
        /// </summary>
        public abstract CssGroup CssBorderImageSupport { get; }

        /// <summary>
        /// Check which Css group provides rounded corner support for Css
        /// </summary>
        public abstract CssGroup CssRoundedCornerSupport { get; }

        /// <summary>
        /// Returns true if the audio format is supported by the device.
        /// </summary>
        /// <param name="format">Audio format to query.</param>
        /// <returns>True if supported. False if not.</returns>
        internal abstract bool IsAudioFormatSupported(AudioFormats format);

        #endregion
        
        #region Methods

        /// <summary>
        /// Called when the class is initialised. Sets the correct text
        /// writer based on the preferred rendering type of the browser.
        /// </summary>
        protected virtual void Init()
        {
            // Set the tagwriter.
            Capabilities["tagwriter"] = GetTagWriter();
        }

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
                foreach (string checkType in Constants.JAVASCRIPT_ACCEPTS)
                    foreach(string acceptType in acceptTypes)
                        if (acceptType.StartsWith(checkType, StringComparison.InvariantCultureIgnoreCase) == true)
                            return true;
            }
            return false;
        }
        
        #endregion

        #region Static Methods

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
            if (value is string && String.IsNullOrEmpty(((string)value)) == true)
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
            EventLog.Debug(String.Format("Setting '{0}' to '{1}'", key, value != null ? value.ToString() : "null"));
            #endif
        }


        private string GetTagWriter()
        {
            switch (Capabilities["preferredRenderingType"] as string)
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