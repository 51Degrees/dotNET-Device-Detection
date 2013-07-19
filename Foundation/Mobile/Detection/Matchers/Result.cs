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
using FiftyOne.Foundation.Mobile.Detection.Handlers;
using System.Collections.Generic;

namespace FiftyOne.Foundation.Mobile.Detection.Matchers
{
    /// <summary>
    /// Contains a device matched via a handler.
    /// </summary>
    public class Result : IComparable<Result>
    {
        private readonly Provider _provider;
        private readonly BaseDeviceInfo _primaryDevice;
        private BaseDeviceInfo _secondaryDevice;
        private readonly Handler _handler;
        private readonly uint _score;
        private readonly string _userAgent;
        private object _difference;

        internal Result(Provider provider, BaseDeviceInfo primaryDevice, Handler handler, uint score, string userAgent)
        {
            _provider = provider;
            _primaryDevice = primaryDevice;
            _handler = handler;
            _score = score;
            _userAgent = userAgent;
        }

        internal Result(Provider provider, BaseDeviceInfo primaryDevice, BaseDeviceInfo secondaryDevice, Handler handler, uint score, string userAgent)
        {
            _provider = provider;
            _primaryDevice = primaryDevice;
            _secondaryDevice = secondaryDevice;
            _handler = handler;
            _score = score;
            _userAgent = userAgent;
        }

        #region Public Properties

        /// <summary>
        /// The confidence of the result.
        /// </summary>
        public byte Confidence
        {
            get { return Difference == 0 ? byte.MaxValue : _handler.Confidence; }
        }

        /// <summary>
        /// The edit distant indicator for the result.
        /// </summary>
        public int Difference
        {
            get 
            { 
                if (_difference == null)
                    _difference = Algorithms.EditDistance(Device.UserAgent, _userAgent, int.MaxValue);
                return (int)_difference;
            }
        }

        /// <summary>
        /// The score for the result.
        /// </summary>
        public uint Score
        {
            get { return _score; }
        }

        /// <summary>
        /// The device found from the user agent provided by the requesting information.
        /// </summary>
        public BaseDeviceInfo Device
        {
            get { return _primaryDevice; }
        }

        /// <summary>
        /// The device found using the UserAgent header User-Agent.
        /// </summary>
        public BaseDeviceInfo DevicePrimary
        {
            get { return _primaryDevice; }
        }

        /// <summary>
        /// The device found using a secondary HTTP header. 
        /// See <see cref="Constants.DeviceUserAgentHeaders"/>
        /// </summary>
        public BaseDeviceInfo DeviceSecondary
        {
            get { return _secondaryDevice; }
        }

