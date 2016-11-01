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
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Collections.Concurrent;

namespace FiftyOne.Foundation.Mobile.Detection.Entities
{
    /// <summary>
    /// Represents a collection of <see cref="Property"/> and 
    /// <see cref="Value"/> entities relating to a profile which in turn 
    /// relates to a component.
    /// </summary>
    /// <para>
    /// All profiles belong to one of the four <see cref="Component"/>: 
    /// hardware, software, crawler and browser and have a number of values 
    /// associated with each profile. The contents of existing profiles should 
    /// not change over time. Each <see cref="Value"/> contains a getter method 
    /// for the corresponding <see cref="Property"/>. Device Id is composed of 
    /// one profile per component.
    /// </para>
    /// <para>
    /// One physical device can have multiple profiles associated with it. 
    /// While the hardware does not generally change, the browser will almost 
    /// certainly vary over time as new versions get released and perhaps even 
    /// a different browser is used.
    /// </para>
    /// <para>
    /// Each <see cref="Signature"/> relates to one profile for each component.
    /// </para>
    /// <para>
    /// New profiles are added with each data file update. Some profiles may be
    /// removed if we do not see any use of the profile for a long period of 
    /// time. Premium and Enterprise data contain a lot more profiles and hence 
    /// provide better detection results, especially for less common devices.
    /// For more information see: https://51degrees.com/compare-data-options
    /// </para>
    public abstract class Profile : BaseEntity<DataSet>, IComparable<Profile>, IEquatable<Profile>
    {
        #region Fields

        /// <summary>
        /// Returned when the property has no values in the provide.
        /// </summary>
        private static readonly int[] EmptyValues = new int[0];

        /// <summary>
        /// Unique Id of the profile. Does not change between different 
        /// data sets.
        /// </summary>
        public readonly int ProfileId;

        #endregion

        #region Internal Properties

        /// <summary>
        /// The release date of the profile if it's a hardware profile.
        /// </summary>
        internal DateTime ReleaseDate
        {
            get
            {
                if (_releaseDateChecked == false)
                {
                    lock (this)
                    {
                        if (_releaseDateChecked == false)
                        {
                            var month = Values.FirstOrDefault(i =>
                                i.Property.Name == "ReleaseMonth");
                            var year = Values.FirstOrDefault(i =>
                                i.Property.Name == "ReleaseYear");
                            if (month != null && year != null)
                            {
                                int monthValue = GetMonthAsInt(month.Name);
                                int yearValue;
                                if (monthValue >= 1 && monthValue <= 12 &&
                                    int.TryParse(year.Name, out yearValue))
                                {
                                    _releaseDate = new DateTime(yearValue, monthValue, 1);
                                }
                            }
                            _releaseDateChecked = true;
                        }
                    }
                }
                return _releaseDate;
            }
        }
        private bool _releaseDateChecked;
        private DateTime _releaseDate;
        
        #endregion

        #region Abstract Properties

        /// <summary>
        /// A array of the indexes of the values associated with the profile.
        /// </summary>
        protected internal abstract int[] ValueIndexes { get; }

        /// <summary>
        /// An array of the signature indexes associated with the profile.
        /// </summary>
        protected internal abstract int[] SignatureIndexes { get; }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the values associated with the property. Can return null.
        /// </summary>
        /// <param name="property">
        /// The property whose values are required.
        /// </param>
        /// <para>
        /// The <see cref="Values"/> type is used to return values so that 
        /// helper methods like ToBool can be used to convert the response to 
        /// a boolean.
        /// </para>
        /// <returns>
        /// Array of the values associated with the property, or null if the 
        /// property does not exist.
        /// </returns>
        public Values this[Property property]
        {
            get
            {
                Values values = null;

                // A read / write upgradable guard could be used in the future
                // should the performance of GetPropertyValueIndexes prove too 
                // slow in the future. The use of a lock on the dictionary
                // ensures that this implementation is thread safe.
                lock (PropertyIndexToValues)
                {
                    if (PropertyIndexToValues.TryGetValue(
                        property.Index, 
                        out values) == false)
                    {
                        values = new Entities.Values(
                            property,
                            GetPropertyValueIndexes(property));
                        PropertyIndexToValues.Add(property.Index, values);
                    }
                }
                return values;
            }
        }

        /// <summary>
        /// A dictionary relating the index of a property to the values 
        /// returned by the profile. Used to speed up subsequent data 
        /// processing.
        /// </summary>
        private IDictionary<int, Values> PropertyIndexToValues
        {
            get
            {
                if (_propertyIndexToValues == null)
                {
                    lock(this)
                    {
                        if (_propertyIndexToValues == null)
                        {
                            _propertyIndexToValues = 
                                new Dictionary<int, Values>();
                        }
                    }
                }
                return _propertyIndexToValues;
            }
        }
        private IDictionary<int, Values> _propertyIndexToValues;

