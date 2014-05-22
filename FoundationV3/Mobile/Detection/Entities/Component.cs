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
    /// Every device can be split into the major components of hardware,
    /// operating system and browser. The properties and values 
    /// associated with these components are accessed via this class.
    /// </summary>
    /// <remarks>
    /// As there are a small number of components they are always held in memory.
    /// </remarks>
    /// <para>
    /// For more information see http://51degrees.com/Support/Documentation/Net.aspx
    /// </para>
    public class Component : BaseEntity, IComparable<Component>
    {
        #region Fields

        /// <summary>
        /// The unique Id of the component. Does not change between different
        /// data sets versions.
        /// </summary>
        public readonly int ComponentId;

        #endregion

        #region Properties
                
        /// <summary>
        /// The unique name of the component.
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
        /// Array of properties associated with the component.
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
        private Property[] _properties;

        /// <summary>
        /// An array of profiles associated with the component.
        /// </summary>
        public Profile[] Profiles
        {
            get
            {
                if (_profiles == null)
                {
                    lock (this)
                    {
                        if (_profiles == null)
                        {
                            _profiles = GetProfiles();
                        }
                    }
                }
                return _profiles;
            }
        }
        private Profile[] _profiles;

        /// <summary>
        /// The default profile that should be returned for the component.
        /// </summary>
        public Profile DefaultProfile
        {
            get 
            {
                if (_defaultProfile == null)
                {
                    lock (this)
                    {
                        if (_defaultProfile == null)
                        {
                            _defaultProfile = DataSet.Profiles[_defaultProfileOffset];
                        }
                    }
                }
                return _defaultProfile;
            }
        }
        private Profile _defaultProfile;
        private readonly int _defaultProfileOffset;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a new instance of see <see cref="Component"/>
        /// </summary>
        /// <param name="dataSet">
        /// The <see cref="DataSet"/> being created
        /// </param>
        /// <param name="index">
        /// Index of the component within the list
        /// </param>
        /// <param name="reader">
        /// Reader connected to the source data structure and positioned to start reading
        /// </param>
        internal Component(
            DataSet dataSet,
            int index,
            BinaryReader reader)
            : base(dataSet, index)
        {
            ComponentId = reader.ReadByte();
            _nameOffset = reader.ReadInt32();
            _defaultProfileOffset = reader.ReadInt32();
        }

        #endregion
       
        #region Methods

        /// <summary>
        /// Initialises the references to profiles. Called from the
        /// <see cref="Factories.MemoryFactory"/> if initialisation is enabled.
        /// </summary>
        internal void Init()
        {
            if (_name == null)
                _name = DataSet.Strings[_nameOffset].ToString();
            if (_defaultProfile == null)
                _defaultProfile = DataSet.Profiles[_defaultProfileOffset];
            if (_profiles == null)
                _profiles = GetProfiles();
        }

        /// <summary>
        /// Returns an array of the properties associated with the component.
        /// </summary>
        /// <returns>Array of properties for the component</returns>
        private Property[] GetProperties()
        {
            return DataSet.Properties.Where(i =>
                i.Component.ComponentId == ComponentId).ToArray();
        }

        /// <summary>
        /// Returns an array of all the profiles that relate to this
        /// component.
        /// </summary>
        /// <returns></returns>
        private Profile[] GetProfiles()
        {
            return DataSet.Profiles.Where(i =>
                i.Component.ComponentId == ComponentId).ToArray();
        }

        /// <summary>
        /// Compares this component to another using the numeric
        /// ComponentId field.
        /// </summary>
        /// <param name="other">The component to be compared against</param>
        /// <returns>Indication of relative value based on ComponentId field</returns>
        public int CompareTo(Component other)
        {
            return ComponentId.CompareTo(other.ComponentId);
        }

        /// <summary>
        /// Returns the components name.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name;
        }

        #endregion
    }
}
