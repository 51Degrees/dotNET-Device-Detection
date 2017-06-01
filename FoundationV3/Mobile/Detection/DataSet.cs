/* *********************************************************************
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited. 
 * Copyright © 2017 51Degrees Mobile Experts Limited, 5 Charlotte Close,
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
using FiftyOne.Foundation.Mobile.Detection.Entities.Memory;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FiftyOne.Foundation.Mobile.Detection.Entities
{
    /// <summary>
    /// Data set used for device detection created by the reader classes.
    /// </summary>
    /// <para>
    /// See <see cref="DataSetBuilder"/> for a convenient way to instantiate this class.
    /// Alteratively, the <see cref="Factories.MemoryFactory"/> and 
    /// <see cref="Factories.StreamFactory"/> factories can be used instead.
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
    public class DataSet : IDeviceDetectionDataSet, IDisposable
    {
        #region Classes

        /// <summary>
        /// Used to search for a signature from a list of nodes.
        /// </summary>
        internal class SearchSignatureByNodes : SearchBase<Signature, IList<Node>, IReadonlyList<Signature>>
        {
            private readonly IReadonlyList<Signature> _signatures;

            internal SearchSignatureByNodes(IReadonlyList<Signature> signatures)
            {
                _signatures = signatures;
            }

            internal int BinarySearch(IList<Node> nodes, out int iterations)
            {
                return BinarySearchBase(this._signatures, nodes, out iterations);
            }

            protected override int GetCount(IReadonlyList<Signature> signatures)
            {
                return signatures.Count;
            }

            protected override Signature GetValue(IReadonlyList<Signature> signatures, int index)
            {
                return signatures[index];
            }

            protected override int CompareTo(Signature signature, IList<Node> nodes)
            {
                return signature.CompareTo(nodes);
            }
        }

        /// <summary>
        /// Used to search for a profile offset from a profile id.
        /// </summary>
        private class SearchProfileOffsetByProfileId : SearchBase<ProfileOffset, int, IReadonlyList<ProfileOffset>>
        {
            private readonly IReadonlyList<ProfileOffset> _profileOffsets;

            internal SearchProfileOffsetByProfileId(IReadonlyList<ProfileOffset> profileOffsets)
            {
                _profileOffsets = profileOffsets;
            }

            internal int BinarySearch(int profileId)
            {
                return BinarySearchBase(this._profileOffsets, profileId);
            }

            protected override int GetCount(IReadonlyList<ProfileOffset> profiles)
            {
                return profiles.Count;
            }

            protected override ProfileOffset GetValue(IReadonlyList<ProfileOffset> profiles, int index)
            {
                return _profileOffsets[index];
            }

            protected override int CompareTo(ProfileOffset profileOffset, int profileId)
            {
                return profileOffset.ProfileId.CompareTo(profileId);
            }
        }

        #endregion

        #region Enumerations

        /// <summary>
        /// The modes of operation the data set can be built in.
        /// </summary>
        public enum Modes
        {
            /// <summary>
            /// The device data is held on disk and loaded into memory
            /// when needed. Caching is used to clear out stale items.
            /// Lowest memory use and slowest device deteciton.
            /// </summary>
            File,
            /// <summary>
            /// The device data is loaded into memory as .NET class instances
            /// and then is no longer referenced. Offers the fastest device 
            /// detection in .NET managed code, but a slower startup time.
            /// </summary>
            Memory,
            /// <summary>
            /// The device data is loaded into memory as a byte array. .NET
            /// class instances are created when needed and then cleared from
            /// the cache.
            /// </summary>
            MemoryMapped
        }

        #endregion

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
                double result = 0;
                IndirectDataSet ids = this as IndirectDataSet;
                if (ids != null)
                {
                    result = ids.CacheMap.SignatureCache.PercentageMisses;
                }

                return result;
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
                double result = 0;
                IndirectDataSet ids = this as IndirectDataSet;
                if (ids != null)
                {
                    result = ids.CacheMap.NodeCache.PercentageMisses;
                }

                return result;
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
                double result = 0;
                IndirectDataSet ids = this as IndirectDataSet;
                if (ids != null)
                {
                    result = ids.CacheMap.StringCache.PercentageMisses;
                }

                return result;
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
                double result = 0;
                IndirectDataSet ids = this as IndirectDataSet;
                if (ids != null)
                {
                    result = ids.CacheMap.ProfileCache.PercentageMisses;
                }

                return result;
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
                double result = 0;
                IndirectDataSet ids = this as IndirectDataSet;
                if (ids != null)
                {
                    result = ids.CacheMap.ValueCache.PercentageMisses;
                }

                return result;
            }
        }

        /// <summary>
        /// The percentage of requests for ranked signatures which were not already
        /// contained in the cache.
        /// </summary>
        /// <remarks>
        /// A value is only returned when operating in Stream mode.
        /// </remarks>
        [Obsolete("Ranked signatures are no longer cached")]
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
        [Obsolete("Cache no longer requires switching")]
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
        [Obsolete("Cache no longer requires switching")]
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
        [Obsolete("Cache no longer requires switching")]
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
        [Obsolete("Cache no longer requires switching")]
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
        [Obsolete("Cache no longer requires switching")]
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
        [Obsolete("Ranked signatures are no longer cached")]
        public long RankedSignatureCacheSwitches
        {
            get
            {
                return 0;
            }
        }
        
        #endregion

        #region Data Set Properties

        /// <summary>
        /// The mode of operation the data set is using.
        /// </summary>
        public readonly Modes Mode;

        /// <summary>
        /// List of unique Http Headers that the data set needs to consider
        /// to perform the most accurate matches.
        /// </summary>
        public string[] HttpHeaders
        {
            get
            {
                if (_httpHeaders == null)
                {
                    lock (this)
                    {
                        if (_httpHeaders == null)
                        {
                            _httpHeaders = Components.SelectMany(i =>
                                i.HttpHeaders).Distinct().ToArray();
                        }
                    }
                }
                return _httpHeaders;
            }
        }
        private string[] _httpHeaders;

        /// <summary>
        /// A unique Tag for the exported data.
        /// </summary>
        public Guid Export { get; internal set; }

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
        /// The minimum number times a User-Agent should have been seen before
        /// it was included in the dataset.
        /// </summary>
        public int MinUserAgentCount { get; internal set; }

        /// <summary>
        /// The version of the data set.
        /// </summary>
        public Version Version { get; internal set; }

        /// <summary>
        /// The version of the data set as an enum.
        /// </summary>
        public BinaryConstants.FormatVersions VersionEnum { get; internal set; }

        /// <summary>
        /// The maximum length of a User-Agent string.
        /// </summary>
        public short MaxUserAgentLength { get; internal set; }

        /// <summary>
        /// The minimum length of a User-Agent string.
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
        /// bogus User-Agents which deliberately require so many signatures to be 
        /// checked that performance is degraded.
        /// </summary>
        public int MaxSignatures { get; internal set; }

        /// <summary>
        /// The number of profiles each signature can contain.
        /// </summary>
        public int SignatureProfilesCount { get; internal set; }

        /// <summary>
        /// The number of nodes each signature can contain.
        /// </summary>
        public int SignatureNodesCount { get; internal set; }

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
        /// The largest rank value that can be returned.
        /// </summary>
        public int MaximumRank
        {
            get
            {
                if (_maximumRank == 0 &&
                    RankedSignatureIndexes != null)
                {
                    lock(this)
                    {
                        if (_maximumRank == 0 &&
                            RankedSignatureIndexes != null)
                        {
                            _maximumRank = RankedSignatureIndexes.Count;
                        }
                    }
                }
                return _maximumRank;
            }
        }
        internal int _maximumRank = 0;

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
        public MemoryFixedList<Component, DataSet> Components
        {
            get { return _components; }
        }
        internal MemoryFixedList<Component, DataSet> _components;

        /// <summary>
        /// A list of all the maps the data set contains.
        /// </summary>
        public MemoryFixedList<Map, DataSet> Maps
        {
            get { return _maps; }
        }
        internal MemoryFixedList<Map, DataSet> _maps;

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
        internal ISimpleList RankedSignatureIndexes
        {
            get { return _rankedSignatureIndexes; }
        }
        internal ISimpleList _rankedSignatureIndexes;

        /// <summary>
        /// List of profile offsets the data set contains.
        /// </summary>
        public IReadonlyList<ProfileOffset> ProfileOffsets
        {
            get { return _profileOffsets; }
        }
        internal IReadonlyList<ProfileOffset> _profileOffsets;

        /// <summary>
        /// List of integers that represent ranked signature indexes.
        /// </summary>
        internal ISimpleList NodeRankedSignatureIndexes
        {
            get { return _nodeRankedSignatureIndexes; }
        }
        internal ISimpleList _nodeRankedSignatureIndexes;

        /// <summary>
        /// List of integers that represent signature node offsets.
        /// </summary>
        internal ISimpleList SignatureNodeOffsets
        {
            get { return _signatureNodeOffsets; }
        }
        internal ISimpleList _signatureNodeOffsets;

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
        /// Nodes for each of the possible character positions in the User-Agent.
        /// </summary>
        internal IReadonlyList<Node> RootNodes;

        /// <summary>
        /// A list of ASCII byte arrays for strings used by the dataset.
        /// </summary>
        internal IReadonlyList<AsciiString> Strings;

        #endregion

        #region Internal Properties

        /// <summary>
        /// Advises FindProfiles methods that the profiles associated with 
        /// a value should be referenced explicitly.
        /// </summary>
        /// <remarks>
        /// Used to increase performance of memory based datasets when
        /// returning true. Returning false will not persist data in 
        /// memory when used in stream operation.
        /// </remarks>
        internal virtual bool FindProfilesInitialiseValueProfiles { get { return true; } }

        /// <summary>
        /// An instance of the signature search.
        /// </summary>
        internal DataSet.SearchSignatureByNodes SignatureSearch
        {
            get
            {
                if (_signatureSearch == null)
                {
                    lock (this)
                    {
                        if (_signatureSearch == null)
                        {
                            _signatureSearch = new 
                                DataSet.SearchSignatureByNodes(this.Signatures);
                        }
                    }
                }
                return _signatureSearch;
            }
        }
        private SearchSignatureByNodes _signatureSearch;
        
        /// <summary>
        /// An instance of the profile offset search.
        /// </summary>
        private DataSet.SearchProfileOffsetByProfileId ProfileOffsetSearch
        {
            get
            {
                if (_profileOffsetSearch == null)
                {
                    lock (this)
                    {
                        if (_profileOffsetSearch == null)
                        {
                            _profileOffsetSearch = new 
                                SearchProfileOffsetByProfileId(ProfileOffsets);
                        }
                    }
                }
                return _profileOffsetSearch;
            }
        }
        private SearchProfileOffsetByProfileId _profileOffsetSearch;


        /// <summary>
        /// Used to find values based on name.
        /// </summary>
        internal SearchReadonlyList<Value, string> ValuesNameSearch
        {
            get
            {
                if (_valuesNameSearch == null)
                {
                    lock (this)
                    {
                        if (_valuesNameSearch == null)
                        {
                            _valuesNameSearch = new SearchReadonlyList<Value, string>(Values);
                        }
                    }
                }
                return _valuesNameSearch;
            }
        }
        private SearchReadonlyList<Value, string> _valuesNameSearch = null;

#if !SQL_BUILD && !NETCORE_BUILD

        /// <summary>
        /// An array of the properties that are of type JavaScript.
        /// </summary>
        internal Property[] JavaScriptProperties
        {
            get
            {
                if (_javaScriptProperties == null)
                {
                    lock (this)
                    {
                        if (_javaScriptProperties == null)
                        {
                            _javaScriptProperties = Properties.Where(i =>
                                i._valueType ==
                                Property.PropertyValueType.JavaScript).ToArray();
                        }
                    }
                }
                return _javaScriptProperties;
            }
        }
        private Property[] _javaScriptProperties = null;

        /// <summary>
        /// Find all the properties that are of type JavaScript and are marked
        /// with the property value override category. 
        /// </summary>
        internal Property[] PropertyValueOverrideProperties
        {
            get
            {
                if (_propertyValueOverrideProperties == null)
                {
                    lock (this)
                    {
                        if (_propertyValueOverrideProperties == null)
                        {
                            _propertyValueOverrideProperties =
                                JavaScriptProperties.Where(i =>
                                    i.Category.Equals(
                                    Constants.PropertyValueOverrideCategory)
                                ).ToArray();
                        }
                    }
                }
                return _propertyValueOverrideProperties;
            }
        }
        private Property[] _propertyValueOverrideProperties;

        /// <summary>
        /// Find all the properties that are of type JavaScript and are marked
        /// with the property value override category. 
        /// </summary>
        /// <returns>Array of JavaScript properties for this feature.</returns>
        private static Property[] GetJavaScriptProperties()
        {
            return WebProvider.ActiveProvider.DataSet.JavaScriptProperties.
                Where(i => i.Category.Equals(
                    Constants.PropertyValueOverrideCategory)).ToArray();
        }

#endif
        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a new data set ready to have lists of data assigned
        /// to it.
        /// </summary>
        /// <param name="lastModified">
        /// The date and time the source of the data was last modified.
        /// </param>
        /// <param name="mode">
        /// The mode of operation the data set will be using.
        /// </param>
        internal DataSet(DateTime lastModified, Modes mode)
        {
            LastModified = lastModified;
            Mode = mode;
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
            Parallel.ForEach(Components, entity => 
            {
                entity.Init();
            });
            Parallel.ForEach(Properties, entity =>
            {
                entity.Init();
            });
            Parallel.ForEach(Values, entity =>
            {
                entity.Init();
            });
            Parallel.ForEach(Nodes, entity =>
            {
                entity.Init();
            });
            Parallel.ForEach(Signatures, entity =>
            {
                entity.Init();
            });
            
            // We no longer need the strings data structure as all dependent
            // data has been taken from it.
            Strings = null;
        }

        /// <summary>
        /// This method is no longer supported. See <see cref="IndirectDataSet.CacheMap"/> 
        /// for details of caches and thier performance stats.
        /// </summary>
        /// <param name="list"></param>
        /// <returns>zero</returns>
        [Obsolete("See IndirectDataSet.CacheMap for caches and details of thier performance stats", true)]
        private static double PercentageMisses(object list)
        {
            return 0;
        }

        /// <summary>
        /// Returns the number of times the cache lists were switched.
        /// </summary>
        /// <param name="list"></param>
        /// <returns>zero</returns>
        [Obsolete("Cache no longer requires switching")]
        private static long Switches(object list)
        {
            return 0;
        }

        #endregion

        #region Destructor

        /// <summary>
        /// Disposes of all the lists that form the dataset.
        /// </summary>
        ~DataSet()
        {
            Dispose(false);
        }

        /// <summary>
        /// Disposes of all the lists that form the dataset.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Disposes of the readonly lists used by the dataset.
        /// </summary>
        /// <param name="disposing">True if the calling method is Dispose, false for the finaliser.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (Values != null)
            {
                Values.Dispose();
            }
            if (Signatures != null)
            {
                Signatures.Dispose();
            }
            if (ProfileOffsets != null)
            {
                ProfileOffsets.Dispose();
            }
            if (Profiles != null)
            {
                Profiles.Dispose();
            }
            if (Nodes != null)
            {
                Nodes.Dispose();
            }
            if (RootNodes != null)
            {
                RootNodes.Dispose();
            }
            if (Strings != null)
            {
                Strings.Dispose();
            }
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Searches the list of profile Ids and returns the profile if the profile 
        /// id is valid.
        /// </summary>
        /// <param name="profileId">Id of the profile to be found</param>
        /// <returns>Profile related to the id, or null if none found</returns>
        public Profile FindProfile(int profileId)
        {
            int index = ProfileOffsetSearch.BinarySearch(profileId);
            return index < 0 ? null : Profiles[ProfileOffsets[index].Offset];
        }

        /// <summary>
        /// Gets the profiles associated with the property name and value
        /// which intersects with the filterProfiles if provided.
        /// For best performance of this method, ensure the 
        /// <see cref="CacheType.ValuesCache"/> is configured to be as large 
        /// as possible.
        /// The total number of Values objects depends on the data file:
        /// Enterprise: approx. 200,000
        /// Premium: approx. 180,000
        /// Lite: approx. 3,000
        /// </summary>
        /// <param name="propertyName">Name of the property</param>
        /// <param name="valueName">
        /// Name of the value associated with the property
        /// </param>
        /// <param name="filterProfiles">
        /// Array of profiles ordered in ascending Index order. Null if 
        /// no filter is required.
        /// </param>
        /// <returns>Array of profiles ordered in ascending Index order.</returns>
        public Profile[] FindProfiles(string propertyName, string valueName, Profile[] filterProfiles = null)
        {
            var property = Properties[propertyName];
            if (property == null)
            {
                throw new ArgumentException(String.Format(
                    "Property '{0}' does not exist in the '{1}' data set. " +
                    "Upgrade to a different data set which includes the property.",
                    propertyName,
                    this.Name));
            }
            return property.FindProfiles(valueName, filterProfiles);
        }

        /// <summary>
        /// Gets the profiles associated with the property name and value
        /// which intersects with the filterProfiles if provided.
        /// For best performance of this method, ensure the 
        /// <see cref="CacheType.ValuesCache"/> is configured to be as large 
        /// as possible.
        /// The total number of Values objects depends on the data file:
        /// Enterprise: approx. 200,000
        /// Premium: approx. 180,000
        /// Lite: approx. 3,000
        /// </summary>
        /// <param name="property">Instance of the property required</param>
        /// <param name="valueName">
        /// Name of the value associated with the property
        /// </param>
        /// <param name="filterProfiles">
        /// Array of profiles ordered in ascending Index order. Null if 
        /// no filter is required.
        /// </param>
        /// <returns>Array of profiles ordered in ascending Index order.</returns>
        public Profile[] FindProfiles(Property property, string valueName, Profile[] filterProfiles = null)
        {
            return property.FindProfiles(valueName, filterProfiles);
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
        [Obsolete("Replaced by the Properties list string accessor.")]
        public Property GetProperty(string propertyName)
        {
            return _properties[propertyName];
        }

        /// <summary>
        /// If there are cached lists being used the stats are reset for them.
        /// </summary>
        public virtual void ResetCache()
        {
            // Do nothing in this implementation.
        }
        
        #endregion        
    }
}