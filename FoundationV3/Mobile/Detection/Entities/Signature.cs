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
using FiftyOne.Foundation.Mobile.Detection.Entities.Headers;
using FiftyOne.Foundation.Mobile.Detection.Readers;

namespace FiftyOne.Foundation.Mobile.Detection.Entities
{
    /// <summary>
    /// Signature of a user agent.
    /// </summary>
    /// <para>
    /// A signature contains those characters of a user agent which are relevent for the
    /// purposes of device detection. For example; most user agents will start with
    /// "Mozilla" and therefore these characters are of very little use when detecting
    /// devices. Other characters such as those that represent the model of the hardware
    /// are very relevent.
    /// </para>
    /// <para>
    /// A signature contains both an array of relevent characters from user agents identified
    /// when the data was created and the unique complete node identifies of relevent sub strings
    /// contained in multiple signatures and user agents. Together this information is used
    /// at detection time to rapidly identify the signature matching a target user agent.
    /// </para>
    /// <para>
    /// Signatures relate to device properties via profiles. Each signature relates to one
    /// profile for each component type.
    /// </para>
    /// <para>
    /// For more information about signature see https://51degrees.com/Support/Documentation/Net
    /// </para>
    /// <para>
    /// For more information see https://51degrees.com/Support/Documentation/Net
    /// </para>
    public abstract class Signature : BaseEntity, IComparable<Signature>
    {
        #region Fields

        /// <summary>
        /// Returned when the property has no values in the provide.
        /// </summary>
        private static readonly Value[] EmptyValues = new Value[0];

        /// <summary>
        /// Offsets to profiles associated with the signature.
        /// </summary>
        internal readonly int[] ProfileOffsets;

        #endregion

        #region Private Properties

        /// <summary>
        /// A dictionary relating the index of a property to the
        /// values returned by the signature. Used to speed up
        /// subsequent data processing.
        /// </summary>
        private IDictionary<int, Values> PropertyIndexToValues
        {
            get
            {
                if (_propertyIndexToValues == null)
                {
                    lock (this)
                    {
                        if (_propertyIndexToValues == null)
                        {
                            _propertyIndexToValues = new Dictionary<int, Values>();
                        }
                    }
                }
                return _propertyIndexToValues;
            }
        }
        private IDictionary<int, Values> _propertyIndexToValues;

