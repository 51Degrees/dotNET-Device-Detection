/* *********************************************************************
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited. 
 * Copyright © 2014 51Degrees Mobile Experts Limited, 5 Charlotte Close,
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

namespace FiftyOne.Foundation.Mobile.Detection.Entities
{
    /// <summary>
    /// Represents a collection of properties and values relating to a profile
    /// which in turn relates to a component.
    /// </summary>
    /// <para>
    /// Each <see cref="Signature"/> relates to one profile for each component.
    /// </para>
    public class Profile : BaseEntity, IComparable<Profile>
    {
        #region Fields

        /// <summary>
        /// Unique Id of the profile. Does not change between different data sets.
        /// </summary>
        public readonly int ProfileId;

        /// <summary>
        /// A array of the indexes of the values associated with the profile.
        /// </summary>
        internal int[] ValueIndexes;

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

        #region Public Properties

        /// <summary>
        /// Gets the values associated with the property.
        /// </summary>
        /// <param name="property">The property whose values are required</param>
        /// <returns>
        /// Array of the values associated with the property, or null if the property does not exist
        /// </returns>
        /// <para>
        /// The <see cref="Values"/> type is used to return values so that 
        /// helper methods like ToBool can be used to convert the response to 
        /// a boolean.
        /// </para>
        public Values this[Property property]
        {
            get
            {
                if (property != null)
                    return this[property.Name];
                return null;
            }
        }

        /// <summary>
        /// Gets the values associated with the property name.
        /// </summary>
        /// <param name="propertyName">Name of the property whose values are required</param>
        /// <returns>
        /// Array of the values associated with the property, or null if the property does not exist
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
                // Does the storage structure already exist?
                if (_nameToValues == null)
                {
                    lock (this)
                    {
                        if (_nameToValues == null)
                        {
                            _nameToValues = new SortedList<string, Values>();
                        }
                    }
                }

                // Do the values already exist for the property?
                lock (_nameToValues)
                {
                    Values values;
                    if (_nameToValues.TryGetValue(propertyName, out values))
                        return values;
                                        
                    // Does not exist already so get the property.
                    var property = DataSet.Properties.FirstOrDefault(i =>
                        i.Name == propertyName);

                    // Create the list of values.
                    values = new Values(
                        property,
                        Values.Where(i => i.Property == property));

                    if (values.Count == 0)
                        values = null;

                    // Store for future reference.
                    _nameToValues.Add(propertyName, values);

                    return values;
                }
            }
        }
        private SortedList<string, Values> _nameToValues;

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
        internal int[] SignatureIndexes;

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
        /// Constructs a new instance of the <see cref="Profile"/>
        /// </summary>
        /// <param name="dataSet">
        /// The data set whose profile list the profile will be contained within
        /// </param>
        /// <param name="offset">
        /// The offset position in the data structure to the profile</param>
        /// <param name="reader">
        /// Reader connected to the source data structure and positioned to start reading
        /// </param>
        internal Profile(
            DataSet dataSet,
            int offset,
            BinaryReader reader)
            : base(dataSet, offset)
        {
            _componentIndex = reader.ReadByte();
            ProfileId = reader.ReadInt32();
            var valueIndexesCount = reader.ReadInt32();
            var signatureIndexesCount = reader.ReadInt32();
            ValueIndexes = BaseEntity.ReadIntegerArray(reader, valueIndexesCount);
            SignatureIndexes = BaseEntity.ReadIntegerArray(reader, signatureIndexesCount);
        }

        #endregion
        
        #region Methods

        /// <summary>
        /// Called after the entire data set has been loaded to ensure 
        /// any further initialisation steps that require other items in
        /// the data set can be completed.
        /// </summary>
        internal void Init()
        {
            if (_properties == null)
                _properties = GetProperties();
            if (_values == null)
            {
                _values = GetValues();
                ValueIndexes = null;
            }
            if (_signatures == null)
            {
                _signatures = GetSignatures();
                SignatureIndexes = null;
            }
            if (_component == null)
                _component = DataSet.Components[_componentIndex];
        }

        /// <summary>
        /// Gets the signatures related to the profile.
        /// </summary>
        /// <returns>Array of signatures related to the profile.</returns>
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
        /// <returns></returns>
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
        /// <returns></returns>
        private Value[] GetValues()
        {
            var values = new Value[ValueIndexes.Length];
            for (int i = 0; i < ValueIndexes.Length; i++)
                values[i] = DataSet.Values[ValueIndexes[i]];
            return values;
        }

        /// <summary>
        /// Returns the month name as an integer.
        /// </summary>
        /// <param name="month">Name of the month, i.e. January</param>
        /// <returns>The integer representation of the month</returns>
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
        /// Compares this profile to another using the numeric
        /// ProfileId field.
        /// </summary>
        /// <param name="other">The profile to be compared against</param>
        /// <returns>Indication of relative value based on ProfileId field</returns>
        public int CompareTo(Profile other)
        {
            return ProfileId.CompareTo(other.ProfileId);
        }
        
        /// <summary>
        /// A string representation of the profiles display values or if none
        /// are available a concatentation of the display properties.
        /// </summary>
        /// <returns>A string that represents the profile</returns>
        /// <para>
        /// If there are properties with the DisplayOrder properties set to 
        /// values greater than 1 their values are concatenated in ascending
        /// display order and returned as a string.
        /// If no DisplayOrder properties are available (Lite data) then the
        /// string version of the ProfileId is returned.
        /// </para>
        /// <para>
        /// For more information see http://51degrees.com/Support/Documentation/Net.aspx
        /// </para>
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
