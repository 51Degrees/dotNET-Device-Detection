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
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace FiftyOne.Foundation.Mobile.Detection.Entities
{
    /// <summary>
    /// A value associated with a <see cref="Property"/> and 
    /// <see cref="Profile"/> within the dataset.
    /// </summary>
    /// <para>
    /// Every property can return one of many values, or multiple values if 
    /// it's a list property. For example; SupportedBearers returns a list of 
    /// the bearers that the device can support.
    /// </para>
    /// <para>
    /// The value class contains all the information associated with the value 
    /// including the display name, and also other information such as a 
    /// description or URL to find out additional information. These other 
    /// properties can be used by UI developers to provide users with more 
    /// information about the meaning and intended use of a value.
    /// </para>
    /// <para>
    /// For more information see 
    /// https://51degrees.com/Support/Documentation/Net
    /// </para>
    public class Value : BaseEntity<DataSet>,
        IComparable<Value>, IComparable<string>, IEquatable<Value>, IEquatable<string>
    {
        #region Static Fields

        /// <summary>
        /// Used to find profiles with values that include this one.
        /// </summary>
        private static readonly SearchLists<int, int> _valuesIndexSearch = 
            new SearchLists<int, int>();

        #endregion

        #region Properties

        /// <summary>
        /// Returns true if the value is the default one for the property.
        /// </summary>
        public bool IsDefault
        {
            get
            {
                return Property.DefaultValue != null && this.Name == Property.DefaultValue.Name;
            }
        }

        /// <summary>
        /// The name of the value.
        /// </summary>
        public string Name
        {
            get
            {
                if (_name == null)
                {
                    lock (this)
                    {
                        if (_name == null)
                        {
                            _name = DataSet.Strings[_nameOffset].ToString();
                        }
                    }
                }
                return _name;
            }
        }
        private string _name;
        private readonly int _nameOffset;

        /// <summary>
        /// Array containing the signatures that the value is associated with.
        /// </summary>
        /// <remarks>
        /// If time taken to determine the signatures associated with a value 
        /// can take a long time as the entire list of signatures needs to be 
        /// read.
        /// </remarks>
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
        /// Array containing the profiles the value is associated with.
        /// </summary>
        /// <remarks>
        /// May take longer to return from Stream data sets as the class in 
        /// normal operation will not store a direct reference to the profiles.
        /// Direct reference to the profiles are set from the ensureValueProfilesSet method
        /// of <see cref="Property"/> when used with FindProfiles.
        /// </remarks>
        public Profile[] Profiles
        {
            get
            {
                return _profiles == null ?
                    ProfileIndexes.Select(i => DataSet.Profiles[i]).ToArray() :
                    _profiles;
            }
        }
        internal Profile[] _profiles = null;

        /// <summary>
        /// Returns the indexes of the profiles associated with the value.
        /// Used to prevent storing a reference to the profile when operating
        /// in stream mode with a cache.
        /// </summary>
        internal int[] ProfileIndexes
                {
            get
            {
                if (_profileIndexes == null)
                {
                    lock (this)
                    {
                        if (_profileIndexes == null)
                        {
                            _profileIndexes = GetProfileIndexes();
                        }
                    }
                }
                return _profileIndexes;
            }
        }
        internal int[] _profileIndexes;

        /// <summary>
        /// The property the value relates to.
        /// </summary>
        public Property Property
        {
            get 
            {
                if (_property == null)
                {
                    lock (this)
                    {
                        if (_property == null)
                        {
                            _property = DataSet.Properties[_propertyIndex];
                        }
                    }
                }
                return _property; 
            }
        }
        private Property _property;
        internal readonly int _propertyIndex;

        /// <summary>
        /// The component the value relates to.
        /// </summary>
        public Component Component
        {
            get { return Property.Component; }
        }

        /// <summary>
        /// A description of the value suitable to be displayed to end
        /// users via a user interface.
        /// </summary>
        public string Description
        {
            get
            {
                if (_descriptionOffset >= 0 &&
                    _description == null)
                {
                    lock(this)
                    {
                        if (_description == null)
                        {
                            _description = DataSet.Strings[_descriptionOffset].ToString();
                        }
                    }
                }
                return _description;
            }
        }
        private string _description;
        private readonly int _descriptionOffset;

        /// <summary>
        /// A URL to more information about the value.
        /// </summary>
        public Uri Url
        {
            get
            {
                if (_urlOffset >= 0 &&
                    _url == null)
                {
                    lock (this)
                    {
                        if (_url == null)
                        {
                            _url = new Uri(DataSet.Strings[_urlOffset].ToString());
                        }
                    }
                }
                return _url;
            }
        }
        private Uri _url;
        private readonly int _urlOffset;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a new instance of <see cref="Value"/>.
        /// </summary>
        /// <param name="dataSet">
        /// The data set the value is contained within.
        /// </param>
        /// <param name="index">
        /// The index in the data structure to the value.
        /// </param>
        /// <param name="reader">
        /// Reader connected to the source data structure and positioned to 
        /// start reading.
        /// </param>
        internal Value(
            DataSet dataSet,
            int index,
            BinaryReader reader) : base(dataSet, index)
        {
            _propertyIndex = reader.ReadInt16();
            _nameOffset = reader.ReadInt32();
            _descriptionOffset = reader.ReadInt32();
            _urlOffset = reader.ReadInt32();
        }

        /// <summary>
        /// Constructs a new instance of <see cref="Value"/>.
        /// </summary>
        /// <param name="dataSet">
        /// The data set the value is contained within.
        /// </param>
        /// <param name="property">
        /// The property the dynamic value will relate to.
        /// </param>
        /// <param name="value">
        /// The string name of the dynamic value.
        /// </param>
        internal Value (
            DataSet dataSet,
            Property property,
            string value) : base(dataSet, -1)
        {
            _propertyIndex = property.Index;
            _name = value;
            _nameOffset = -1;
            _descriptionOffset = -1;
            _urlOffset = -1;
        }

        #endregion
        
        #region Internal Methods

        /// <summary>
        /// Called after the entire data set has been loaded to ensure 
        /// any further initialisation steps that require other items in
        /// the data set can be completed.
        /// </summary>
        /// <remarks>
        /// The Profiles and Signatures are not initialised as they are very 
        /// rarely used and take a long time to initialise.
        /// </remarks>
        internal void Init()
        {
            if (_name == null)
                _name = DataSet.Strings[_nameOffset].ToString();
            if (_property == null)
                _property = DataSet.Properties[_propertyIndex];
            if (_descriptionOffset >= 0)
                _description = DataSet.Strings[_descriptionOffset].ToString();
            if (_urlOffset >= 0)
                _url = new Uri(DataSet.Strings[_urlOffset].ToString());
        }

        /// <summary>
        /// Gets all the profile indexes associated with the value.
        /// </summary>
        /// <returns>
        /// Returns the profile indexes from the component that relate to this value.
        /// </returns>
        private int[] GetProfileIndexes()
        {
            return Component.Profiles.Where(i =>
                _valuesIndexSearch.BinarySearch(i.ValueIndexes, Index) >= 0).Select(i => 
                    i.Index).ToArray();
        }

        /// <summary>
        /// Gets all the signatures associated with the value.
        /// </summary>
        /// <returns
        /// >Returns the signatures associated with the value.
        /// </returns>
        private Signature[] GetSignatures()
        {
            var list = new List<Signature>();
            foreach (var signature in Profiles.SelectMany(i => 
                i.Signatures))
            {
                var index = list.BinarySearch(signature);
                if (index < 0)
                    list.Insert(~index, signature);
            }
            return list.ToArray();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Compares the name of this value to the one provided.
        /// </summary>
        /// <param name="other">
        /// String containing a name of another value to compare to this value.
        /// </param>
        /// <returns>
        /// Indication of relative value based on name.
        /// </returns>
        public int CompareTo(string other)
        {
            var length = Math.Min(Name.Length, other.Length);
            for (int i = 0; i < length; i++)
            {
                var difference = Name[i].CompareTo(other[i]);
                if (difference != 0)
                {
                    return difference;
                }
            }
            if (Name.Length < other.Length)
            {
                return -1;
            }
            if (Name.Length > other.Length)
        {
                return 1;
            }
            return 0;
        }

        /// <summary>
        /// Compares this value to another using the index field if they're in the
        /// same list other wise the name value.
        /// </summary>
        /// <param name="other">
        /// The value to be compared against.
        /// </param>
        /// <returns>
        /// Indication of relative value based on index field.
        /// </returns>
        public int CompareTo(Value other)
        {
            return DataSet == other.DataSet ? 
                Index.CompareTo(other.Index) :
                CompareTo(other.Name);
        }

        /// <summary>
        /// Compares this value instance to the other instance.
        /// </summary>
        /// <param name="other">Instance to compare</param>
        /// <returns>True if the instance is equal, otherwise false</returns>
        public bool Equals(Value other)
        {
            return DataSet == other.DataSet ? 
                Index.Equals(other.Index) :
                Name.Equals(other.Name);
        }

        /// <summary>
        /// Compares this value instance to the string.
        /// </summary>
        /// <param name="other">String to compare</param>
        /// <returns>True if the string is equal, otherwise false</returns>
        public bool Equals(string other)
        {
            return Name.Equals(other);
        }

        /// <summary>
        /// Returns the value as a string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Returns the value as a number.
        /// </summary>
        /// <para>
        /// If the value can not convert to a double and the value is not
        /// equal to the null value then the null value for the property 
        /// will be used. If no conversion is possible 0 is returned.
        /// </para>
        public double ToDouble()
        {
            if (_asNumber == null)
            {
                lock (this)
                {
                    if (_asNumber == null)
                    {
                        double value;
                        if (double.TryParse(Name, out value))
                        {
                            _asNumber = value;
                        }
                        else if (Property.DefaultValue != null &&
                            this != Property.DefaultValue)
                        {
                            _asNumber = Property.DefaultValue.ToDouble();
                        }
                        else
                        {
                            _asNumber = 0;
                        }
                    }
                }
            }
            return (double)_asNumber;
        }
        private double? _asNumber;

        /// <summary>
        /// Returns the value as a boolean.
        /// </summary>
        /// <para>
        /// If the value can not convert to a boolean and the value is not
        /// equal to the null value then the null value for the property 
        /// will be used. If no conversion is possible false is returned.
        /// </para>
        public bool ToBool()
        {
            if (_asBool == null)
            {
                lock (this)
                {
                    if (_asBool == null)
                    {
                        bool value;
                        if (bool.TryParse(Name, out value))
                        {
                            _asBool = value;
                        }
                        else if (Property.DefaultValue != null &&
                            this != Property.DefaultValue)
                        {
                            _asBool = Property.DefaultValue.ToBool();
                        }
                        else
                        {
                            _asBool = false;
                        }
                    }
                }
            }
            return _asBool.Value;
        }
        private bool? _asBool;

        /// <summary>
        /// Returns the value as an integer.
        /// </summary>
        /// <para>
        /// If the value can not convert to an integer and the value is not
        /// equal to the null value then the null value for the property 
        /// will be used. If no conversion is possible 0 is returned.
        /// </para>
        internal int ToInt()
        {
            if (_asInt == null)
            {
                lock (this)
                {
                    if (_asInt == null)
                    {
                        _asInt = (int)ToDouble();
                    }
                }
            }
            return _asInt.Value;
        }
        private int? _asInt;

        #endregion
    }
}
