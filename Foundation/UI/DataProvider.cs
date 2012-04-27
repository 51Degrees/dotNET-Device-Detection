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

#if VER4 || VER35

using System.Linq;

#endif

namespace FiftyOne.Foundation.UI
{
    /// <summary>
    /// Static class providing UI optimised methods for accessing device data.
    /// </summary>
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
        public static Provider Provider
        {
            get { return Factory.ActiveProvider; }
        }

        /// <summary>
        /// Returns a list of those properties that relate to the device hardware.
        /// </summary>
        public static IList<Property> HardwareProperties
        {
            get
            {
#if VER4 || VER35
                return Provider.Properties.Where(i =>
                    Constants.Hardware.Contains(i.Name) &&
                    i.Values.Count > 0).ToList();
#else
                return GetProperties(Provider.Properties, Constants.Hardware);
#endif
            }
        }

        /// <summary>
        /// Returns a list of the properties that relate to the software.
        /// </summary>
        public static IList<Property> SoftwareProperties
        {
            get
            {
#if VER4 || VER35
                return Provider.Properties.Where(i =>
                  Constants.Software.Contains(i.Name) &&
                  i.Values.Count > 0).ToList();
#else
                return GetProperties(Provider.Properties, Constants.Software);
#endif
            }
        }

        /// <summary>
        /// Returns a list of the properties that relate to the browser.
        /// </summary>
        public static IList<Property> BrowserProperties
        {
            get
            {
#if VER4 || VER35
                return Provider.Properties.Where(i =>
                  Constants.Browser.Contains(i.Name) &&
                  i.Values.Count > 0).ToList();
#else
                return GetProperties(Provider.Properties, Constants.Browser);
#endif
            }
        }

        /// <summary>
        /// Returns a list of the properties that relate to the content.
        /// </summary>
        public static IList<Property> ContentProperties
        {
            get
            {
#if VER4 || VER35
                return Provider.Properties.Where(i =>
                  Constants.Content.Contains(i.Name) &&
                  i.Values.Count > 0).ToList();
#else
                return GetProperties(Provider.Properties, Constants.Content);
#endif
            }
        }

        /// <summary>
        /// Returns true if premium data is being used by the active provider. The number
        /// of available properties is used to determine if this is true.
        /// </summary>
        public static bool IsPremium
        {
            get { return Provider.Properties.Count > 90; }
        }

