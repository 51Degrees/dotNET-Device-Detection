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
using System.Text;
using FiftyOne.Foundation.Mobile.Detection.Entities.Stream;
using System.Diagnostics;

namespace FiftyOne.Foundation.Mobile.Detection.Entities
{
    /// <summary>
    /// Encapsulates all the information about a property including how it's 
    /// values should be used and what they mean.
    /// </summary>
    /// <para>
    /// Some properties are not mandatory and may not always contain values. 
    /// For example: information concerning features of a television may not 
    /// be applicable to a mobile phone. The <see cref="Property.IsMandatory"/> 
    /// value should be checked before assuming a value will be returned.
    /// </para>
    /// <para>
    /// Properties can return none, one or many values. The 
    /// <see cref="Property.IsList"/> property should be referred to check if 
    /// more than one value may be returned. Properties where IsList is false 
    /// will only return up to one value.
    /// </para>
    /// <para>
    /// The property also provides additional meta information about itself. 
    /// The <see cref="Property.Description"/> can be used by UI developers to 
    /// provide more information about the intended use of the property and 
    /// it's values. The <see cref="Property.Category"/> can be used to group 
    /// together related properties in configuration and report user interfaces.
    /// </para>
    /// <para>
    /// 51Degrees property dictionary is generated dynamically using the 
    /// contents of the data file where each property is displayed along with 
    /// its description and a list of values that the property can have.
    /// https://51degrees.com/resources/property-dictionary
    /// </para>
    /// <para>
    /// Values are returned in the type <see cref="Values"/> which includes 
    /// utility methods to easily extract strongly typed values.
    /// </para>
    /// <para>
    /// For more information see 
    /// https://51degrees.com/Support/Documentation/Net
    /// </para>
    public class Property : BaseEntity,  IComparable<Property>, IEquatable<Property>
    {
        #region Constants

        /// <summary>
        /// Empty array of profiles for instances where no profiles were 
        /// returned. 
        /// </summary>
        protected static readonly Entities.Profile[] EmptyProfiles = new Profile[0];

        #endregion

        #region Fields

        /// <summary>
        /// True if the property can have more than one value.
        /// </summary>
        public readonly bool IsList;

        /// <summary>
        /// True if the property must contain values.
        /// </summary>
        public readonly bool IsMandatory;

        /// <summary>
        /// True if the values the property returns are relevant to 
        /// configuration user interfaces and are suitable to be 
        /// selected from a list of table of options.
        /// </summary>
        public readonly bool ShowValues;

        /// <summary>
        /// True if the property is relevant to be shown in a configuration
        /// user interface where the property may appear in a list of 
        /// options.
        /// </summary>
        public readonly bool Show;

        /// <summary>
        /// True if the property is marked as obsolete and will be 
        /// removed from a future version of the data set. Check the
        /// property description for more information.
        /// </summary>
        public readonly bool IsObsolete;

        /// <summary>
        /// The order in which the property should appear in relation
        /// to others with a position value set when used to create
        /// a display string for the profile.
        /// </summary>
        public readonly byte DisplayOrder;

        /// <summary>
        /// The index of the first value related to the property.
        /// </summary>
        internal readonly int FirstValueIndex;

        /// <summary>
        /// The index of the last value related to the property.
        /// </summary>
        internal readonly int LastValueIndex;

        /// <summary>
        /// The number of maps the property is assigned to.
        /// </summary>
        private readonly int _mapCount;

        /// <summary>
        /// The first index in the list of maps.
        /// </summary>
        private readonly int _firstMapIndex;

        #endregion

        #region Properties

        /// <summary>
        /// The maps the property can be found in.
        /// </summary>
        public IList<string> Maps
        {
            get
            {
                if (_maps == null)
                {
                    lock (this)
                    {
                        if (_maps == null)
                        {
                            _maps = DataSet.Maps.Skip(_firstMapIndex).Take(_mapCount).Select(i => 
                                i.Name).ToArray();
                        }
                    }
                }
                return _maps;
            }
        }
        private IList<string> _maps;

        /// <summary>
        /// The name of the property to use when adding to Javascript as a 
        /// property name. Unacceptable characters such as '/' are removed.
        /// </summary>
        public string JavaScriptName
        {
            get
            {
                if (_javascriptName == null)
                {
                    lock (this)
                    {
                        if (_javascriptName == null)
                        {
                            _javascriptName = new String(Name.Where(i =>
                                char.IsLetterOrDigit(i)).ToArray());
                        }
                    }
                }
                return _javascriptName;
            }
        }
        private string _javascriptName = null;

