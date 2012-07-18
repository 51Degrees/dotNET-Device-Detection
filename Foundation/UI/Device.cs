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
using System.Collections.Generic;
using FiftyOne.Foundation.Mobile.Detection;

namespace FiftyOne.Foundation.UI
{
    /// <summary>
    /// Wraps a Foundation device exposing UI specific properties.
    /// </summary>
    public class Device
    {
        #region Constants

        private static readonly string[] UNKNOWN_FIELDS = new string[] {
            "HardwareVendor",
            "HardwareModel",
            "PlatformVendor",
            "PlatformName",
            "BrowserVendor",
            "BrowserName" };

        private static readonly string[] HARDWARE_CAPTION = new string[] {
            "HardwareVendor",
            "HardwareModel" };

        private static readonly string[] SOFTWARE_CAPTION = new string[] {
            "PlatformVendor",
            "PlatformName",
            "PlatformVersion" };

        private static readonly string[] BROWSER_CAPTION = new string[] {
            "BrowserVendor",
            "BrowserName",
            "BrowserVersion" };

        #endregion

        #region Fields

        /// <summary>
        /// The device data properties are based on.
        /// </summary>
        private readonly BaseDeviceInfo _device = null;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a new instance of <see cref="Device"/> based on the
        /// the core base device info passed into the method.
        /// </summary>
        /// <param name="device"></param>
        public Device(BaseDeviceInfo device) 
        {
            if (device == null)
                throw new ArgumentNullException("device");

            _device = device; 
        }

        #endregion

        #region Properties

        /// <summary>
        /// Provides a list of all possible properties.
        /// </summary>
        public SortedList<string, List<string>> Properties
        {
            get { return _device.GetAllProperties(); }
        }

        /// <summary>
        /// Returns true if the device is only available in the 
        /// premium data set.
        /// </summary>
        public bool IsPremium
        {
            get { return _device.IsPremium; }
        }

        /// <summary>
        /// Returns true if the device is one that should be considered unknown.
        /// </summary>
        public bool IsUnknown
        {
            get
            {
                foreach (string key in UNKNOWN_FIELDS)
                    if (_device.GetFirstPropertyValue(key) == "Unknown")
                        return true;
                return false;
            }
        }

        /// <summary>
        /// Returns the caption for the device including only the software and browser 
        /// details.
        /// </summary>
        public string SoftwareBrowserCaption
        {
            get
            {
                return String.Format("{0} & {1}",
                    SoftwareCaption,
                    BrowserCaption);
            }
        }

        /// <summary>
        /// Returns the full caption of the device.
        /// </summary>
        public string Caption
        {
            get
            {
                return String.Format("{0} & {1} & {2}",
                    HardwareCaption,
                    SoftwareCaption,
                    BrowserCaption);
            }
        }

        /// <summary>
        /// Returns the hardware caption for the device.
        /// </summary>
        public string HardwareCaption
        {
            get
            {
                return GetCaption(HARDWARE_CAPTION);
            }
        }

        /// <summary>
        /// Returns the software caption for the device.
        /// </summary>
        public string SoftwareCaption
        {
            get
            {
                return GetCaption(SOFTWARE_CAPTION);
            }
        }

        /// <summary>
        /// Returns the browser caption for the device.
        /// </summary>
        public string BrowserCaption
        {
            get
            {
                return GetCaption(BROWSER_CAPTION);
            }
        }

        /// <summary>
        /// Returns the seperate profile IDs for the device.
        /// </summary>
        public string[] ProfileIDs
        {
            get { return DeviceID.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries); }
        }

        /// <summary>
        /// Returns the name of the device.
        /// </summary>
        public string Name
        {
            get
            {
                return String.Format("{0},{1}",
                    HardwareVendor,
                    HardwareModel);
            }
        }

        /// <summary>
        /// Returns the device ID.
        /// </summary>
        public string DeviceID
        {
            get { return _device.DeviceId; }
        }

        /// <summary>
        /// Returns the hardware vendor.
        /// </summary>
        public string HardwareVendor
        {
            get { return _device.GetFirstPropertyValue("HardwareVendor"); }
        }
        
        /// <summary>
        /// Returns the hardware model.
        /// </summary>
        public string HardwareModel
        {
            get { return _device.GetFirstPropertyValue("HardwareModel"); }
        }

        /// <summary>
        /// Returns the hardware name.
        /// </summary>
        public string HardwareName
        {
            get { return String.Join(", ", _device.GetPropertyValues("HardwareName").ToArray()); }
        }

        /// <summary>
        /// Returns the content suitable for the search system.
        /// </summary>
        public string Content
        {
            get
            {
                List<string> list = new List<string>();
                SortedList<string, List<string>> all = _device.GetAllProperties();
                foreach (string key in all.Keys)
                    if (all[key].Count > 0)
                        list.Add(String.Format("{0} = {1}",
                            key,
                            String.Join(", ", all[key].ToArray())));
                return String.Join("<br/>", list.ToArray());
            }
        }

        #endregion

        #region Private Methods
        
        /// <summary>
        /// Constructs a caption removing any Unknown values.
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        private string GetCaption(string[] keys)
        {
            List<string> list = new List<string>();
            foreach (string key in keys)
            {
                List<string> values = _device.GetPropertyValues(key);
                if (values.Count != 0 &&
                    values.Contains("Unknown") == false)
                    list.AddRange(values);
            }
            if (list.Count == 0)
                return "Unknown";
            return String.Join(" - ", list.ToArray());
        }

        #endregion
    }
}