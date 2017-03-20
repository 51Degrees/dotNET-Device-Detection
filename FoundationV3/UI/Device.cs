/* *********************************************************************
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited. 
 * Copyright © 2017 51Degrees Mobile Experts Limited, 5 Charlotte Close,
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
 * This Source Code Form is “Incompatible With Secondary Licenses”, as
 * defined by the Mozilla Public License, v. 2.0.
 * ********************************************************************* */

using System;
using System.Collections.Generic;
using FiftyOne.Foundation.Mobile.Detection;
using System.Linq;
using FiftyOne.Foundation.Mobile.Detection.Entities;

namespace FiftyOne.Foundation.UI
{
    // Disable obsolete warnings.
    #pragma warning disable 0618

    /// <summary>
    /// Wraps a Foundation device exposing UI specific properties.
    /// </summary>
    [Obsolete("Device is deprecated. All data access should use DataSet.")]
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
        private readonly Signature _signature = null;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a new instance of <see cref="Device"/> based on the
        /// the first signature of the profile passed into the method.
        /// </summary>
        /// <param name="profile"></param>
        [Obsolete("Device is deprecated. All data access should use DataSet.")]
        public Device(Profile profile) 
        {
            if (profile == null)
                throw new ArgumentNullException("profile");
            _signature = profile.Signatures.First(); 
        }

        /// <summary>
        /// Constructs a new instance of <see cref="Device"/> based on the
        /// the core signature passed into the method.
        /// </summary>
        /// <param name="signature"></param>
        [Obsolete("Device is deprecated. All data access should use DataSet.")]
        public Device(Signature signature)
        {
            if (signature == null)
                throw new ArgumentNullException("signature");
            _signature = signature;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Provides a list of all possible properties.
        /// </summary>
        [Obsolete("Device is deprecated. All data access should use DataSet.")]
        public SortedList<string, List<string>> Properties
        {
            get 
            {
                return GetPropertyValuesAsStrings();
            }
        }

        /// <summary>
        /// Returns true if the <see cref="Device"/> is one that should be
        /// considered unknown.
        /// </summary>
        /// <returns>
        /// True if the <see cref="Device"/> should be considered
        /// unknown</returns>
        [Obsolete("Device is deprecated. All data access should use DataSet.")]
        public bool IsUnknown
        {
            get
            {
                return _signature.Values.Any(i =>
                    i.Property.DisplayOrder > 0 &&
                    i == i.Property.DefaultValue);
            }
        }

        /// <summary>
        /// Returns the caption for the <see cref="Device"/> including only
        /// the software and browser 
        /// details.
        /// </summary>
        /// <returns>
        /// The software and browser caption.</returns>
        [Obsolete("Device is deprecated. All data access should use DataSet.")]
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
        /// Returns the full caption of the <see cref="Device"/>.
        /// </summary>
        /// <returns>
        /// The full caption of the <see cref="Device"/></returns>
        [Obsolete("Device is deprecated. All data access should use DataSet.")]
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
        /// Returns the hardware caption for the <see cref="Device"/>.
        /// </summary>
        /// <returns>
        /// The hardware caption.</returns>
        [Obsolete("Device is deprecated. All data access should use DataSet.")]
        public string HardwareCaption
        {
            get
            {
                return GetCaption(HARDWARE_CAPTION);
            }
        }

        /// <summary>
        /// Returns the software caption for the <see cref="Device"/>.
        /// </summary>
        /// <returns>
        /// The software caption.</returns>
        [Obsolete("Device is deprecated. All data access should use DataSet.")]
        public string SoftwareCaption
        {
            get
            {
                return GetCaption(SOFTWARE_CAPTION);
            }
        }

        /// <summary>
        /// Returns the browser caption for the <see cref="Device"/>.
        /// </summary>
        /// <return>
        /// The browser caption.</return>
        [Obsolete("Device is deprecated. All data access should use DataSet.")]
        public string BrowserCaption
        {
            get
            {
                return GetCaption(BROWSER_CAPTION);
            }
        }

        /// <summary>
        /// Returns the seperate profile IDs for the <see cref="Device"/>.
        /// </summary>
        /// <returns>
        /// The separate profile ids for the
        /// <see cref="Device"/>.</returns>
        [Obsolete("Device is deprecated. All data access should use DataSet.")]
        public string[] ProfileIDs
        {
            get { return DeviceID.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries); }
        }

        /// <summary>
        /// Returns the name of the device.
        /// </summary>
        [Obsolete("Device is deprecated. All data access should use DataSet.")]
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
        /// Returns the signature as a string related to the <see cref="Device"/>.
        /// </summary>
        /// <returns>
        /// The signature as a string.</returns>
        [Obsolete("Device is deprecated. All data access should use DataSet.")]
        public string UserAgent
        {
            get { return _signature.ToString(); }
        }

        /// <summary>
        /// Returns the device ID.
        /// </summary>
        /// <returns>
        /// The device id.</returns>
        [Obsolete("Device is deprecated. All data access should use DataSet.")]
        public string DeviceID
        {
            get { return _signature.DeviceId; }
        }

        /// <summary>
        /// Returns the hardware vendor.
        /// </summary>
        /// <returns>
        /// The hardware vendor.</returns>
        [Obsolete("Device is deprecated. All data access should use DataSet.")]
        public string HardwareVendor
        {
            get 
            { 
                var value = _signature.Values.FirstOrDefault(i =>
                    i.Property.Name == "HardwareVendor");
                if (value != null)
                    return value.Name;
                return null;
            }
        }
        
        /// <summary>
        /// Returns the hardware model.
        /// </summary>
        /// <returns>
        /// The hardware model.</returns>
        [Obsolete("Device is deprecated. All data access should use DataSet.")]
        public string HardwareModel
        {
            get
            {
                var value = _signature.Values.FirstOrDefault(i =>
                    i.Property.Name == "HardwareModel");
                if (value != null)
                    return value.Name;
                return null;
            }
        }

        /// <summary>
        /// Returns the hardware name.
        /// </summary>
        /// <returns>
        /// The hardware name.</returns>
        [Obsolete("Device is deprecated. All data access should use DataSet.")]
        public string HardwareName
        {
            get { return String.Join(", ", _signature.Values.Where(i => 
                i.Property.Name == "HardwareName").Select(i => 
                    i.Name).ToArray()); }
        }

        /// <summary>
        /// Returns the content suitable for the search system.
        /// </summary>
        /// <returns>
        /// The content suitable for the search system.</returns>
        [Obsolete("Device is deprecated. All data access should use DataSet.")]
        public string Content
        {
            get
            {
                List<string> list = new List<string>();
                SortedList<string, List<string>> all = GetPropertyValuesAsStrings();
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
        /// Returns the properties and values for the signature.
        /// </summary>
        [Obsolete("Device is deprecated. All data access should use DataSet.")]
        public SortedList<string, List<string>> GetPropertyValuesAsStrings()
        {
            var results = new SortedList<string, List<string>>();

            // Add the properties and values first.
            foreach (var profile in _signature.Profiles.Where(i => i != null))
            {
                foreach (var property in profile.Properties)
                {
                    results.Add(
                        property.Name,
                        new List<string>(profile[property].ToStringArray()));
                }
            }

            return results;
        }

        /// <summary>
        /// Constructs a caption removing any Unknown values.
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        private string GetCaption(string[] keys)
        {
            var all = GetPropertyValuesAsStrings();
            List<string> list = new List<string>();
            foreach (var values in keys.Select(i => all[i]))
            {
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