        /// <summary>
        /// The name of the property.
        /// </summary>
        public string Name
        {
            get 
            { 
                if (_name == null)
                {
                    lock(this)
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
        /// The strongly type data type the property returns.
        /// </summary>
        public Type ValueType
        {
            get 
            {
                switch (_valueType)
                {
                    default:
                    case PropertyValueType.JavaScript:
                    case PropertyValueType.String: return typeof(string);
                    case PropertyValueType.Int: return typeof(int);
                    case PropertyValueType.Double: return typeof(double);
                    case PropertyValueType.Bool: return typeof(bool);
                }
            }
        }
        internal readonly PropertyValueType _valueType;

        /// <summary>
        /// The default value the property which is also used when a strongly 
        /// type value is not available when converting to strong type like 
        /// bool or double.
        /// </summary>
        public Value DefaultValue
        {
            get
            {
                if (_defaultValue == null &&
                    _defaultValueIndex >= 0)
                {
                    lock (this)
                    {
                        if (_defaultValue == null)
                        {
                            _defaultValue = DataSet.Values[_defaultValueIndex];
                        }
                    }
                }
                return _defaultValue;
            }
        }
        private Value _defaultValue;
        private readonly int _defaultValueIndex;
                
        /// <summary>
        /// The component the property relates to.
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
        /// An list of values the property has available.
        /// </summary>
        public Values Values
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
        private Values _values;

        /// <summary>
        /// A description of the property suitable to be displayed to end
        /// users via a user interface.
        /// </summary>
        public string Description
        {
            get
            {
                if (_description == null &&
                    _descriptionOffset >= 0)
                {
                    lock (this)
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
        /// The category the property relates to within the data set.
        /// </summary>
        public string Category
        {
            get
            {
                if (_category == null &&
                    _categoryOffset >= 0)
                {
                    lock (this)
                    {
                        if (_category == null)
                        {
                            _category = DataSet.Strings[_categoryOffset].ToString();
                        }
                    }
                }
                return _category;
            }
        }
        private string _category;
        private readonly int _categoryOffset;

        /// <summary>
        /// A url to more information about the property.
        /// </summary>
        public Uri Url
        {
            get
            {
                if (_url == null &&
                    _urlOffset >= 0)
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

        #region Constructor

        /// <summary>
        /// Constructs a new instance of <see cref="Property"/>.
        /// </summary>
        /// <param name="dataSet">
        /// The data set the property is contained within.
        /// </param>
        /// <param name="index">
        /// The index in the data structure to the property.
        /// </param>
        /// <param name="reader">
        /// Reader connected to the source data structure and positioned to 
        /// start reading.
        /// </param>
        internal Property(
            DataSet dataSet,
            int index,
            BinaryReader reader) : base(dataSet, index)
        {
            _componentIndex = reader.ReadByte();
            DisplayOrder = reader.ReadByte();
            IsMandatory = reader.ReadBoolean();
            IsList = reader.ReadBoolean();
            ShowValues = reader.ReadBoolean();
            IsObsolete = reader.ReadBoolean();
            Show = reader.ReadBoolean();
            _valueType  = (PropertyValueType)reader.ReadByte();
            _defaultValueIndex = reader.ReadInt32();
            _nameOffset = reader.ReadInt32();
            _descriptionOffset = reader.ReadInt32();
            _categoryOffset = reader.ReadInt32();
            _urlOffset = reader.ReadInt32();
            FirstValueIndex = reader.ReadInt32();
            LastValueIndex = reader.ReadInt32();
            _mapCount = reader.ReadInt32();
            _firstMapIndex = reader.ReadInt32();
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
            _name = DataSet.Strings[_nameOffset].ToString();
            if (_categoryOffset >= 0)
                _category = DataSet.Strings[_categoryOffset].ToString();
            if (_descriptionOffset >= 0)
                _description = DataSet.Strings[_descriptionOffset].ToString();
            if (_urlOffset >= 0)
                _url = new Uri(DataSet.Strings[_urlOffset].ToString());
            if (_values == null)
                _values = GetValues();
            if (_component == null)
                _component = DataSet.Components[_componentIndex];
            if (_maps == null)
                _maps = DataSet.Maps.Skip(_firstMapIndex).Take(_mapCount).Select(i =>
                            i.Name).ToArray();
        }

        /// <summary>
        /// Returns the values which reference the property by starting
        /// at the first value index and moving forward until a new
        /// property is found.
        /// </summary>
        /// <returns>
        /// A values instance configured with the value indexes associated with
        /// the property.
        /// </returns>
        private Values GetValues()
        {
            var valueIndexes = new int[LastValueIndex - FirstValueIndex + 1];
            for (int i = FirstValueIndex, v = 0; i <= LastValueIndex; i++, v++)
            {
                valueIndexes[v] = i;
            }
            return new Values(this, valueIndexes);
        }

        /// <summary>
        /// Ensures that the values are initialised to improve performance the
        /// next time that the property is accessed for the purposes of finding
        /// profiles associated with values.
        /// </summary>
        /// <remarks>
        /// Uses the internal fields of the Value class to set an array of 
        /// profile indexes and also the array of profiles. Once these are
        /// set they will not need to be calculated again.
        /// </remarks>
        private void InitValues()
        {
            if (InitialisedValues == false)
            {
                lock (this)
                {
                    if (InitialisedValues == false)
                    {
                        // If the Values list is cached increase the size
                        // of the cache to improve performance for this
                        // feature by storing all related values in the cache.
                        // Having all the possible values cached will improve 
                        // performance for subsequent requests. if the data 
                        // set isn't cached then there will only be one instance
                        // of each profile and value in memory so the step isn't
                        // needed as the direct reference will be used.
                        if (DataSet.Values is ICacheList)
                        {
                            ((ICacheList)DataSet.Values).CacheSize += Values.Count;
                        }

                        // Build a dictionary to store the growing list of 
                        // profiles associated with each value.
                        var tempValues = new Dictionary<int, List<Profile>>();
                        foreach (var value in Values)
                        {
                            tempValues.Add(value.Index, new List<Profile>());
                        }

                        // Loop through the profiles associated with the 
                        // component adding then to the dictionary keyed
                        // on value where the property associated with 
                        // the value matches.
                        foreach (Profile profile in Component.Profiles)
                        {
                            foreach (var value in profile[this])
                            {
                                tempValues[value.Index].Add(profile);
                            }
                        }

                        // Finally set the values profile indexes and the profiles
                        // for each of the values.
                        foreach (var valueProfiles in tempValues)
                        {
                            // Get the value from the index.
                            var value = DataSet.Values[valueProfiles.Key];

                            // Only set the relationship between the value and profiles
                            // where the data set indicates this is desired. It makes
                            // sense to do this in memory mode as there will only ever
                            // be one instance of each profile. However in stream mode
                            // where caching is used there could be multiple instances 
                            // and this is undesirable from a memory management 
                            // perspective.
                            if (DataSet.FindProfilesInitialiseValueProfiles)
                            {
                                value._profiles = valueProfiles.Value.ToArray();
                                value._profileIndexes = value._profiles.Select(i => i.Index).ToArray();
                            }
                            else
                            {
                                value._profileIndexes = valueProfiles.Value.Select(i => i.Index).ToArray();
                            }
                        }

                        // Set the flag to avoid repeating this method for the
                        // property.
                        InitialisedValues = true;
                    }
                }
            }
        }

        // Marked as internal so that the state can be monitored in
        // integration tests.
        internal bool InitialisedValues { get; private set; }

        #endregion

        #region Public Methods


        /// <summary>
        /// Gets the profiles associated with the value name where the 
        /// value's profiles intersects with the filterProfiles if provided.
        /// </summary>
        /// <param name="valueName">
        /// Name of the value associated with the property
        /// </param>
        /// <param name="filterProfiles">
        /// Array of profiles ordered in ascending Index order. Null if 
        /// no filter is required.
        /// </param>
        /// <returns>Array of profiles ordered in ascending Index order.</returns>
        public Entities.Profile[] FindProfiles(string valueName, Entities.Profile[] filterProfiles = null)
        {
            InitValues();
            var result = Property.EmptyProfiles;
            var value = Values[valueName];
            if (value != null)
            {
                if (filterProfiles == null)
                {
                    // Return the profiles associated with the value.
                    result = value.Profiles;
                }
                else
                {
                    // Return the intersection between the value profiles
                    // and the filter profiles. The index of the profiles are
                    // used to avoid getting each one associated with the property
                    // if it's not contained in the filter profiles.
                    result = value.ProfileIndexes.Intersect(filterProfiles.Select(i =>
                        i.Index)).Select(i => DataSet.Profiles[i]).ToArray();
                }
            }
            return result;
        }

        /// <summary>
        /// Compares this property to another using the index field if they're
        /// in the same list, otherwise the name field.
        /// </summary>
        /// <param name="other">
        /// The property to be compared against.
        /// </param>
        /// <returns>
        /// Indication of relative value.
        /// </returns>
        public int CompareTo(Property other)
        {
            if (DataSet == other.DataSet)
                return Index.CompareTo(other.Index);
            return Name.CompareTo(other.Name);
        }

        /// <summary>
        /// A string representation of the property.
        /// </summary>
        /// <returns>
        /// The property's name.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Compares the properties using name value.
        /// </summary>
        /// <param name="other">
        /// Another property to compare this property to.
        /// </param>
        /// <returns>
        /// True if this property's name equals the name of the other property, 
        /// False otherwise.
        /// </returns>
        public bool Equals(Property other)
        {
            return this.Name.Equals(other.Name);
        }

        #endregion

        #region Enumerations

        /// <summary>
        /// Enumeration of strongly typed property values which relate to
        /// <see cref="Property"/>.
        /// </summary>
        public enum PropertyValueType
        {
            /// <summary>
            /// The property returns string values.
            /// </summary>
            String = 0,
            /// <summary>
            /// The property returns interger values.
            /// </summary>
            Int = 1,
            /// <summary>
            /// The property returns double floating point values.
            /// </summary>
            Double = 2,
            /// <summary>
            /// The property returns boolean values.
            /// </summary>
            Bool = 3,
            /// <summary>
            /// The property returns javascript to be executed on the client 
            /// device. The javascript can be used by all versions of the 
            /// server component.
            /// </summary>
            JavaScript = 4
        }

        #endregion
    }
}