        /// <summary>
        /// The handler used to obtain the result.
        /// </summary>
        public Handler Handler
        {
            get { return _handler; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the first value for the property.
        /// </summary>
        /// <param name="property">Name of the property to be returned.</param>
        /// <returns>Value of the property.</returns>
        public string GetFirstPropertyValue(string property)
        {
            int index = GetFirstPropertyValueStringIndex(_provider.Strings.Add(property));
            if (index >= 0)
                return _provider.Strings.Get(index);
            return null;
        }

        /// <summary>
        /// Returns a list of the string values for the property.
        /// </summary>
        /// <param name="property">Name of the property to be returned.</param>
        /// <returns>List of values for the property.</returns>
        public List<string> GetPropertyValues(string property)
        {
            int propertyStringIndex = _provider.Strings.Add(property);
            List<string> values = new List<string>();
            List<int> indexes = GetPropertyValueStringIndexes(propertyStringIndex);
            if (indexes != null)
            {
                foreach (int index in indexes)
                {
                    if (index >= 0)
                        values.Add(_provider.Strings.Get(index));
                }
            }
            else
            {
                // Check for any special values not held in
                // the strings collection.
                switch (property)
                {
                    case Constants.DeviceId: values.Add(
                        _secondaryDevice == null ?
                        _primaryDevice.DeviceId : 
                        _secondaryDevice.DeviceId);
                        break;
                }
            }
            return values;
        }

        /// <summary>
        /// Returns a sorted list containing all the property values for the
        /// the result.
        /// </summary>
        public SortedList<string, List<string>> GetAllProperties()
        {
            SortedList<string, List<string>> collection = new SortedList<string, List<string>>();
            collection.Add(Constants.DeviceId, new List<string>(new string[] { 
                _secondaryDevice == null ?
                _primaryDevice.DeviceId : 
                _secondaryDevice.DeviceId }));
#if DEBUG
            List<string> handlerNames = new List<string>();
            foreach (FiftyOne.Foundation.Mobile.Detection.Handlers.Handler handler in _provider.GetHandlers(
                _secondaryDevice == null ?
                _primaryDevice.UserAgent :
                _secondaryDevice.UserAgent))
                handlerNames.Add(handler.Name);
            collection.Add("Handlers", new List<string>(new string[] { String.Join(", ", handlerNames.ToArray()) }));
            collection.Add("UserAgent", new List<string>(new string[] {
                _secondaryDevice == null ?
                _primaryDevice.UserAgent : 
                _secondaryDevice.UserAgent }));
#endif
            AddProperties(collection);
            return collection;
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Sets the secondary device if one has been detected.
        /// </summary>
        /// <param name="device"></param>
        internal void SetSecondaryDevice(BaseDeviceInfo device)
        {
            _secondaryDevice = device;
        }

        /// <summary>
        /// Adds the device properties to the collection.
        /// </summary>
        /// <param name="collection">Collection to have properties added to.</param>
        internal protected void AddProperties(SortedList<string, List<string>> collection)
        {
            foreach (int propertyStringIndex in _provider.Properties.Keys)
            {
                string property = _provider.Strings.Get(propertyStringIndex);
                if (Constants.ExcludePropertiesFromAllProperties.Contains(property) == false &&
                    collection.ContainsKey(property) == false)
                    collection.Add(
                        property,
                        GetPropertyValues(propertyStringIndex));
            }
        }

        /// <summary>
        /// Gets the capability value index in the static Strings collection for this device
        /// based on the index of the capability name. If this device does not have the 
        /// value then checks the parent if one exists.
        /// </summary>
        /// <param name="propertyStringIndex">The string index of the property name.</param>
        /// <returns>Capability index value in the String collection, or null if the capability does not exist.</returns>
        internal List<int> GetPropertyValueStringIndexes(int propertyStringIndex)
        {
            return GetDeviceForProperty(propertyStringIndex).GetPropertyValueStringIndexes(propertyStringIndex);
        }

        /// <summary>
        /// Returns the string index of the first element in the collection.
        /// </summary>
        /// <param name="propertyStringIndex">The string index of the property name.</param>
        /// <returns>Capability index value in the String collection, or -1 if the capability does not exist.</returns>
        internal int GetFirstPropertyValueStringIndex(int propertyStringIndex)
        {
            return GetDeviceForProperty(propertyStringIndex).GetFirstPropertyValueStringIndex(propertyStringIndex);
        }

        /// <summary>
        /// Returns a list of the string values for the property index string provided.
        /// </summary>
        /// <param name="propertyStringIndex">The string index of the property name.</param>
        /// <returns>A list of string values.</returns>
        internal List<string> GetPropertyValues(int propertyStringIndex)
        {
            return GetDeviceForProperty(propertyStringIndex).GetPropertyValues(propertyStringIndex);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// If there are two devices, both a primary and secondary device,
        /// this method works out which one should be used to return the 
        /// property value.
        /// </summary>
        /// <param name="propertyNameStringIndex">The string index of the property being sought.</param>
        /// <returns>The device that should be used to provide the property.</returns>
        private BaseDeviceInfo GetDeviceForProperty(int propertyNameStringIndex)
        {
            if (_secondaryDevice != null)
            {
                Property property = null;
                if (_provider.Properties.TryGetValue(propertyNameStringIndex, out property))
                {
                    switch(property.Component)
                    {
                        case Provider.Components.Hardware:
                        case Provider.Components.Software:
                            return _secondaryDevice;
                        default:
                            return _primaryDevice;
                    }
                }
            }
            return _primaryDevice;
        }

        #endregion

        #region IComparable<Result> Members

        /// <summary>
        /// Compare this instance to another.
        /// </summary>
        /// <param name="other">Instance for comparison.</param>
        /// <returns>Zero if equal. 1 if higher or -1 if lower.</returns>
        public int CompareTo(Result other)
        {
            if (Device == null && other.Device == null)
                return 0;
            return Device.DeviceId.CompareTo(other.Device.DeviceId);
        }

        #endregion
    }
}