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
using System.IO;
using FiftyOne.Foundation.Mobile.Detection.Entities.Memory;
using System.Linq;

namespace FiftyOne.Foundation.Mobile.Detection.Entities
{
    /// <summary>
    /// Data set used for device detection created by the reader classes.
    /// </summary>
    /// <para>
    /// The <see cref="Factories.MemoryFactory"/> and <see cref="Factories.StreamFactory"/> factories
    /// should be used to create detector data sets. They can not be constructed
    /// directly from external code.
    /// </para>
    /// <para>
    /// All information about the detector data set is exposed in this class including
    /// meta data and data used for device detection in the form of lists.
    /// </para>
    /// <remarks>
    /// Detector data sets created using the <see cref="Factories.StreamFactory"/> factory
    /// using a file must be disposed of to ensure any readers associated with the
    /// file are closed elegantly.
    /// </remarks>
    /// <para>
    /// For more information see https://51degrees.com/Support/Documentation/Net
    /// </para>
    public class DataSet : IDisposable
    {
        #region Public Properties
              
        #region Cache Stats

        /// <summary>
        /// The percentage of requests for signatures which were not already
        /// contained in the cache.
        /// </summary>
        /// <remarks>
        /// A value is only returned when operating in Stream mode.
        /// </remarks>
        public double PercentageSignatureCacheMisses
        {
            get
            {
                return PercentageMisses(Signatures);
            }
        }

        /// <summary>
        /// The percentage of requests for nodes which were not already
        /// contained in the cache.
        /// </summary>
        /// <remarks>
        /// A value is only returned when operating in Stream mode.
        /// </remarks>
        public double PercentageNodeCacheMisses
        {
            get
            {
                return PercentageMisses(Nodes);
            }
        }

        /// <summary>
        /// The percentage of requests for strings which were not already
        /// contained in the cache.
        /// </summary>
        /// <remarks>
        /// A value is only returned when operating in Stream mode.
        /// </remarks>
        public double PercentageStringsCacheMisses
        {
            get
            {
                return PercentageMisses(Strings);
            }
        }

        /// <summary>
        /// The percentage of requests for profiles which were not already
        /// contained in the cache.
        /// </summary>
        /// <remarks>
        /// A value is only returned when operating in Stream mode.
        /// </remarks>
        public double PercentageProfilesCacheMisses
        {
            get
            {
                return PercentageMisses(Profiles);
            }
        }

        /// <summary>
        /// The percentage of requests for values which were not already
        /// contained in the cache.
        /// </summary>
        /// <remarks>
        /// A value is only returned when operating in Stream mode.
        /// </remarks>
        public double PercentageValuesCacheMisses
        {
            get
            {
                return PercentageMisses(Values);
            }
        }

        /// <summary>
        /// The percentage of requests for ranked signatures which were not already
        /// contained in the cache.
        /// </summary>
        /// <remarks>
        /// A value is only returned when operating in Stream mode.
        /// </remarks>
        public double PercentageRankedSignatureCacheMisses
        {
            get
            {
                return PercentageMisses(RankedSignatureIndexes);
            }
        }

        /// <summary>
        /// Number of times the signature cache was switched.
        /// </summary>
        /// <remarks>
        /// A value is only returned when operating in Stream mode.
        /// </remarks>
        public long SignatureCacheSwitches
        {
            get
            {
                return Switches(Signatures);
            }
        }

        /// <summary>
        /// Number of times the node cache was switched.
        /// </summary>
        /// <remarks>
        /// A value is only returned when operating in Stream mode.
        /// </remarks>
        public long NodeCacheSwitches
        {
            get
            {
                return Switches(Nodes);
            }
        }

        /// <summary>
        /// Number of times the strings cache was switched.
        /// </summary>
        /// <remarks>
        /// A value is only returned when operating in Stream mode.
        /// </remarks>
        public long StringsCacheSwitches
        {
            get
            {
                return Switches(Strings);
            }
        }

        /// <summary>
        /// Number of times the profiles cache was switched.
        /// </summary>
        /// <remarks>
        /// A value is only returned when operating in Stream mode.
        /// </remarks>
        public long ProfilesCacheSwitches
        {
            get
            {
                return Switches(Profiles);
            }
        }

