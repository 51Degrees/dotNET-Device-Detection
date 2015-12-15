using System;
using System.Collections.Generic;
using FiftyOne.Foundation.Mobile.Detection;
using System.Linq;
using System.Web;

namespace MVC.Models
{
    public class Device
    {
        /// <summary>
        /// HTML used if the property has no value.
        /// </summary>
        private const string SWITCH_HTML = "<a href=\"" +
            "https://51degrees.com/compare-data-options\">" +
            "Switch Data Set</a>";

        // Snippet Start
        /// <summary>
        /// Instance of a device detection match to use for device properties.
        /// </summary>
        private readonly Match _match;

        // These properties are fetched only when requested by the view. This 
        // can be a more efficient method of accessing property values as 
        // they're only retrieved when requested.
        public bool IsMobile 
        { 
            get 
            { 
                return _match["IsMobile"] != null ? 
                    _match["IsMobile"].ToBool() : 
                    false; 
            } 
        }
        public int ScreenPixelsHeight 
        { 
            get 
            { 
                return _match["ScreenPixelsHeight"] != null ?
                    (int)_match["ScreenPixelsHeight"].ToDouble() :
                    0; 
            } 
        }
        public int ScreenPixelsWidth
        {
            get
            {
                return _match["ScreenPixelsWidth"] != null ?
                    (int)_match["ScreenPixelsWidth"].ToDouble() :
                    0;
            }
        }

        // These properties are populated in the constructor with
        // 51Degrees properties of the same name. More can be added
        // as long as their name is the same as the 51Degrees
        // property and are string types. More properties
        // are listed at https://51degrees.com/Resources/Property-Dictionary
        public string HardwareVendor { get; private set; }
        public string HardwareModel { get; private set; }
        public string PlatformVendor { get; private set; }
        public string PlatformName { get; private set; }
        public string PlatformVersion { get; private set; }
        public string BrowserVendor { get; private set; }
        public string BrowserName { get; private set; }
        public string BrowserVersion { get; private set; }
        
        /// <summary>
        /// Uses reflection to see if there is a 51Degrees property with the 
        /// same name as a Device class property. This means getting a new 
        /// 51Degrees property in this object only requires creating a 
        /// property with that name.
        /// </summary>
        /// <param name="match">Instance of a device detection match</param>
        internal Device(Match match)
        {
            _match = match;
            foreach (var classProperty in this.GetType().GetProperties().Where(p => 
                p.PropertyType == typeof(string)))
            {
                var values = match[classProperty.Name];
                if (values != null && values.Count > 0)
                {
                    // There is a value for the property. Set the value
                    // now.
                    classProperty.SetValue(this, values.ToString());
                }
                else
                {
                    // Property is not contained in the active 51Degrees
                    // data set. Display a link to switch the data set
                    // and re-run the example.
                    classProperty.SetValue(this, SWITCH_HTML);
                }
            }
        }
        // Snippet End
    }
}