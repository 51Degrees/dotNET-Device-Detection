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

#region Usings

using System;
using System.Collections.Generic;

#if VER4 || VER35

using System.Linq;

#endif

#endregion

namespace FiftyOne.Foundation.Mobile.Detection
{
    /// <summary>
    /// Represents a device and holds all its settings.
    /// </summary>
    public class BaseDeviceInfo
    {
        #region Fields

        /// <summary>
        /// Used to populate the active children.
        /// </summary>
        private object _lock = new object();

        /// <summary>
        /// A list of child devices.
        /// </summary>
        internal List<BaseDeviceInfo> _children = new List<BaseDeviceInfo>();
    
        /// <summary>
        /// A list of the active children for the device.
        /// </summary>
        internal List<BaseDeviceInfo> _activeChildren = null;

        /// <summary>
        /// The parent device.
        /// </summary>
        internal BaseDeviceInfo _parent;

        /// <summary>
        /// Holds all properties from the current device
        /// </summary>
        private Collection _deviceProperties;

        /// <summary>
        /// The unique Id of the device.
        /// </summary>
        private string _deviceId;

        /// <summary>
        /// A reference to the provider associated with this device.
        /// </summary>
        protected BaseProvider _provider;

        /// <summary>
        /// The useragent string of the device.
        /// </summary>
        private string _userAgent;

        /// <summary>
        /// A list of the profile IDs which make up the device.
        /// </summary>
        private string[] _profileIDs = null;

        /// <summary>
        /// A collection of handler specific data.
        /// </summary>
        private SortedList<Handlers.Handler, object> _handlerData;

        #endregion

        #region Public Properties

        /// <summary>
        /// Contains the device user agent string.
        /// </summary>
        public string UserAgent
        {
            get { return _userAgent; }
        }

        /// <summary>
        /// The parent of the device, or null if this is a root device.
        /// </summary>
        public BaseDeviceInfo Parent
        {
            get { return _parent; }
            set 
            {
                // update the parent, ensuring the child collection
                // is also updated.
                if (_parent != null)
                {
                    _parent._children.Remove(this);
                    _parent._activeChildren = null;
                }
                _parent = value;
                if (_parent != null)
                    _parent._children.Add(this);
            }
        }

        /// <summary>
        /// Gets the unique identifier of the device.
        /// </summary>
        public string DeviceId
        {
            get { return _deviceId; }
        }

        /// <summary>
        /// Returns the profile IDs which make up the device ID.
        /// </summary>
        public string[] ProfileIDs
        {
            get
            {
                if (_profileIDs == null)
                {
                    List<string> list = new List<string>();
                    foreach (string id in DeviceId.Split(new string[] { Constants.ProfileSeperator, " " }, StringSplitOptions.RemoveEmptyEntries))
                        list.Add(id);
                    _profileIDs = list.ToArray();
                }
                return _profileIDs;
            }
        }

        /// <summary>
        /// Returns true if the device is only available in the premium data set.
        /// </summary>
        public bool IsPremium
        {
            get
            {
                return Detection.Provider.EmbeddedProvider.GetDeviceInfoByID(DeviceId) == null;
            }
        }

        #endregion

        #region Internal Properties

        /// <summary>
        /// Returns the provider associated with the device.
        /// </summary>
        internal BaseProvider Provider
        {
            get { return _provider; }
        }

        /// <summary>
        /// Internal accessor for user agent string.
        /// </summary>
        internal string InternalUserAgent
        {
            set { _userAgent = value; }
        }

        /// <summary>
        /// The list of device properties.
        /// </summary>
        internal Collection Properties
        {
            get { return _deviceProperties; }
        }

        /// <summary>
        /// Returns a list of the children that are active
        /// with properties assigned to them.
        /// </summary>
        internal List<BaseDeviceInfo> ActiveChildren
        {
            get
            {
                if (_activeChildren == null)
                {
                    lock (_lock)
                    {
                        if (_activeChildren == null)
                        {
                            _activeChildren = new List<BaseDeviceInfo>();
#if VER4 || VER35
                            _activeChildren = _children.Where(i =>
                                i.Properties.Count > 0 ||
                                i._children.Count > 0).ToList();
#else
                            foreach (BaseDeviceInfo child in _children)
                                if (child.Properties.Count > 0 ||
                                    child._children.Count > 0)
                                    _activeChildren.Add(child);
#endif
                        }
                    }
                }
                return _activeChildren;
            }
        }
               
        #endregion

        #region Private Properties