        /// <summary>
        /// Number of times the values cache was switched.
        /// </summary>
        /// <remarks>
        /// A value is only returned when operating in Stream mode.
        /// </remarks>
        public long ValuesCacheSwitches
        {
            get
            {
                return Switches(Values);
            }
        }

        /// <summary>
        /// Number of times the ranked signature cache was switched.
        /// </summary>
        /// <remarks>
        /// A value is only returned when operating in Stream mode.
        /// </remarks>
        public long RankedSignatureCacheSwitches
        {
            get
            {
                return Switches(RankedSignatureIndexes);
            }
        }
        
        #endregion

        #region Data Set Properties

        /// <summary>
        /// A unique Tag for the data set.
        /// </summary>
        public Guid Tag { get; internal set; }

        /// <summary>
        /// The date the data source last modified if application.
        /// </summary>
        public DateTime LastModified { get; internal set; }

        /// <summary>
        /// The date the data set was published.
        /// </summary>
        public DateTime Published { get; internal set; }

        /// <summary>
        /// The date the data set is next expected to be updated by 51Degrees.
        /// </summary>
        public DateTime NextUpdate { get; internal set; }

        /// <summary>
        /// The minimum number times a user agent should have been seen before
        /// it was included in the dataset.
        /// </summary>
        public int MinUserAgentCount { get; internal set; }

        /// <summary>
        /// The version of the data set.
        /// </summary>
        public Version Version { get; internal set; }

        /// <summary>
        /// The maximum length of a user agent string.
        /// </summary>
        public short MaxUserAgentLength { get; internal set; }

        /// <summary>
        /// The minimum length of a user agent string.
        /// </summary>
        public short MinUserAgentLength { get; internal set; }
        
        /// <summary>
        /// The lowest character the character trees can contain.
        /// </summary>
        public byte LowestCharacter { get; internal set; }

        /// <summary>
        /// The highest character the character trees can contain.
        /// </summary>
        public byte HighestCharacter { get; internal set; }
               
        /// <summary>
        /// The number of unique device combinations available in the data set.
        /// </summary>
        public int DeviceCombinations { get; internal set; }

        /// <summary>
        /// The maximum number of signatures that can be checked. Needed to avoid
        /// bogus user agents which deliberately require so many signatures to be 
        /// checked that performance is degraded.
        /// </summary>
        public int MaxSignatures { get; internal set; }

        /// <summary>
        /// The number of profiles each signature can contain.
        /// </summary>
        internal int SignatureProfilesCount { get; set; }

        /// <summary>
        /// The number of nodes each signature can contain.
        /// </summary>
        internal int SignatureNodesCount { get; set; }

        /// <summary>
        /// The maximum number of values that can be returned by a profile
        /// and a property supporting a list of values.
        /// </summary>
        internal short MaxValues { get; set; }

        /// <summary>
        /// The number of bytes to allocate to a buffer returning
        /// CSV format data for a match.
        /// </summary>
        public int CsvBufferLength { get; internal set; }

        /// <summary>
        /// The number of bytes to allocate to a buffer returning
        /// JSON format data for a match.
        /// </summary>
        public int JsonBufferLength { get; internal set; }

        /// <summary>
        /// The number of bytes to allocate to a buffer returning
        /// XML format data for a match.
        /// </summary>
        public int XmlBufferLength { get; internal set; }

        /// <summary>
        /// The maximum number of signatures that could possibly 
        /// be returned during a closest match.
        /// </summary>
        internal int MaxSignaturesClosest { get; set; }
        
        /// <summary>
        /// Set when the disposed method is called indicating the data
        /// set is no longer valid and can't be used.
        /// </summary>
        public bool Disposed
        {
            get
            {
                return _disposed;
            }
        }
        private bool _disposed = false;

        /// <summary>
        /// The hardware component.
        /// </summary>
        public Component Hardware
        {
            get
            {
                if (_hardware == null)
                {
                    lock (this)
                    {
                        if (_hardware == null)
                        {
                            _hardware = GetComponent("HardwarePlatform");
                        }
                    }
                }
                return _hardware;
            }
        }
        private Component _hardware;

