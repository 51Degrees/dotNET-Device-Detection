/* *********************************************************************
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited. 
 * Copyright © 2015 51Degrees Mobile Experts Limited, 5 Charlotte Close,
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
using FiftyOne.Foundation.Mobile.Detection.Entities;
using System.Linq;
using FiftyOne.Foundation.Mobile.Detection;
using System.Web;

namespace FiftyOne.Foundation.UI
{
    // Disable obsolete warnings.
    #pragma warning disable 0618

    /// <summary>
    /// Static class providing UI optimised methods for accessing device data.
    /// </summary>
    [Obsolete("DataProvider is deprecated. All data access should use DataSet.")]
    public static class DataProvider
    {
        #region Fields

        // Lock used when loading the vendors.
        private static readonly object _lock = new object();

        // A list of hardware vendors and the number of devices assigned to each.
        private static SortedList<Value, List<Device>> _vendors = null;
        private static List<Device> _devices;

        #endregion

        #region Properties

        /// <summary>
        /// Returns the active provider for the factory.
        /// </summary>
        [Obsolete("Use WebProvider ActiveProvider property")]
        public static Provider Provider
        {
            get { return WebProvider.ActiveProvider; }
        }

        /// <summary>
        /// Returns a list of those properties that relate to the device hardware.
        /// </summary>
        [Obsolete("DataProvider is deprecated. All data access should use DataSet.")]
        public static IList<Property> HardwareProperties
        {
            get
            {
                return Provider.DataSet.Hardware.Properties.Where(i =>
                    i.Values != null &&
                    i.Values.Count > 0).OrderBy(i => i.Name).ToList();
            }
        }

        /// <summary>
        /// Returns a list of all the available properties.
        /// </summary>
        [Obsolete("DataProvider is deprecated. All data access should use DataSet.")]
        public static IList<Property> Properties
        {
            get
            {
                return Provider.DataSet.Properties.Where(i =>
                    i.Values != null &&
                    i.Values.Count > 0).OrderBy(i => i.Name).ToList();
            }
        }

        /// <summary>
        /// Returns a list of the properties that relate to the software.
        /// </summary>
        [Obsolete("DataProvider is deprecated. All data access should use DataSet.")]
        public static IList<Property> SoftwareProperties
        {
            get
            {
                return Provider.DataSet.Software.Properties.Where(i =>
                    i.Values != null &&
                    i.Values.Count > 0).OrderBy(i => i.Name).ToList();
            }
        }

        /// <summary>
        /// Returns a list of the properties that relate to the browser.
        /// </summary>
        [Obsolete("DataProvider is deprecated. All data access should use DataSet.")]
        public static IList<Property> BrowserProperties
        {
            get
            {
                return Provider.DataSet.Browsers.Properties.Where(i =>
                    i.Values != null &&
                    i.Values.Count > 0).OrderBy(i => i.Name).ToList();
            }
        }

        /// <summary>
        /// Returns a list of the properties that relate to the content.
        /// </summary>
        [Obsolete("DataProvider is deprecated. All data access should use DataSet.")]
        public static IList<Property> ContentProperties
        {
            get
            {
                return Provider.DataSet.Properties.Where(i =>
                  Constants.Content.Contains(i.Name) &&
                  i.Values != null &&
                  i.Values.Count > 0).OrderBy(i => i.Name).ToList();
            }
        }

        /// <summary>
        /// Returns true if CMS data is being used by the active provider.
        /// </summary>
        [Obsolete("DataProvider is deprecated. All data access should use DataSet.")]        
        public static bool IsCms
        {
            get
            {
                // Try the new data set name property first.
                if (Provider.DataSet.Name.Equals("CMS"))
                    return true;

                return IsPremium == false &&
                    (from i in Provider.DataSet.Properties select i.Name).Intersect(UI.Constants.CMS).Count() == UI.Constants.CMS.Length;
            }
        }

        /// <summary>
        /// Returns true if premium data is being used by the active provider. The number
        /// of available properties is used to determine if this is true.
        /// </summary>
        [Obsolete("DataProvider is deprecated. All data access should use DataSet.")]
        public static bool IsPremium
        {
            get
            {
                return Provider.DataSet.Name == "Premium" || Provider.DataSet.Name == "Ultimate";
            }
        }

        /// <summary>
        /// Returns a list of all the devices.
        /// </summary>
        [Obsolete("DataProvider is deprecated. All data access should use DataSet.")]
        public static List<Device> Devices
        {
            get
            {
                if (_devices == null)
                {
                    lock (_lock)
                    {
                        if (_devices == null)
                        {
                            _devices = new List<Device>();
                            foreach (KeyValuePair<Value, List<Device>> vendor in Vendors)
                                _devices.AddRange(vendor.Value);
                        }
                    }
                }
                return _devices;
            }
        }

        /// <summary>
        /// Returns a list of the available hardware vendors.
        /// </summary>
        [Obsolete("DataProvider is deprecated. All data access should use DataSet.")]
        public static SortedList<Value, List<Device>> Vendors
        {
            get
            {
                if (_vendors == null)
                {
                    lock (_lock)
                    {
                        if (_vendors == null)
                        {
                            _vendors = new SortedList<Value, List<Device>>();
                            var property = Provider.DataSet.Hardware.Properties.FirstOrDefault(i =>
                                i.Name == "HardwareVendor");
                            if (property != null)
                            {
                                foreach (var value in property.Values.Where(i =>
                                    i.Name != "Unknown" &&
                                    i.Signatures.Length > 0))
                                    _vendors.Add(value, GetVendorDevices(value));
                            }
                        }
                    }
                }
                return _vendors;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns true if the property is only available in the premium data.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        [Obsolete("DataProvider is deprecated. All data access should use DataSet.")]
        public static bool GetIsPremium(Property property)
        {
            return "Lite".Equals(WebProvider.ActiveProvider.DataSet.Name) == false;
        }

        /// <summary>
        /// Returns the device from the user agent.
        /// </summary>
        /// <param name="userAgent"></param>
        /// <returns></returns>
        [Obsolete("DataProvider is deprecated. All data access should use DataSet.")]
        public static Device GetDevice(string userAgent)
        {
            var match = Provider.Match(userAgent);
            if (match != null)
            {
                var profile = match.Profiles.FirstOrDefault(i =>
                    i.Component.ComponentId == i.DataSet.Hardware.ComponentId);
                if (profile != null)
                    return new Device(profile);
            }   
            return null;
        }

        /// <summary>
        /// Returns the device ID for the device matching the user agent provided.
        /// </summary>
        /// <param name="userAgent"></param>
        /// <returns></returns>
        [Obsolete("DataProvider is deprecated. All data access should use DataSet.")]
        public static string GetDeviceID(string userAgent)
        {
            var match = Provider.Match(userAgent);
            if (match != null)
                return match.DeviceId;
            return null;
        }

        /// <summary>
        /// Get the property for the name provided.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [Obsolete("DataProvider is deprecated. All data access should use DataSet.")]
        public static Property GetProperty(string name)
        {
            return Provider.DataSet.Properties.FirstOrDefault(i =>
                i.Name == name);
        }

        /// <summary>
        /// Returns properties based on the unique ID of the device.
        /// </summary>
        /// <param name="deviceID"></param>
        /// <returns></returns>
        [Obsolete("DataProvider is deprecated. All data access should use DataSet.")]
        public static Device GetDeviceFromDeviceID(string deviceID)
        {
            var device = Provider.DataSet.Signatures.FirstOrDefault(i => deviceID.Equals(i.DeviceId));
            if (device != null)
                return new Device(device);
            return null;
        }

        /// <summary>
        /// Returns the first device which contains the profile ID passed.
        /// </summary>
        /// <param name="profileID"></param>
        /// <returns></returns>
        [Obsolete("DataProvider is deprecated. All data access should use DataSet.")]
        public static Device GetDeviceFromProfileID(string profileID)
        {
            int profileValue;

            if (int.TryParse(profileID, out profileValue))
            {
                // Try the profiles without unknown software and browsers first.
                var profile = Provider.DataSet.FindProfile(profileValue);

                // If not found drop the requirement for no unknown browsers or software.
                if (profile == null)
                    return new Device(profile);
            }
            return null;
        }

        /// <summary>
        /// Returns a device based on the hardware vendor and model seperated by a pipe sign.
        /// </summary>
        /// <param name="vendor"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [Obsolete("DataProvider is deprecated. All data access should use DataSet.")]
        public static Device GetDeviceFromModel(string vendor, string model)
        {
            // Try the profiles without unknown software and browsers first.
            Profile profile = Provider.DataSet.Hardware.Profiles.FirstOrDefault(i =>
                i["HardwareVendor"] != null && i["HardwareVendor"].Equals(vendor) &&
                i["HardwareModel"] != null && i["HardwareModel"].Equals(model));

            // The device does not exist.
            if (profile != null)
                return new Device(profile);
            return null;            
        }

        /// <summary>
        /// Get any device which has the same hardware model as the one provided.
        /// </summary>
        /// <param name="device">The device which others should relate to.</param>
        /// <returns>A list of related devices.</returns>
        [Obsolete("DataProvider is deprecated. All data access should use DataSet.")]
        public static List<Device> GetRelatedInfo(Device device)
        {
            List<Device> list = null;
            if (device.HardwareModel != null)
            {
                var profile = Provider.DataSet.Hardware.Profiles.FirstOrDefault(i =>
                    i["HardwareModel"] != null && i["HardwareModel"].Equals(device.HardwareModel));
                if (profile != null)
                {
                    list = profile.Signatures.Select(i => new Device(i)).ToList();
                }
            }
            list.Sort(CompareDeviceNames);
            return list;
        }

        /// <summary>
        /// Returns a list of devices that match the search value provided.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [Obsolete("DataProvider is deprecated. All data access should use DataSet.")]
        public static List<Device> FindDevices(string value)
        {
            return Devices.Where(i =>
                (i.HardwareVendor != null && i.HardwareVendor.IndexOf(value, StringComparison.InvariantCultureIgnoreCase) >= 0) ||
                (i.HardwareModel != null && i.HardwareModel.IndexOf(value, StringComparison.InvariantCultureIgnoreCase) >= 0) ||
                (i.HardwareName != null && i.HardwareName.IndexOf(value, StringComparison.InvariantCultureIgnoreCase) >= 0) ||
                (i.SoftwareCaption != null && i.SoftwareCaption.IndexOf(value, StringComparison.InvariantCultureIgnoreCase) >= 0) ||
                (i.BrowserCaption != null && i.BrowserCaption.IndexOf(value, StringComparison.InvariantCultureIgnoreCase) >= 0)).ToList<Device>();
        }

        /// <summary>
        /// Determines if the two devices are identical.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private static bool CompareRelatedDevices(Device x, Device y)
        {
            return x.HardwareVendor == y.HardwareVendor &&
                x.IsUnknown == false &&
                String.IsNullOrEmpty(x.SoftwareBrowserCaption) == false &&
                x.DeviceID != y.DeviceID;
        }

        /// <summary>
        /// Compares two different devices by name and returns the sort order
        /// for the list.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        [Obsolete("DataProvider is deprecated. All data access should use DataSet.")]
        public static int CompareDeviceNames(Device x, Device y)
        {
            return x.SoftwareBrowserCaption.CompareTo(y.SoftwareBrowserCaption);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Returns a list of the hardware devices for the vendor requested.
        /// </summary>
        /// <param name="vendor"></param>
        /// <returns></returns>
        private static List<Device> GetVendorDevices(Value vendor)
        {
            var list = new List<Device>();
            foreach (var signature in vendor.Signatures)
            {
                var device = new Device(signature);
                if (list.FirstOrDefault(i =>
                    i.HardwareModel == device.HardwareModel) == null)
                    list.Add(device);
            }
            return list.OrderBy(i => i.HardwareModel).ToList();
        }

        #endregion
    }
}