        /// <summary>
        /// Returns the data object for the specific handler.
        /// </summary>
        private SortedList<Handlers.Handler, object> HandlerData
        {
            get
            {
                if (_handlerData == null)
                {
                    lock (_lock)
                    {
                        if (_handlerData == null)
                        {
                            _handlerData = new SortedList<Handlers.Handler, object>();
                        }
                    }
                }
                return _handlerData;
            }
        }

        #endregion

        #region Handler Data Methods

        /// <summary>
        /// Returns a the handlers data object of type T. If no data has
        /// been created for the handler then new data is created.
        /// </summary>
        /// <typeparam name="T">The type of handler data to store.</typeparam>
        /// <param name="handler">The handler related to the device.</param>
        /// <returns>The data of type T, or a new instance.</returns>
        internal object GetHandlerData<T>(Handlers.Handler handler) where T : new()
        {
            object data = null;
            if (HandlerData.TryGetValue(handler, out data) == false)
            {
                lock (HandlerData)
                {
                    if (HandlerData.TryGetValue(handler, out data) == false)
                    {
                        data = new T();
                        HandlerData.Add(handler, data);
                    }
                }
            }
            return data;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Hide the default constructor.
        /// </summary>
        private BaseDeviceInfo()
        {
        }

        /// <summary>
        /// Creates an instance of <cref see="BaseDeviceInfo"/>.
        /// </summary>
        /// <param name="userAgent">User agent string used to identify this device.</param>
        /// <param name="deviceId">A unique Identifier of the device.</param>
        /// <param name="devices">A reference to the complete index of devices.</param>
        /// <param name="parent">The parent device if one exists.</param>
        internal BaseDeviceInfo(
            BaseProvider devices,
            string deviceId,
            string userAgent,
            BaseDeviceInfo parent)
        {
            Init(devices, deviceId, userAgent, parent);
        }

        /// <summary>
        /// Creates an instance of <cref see="BaseDeviceInfo"/>.
        /// </summary>
        /// <param name="userAgent">User agent string used to identify this device.</param>
        /// <param name="deviceId">A unique Identifier of the device.</param>
        /// <param name="devices">A reference to the complete index of devices.</param>
        internal BaseDeviceInfo(
            BaseProvider devices,
            string deviceId,
            string userAgent)
        {
            Init(devices, deviceId, userAgent);
        }

        /// <summary>
        /// Creates an instance of DeviceInfo.
        /// </summary>
        /// <param name="deviceId">A unique Identifier of the device.</param>
        /// <param name="devices">A reference to the complete index of devices.</param>
        internal BaseDeviceInfo(
            BaseProvider devices,
            string deviceId)
        {
            Init(devices, deviceId);
        }

        private void Init(
            BaseProvider devices,
            string deviceId)
        {
            if (string.IsNullOrEmpty(deviceId))
                throw new ArgumentNullException("deviceId");

            if (devices == null)
                throw new ArgumentNullException("devices");

            _provider = devices;
            _deviceId = deviceId;
            _deviceProperties = new Collection(devices.Strings);
        }

        private void Init(
            BaseProvider devices,
            string deviceId,
            string userAgent,
            BaseDeviceInfo parent)
        {
            Parent = parent;
            Init(devices, deviceId, userAgent);
        }
        
        private void Init(
            BaseProvider devices,
            string deviceId,
            string userAgent)
        {
            _userAgent = userAgent;
            Init(devices, deviceId);
        }

        #endregion

        #region Parent Methods

        /// <summary>
        /// Returns true if the deviceId is in the parent hierarchy of the 
        /// device.
        /// </summary>
        /// <param name="device">The device being checked.</param>
        /// <returns>True if the device is within the parent hierarchy.</returns>
        internal bool GetIsParent(BaseDeviceInfo device)
        {
            if (this == device)
                return true;
            if (Parent != null)
                return Parent.GetIsParent(device);
            return false;
        }

        #endregion

        #region Property Methods

        /// <summary>
        /// Gets the capability value index in the static Strings collection for this device
        /// based on the index of the capability name. If this device does not have the 
        /// value then checks the parent if one exists.
        /// </summary>
        /// <param name="index">Capability name index.</param>
        /// <returns>Capability index value in the String collection, or null if the capability does not exist.</returns>
        internal protected virtual List<int> GetPropertyValueStringIndexes(int index)
        {
            List<int> value;
            if (_deviceProperties.TryGetValue(index, out value))
                return value;
            if (Parent != null)
                return Parent.GetPropertyValueStringIndexes(index);
            return null;
        }