        /// <summary>
        /// The software component.
        /// </summary>
        public Component Software
        {
            get
            {
                if (_software == null)
                {
                    lock (this)
                    {
                        if (_software == null)
                        {
                            _software = GetComponent("SoftwarePlatform");
                        }
                    }
                }
                return _software;
            }
        }
        private Component _software;

        /// <summary>
        /// The browser component.
        /// </summary>
        public Component Browsers
        {
            get
            {
                if (_browsers == null)
                {
                    lock (this)
                    {
                        if (_browsers == null)
                        {
                            _browsers = GetComponent("BrowserUA");
                        }
                    }
                }
                return _browsers;
            }
        }
        private Component _browsers;

        /// <summary>
        /// The crawler component.
        /// </summary>
        public Component Crawlers
        {
            get
            {
                if (_crawlers == null)
                {
                    lock (this)
                    {
                        if (_crawlers == null)
                        {
                            _crawlers = GetComponent("Crawler");
                        }
                    }
                }
                return _crawlers;
            }
        }
        private Component _crawlers;

        /// <summary>
        /// The time that has elapsed since the data in the data set was current.
        /// </summary>
        public TimeSpan Age
        {
            get
            {
                return (DateTime.UtcNow - Published) - AgeAtPublication;
            }
        }
        internal TimeSpan AgeAtPublication;

        /// <summary>
        /// The copyright notice associated with the data set.
        /// </summary>
        public string Copyright
        {
            get 
            {
                if (_copyright == null)
                {
                    lock (this)
                    {
                        if (_copyright == null)
                        {
                            _copyright = Strings[CopyrightOffset].ToString();
                        }
                    }
                }
                return _copyright; 
            }
        }
        private string _copyright;
        internal int CopyrightOffset { private get; set; }

