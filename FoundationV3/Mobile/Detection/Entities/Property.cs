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
using System.Text;

namespace FiftyOne.Foundation.Mobile.Detection.Entities
{
    /// <summary>
    /// Encapsulates all the information about a property including how it's 
    /// values should be used and what they mean.
    /// </summary>
    /// <para>
    /// Some properties are not mandatory and may not always contain values. For example; information
    /// concerning features of a television may not be applicable to a mobile phone. The IsMandatory
    /// property should be checked before assuming a value will be returned.
    /// </para>
    /// <para>
    /// Properties can return none, one or many values. The IsList property should be refered to
    /// to determine the number of values to expect. Properties where IsList is false will only
    /// return upto one value.
    /// </para>
    /// <para>
    /// The property also provides other information about the intended use of the property. The 
    /// Description can be used by UI developers to provide more information about the intended
    /// use of the property and it's values. The Category property can be used to group together
    /// related properties in configuration UIs.
    /// </para>
    /// <para>
    /// Values are returned in the type <see cref="Values"/> which includes utility methods to 
    /// easilly extract strongly typed values.
    /// </para>
    /// <para>
    /// For more information see https://51degrees.com/Support/Documentation/Net
    /// </para>
    public class Property : BaseEntity,  IComparable<Property>, IEquatable<Property>
    {
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
        internal readonly int MapCount;

        /// <summary>
        /// The first index in the list of maps.
        /// </summary>
        internal readonly int FirstMapIndex;

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
                            _maps = DataSet.Maps.Skip(FirstMapIndex).Take(MapCount).Select(i => 
                                i.Name).ToArray();
                        }
                    }
                }
                return _maps;
            }
        }
        private IList<string> _maps;

        /// <summary>
        /// The name of the property to use when adding to Javascript as a property name.
        /// Unacceptable characters such as / are removed.
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
        /// The default value the property which is also used when a strongly type value
        /// is not available when converting to strong type like bool or double.
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
        /// Constructs a new instance of <see cref="Property"/>
        /// </summary>
        /// <param name="dataSet">
        /// The data set the property is contained within
        /// </param>
        /// <param name="index">
        /// The index in the data structure to the property
        /// </param>
        /// <param name="reader">
        /// Reader connected to the source data structure and positioned to start reading
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
            MapCount = reader.ReadInt32();
            FirstMapIndex = reader.ReadInt32();
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
                _maps = DataSet.Maps.Skip(FirstMapIndex).Take(MapCount).Select(i =>
                            i.Name).ToArray();
        }

        /// <summary>
        /// Returns the values which reference the property by starting
        /// at the first value index and moving forward until a new
        /// property is found.
        /// </summary>
        /// <returns>A values list initialised with the property values</returns>
        private Values GetValues()
        {
            var list = new List<Value>();
            for (var index = FirstValueIndex; index <= LastValueIndex; index++)
            {
                list.Add(DataSet.Values[index]);
            }
            return new Values(this, list);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Compares this property to another using the index field if they're
        /// in the same list, otherwise the name field.
        /// </summary>
        /// <param name="other">The property to be compared against</param>
        /// <returns>Indication of relative value</returns>
        public int CompareTo(Property other)
        {
            if (DataSet == other.DataSet)
                return Index.CompareTo(other.Index);
            return Name.CompareTo(other.Name);
        }

        /// <summary>
        /// A string representation of the property.
        /// </summary>
        /// <returns>The property's name</returns>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Compares the properties using name value.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
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
            /// The property returns javascript to be executed on the client device.
            /// The javascript can be used by all versions of the server component.
            /// </summary>
            JavaScript = 4
        }

        #endregion
    }
}