        /// <summary>
        /// A dictionary relating the name of a property to the
        /// values returned by the signature. Used to speed up
        /// subsequent data processing.
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
                            _propertyNameToValues = new Dictionary<string, Values>();
                        }
                    }
                }
                return _propertyNameToValues;
            }
        }
        private IDictionary<string, Values> _propertyNameToValues;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the rank, where a lower number means the signature is more popular, of
        /// the signature compared to other signatures.
        /// </summary>
        public abstract int Rank { get; }

        /// <summary>
        /// The length in bytes of the signature.
        /// </summary>
        public int Length
        {
            get
            {
                if (_length == 0)
                {
                    lock (this)
                    {
                        if (_length == 0)
                        {
                            _length = GetSignatureLength();
                        }
                    }
                }
                return _length;
            }
        }
        private int _length;

        /// <summary>
        /// List of the profiles the signature relates to.
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
        private Profile[] _profiles = null;

        /// <summary>
        /// Gets the values associated with the property.
        /// </summary>
        /// <param name="property">The property whose values are required</param>
        /// <returns>Value(s) associated with the property, or null if the property does not exist</returns>
        public Values this[Property property]
        {
            get
            {
                Values values = null;
                if (PropertyIndexToValues.TryGetValue(property.Index, out values) == false)
                {
                    lock (this)
                    {
                        if (PropertyIndexToValues.TryGetValue(property.Index, out values) == false)
                        {
                            values = GetPropertyValues(property);
                            PropertyIndexToValues.Add(property.Index, values);
                        }
                    }
                }
                return values;
            }
        }

        /// <summary>
        /// Gets the values associated with the property name.
        /// </summary>
        /// <param name="propertyName">Name of the property whose values are required</param>
        /// <returns>Value(s) associated with the property, or null if the property does not exist</returns>
        public Values this[string propertyName]
        {
            get
            {
                Values values = null;
                if (PropertyNameToValues.TryGetValue(propertyName, out values) == false)
                {
                    lock (this)
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
                }
                return values;
            }
        }

        /// <summary>
        /// The unique Device Id for the signature.
        /// </summary>
        /// <para>
        /// The unique Id is formed by concatentating the profile Ids of the 
        /// profiles associated with it in ascending order of the component Id
        /// the profile relates to.
        /// </para>
        public string DeviceId
        {
            get
            {
                if (_deviceId == null)
                {
                    lock (this)
                    {
                        if (_deviceId == null)
                        {
                            _deviceId = GetDeviceId();
                        }
                    }
                }
                return _deviceId;
            }
        }
        private string _deviceId;

        #endregion

        #region Internal Properties
        
        /// <summary>
        /// Array of node offsets associated with the signature.
        /// </summary>
        internal abstract int[] NodeOffsets { get; }

        /// <summary>
        /// An array of nodes associated with the signature.
        /// </summary>
        internal Node[] Nodes
        {
            get
            {
                if (_nodes == null)
                {
                    lock (this)
                    {
                        if (_nodes == null)
                        {
                            _nodes = GetNodes();
                        }
                    }
                }
                return _nodes;
            }
        }
        private Node[] _nodes;

        /// <summary>
        /// An array of all the values irrespective of the profile associated
        /// with the signature.
        /// </summary>
        internal Value[] Values
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
        
        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a new instance of <see cref="Signature"/>
        /// </summary>
        /// <param name="dataSet">
        /// The data set the signature is contained within
        /// </param>
        /// <param name="index">
        /// The index in the data structure to the signature
        /// </param>
        /// <param name="reader">
        /// Reader connected to the source data structure and positioned to start reading
        /// </param>
        internal Signature(
            DataSet dataSet,
            int index,
            Reader reader)
            : base(dataSet, index)
        {
            ProfileOffsets = ReadOffsets(dataSet, reader, dataSet.SignatureProfilesCount);
        }

        /// <summary>
	    /// Uses the offsets list which must be locked to read in the arrays of nodes 
	    /// or profiles that relate to the signature.
        /// </summary>
        /// <param name="dataSet">The data set the signature is contained within</param>
        /// <param name="reader">Reader connected to the source data structure and positioned to start reading</param>
        /// <param name="length">The number of offsets to read in</param>
        /// <returns>An array of the offsets as integers read from the reader</returns>
	    internal static int[] ReadOffsets(DataSet dataSet, Reader reader, int length) 
        {
            reader.List.Clear();
		    for (int i = 0; i < length; i++) {
			    int profileIndex = reader.ReadInt32();
			    if (profileIndex >= 0) {
				    reader.List.Add(profileIndex);
			    }
		    }
            return reader.List.ToArray();
	    }

        #endregion

        #region Private Methods
       
        /// <summary>
        /// Gets the values associated with the property for this signature.
        /// </summary>
        /// <param name="property">Property to be returned.</param>
        /// <returns>
        /// Array of values associated with the property and signature, 
        /// or an empty array if property not found
        /// </returns>
        private Values GetPropertyValues(Property property)
        {
            var profileForProperty = Profiles.FirstOrDefault(i => property.Component.Equals(i.Component));
            return profileForProperty != null ?
                    profileForProperty[property] :
                    null;
        }

        /// <summary>
        /// Returns the unique device Id for the signature based on the
        /// profile Ids it contains.
        /// </summary>
        /// <returns></returns>
        private string GetDeviceId()
        {
            return String.Join(
                Constants.ProfileSeperator,
                Profiles.Select(i => 
                        i.ProfileId.ToString()).ToArray());
        }

        /// <summary>
        /// Returns an array of values associated with the signature.
        /// </summary>
        /// <returns></returns>
        private Value[] GetValues()
        {
            // Get the number of values in against the signature.
            var valuesCount = Profiles.Sum(i => i.ValueIndexes.Length);

            // Add the values to the array for each of the profiles.
            var values = new Value[valuesCount];
            var index = 0;
            var profileIndex = 0;
            while (index < valuesCount)
            {
                var profile = Profiles[profileIndex];
                for (int v = 0; v < profile.Values.Length; v++)
                {
                    values[index] = profile.Values[v];
                    index++;
                }
                profileIndex++;
            }

            return values;
        }

        /// <summary>
        /// Returns an array of profiles associated with the signature.
        /// </summary>
        /// <returns>Array of profiles for the signature</returns>
        private Profile[] GetProfiles()
        {
            var profiles = new Profile[ProfileOffsets.Length];
            for (int i = 0; i < ProfileOffsets.Length; i++)
            {
                profiles[i] = DataSet.Profiles[ProfileOffsets[i]];
            }
            return profiles;
        }

        #endregion

        #region Internal Methods
        
        /// <summary>
        /// The number of characters in the signature.
        /// </summary>
        /// <returns>The number of characters in the signature</returns>
        internal abstract int GetSignatureLength();

        /// <summary>
        /// Returns an array of nodes associated with the signature.
        /// </summary>
        /// <returns></returns>
        internal Node[] GetNodes()
        {
            var nodes = new Node[NodeOffsets.Length];
            for (int i = 0; i < NodeOffsets.Length; i++)
            {
                nodes[i] = DataSet.Nodes[NodeOffsets[i]];
            }
            return nodes;
        }

        /// <summary>
        /// Called after the entire data set has been loaded to ensure 
        /// any further initialisation steps that require other items in
        /// the data set can be completed.
        /// </summary>
        internal void Init()
        {
            if (_nodes == null)
                _nodes = GetNodes();
            if (_profiles == null)
                _profiles = GetProfiles();
            if (_values == null)
                _values = GetValues();
            if (_deviceId == null)
                _deviceId = GetDeviceId();
            if (_length == 0)
                _length = GetSignatureLength();
        }

        /// <summary>
        /// Returns true if the signature starts with the nodes provided.
        /// </summary>
        /// <param name="Nodes"></param>
        /// <returns></returns>
        internal bool StartsWith(List<Node> Nodes)
        {
            for (int i = 0; i < Nodes.Count && i < NodeOffsets.Length; i++)
                if (Nodes[i].Index != NodeOffsets[i])
                    return false;
            return true;
        }

        /// <summary>
        /// Compares this signature to a list of node offsets.
        /// </summary>
        /// <param name="nodes">The nodes to be compared against</param>
        /// <returns>Indication of relative value based on the node offsets</returns>
        internal int CompareTo(List<Node> nodes)
        {
            var length = Math.Min(
                NodeOffsets.Length,
                nodes.Count);

            for (int i = 0; i < length; i++)
            {
                var difference = NodeOffsets[i].CompareTo(nodes[i].Index);
                if (difference != 0)
                    return difference;
            }

            if (NodeOffsets.Length < nodes.Count)
                return -1;
            if (NodeOffsets.Length > nodes.Count)
                return 1;

            return 0;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Compares this signature to another based on the node offsets. The node
        /// offsets in both signtures must be in ascending order.
        /// </summary>
        /// <param name="other">The signature to be compared against</param>
        /// <returns>Indication of relative value based based on node offsets</returns>
        public int CompareTo(Signature other)
        {
            var length = Math.Min(
                NodeOffsets.Length,
                other.NodeOffsets.Length);

            for (int i = 0; i < length; i++)
            {
                var difference = NodeOffsets[i].CompareTo(other.NodeOffsets[i]);
                if (difference != 0)
                    return difference;
            }

            if (NodeOffsets.Length < other.NodeOffsets.Length)
                return -1;
            if (NodeOffsets.Length > other.NodeOffsets.Length)
                return 1;

            return 0;
        }

        /// <summary>
        /// String representation of the signature where irrelevant characters 
        /// are removed.
        /// </summary>
        /// <returns>The signature as a string</returns>
        public override string ToString()
        {
            if (_stringValue == null)
            {
                lock (this)
                {
                    if (_stringValue == null)
                    {
                        var buffer = new byte[Length];
                        foreach (var node in Nodes)
                            node.AddCharacters(buffer);
                        for (int i = 0; i < buffer.Length; i++)
                            if (buffer[i] == 0)
                                buffer[i] = (byte)' ';
                        _stringValue = Encoding.ASCII.GetString(buffer);
                    }
                }
            }
            return _stringValue;
        }
        private string _stringValue;

        #endregion
    }
}