        /// <summary>
        /// The common name of the data set.
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
                            _name = Strings[NameOffset].ToString();
                        }
                    }
                }
                return _name; 
            }
        }
        internal int NameOffset;
        private string _name;

        /// <summary>
        /// The name of the property map used to create the dataset.
        /// </summary>
        public string Format
        {
            get 
            {
                if (_format == null)
                {
                    lock (this)
                    {
                        if (_format == null)
                        {
                            _format = Strings[FormatOffset].ToString();
                        }
                    }
                }
                return _format; 
            }
        }
        internal int FormatOffset;
        private string _format;
                
        #endregion

        #endregion

        #region Public Lists

        /// <summary>
        /// A list of all the components the data set contains.
        /// </summary>
        public MemoryFixedList<Component> Components
        {
            get { return _components; }
        }
        internal MemoryFixedList<Component> _components;

        /// <summary>
        /// A list of all the maps the data set contains.
        /// </summary>
        public MemoryFixedList<Map> Maps
        {
            get { return _maps; }
        }
        internal MemoryFixedList<Map> _maps;

        /// <summary>
        /// A list of all properties the data set contains.
        /// </summary>
        public PropertiesList Properties
        {
            get { return _properties; }
        }
        internal PropertiesList _properties;

        /// <summary>
        /// A list of all property values the data set contains.
        /// </summary>
        public IReadonlyList<Value> Values
        {
            get { return _values; }
        }
        internal IReadonlyList<Value> _values;

        /// <summary>
        /// List of signatures the data set contains.
        /// </summary>
        public IReadonlyList<Signature> Signatures
        {
            get { return _signatures; }
        }
        internal IReadonlyList<Signature> _signatures;

        /// <summary>
        /// A list of signature indexes ordered in ascending order of rank.
        /// Used by the node ranked signature indexes lists to identify
        /// the corresponding signature.
        /// </summary>
        public IReadonlyList<RankedSignatureIndex> RankedSignatureIndexes
        {
            get { return _rankedSignatureIndexes; }
        }
        internal IReadonlyList<RankedSignatureIndex> _rankedSignatureIndexes;

        /// <summary>
        /// List of profile offsets the data set contains.
        /// </summary>
        public IReadonlyList<ProfileOffset> ProfileOffsets
        {
            get { return _profileOffsets; }
        }
        internal IReadonlyList<ProfileOffset> _profileOffsets;

        #endregion

        #region Internal Lists
        
        /// <summary>
        /// A list of all the possible profiles the data set contains.
        /// </summary>
        internal IReadonlyList<Profile> Profiles;

        /// <summary>
        /// List of nodes the data set contains.
        /// </summary>
        internal IReadonlyList<Node> Nodes;

        /// <summary>
        /// Nodes for each of the possible character positions in the user agent.
        /// </summary>
        internal IReadonlyList<Node> RootNodes;

        /// <summary>
        /// A list of ASCII byte arrays for strings used by the dataset.
        /// </summary>
        internal IReadonlyList<AsciiString> Strings;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a new data set ready to have lists of data assigned
        /// to it.
        /// </summary>
        /// <param name="lastModified">
        /// The date and time the source of the data was last modified.
        /// </param>
        internal DataSet(DateTime lastModified)
        {
            LastModified = lastModified;
        }

        /// <summary>
        /// Called after the entire data set has been loaded to ensure 
        /// any further initialisation steps that require other items in
        /// the data set can be completed.
        /// </summary>
        internal void Init()
        {
            // Set the string values of the data set.
            _name = Strings[NameOffset].ToString();
            _format = Strings[FormatOffset].ToString();
            _copyright = Strings[CopyrightOffset].ToString();

            // Initialise any objects that can be pre referenced to speed up
            // initial matching.
            foreach (var entity in Components)
                entity.Init();
            foreach (var entity in Properties)
                entity.Init();
            foreach (var entity in Values)
                entity.Init();
            foreach (var entity in Profiles)
                entity.Init();
            foreach (var entity in Nodes)
                entity.Init();
            foreach (var entity in Signatures)
                entity.Init();
            foreach (var entity in RankedSignatureIndexes)
                entity.Init();
            
            // We no longer need the strings data structure as all dependent
            // data has been taken from it.
            Strings = null;
        }

        /// <summary>
        /// Returns the percentage of requests that weren't serviced by the cache.
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private static double PercentageMisses(object list)
        {
            if (list is Stream.ICacheList)
            {
                return ((Stream.ICacheList)list).PercentageMisses;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Returns the number of times the cache lists were switched.
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private static long Switches(object list)
        {
            if (list is Stream.ICacheList)
            {
                return ((Stream.ICacheList)list).Switches;
            }
            else
            {
                return 0;
            }
        }

        #endregion
        
        #region Public Methods

        /// <summary>
        /// Disposes of all the lists that form the dataset.
        /// </summary>
        public virtual void Dispose()
        {
            _disposed = true;
        }


        /// <summary>
        /// Searches the list of profile Ids and returns the profile if the profile 
        /// id is valid.
        /// </summary>
        /// <param name="profileId">Id of the profile to be found</param>
        /// <returns>Profile related to the id, or null if none found</returns>
        public Profile FindProfile(int profileId)
        {
            var lower = 0;
            var upper = ProfileOffsets.Count - 1;

            while (lower <= upper)
            {
                var middle = lower + (upper - lower) / 2;
                var comparisonResult = ProfileOffsets[middle].ProfileId.CompareTo(profileId);
                if (comparisonResult == 0)
                    return Profiles[ProfileOffsets[middle].Offset];
                else if (comparisonResult > 0)
                    upper = middle - 1;
                else
                    lower = middle + 1;
            }

            return null;
        }

        /// <summary>
        /// Returns the <see cref="Component"/> associated with the name provided.
        /// </summary>
        /// <param name="componentName">Name of the component required</param>
        /// <returns>The component matching the name, or null if no component is found</returns>
        public Component GetComponent(string componentName)
        {
            return _components.FirstOrDefault(i => i.Name == componentName);
        }

        /// <summary>
        /// Returns the property associated with the property name.
        /// </summary>
        /// <param name="propertyName">Name of the property whose values are required</param>
        /// <returns><see cref="Property"/> object associated with the name</returns>
        public Property GetProperty(string propertyName)
        {
            return _properties[propertyName];
        }
        
        #endregion
    }
}