        /// <summary>
        /// Returns the string index of the first element in the collection.
        /// </summary>
        /// <param name="index">Capability name index.</param>
        /// <returns>Capability index value in the String collection, or -1 if the capability does not exist.</returns>
        internal protected virtual int GetFirstPropertyValueStringIndex(int index)
        {
            IList<int> value = GetPropertyValueStringIndexes(index);
            if (value != null && value.Count > 0)
                return value[0];
            return -1;
        }

        /// <summary>
        /// Returns a list of the string values for the property index string provided.
        /// </summary>
        /// <param name="propertyStringIndex">The string index of the property name.</param>
        /// <returns>A list of string values.</returns>
        internal protected List<string> GetPropertyValues(int propertyStringIndex)
        {
            List<string> values = new List<string>();
            foreach (int index in GetPropertyValueStringIndexes(propertyStringIndex))
                if (index >= 0)
                    values.Add(Provider.Strings.Get(index));
            return values;
        }

        /// <summary>
        /// Adds the device properties to the collection.
        /// </summary>
        /// <param name="collection">Collection to have properties added to.</param>
        internal protected void AddProperties(SortedList<string, List<string>> collection)
        {
            foreach (int propertyStringIndex in Properties.Keys)
            {
                string property = _provider.Strings.Get(propertyStringIndex);
                if (Constants.ExcludePropertiesFromAllProperties.Contains(property) == false &&
                    collection.ContainsKey(property) == false)
                    collection.Add(
                        property,
                        GetPropertyValues(propertyStringIndex));
            }
            if (Parent != null)
                Parent.AddProperties(collection);
        }

        /// <summary>
        /// Gets the value of the first value for the property.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public string GetFirstPropertyValue(string property)
        {
            int index = GetFirstPropertyValueStringIndex(Provider.Strings.Add(property));
            if (index >= 0)
                return Provider.Strings.Get(index);

            // Check for any special values not held in
            // the strings collection.
            switch (property)
            {
                case Constants.DeviceId: return DeviceId;
            }

            return null;
        }

        /// <summary>
        /// Returns a list of the string values for the property.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public List<string> GetPropertyValues(string property)
        {
            List<string> values = new List<string>();
            List<int> indexes = GetPropertyValueStringIndexes(Provider.Strings.Add(property));
            if (indexes != null)
            {
                foreach (int index in indexes)
                {
                    if (index >= 0)
                        values.Add(Provider.Strings.Get(index));
                }
            }
            else
            {
                // Check for any special values not held in
                // the strings collection.
                switch (property)
                {
                    case Constants.DeviceId: values.Add(DeviceId); break;
                }
            }
            return values;
        }

        /// <summary>
        /// Returns a sorted list containing all the property values for the
        /// the device.
        /// </summary>
        public SortedList<string, List<string>> GetAllProperties()
        {
            SortedList<string, List<string>> collection = new SortedList<string, List<string>>();
            collection.Add(Constants.DeviceId, new List<string>(new string[] { DeviceId }));
#if DEBUG
            List<string> handlerNames = new List<string>();
            foreach (FiftyOne.Foundation.Mobile.Detection.Handlers.Handler handler in _provider.GetHandlers(UserAgent))
                handlerNames.Add(handler.Name);
            collection.Add("Handlers", new List<string>(new string[] { String.Join(", ", handlerNames.ToArray()) }));
            collection.Add("UserAgent", new List<string>(new string[] { UserAgent }));
#endif
            AddProperties(collection);
            return collection;
        }

        #endregion

        #region Equal Members

        /// <summary>
        /// Checks if another BaseDeviceInfo is equal to this one.
        /// </summary>
        /// <param name="other">Other BaseDeviceInfo.</param>
        /// <returns>True if the object instances are the same.</returns>
        internal bool Equals(BaseDeviceInfo other)
        {
            return DeviceId.Equals(other.DeviceId) &&
                   UserAgent.Equals(other.UserAgent) &&
                   Properties.Equals(other.Properties) &&
                   CapabilitiesEquals(other);
        }

        /// <summary>
        /// Check the strings are all equal.
        /// </summary>
        /// <param name="other">Other BaseDeviceInfo.</param>
        /// <returns>True if the object capability strings are the same.</returns>
        private bool CapabilitiesEquals(BaseDeviceInfo other)
        {
            foreach(int key in Properties.Keys)
            {
                if (_provider.Strings.Get(key).Equals(other.Provider.Strings.Get(key)) == false)
                    return false;
                foreach(int value in GetPropertyValueStringIndexes(key))
                    if (_provider.Strings.Get(value).Equals(other.Provider.Strings.Get(value)) == false)
                    return false;
            }
            return true;
        }

        #endregion
    }
}