        /// <summary>
        /// A dictionary relating the name of a property to the values returned
        /// by the profile. Used to speed up subsequent data processing.
        /// </summary>
        private IDictionary<string, Values> PropertyNameToValues
        {
            get
            {
                if (_propertyNameToValues == null)
                {
                    lock (this)
                    {
                        if (_propertyNameToValues == null)
                        {
                            _propertyNameToValues = 
                                new Dictionary<string, Values>();
                        }
                    }
                }
                return _propertyNameToValues;
            }
        }
        private IDictionary<string, Values> _propertyNameToValues;

        /// <summary>
        /// Gets the values associated with the property name.
        /// </summary>
        /// <param name="propertyName">
        /// Name of the property whose values are required.
        /// </param>
        /// <returns>
        /// Array of the values associated with the property, or null if the 
        /// property does not exist.
        /// </returns>
        /// <para>
        /// The <see cref="Values"/> type is used to return values so that 
        /// helper methods like ToBool can be used to convert the response to
        /// a boolean.
        /// </para>
        public Values this[string propertyName]
        {
            get
            {
                Values values = null;

                // A read / write upgradable guard could be used in the future
                // should the performance of this[property] prove too slow in 
                // the future. The use of a lock on the dictionary ensures that 
                // this implementation is thread safe.
                lock (PropertyNameToValues)
                {
                    if (PropertyNameToValues.TryGetValue(propertyName, out values) == false)
                    {
                        var property = DataSet.Properties[propertyName];
                        if (property != null)
                        {
                            values = this[property];
                        }
                        PropertyNameToValues.Add(propertyName, values);
                    }
                }
                return values;
            }
        }

        /// <summary>
        /// Array of signatures associated with the profile.
        /// </summary>
        public Signature[] Signatures
        {
            get
            {
                if (_signatures == null)
                {
                    lock (this)
                    {
                        if (_signatures == null)
                        {
                            _signatures = GetSignatures();
                        }
                    }
                }
                return _signatures;
            }
        }
        private Signature[] _signatures;

        /// <summary>
        /// The component the profile belongs to.
        /// </summary>
        public Component Component
        {
            get 
            {
                if (_component == null)
                {
                    lock (this)
                    {
                        if (_component == null)
                        {
                            _component = DataSet.Components[_componentIndex];
                        }
                    }
                }
                return _component; 
            }
        }
        private Component _component;
        private readonly int _componentIndex;
        
        /// <summary>
        /// An array of values associated with the profile.
        /// </summary>
        public Value[] Values
        {
            get
            {
                if (_values == null)
                {
                    lock (this)
                    {
                        if (_values == null)
                        {
                            _values = GetValues();
                        }
                    }
                }
                return _values;
            }
        }
        private Value[] _values;
        
        /// <summary>
        /// An array of properties associated with the profile.
        /// </summary>
        public Property[] Properties
        {
            get 
            {
                if (_properties == null)
                {
                    lock (this)
                    {
                        if (_properties == null)
                        {
                            _properties = GetProperties();
                        }
                    }
                }
                return _properties;
            }
        }
        private Property[] _properties = null;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a new instance of the <see cref="Profile"/>.
        /// </summary>
        /// <param name="dataSet">
        /// The data set whose profile list the profile will be contained 
        /// within.
        /// </param>
        /// <param name="offset">
        /// The offset position in the data structure to the profile.
        /// </param>
        /// <param name="reader">
        /// Reader connected to the source data structure and positioned to 
        /// start reading.
        /// </param>
        internal Profile(
            DataSet dataSet,
            int offset,
            BinaryReader reader)
            : base(dataSet, offset)
        {
            _componentIndex = reader.ReadByte();
            ProfileId = reader.ReadInt32();
        }

        #endregion
        
        #region Methods

        /// <summary>
        /// Called after the entire data set has been loaded to ensure 
        /// any further initialisation steps that require other items in
        /// the data set can be completed.
        /// </summary>
        internal virtual void Init()
        {
            if (_properties == null)
                _properties = GetProperties();
            if (_values == null)
            {
                _values = GetValues();
            }
            if (_signatures == null)
            {
                _signatures = GetSignatures();
            }
            if (_component == null)
                _component = DataSet.Components[_componentIndex];
        }
                