        /// <summary>
        /// Returns a list of all the devices.
        /// </summary>
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
                            foreach (var vendor in Vendors)
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
#if VER4 || VER35
                            var property = Provider.Properties.FirstOrDefault(i =>
                                i.Name == "HardwareVendor");
                            if (property != null)
                            {
                                foreach (var value in property.Values.Where(i =>
                                    i.Name != "Unknown" &&
                                    i.Devices.Count > 0))
                                    _vendors.Add(value, GetVendorDevices(value));
                            }
#else
                            var property = GetFirstWhereNameEquals(Provider.Properties, "HardwareVendor");
                            if (property != null)
                            {
                                foreach (var value in property.Values)
                                    if (value.Name != "Unknown" &&
                                        value.Devices.Count > 0)
                                        _vendors.Add(value, GetVendorDevices(value));
                            }
#endif
                        }
                    }
                }
                return _vendors;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns the device ID for the device matching the user agent provided.
        /// </summary>
        /// <param name="userAgent"></param>
        /// <returns></returns>
        public static string GetDeviceID(string userAgent)
        {
            var device = Provider.GetDeviceInfo(userAgent);
            if (device != null)
                return device.DeviceId;
            return null;
        }

        /// <summary>
        /// Get the property for the name provided.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Property GetProperty(string name)
        {
#if VER4 || VER35
            return Factory.ActiveProvider.Properties.FirstOrDefault(i =>
                i.Name == name);
#else
            return GetFirstWhereNameEquals(Factory.ActiveProvider.Properties, name);
#endif
        }

        /// <summary>
        /// Returns properties based on the unique ID of the device.
        /// </summary>
        /// <param name="deviceID"></param>
        /// <returns></returns>
        public static Device GetDeviceFromDeviceID(string deviceID)
        {
            return new Device(Provider.GetDeviceInfoByID(deviceID));
        }

        /// <summary>
        /// Returns the first device which contains the profile ID passed.
        /// </summary>
        /// <param name="profileID"></param>
        /// <returns></returns>
        public static Device GetDeviceFromProfileID(string profileID)
        {
            BaseDeviceInfo baseDevice = null;

            // Try the profiles without unknown software and browsers first.
#if VER4 || VER35
            baseDevice = Provider.FindDevices(profileID).FirstOrDefault(i =>
                i.ProfileIDs[0] == profileID &&
                new Device(i).SoftwareBrowserCaption.Contains("Unknown") == false);
#else
            foreach (var device in Provider.FindDevices(profileID))
            {
                if (device.ProfileIDs[0] == profileID &&
                    new Device(device).Caption.Contains("Unknown") == false)
                {
                    baseDevice = device;
                    break;
                }
            }
#endif

            // If not found drop the requirement for no unknown browsers or software.
            if (baseDevice == null)
#if VER4 || VER35
                baseDevice = Provider.FindDevices(profileID).FirstOrDefault(i =>
                i.ProfileIDs[0] == profileID);
#else
                foreach (var device in Provider.FindDevices(profileID))
                {
                    if (device.ProfileIDs[0] == profileID)
                    {
                        baseDevice = device;
                        break;
                    }
                }
#endif

            return new Device(baseDevice);
        }

        /// <summary>
        /// Returns a device based on the hardware vendor and model seperated by a pipe sign.
        /// </summary>
        /// <param name="vendor"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public static Device GetDeviceFromModel(string vendor, string model)
        {
            BaseDeviceInfo device = null;
            // Try the profiles without unknown software and browsers first.
#if VER4 || VER35
            device = Provider.Devices.FirstOrDefault(i =>
                new Device(i).HardwareVendor == vendor &&
                new Device(i).HardwareModel == model &&
                new Device(i).SoftwareBrowserCaption.Contains("Unknown") == false);
#else
            foreach (var item in Provider.Devices)
            {
                var newDevice = new Device(item);
                if (newDevice.HardwareVendor == vendor &&
                    newDevice.HardwareModel == model &&
                    newDevice.Caption.Contains("Unknown") == false)
                {
                    device = item;
                    break;
                }
            }
#endif

            // If not found drop the requirement for no unknown browsers or software.
#if VER4 || VER35
            if (device == null)
                device = Provider.Devices.FirstOrDefault(i =>
                new Device(i).HardwareVendor == vendor &&
                new Device(i).HardwareModel == model);
#else
            foreach (var item in Provider.Devices)
            {
                var newDevice = new Device(item);
                if (newDevice.HardwareVendor == vendor &&
                    newDevice.HardwareModel == model)
                {
                    device = item;
                    break;
                }
            }
#endif
            // The device does not exist.
            if (device == null)
                return null;

            return new Device(device);
        }

        /// <summary>
        /// Get any device which has the same hardware model as the one provided.
        /// </summary>
        /// <param name="device">The device which others should relate to.</param>
        /// <returns>A list of related devices.</returns>
        public static List<Device> GetRelatedInfo(Device device)
        {
            var list = new List<Device>();
            if (device.HardwareModel != null)
            {
#if VER4 || VER35
                foreach (var item in Provider.FindDevices("HardwareModel", device.HardwareModel).Where(i =>
                    CompareRelatedDevices(new Device(i), device)))
                    list.Add(new Device(item));
#else
                foreach (var item in Provider.FindDevices("HardwareModel", device.HardwareModel))
                {
                    var newDevice = new Device(item);
                    if (CompareRelatedDevices(newDevice, device))
                        list.Add(newDevice);
                }
#endif
            }
            list.Sort(CompareDeviceNames);
            return list;
        }

        /// <summary>
        /// Returns a list of devices that match the search value provided.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static List<Device> FindDevices(string value)
        {
#if VER4 || VER35
            return Devices.Where(i =>
                (i.HardwareVendor != null && i.HardwareVendor.IndexOf(value, StringComparison.InvariantCultureIgnoreCase) >= 0) ||
                (i.HardwareModel != null && i.HardwareModel.IndexOf(value, StringComparison.InvariantCultureIgnoreCase) >= 0) ||
                (i.HardwareName != null && i.HardwareName.IndexOf(value, StringComparison.InvariantCultureIgnoreCase) >= 0) ||
                (i.SoftwareCaption != null && i.SoftwareCaption.IndexOf(value, StringComparison.InvariantCultureIgnoreCase) >= 0) ||
                (i.BrowserCaption != null && i.BrowserCaption.IndexOf(value, StringComparison.InvariantCultureIgnoreCase) >= 0)).ToList<Device>();
#else
            var list = new List<Device>();
            foreach (var item in Devices)
            {
                if ((item.HardwareVendor != null && item.HardwareVendor.IndexOf(value, StringComparison.InvariantCultureIgnoreCase) >= 0) ||
                    (item.HardwareModel != null && item.HardwareModel.IndexOf(value, StringComparison.InvariantCultureIgnoreCase) >= 0) ||
                    (item.HardwareName != null && item.HardwareName.IndexOf(value, StringComparison.InvariantCultureIgnoreCase) >= 0) ||
                    (item.SoftwareCaption != null && item.SoftwareCaption.IndexOf(value, StringComparison.InvariantCultureIgnoreCase) >= 0) ||
                    (item.BrowserCaption != null && item.BrowserCaption.IndexOf(value, StringComparison.InvariantCultureIgnoreCase) >= 0))
                    list.Add(item);
            }
            return list;
#endif
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
        public static int CompareDeviceNames(Device x, Device y)
        {
            return x.SoftwareBrowserCaption.CompareTo(y.SoftwareBrowserCaption);
        }

        #endregion

        #region Private Methods

#if VER4 || VER35

        /// <summary>
        /// Returns a list of the hardware devices for the vendor requested.
        /// </summary>
        /// <param name="vendor"></param>
        /// <returns></returns>
        private static List<Device> GetVendorDevices(Value vendor)
        {
            var list = new List<Device>();
            foreach (var item in vendor.Devices)
            {
                var device = new Device(item);
                if (list.FirstOrDefault(i =>
                    i.HardwareModel == device.HardwareModel) == null)
                    list.Add(device);
            }
            return list.OrderBy(i => i.HardwareModel).ToList();
        }

#else

        /// <summary>
        /// Returns a list of the hardware devices for the vendor requested.
        /// </summary>
        /// <param name="vendor"></param>
        /// <returns></returns>
        private static List<Device> GetVendorDevices(Value vendor)
        {
            var list = new List<Device>();
            foreach (var baseDevice in vendor.Devices)
            {
                var device = new Device(baseDevice);
                
                bool found = false;
                foreach (var existingDevice in list)
                {
                    if (existingDevice.HardwareModel == device.HardwareModel)
                    {
                        found = true;
                    }
                }

                if (found == false)
                    list.Add(device);
            }
            list.Sort(CompareDevicesByHardwareModel);
            return list;
        }

        /// <summary>
        /// Compares two device objects by hardware model.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private static int CompareDevicesByHardwareModel(Device x, Device y)
        {
            return x.HardwareModel.CompareTo(y.HardwareModel);
        }

        /// <summary>
        /// Checks to determine if the list of properties are included in the 
        /// matches list.
        /// </summary>
        /// <param name="properties">A list of properties to be compared</param>
        /// <param name="matches"></param>
        /// <returns></returns>
        private static IList<Property> GetProperties(List<Property> properties, string[] matches)
        {
            var list = new List<Property>();
            var matchesList = new List<string>(matches);
            foreach (var property in Provider.Properties)
                if (matchesList.Contains(property.Name))
                    list.Add(property);
            return list;
        }

        /// <summary>
        /// Returns the first property which matches the name provided.
        /// </summary>
        /// <param name="properties">List of properties.</param>
        /// <param name="name">Property name required.</param>
        /// <returns></returns>
        private static Property GetFirstWhereNameEquals(List<Property> properties, string name)
        {
            foreach (var property in properties)
                if (property.Name == name)
                    return property;
            return null;
        }
#endif

        #endregion
    }
}
