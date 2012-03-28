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

using System.Collections.Generic;
using System;

#if VER4

using System.Linq;

#endif

namespace FiftyOne.Foundation.Mobile.Detection
{
    /// <summary>
    /// A possible value associated with a property.
    /// </summary>
    public class Value : PropertyValue, IComparable<Value>
    {
        #region Fields

        /// <summary>
        /// Lock used when calculating the devices associated with the value.
        /// </summary>
        private static readonly object _lock = new object();

        /// <summary>
        /// The property the value is associted with.
        /// </summary>
        private Property _property;

        /// <summary>
        /// A list of devices associated with the value.
        /// </summary>
        private List<BaseDeviceInfo> _devices = null;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs an instance of <see cref="Value"/>.
        /// </summary>
        /// <param name="property">The property the value is associated with.</param>
        /// <param name="name">The string name.</param>
        internal Value(Property property, string name)
            : base(property.Provider, name)
        {
            _property = property;
        }

        /// <summary>
        /// Constructs an instance of <see cref="Value"/>.
        /// </summary>
        /// <param name="property">The property the value is associated with.</param>
        /// <param name="name">The string name.</param>
        /// <param name="description">The description of the name.</param>
        internal Value(Property property, string name, string description)
            : base(property.Provider, name, description)
        {
            _property = property;
        }

        /// <summary>
        /// Constructs an instance of <see cref="Value"/>.
        /// </summary>
        /// <param name="property">The property the value is associated with.</param>
        /// <param name="name">The string name.</param>
        /// <param name="description">The description of the name.</param>
        /// <param name="url">An optional URL linking to more information about the name.</param>
        internal Value(Property property, string name, string description, string url)
            : base(property.Provider, name, description, url)
        {
            _property = property;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Returns the property the value is associated with.
        /// </summary>
        public Property Property
        {
            get { return _property; }
        }

        /// <summary>
        /// Returns a list of devices associated with the value.
        /// </summary>
        public List<BaseDeviceInfo> Devices
        {
            get
            {
                if (_devices == null)
                {
                    lock (_lock)
                    {
                        if (_devices == null)
                        {
#if VER4
                            _devices = Provider.Devices.Where(i =>
                                i.GetPropertyValueStringIndexes(_property.NameStringIndex).Contains(NameStringIndex)).ToList();
#else
                            _devices = new List<BaseDeviceInfo>();
                            foreach (var device in Provider.Devices)
                            {
                                // Get all the values this property has for the current device.
                                foreach (var index in device.GetPropertyValueStringIndexes(_property.NameStringIndex))
                                {
                                    // If the value of the device property and this value match 
                                    // then add the device to the list and move to the next device.
                                    if (index == NameStringIndex)
                                    {
                                        _devices.Add(device);
                                        break;
                                    }
                                }
                            }
#endif
                        }
                    }
                }
                return _devices;
            }
        }

        #endregion

        #region Interface Implementation

        /// <summary>
        /// Compares this instance with another.
        /// </summary>
        /// <param name="other">An object of type PropertyValue</param>
        /// <returns></returns>
        public int CompareTo(Value other)
        {
            return Name.CompareTo(other.Name);
        }

        #endregion
    }
}