        /// <summary>
        /// Gets the value indexes associated with the property for this 
        /// profile.
        /// </summary>
        /// <param name="property">Property to be returned.</param>
        /// <returns>
        /// Array of value indexes associated with the property AND profile.
        /// </returns>
        internal int[] GetPropertyValueIndexes(Property property)
        {
            int[] result;

            // Work out the start and end index in the values associated
            // with the profile that relate to this property.
            var start = Array.BinarySearch<int>(
                ValueIndexes, 
                property.FirstValueIndex);

            // If the start is negative then the first value doesn't exist.
            // Take the complement and use this as the first index. 
            if (start < 0)
            {
                start = ~start;
            }

            var end = Array.BinarySearch<int>(
                ValueIndexes, 
                start,
                ValueIndexes.Length - start, 
                property.LastValueIndex);

            // If the end is negative then the last value doesn't exist. Take
            // the complement and use this as the last index. However if this 
            // value doesn't relate to the property then it's the first value
            // for the next property and we need to move back one in the list.
            if (end < 0)
            {
                end = ~end;
                if (end >= ValueIndexes.Length ||
                    ValueIndexes[end] > property.LastValueIndex)
                {
                    end--;
                }
            }

            // If start is greater than end then there are no values for this
            // property in this profile. Return an empty array.
            if (start > end)
            {
                result = EmptyValues;
            }
            else
            {
                // Perform gross error checks on the start and end values.
                Debug.Assert(start == 0 ||
                    DataSet.Values[ValueIndexes[start - 1]].Property.Index != property.Index);
                Debug.Assert(end == ValueIndexes.Length - 1 ||
                    DataSet.Values[ValueIndexes[end + 1]].Property.Index != property.Index);
                Debug.Assert(DataSet.Values[ValueIndexes[start]].Property.Index == property.Index);
                Debug.Assert(DataSet.Values[ValueIndexes[end]].Property.Index == property.Index);

                // Create the array and populate it with the value indexes for 
                // the profile AND property.
                result = new int[end - start + 1];
                for (int i = start, v = 0; i <= end; i++, v++)
                {
                    // Perform a gross error check to ensure the property 
                    // relates to the value index being returned.
                    Debug.Assert(DataSet.Values[ValueIndexes[i]].Property.Index == property.Index);
                    result[v] = ValueIndexes[i];
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the signatures related to the profile.
        /// </summary>
        /// <returns>
        /// Array of signatures related to the profile.
        /// </returns>
        private Signature[] GetSignatures()
        {
            var signatures = new Signature[SignatureIndexes.Length];
            for (int i = 0; i < SignatureIndexes.Length; i++)
            {
                signatures[i] = DataSet.Signatures[SignatureIndexes[i]];
            }
            return signatures;
        }

        /// <summary>
        /// Returns an array of properties the profile relates to.
        /// </summary>
        /// <returns>
        /// Array of all properties for this profile.
        /// </returns>
        private Property[] GetProperties()
        {
            var properties = new List<Property>();
            foreach (var value in Values)
            {
                var index = properties.BinarySearch(value.Property);
                if (index < 0)
                    properties.Insert(~index, value.Property);
            }
            return properties.ToArray();
        }

        /// <summary>
        /// Returns an array of values the profile relates to.
        /// </summary>
        /// <returns>
        /// Array of all values for this profile.
        /// </returns>
        private Value[] GetValues()
        {
            var values = new Value[ValueIndexes.Length];
            for (int i = 0; i < ValueIndexes.Length; i++)
            {
                values[i] = DataSet.Values[ValueIndexes[i]];
            }
            return values;
        }

        /// <summary>
        /// Returns the month name as an integer.
        /// </summary>
        /// <param name="month">
        /// Name of the month, i.e. January.
        /// </param>
        /// <returns>
        /// The integer representation of the month
        /// </returns>
        private static int GetMonthAsInt(string month)
        {
            switch (month)
            {
                case "January": return 1;
                case "February": return 2;
                case "March": return 3;
                case "April": return 4;
                case "May": return 5;
                case "June": return 6;
                case "July": return 7;
                case "August": return 8;
                case "September": return 9;
                case "October": return 10;
                case "November": return 11;
                case "December": return 12;
                default: return 0;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Compares this instance of a profile to another instance 
        /// for equality using the profile Id value.
        /// </summary>
        /// <param name="other">
        /// The profile to be compared against.
        /// </param>
        /// <returns>True if they equal, otherwise false</returns>
        public bool Equals(Profile other)
        {
            return ProfileId.Equals(other.ProfileId);
        }

        /// <summary>
        /// Compares this profile to another using the numeric ProfileId field.
        /// </summary>
        /// <param name="other">
        /// The profile to be compared against.
        /// </param>
        /// <returns>
        /// Indication of relative value based on ProfileId field.
        /// </returns>
        public int CompareTo(Profile other)
        {
            return ProfileId.CompareTo(other.ProfileId);
        }
        
        /// <summary>
        /// A string representation of the profiles display values or if none
        /// are available a concatentation of the display properties.
        /// </summary>
        /// <para>
        /// If there are properties with the DisplayOrder properties set to 
        /// values greater than 1 their values are concatenated in ascending
        /// display order and returned as a string.
        /// If no DisplayOrder properties are available (Lite data) then the
        /// string version of the ProfileId is returned.
        /// </para>
        /// <para>
        /// For more information see 
        /// https://51degrees.com/Support/Documentation/Net
        /// </para>
        /// <returns>
        /// A string that represents the profile.
        /// </returns>
        public override string ToString()
        {
            if (_stringValue == null)
            {
                lock (this)
                {
                    if (_stringValue == null)
                    {
                        _stringValue =
                            String.Join(
                                "/",
                                Values.Where(i =>
                                    i.Property.DisplayOrder > 0 &&
                                    i.Name != "Unknown").OrderByDescending(i =>
                                        i.Property.DisplayOrder).Select(i =>
                                            i.Name).Distinct().ToArray()).Trim();
                        if (String.IsNullOrEmpty(_stringValue))
                        {
                            _stringValue = ProfileId.ToString();
                        }
                    }
                }
            }
            return _stringValue;
        }
        private string _stringValue = null;

        #endregion
    }
}
