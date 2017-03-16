/* *********************************************************************
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited.
 * Copyright (c) 2017 51Degrees Mobile Experts Limited, 5 Charlotte Close,
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
 * This Source Code Form is "Incompatible With Secondary Licenses", as
 * defined by the Mozilla Public License, v. 2.0.
 */

using FiftyOne.Foundation.Mobile.Detection.Caching;
using FiftyOne.Foundation.Mobile.Detection.Entities;
using FiftyOne.Foundation.Mobile.Detection.Entities.Headers;
using FiftyOne.Foundation.Mobile.Detection.Entities.Stream;
using FiftyOne.Foundation.Mobile.Detection.Entities.Memory;
using FiftyOne.Foundation.Mobile.Detection.Factories;
using FiftyOne.Foundation.Mobile.Detection.Readers;
using System;
using System.Collections.Generic;
using System.IO;
using DataSet = FiftyOne.Foundation.Mobile.Detection.Entities.DataSet;

namespace FiftyOne.Foundation.Mobile.Detection
{

    /// <summary>
    /// Cache types for Stream DataSet
    /// </summary>
    public enum CacheType
    {
        /// <summary>
        /// The cache used to store <see cref="AsciiString"/> objects 
        /// </summary>
        StringsCache,
        /// <summary>
        /// The cache used to store <see cref="Entities.Node"/> objects 
        /// </summary>
        NodesCache,
        /// <summary>
        /// The cache used to store <see cref="Value"/> objects
        /// </summary>
        ValuesCache,
        /// <summary>
        /// The cache used to store <see cref="Entities.Profile"/> objects 
        /// </summary>
        ProfilesCache,
        /// <summary>
        /// The cache used to store <see cref="Signature"/> objects 
        /// </summary>
        SignaturesCache
    }

    /// <summary>
    /// Used for creating a <see cref="DataSet"/>.
    /// This uses the fluent builder pattern to create a DataSet with 
    /// minimal effort from the user initially while also allowing a 
    /// deep level of customisation if requried.
    /// </summary>
    public class DataSetBuilder
    {
        #region Classes

        /// <summary>
        /// Used to specify different preset caching configurations.
        /// Extensive testing has been carried out to determine the optimum
        /// cache configuration for device detection running in various 
        /// different environments.
        /// <para>
        /// This class uses the 'SmartEnum' pattern, which restricts
        /// the class instances to only being created by the readonly 
        /// properties defined against it.
        /// </para>
        /// Example usage:
        /// <code>
        /// builder.ConfigureCachesFromTemplate(CacheTemplate.Default)
        /// </code>
        /// </summary>
        public abstract class CacheTemplate : ICacheSet
        {
            #region Classes

            /// <summary>
            /// Used by the public fields of <see cref="CacheTemplate"/> to create a new 
            /// instance and populate it with the required configuration.
            /// </summary>
            private class InternalCacheTemplate : CacheTemplate
            {
                private Dictionary<CacheType, ICacheOptions> _options = new Dictionary<CacheType, ICacheOptions>();

                public InternalCacheTemplate(Dictionary<CacheType, ICacheOptions> options)
                {
                    foreach (var element in options) { _options.Add(element.Key, element.Value); }
                }

                public override Dictionary<CacheType, ICacheOptions> GetCacheConfiguration()
                {
                    return _options;
                }
            }

            #region Standard Caches

            /// <summary>
            /// The default settings. All-around good performance. 
            /// Suitable for most users but not optimised for any specific
            /// environment.
            /// </summary>
            public static readonly CacheTemplate Default = 
                new InternalCacheTemplate(defaultCacheSizes);
            /// <summary>
            /// Configuration optimised for a single-threaded environment
            /// where memory usage is not a particular concern.
            /// </summary>
            public static readonly CacheTemplate SingleThread = 
                new InternalCacheTemplate(StCacheSizes);
            /// <summary>
            /// Configuration optimised for a single-threaded environment
            /// where low memory usage is required.
            /// </summary>
            public static readonly CacheTemplate SingleThreadLowMemory = 
                new InternalCacheTemplate(StlmCacheSizes);
            /// <summary>
            /// Configuration optimised for a multi-threaded environemnt
            /// where memory usage is not a particular concern.
            /// Note: Currently, the .NET API uses the same configuration
            /// for multi-threaded and single-threaded templates.
            /// This may change in future in response to testing.
            /// </summary>
            public static readonly CacheTemplate MultiThread = 
                new InternalCacheTemplate(MtCacheSizes);
            /// <summary>
            /// Configuration optimised for a multi-threaded environemnt
            /// where low memory usage is required.
            /// Note: Currently, the .NET API uses the same configuration
            /// for multi-threaded and single-threaded templates.
            /// This may change in future in response to testing.
            /// </summary>
            public static readonly CacheTemplate MultiThreadLowMemory =
                new InternalCacheTemplate(StlmCacheSizes);

            #endregion

            #region Constructor

            /// <summary>
            /// Prevent non-nested subclassing by using a private constrcutor
            /// </summary>
            private CacheTemplate() { }

            #endregion

            #region Abstract Methods

            /// <summary>
            /// Return that cache configuration associated with this template
            /// </summary>
            /// <returns></returns>
            public abstract Dictionary<CacheType, ICacheOptions> GetCacheConfiguration();

            #endregion
        }

        #endregion

        /// <summary>
        ///  Holds cache configuration methods for buffer and file stream mode
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public abstract class HasCaches<T> : DataSetBuilder where T : HasCaches<T>
        {
            /// <summary>
            /// Maintains a set of cache options for each cache type describing how 
            /// to build that cache. 
            /// </summary>
            protected Dictionary<CacheType, ICacheOptions> _cacheConfiguration =
                new Dictionary<CacheType, ICacheOptions>();

            /// <summary>
            /// Set a cache builder to use for the specified type of cache
            /// </summary>
            /// <param name="cacheType">The cache type</param>
            /// <param name="cacheBuilder">The cache builder used to create the cache. 
            /// If null is passed then the specified cache type will operate without a
            /// cache.
            /// </param>
            /// <returns>The <see cref="DataSetBuilder"/></returns>
            public T SetCacheBuilder(CacheType cacheType, ICacheBuilder cacheBuilder)
            {
                if (_cacheConfiguration.ContainsKey(cacheType))
                {
                    _cacheConfiguration[cacheType].Builder = cacheBuilder;
                }
                else
                {
                    _cacheConfiguration.Add(cacheType, new CacheOptions()
                    {
                        Builder = cacheBuilder,
                        Size = CacheTemplate.Default.GetCacheConfiguration()[cacheType].Size
                    });
                }
                return (T)this;
            }

            /// <summary>
            /// Set cache builders for multiple cache types
            /// </summary>
            /// <param name="map">A dictionary of cache types with the associated 
            /// builder to use for that cache.
            /// Where a null builder is supplied, the associated cache type 
            /// will operate without a cache.</param>
            /// <returns>The <see cref="DataSetBuilder"/></returns>
            public T SetCacheBuilders(Dictionary<CacheType, ICacheBuilder> map)
            {
                foreach (var element in map)
                {
                    SetCacheBuilder(element.Key, element.Value);
                }
                return (T)this;
            }

            /// <summary>
            /// Set cache size for the specified cache type
            /// </summary>
            /// <param name="cacheType">
            /// The cache type
            /// </param>
            /// <param name="cacheSize">
            /// The size to use for this cache. 
            /// i.e. the maximum number of items it can store
            /// </param>
            /// <returns>The <see cref="DataSetBuilder"/></returns>
            public T SetCacheSize(CacheType cacheType, int cacheSize)
            {
                if (_cacheConfiguration.ContainsKey(cacheType))
                {
                    _cacheConfiguration[cacheType].Size = cacheSize;
                }
                else
                {
                    _cacheConfiguration.Add(cacheType, new CacheOptions()
                    {
                        Builder = CacheTemplate.Default
                            .GetCacheConfiguration()[cacheType].Builder,
                        Size = cacheSize
                    });
                }
                return (T)this;
            }

            /// <summary>
            /// Set cache sizes for multiple cache types
            /// </summary>
            /// <param name="map">
            /// A dictionary of cache types with the associated 
            /// size to use for that cache.
            /// </param>
            /// <returns>The <see cref="DataSetBuilder"/></returns>
            public T SetCacheSizes(Dictionary<CacheType, int> map)
            {
                foreach (var element in map)
                {
                    SetCacheSize(element.Key, element.Value);
                }
                return (T)this;
            }

            /// <summary>
            /// Set the builder and size parameter for the specified cache type
            /// </summary>
            /// <param name="cacheType">The cache type</param>
            /// <param name="options">An <see cref="ICacheOptions"/> object that
            /// specifies a cache builder and size to use when constructing the 
            /// specified cache type</param>
            /// <returns>The <see cref="DataSetBuilder"/></returns>
            internal T ConfigureCache(CacheType cacheType, ICacheOptions options)
            {
                if (_cacheConfiguration.ContainsKey(cacheType))
                {
                    _cacheConfiguration.Remove(cacheType);
                }
                _cacheConfiguration.Add(cacheType, options);
                return (T)this;
            }

            /// <summary>
            /// Set builders and size parameters for multiple cache types
            /// </summary>
            /// <param name="map">
            /// A dictionary of <see cref="ICacheOptions"/> objects that
            /// specify a cache builder and size to use when constructing each
            /// cache type.
            /// </param>
            /// <returns>The <see cref="DataSetBuilder"/></returns>
            internal T ConfigureCaches(Dictionary<CacheType, ICacheOptions> map)
            {
                foreach (var element in map)
                {
                    ConfigureCache(element.Key, element.Value);
                }
                return (T)this;
            }

            /// <summary>
            /// Initialises the <see cref="DataSetBuilder"/> with the default 
            /// cache configuration. Individual elements of this configuration 
            /// can be overridden by using the 
            /// <see cref="SetCacheBuilder(CacheType, ICacheBuilder)"/> and
            /// <see cref="SetCacheBuilders(Dictionary{CacheType, ICacheBuilder})"/>
            /// methods 
            /// </summary>
            /// <returns>The <see cref="DataSetBuilder"/></returns>
            public T ConfigureDefaultCaches()
            {
                ConfigureCaches(CacheTemplate.Default.GetCacheConfiguration());
                return (T)this;
            }

            /// <summary>
            /// Add cache configuration from a predefined cache template.
            /// Individual elements of this configuration can be overridden by 
            /// using the 
            /// <see cref="ConfigureCache(CacheType, ICacheOptions)"/>,
            /// <see cref="ConfigureCaches(Dictionary{CacheType, ICacheOptions})"/>,
            /// <see cref="SetCacheBuilder(CacheType, ICacheBuilder)"/> and
            /// <see cref="SetCacheBuilders(Dictionary{CacheType, ICacheBuilder})"/> 
            /// methods 
            /// </summary>
            /// <param name="template">A <see cref="CacheTemplate"/>. 
            /// Example: <code>CacheTemplate.SingleThread</code></param>
            /// <returns>The <see cref="DataSetBuilder"/></returns>
            public T ConfigureCachesFromTemplate(CacheTemplate template)
            {                
                return ConfigureCachesFromCacheSet(template);
            }

            /// <summary>
            /// Add caches from an object that implements <see cref="ICacheSet"/> 
            /// This allows maximum control over cache creation.
            /// </summary>
            /// <example>
            ///private class MyCacheSet : ICacheSet
            ///{
            ///    private ICacheBuilder myCacheBuilder = new MyCacheBuilder();
            ///    private ICacheBuilder lruBuilder = new LruCacheBuilder();
            ///    
            ///    public Dictionary&lt;CacheType, ICacheOptions&gt; GetCacheConfiguration()
            ///    {
            ///        return new Dictionary&lt;CacheType, ICacheOptions&gt;()
            ///        {
            ///            { CacheType.StringsCache, new CacheOptions() { Builder = myCacheBuilder, Size = 5000 } },
            ///            { CacheType.NodesCache, new CacheOptions() { Builder = lruBuilder, Size = 15000 } },
            ///            { CacheType.ValuesCache, new CacheOptions() { Builder = lruBuilder, Size = 5000 } },
            ///            { CacheType.ProfilesCache, new CacheOptions() { Builder = null, Size = 600 } },
            ///            { CacheType.SignaturesCache, new CacheOptions() { Builder = myCacheBuilder, Size = 500 } }
            ///        };
            ///    }
            ///}
            ///
            ///public DataSet CreateDataSet()
            ///{
            ///    return DataSetBuilder.File()
            ///        .ConfigureCachesFromCacheSet(new MyCacheSet())
            ///        .Build(@"C:\datafile.dat");
            ///}
            /// </example>
            /// <param name="set">The cache set to use</param>
            /// <returns>The <see cref="DataSetBuilder"/></returns>
            public T ConfigureCachesFromCacheSet(ICacheSet set)
            {
                ConfigureCaches(set.GetCacheConfiguration());
                return (T)this;
            }
        }

        /// <summary>
        /// Buffer dataset builder
        /// </summary>
        public class BuildFromBuffer : HasCaches<BuildFromBuffer>
        {
            /// <summary>
            /// Do not allow external instantiation
            /// </summary>
            internal BuildFromBuffer() { }

            /// <summary>
            /// build the dataset from a buffer
            /// </summary>
            /// <param name="buffer">the buffer</param>
            /// <returns>
            /// An <see cref="IndirectDataSet"/> built from the specified buffer
            /// </returns>
            public IndirectDataSet Build(byte[] buffer)
            {
                IndirectDataSet dataSet = new IndirectDataSet(buffer, DataSet.Modes.MemoryMapped);
                LoadForStreaming(dataSet, _cacheConfiguration);
                return dataSet;
            }
        }

        /// <summary>
        /// File dataset builder
        /// </summary>
        public class BuildFromFile : HasCaches<BuildFromFile>
        {
            private bool _isTempFile = false;
            private DateTime? _lastModified = null;

            /// <summary>
            /// Do not allow external instantiation
            /// </summary>
            internal BuildFromFile() { }

            /// <summary>
            /// If this dataset is built from a file, delete the file after close
            /// </summary>
            /// <returns>The <see cref="DataSetBuilder"/></returns>
            public BuildFromFile SetTempFile()
            {
                _isTempFile = true;
                return this;
            }

            /// <summary>
            /// If this dataset is built from a file
            /// </summary>
            /// <param name="isTemp">if true, delete the file after close</param>
            /// <returns>The <see cref="DataSetBuilder"/></returns>
            public BuildFromFile SetTempFile(bool isTemp)
            {
                _isTempFile = isTemp;
                return this;
            }

            /// <summary>
            /// If this dataset is built from a file, override the creation date
            /// </summary>
            /// <param name="date">the date</param>
            /// <returns>The <see cref="DataSetBuilder"/></returns>
            public BuildFromFile LastModified(DateTime date)
            {
                _lastModified = date;
                return this;
            }

            /// <summary>
            /// build the dataset from a file
            /// </summary>
            /// <param name="filename">the filename to build from</param>
            /// <returns>
            /// An <see cref="IndirectDataSet"/> built from the specified file
            /// </returns>
            public IndirectDataSet Build(string filename)
            {
                DateTime? modDate = _lastModified;
                if (modDate == null)
                {
                    modDate = new FileInfo(filename).LastWriteTimeUtc;
                }
                IndirectDataSet dataSet = new IndirectDataSet(filename, modDate.Value, DataSet.Modes.File, _isTempFile);
                LoadForStreaming(dataSet, _cacheConfiguration);
                return dataSet;
            }
        }

        /// <summary>
        /// This class exists to adapt an EntityFactory to a Loader
        /// </summary>
        /// <typeparam name="V">type of the entity</typeparam>
        /// <typeparam name="D">type of dataset</typeparam>
        public class EntityLoader<V, D> : IValueLoader<int, V>
            where D : IStreamDataSet
        {
            private D _dataset;
            private BaseEntityFactory<V, D> _entityFactory;
            private Header _header;
            private bool _fixedLength = false;

            /// <summary>
            /// Create the EntityLoader
            /// </summary>
            /// <param name="header">
            /// The header for this group of entities
            /// </param>
            /// <param name="dataset">
            /// The dataset the entities belong to
            /// </param>
            /// <param name="entityFactory">
            /// The entity factory used to create the entities
            /// </param>
            public EntityLoader(Header header, D dataset, 
                BaseEntityFactory<V, D> entityFactory)
            {
                _dataset = dataset;
                _entityFactory = entityFactory;
                _header = header;
                try
                {
                    GetEntityFactory().GetLength();
                    _fixedLength = true;
                }
                catch (NotImplementedException)
                {
                    // expected for variable length entities
                }
            }

            /// <summary>
            /// Load the item with the specified key from the data source
            /// </summary>
            /// <param name="key">
            /// The position (if variable length) or index (if fixed length)
            /// of the entity to load
            /// </param>
            /// <returns>
            /// The data entity
            /// </returns>
            public virtual V Load(int key)
            {
                Reader reader = _dataset.Pool.GetReader();
                try
                {
                    if (_fixedLength)
                    {
                        reader.BaseStream.Seek(_header.StartPosition
                                + (GetEntityFactory().GetLength() * key),
                                SeekOrigin.Begin);
                    }
                    else
                    {
                        reader.BaseStream.Seek(_header.StartPosition + key,
                            SeekOrigin.Begin);
                    }
                    return _entityFactory.Create(_dataset, key, reader);
                }
                finally
                {
                    _dataset.Pool.Release(reader);
                }
            }

            /// <summary>
            /// Get the position (if variable length) or index (if fixed length)
            /// of the next entity in the data source
            /// </summary>
            /// <param name="position">
            /// The position or index of the current entity
            /// </param>
            /// <param name="result">
            /// The current entity
            /// </param>
            /// <returns>
            /// The position (if variable length) or index (if fixed length)
            /// of the next entity in the data source
            /// </returns>
            public int NextPosition(int position, V result)
            {
                if (_fixedLength) {
                    return ++position;
                } else {
                    // this method supported only for variable length entities
                    return position + GetEntityFactory().GetLength(result);
                }
            }

            /// <summary>
            /// Get the entity factory used by this loader
            /// </summary>
            /// <returns>
            /// The entity factory used by this loader
            /// </returns>
            public BaseEntityFactory<V, D> GetEntityFactory()
            {
                return _entityFactory;
            }

            /// <summary>
            /// Get the header for the entity collection loaded
            /// by this loader
            /// </summary>
            /// <returns>
            /// The header for the entity collection loaded
            /// by this loader
            /// </returns>
            public Header GetHeader()
            {
                return _header;
            }
        }

        /// <summary>
        /// A caching entity loader that uses an <see cref="LruCache{K, V}"/> 
        /// </summary>
        /// <typeparam name="V">type of entity</typeparam>
        /// <typeparam name="D">type of dataset</typeparam>
        internal class LruEntityLoader<V, D> : EntityLoader<V, D>
            where D : IStreamDataSet
        {
            private LruCache<int, V> _cache;

            public LruEntityLoader(Header header, D dataset,
                BaseEntityFactory<V, D> entityFactory, LruCache<int, V> cache)
                : base(header, dataset, entityFactory)
            {
                _cache = cache;
                _cache.SetValueLoader(new EntityLoader<V, D>(header, dataset, entityFactory));
            }

            /// <summary>
            /// Load the item with the specified key from the data source
            /// </summary>
            /// <param name="key">
            /// The position (if variable length) or index (if fixed length)
            /// of the entity to load
            /// </param>
            /// <returns>
            /// The data entity
            /// </returns>
            public override V Load(int key)
            {
                return _cache[key];
            }
        }

        /// <summary>
        /// A caching entity loader that uses a <see cref="IPutCache{K, V}"/> 
        /// </summary>
        /// <typeparam name="V">type of entity</typeparam>
        /// <typeparam name="D">type of dataset</typeparam>
        internal class CachedEntityLoader<V, D> : EntityLoader<V, D>
            where D : IStreamDataSet
        {
            private IPutCache<int, V> _cache;

            public CachedEntityLoader(Header header, D dataset,
                BaseEntityFactory<V, D> entityFactory, IPutCache<int, V> cache)
                    : base(header, dataset, entityFactory)
            {
                _cache = cache;
            }

            /// <summary>
            /// Load the item with the specified key from the data source
            /// </summary>
            /// <param name="key">
            /// The position (if variable length) or index (if fixed length)
            /// of the entity to load
            /// </param>
            /// <returns>
            /// The data entity
            /// </returns>
            public override V Load(int key)
            {
                V value;
                value = _cache[key];
                if (value == null)
                {
                    value = base.Load(key);
                    if (value != null)
                    {
                        _cache.Put(key, value);
                    }
                }
                return value;
            }
        }
        #endregion

        #region Cache size configurations

        /* Default Cache sizes */
        private const int STRINGS_CACHE_SIZE = 8000;
        private const int NODES_CACHE_SIZE = 30000;
        private const int VALUES_CACHE_SIZE = 3000;
        private const int PROFILES_CACHE_SIZE = 600;
        private const int SIGNATURES_CACHE_SIZE = 16000;

        private static ICacheBuilder LruBuilder = new LruCacheBuilder();
        
        private static Dictionary<CacheType, ICacheOptions> defaultCacheSizes = new Dictionary<CacheType, ICacheOptions>()
        {
            { CacheType.StringsCache, new CacheOptions() { Builder = LruBuilder, Size = 8000 } },
            { CacheType.NodesCache, new CacheOptions() { Builder = LruBuilder, Size = 40000 } },
            { CacheType.ValuesCache, new CacheOptions() { Builder = LruBuilder, Size = 1000 } },
            { CacheType.ProfilesCache, new CacheOptions() { Builder = LruBuilder, Size = 1000 } },
            { CacheType.SignaturesCache, new CacheOptions() { Builder = LruBuilder, Size = 20000 } },
        };
        
        private static Dictionary<CacheType, ICacheOptions> MtCacheSizes = new Dictionary<CacheType, ICacheOptions>()
        {
            { CacheType.StringsCache, new CacheOptions() { Builder = LruBuilder, Size = 8000 } },
            { CacheType.NodesCache, new CacheOptions() { Builder = LruBuilder, Size = 80000 } },
            { CacheType.ValuesCache, new CacheOptions() { Builder = LruBuilder, Size = 5000 } },
            { CacheType.ProfilesCache, new CacheOptions() { Builder = LruBuilder, Size = 1000 } },
            { CacheType.SignaturesCache, new CacheOptions() { Builder = LruBuilder, Size = 40000 } },
        };
        
        private static Dictionary<CacheType, ICacheOptions> StCacheSizes = new Dictionary<CacheType, ICacheOptions>()
        {
            { CacheType.StringsCache, new CacheOptions() { Builder = LruBuilder, Size = 8000 } },
            { CacheType.NodesCache, new CacheOptions() { Builder = LruBuilder, Size = 80000 } },
            { CacheType.ValuesCache, new CacheOptions() { Builder = LruBuilder, Size = 5000 } },
            { CacheType.ProfilesCache, new CacheOptions() { Builder = LruBuilder, Size = 1000 } },
            { CacheType.SignaturesCache, new CacheOptions() { Builder = LruBuilder, Size = 40000 } },
        };
        
        private static Dictionary<CacheType, ICacheOptions> StlmCacheSizes = new Dictionary<CacheType, ICacheOptions>()
        {
            { CacheType.StringsCache, new CacheOptions() { Builder = LruBuilder, Size = 5000 } },
            { CacheType.NodesCache, new CacheOptions() { Builder = LruBuilder, Size = 30000 } },
            { CacheType.ValuesCache, new CacheOptions() { Builder = LruBuilder, Size = 1000 } },
            { CacheType.ProfilesCache, new CacheOptions() { Builder = LruBuilder, Size = 500 } },
            { CacheType.SignaturesCache, new CacheOptions() { Builder = LruBuilder, Size = 10000 } },
        };
        
        private static Dictionary<CacheType, ICacheOptions> MtlmCacheSizes = new Dictionary<CacheType, ICacheOptions>()
        {
            { CacheType.StringsCache, new CacheOptions() { Builder = LruBuilder, Size = 5000 } },
            { CacheType.NodesCache, new CacheOptions() { Builder = LruBuilder, Size = 30000 } },
            { CacheType.ValuesCache, new CacheOptions() { Builder = LruBuilder, Size = 1000 } },
            { CacheType.ProfilesCache, new CacheOptions() { Builder = LruBuilder, Size = 500 } },
            { CacheType.SignaturesCache, new CacheOptions() { Builder = LruBuilder, Size = 10000 } },
        };

        #endregion
        
        private DataSetBuilder()
        {
        }

        #region Public methods

        /// <summary>
        /// Create a stream file dataset
        /// </summary>
        /// <returns>
        /// A <see cref="DataSetBuilder"/> that exposes methods required 
        /// to build a <see cref="DataSet"/> directly from a file
        /// </returns>
        public static BuildFromFile File()
        {
            return new BuildFromFile();
        }

        /// <summary>
        /// Create a stream buffer dataset
        /// </summary>
        /// <returns>
        /// A <see cref="DataSetBuilder"/> that exposes methods required 
        /// to build a <see cref="DataSet"/> from a byte array
        /// </returns>
        public static BuildFromBuffer Buffer()
        {
            return new BuildFromBuffer();
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Load the necessary values from the data file in to the DataSet.
        /// Initially, this will only load data such as file headers
        /// and the smaller lists.
        /// </summary>
        /// <param name="dataSet">The dataset object to load in to</param>
        /// <param name="cacheConfiguration">the cache configuration to use when creating the caches</param>
        private static void LoadForStreaming(IndirectDataSet dataSet, Dictionary<CacheType, ICacheOptions> cacheConfiguration)
        {
            var reader = dataSet.Pool.GetReader();
            try
            {
                CacheMap cacheMap = BuildCaches(cacheConfiguration);
                dataSet.CacheMap = cacheMap;

                reader.BaseStream.Position = 0;
                CommonFactory.LoadHeader(dataSet, reader);

                // ---- Create AsciiString list
                var stringLoader = EntityLoaderFactory.GetLoaderFor(
                    new Header(reader),
                    cacheMap.StringCache,
                    dataSet,
                    new StreamAsciiStringFactory());
                dataSet.Strings = new StreamList<AsciiString, IStreamDataSet>(stringLoader);

                MemoryFixedList<Component, DataSet> components = null;
                switch (dataSet.VersionEnum)
                {
                    case BinaryConstants.FormatVersions.PatternV31:
                        components = new MemoryFixedList<Component, DataSet>(
                            dataSet,
                            reader, 
                            new ComponentFactoryV31());
                        break;
                    case BinaryConstants.FormatVersions.PatternV32:
                        components = new MemoryFixedList<Component, DataSet>(
                            dataSet, 
                            reader, 
                            new ComponentFactoryV32());
                        break;
                }
                dataSet._components = components;
                var maps = new MemoryFixedList<Map, DataSet>(
                    dataSet, 
                    reader, 
                    new MapFactory());
                dataSet._maps = maps;
                var properties = new PropertiesList(
                    dataSet, 
                    reader, 
                    new PropertyFactory());
                dataSet._properties = properties;

                // ---- Create Value list
                var valueLoader = EntityLoaderFactory.GetLoaderFor(new Header(reader),
                    cacheMap.ValueCache,
                    dataSet,
                    new ValueFactory<IStreamDataSet>());
                dataSet._values = new StreamList<Value, IStreamDataSet>(valueLoader);

                // ---- Create Profile list
                var profileLoader = EntityLoaderFactory.GetLoaderFor(new Header(reader),
                    cacheMap.ProfileCache,
                    dataSet,
                    new ProfileStreamFactory(dataSet.Pool));
                dataSet.Profiles = new StreamList<Entities.Profile, IStreamDataSet>(profileLoader);

                switch (dataSet.VersionEnum)
                {
                    case BinaryConstants.FormatVersions.PatternV31:
                        // ---- Create Signature list for V31
                        var signatureLoaderV31 = EntityLoaderFactory.GetLoaderFor(
                            new Header(reader),
                            cacheMap.SignatureCache,
                            dataSet,
                            new SignatureFactoryV31<IndirectDataSet>(dataSet));
                        dataSet._signatures = new StreamList<Signature, IndirectDataSet>(
                            signatureLoaderV31);
                        break;
                    case BinaryConstants.FormatVersions.PatternV32:
                        // ---- Create Signature list for V32
                        var signatureLoaderV32 = EntityLoaderFactory.GetLoaderFor(
                            new Header(reader),
                            cacheMap.SignatureCache,
                            dataSet,
                            new SignatureFactoryV32<IndirectDataSet>(dataSet));
                        dataSet._signatures = new StreamList<Signature, IndirectDataSet>(
                            signatureLoaderV32);
                        dataSet._signatureNodeOffsets = new IntegerList(dataSet, reader);
                        dataSet._nodeRankedSignatureIndexes = new IntegerList(dataSet, reader);
                        break;
                }
                dataSet._rankedSignatureIndexes = new IntegerList(dataSet, reader);
                switch (dataSet.VersionEnum)
                {
                    case BinaryConstants.FormatVersions.PatternV31:
                        // ---- Create Nodes list for V31
                        var nodeLoaderV31 = EntityLoaderFactory.GetLoaderFor(
                            new Header(reader),
                            cacheMap.NodeCache,
                            dataSet,
                            new NodeStreamFactoryV31(dataSet.Pool));
                        dataSet.Nodes = new StreamList<Entities.Node, IStreamDataSet>(
                            nodeLoaderV31);
                        break;
                    case BinaryConstants.FormatVersions.PatternV32:
                        // ---- Create Nodes list for V31
                        var nodeLoaderV32 = EntityLoaderFactory.GetLoaderFor(
                            new Header(reader),
                            cacheMap.NodeCache,
                            dataSet,
                            new NodeStreamFactoryV32(dataSet.Pool));
                        dataSet.Nodes = new StreamList<Entities.Node, IStreamDataSet>(
                            nodeLoaderV32);
                        break;
                }
                var rootNodes = new MemoryFixedList<Entities.Node, DataSet>(
                    dataSet, 
                    reader, 
                    new RootNodeFactory());
                dataSet.RootNodes = rootNodes;
                var profileOffsets = new MemoryFixedList<ProfileOffset, DataSet>(
                    dataSet, 
                    reader, 
                    new ProfileOffsetFactory());
                dataSet._profileOffsets = profileOffsets;

                // Read into memory all the small lists which are frequently accessed.
                reader.BaseStream.Position = components.Header.StartPosition;
                components.Read(reader);
                reader.BaseStream.Position = maps.Header.StartPosition;
                maps.Read(reader);
                reader.BaseStream.Position = properties.Header.StartPosition;
                properties.Read(reader);
                reader.BaseStream.Position = rootNodes.Header.StartPosition;
                rootNodes.Read(reader);
                reader.BaseStream.Position = profileOffsets.Header.StartPosition;
                profileOffsets.Read(reader);
            }
            finally
            {
                dataSet.Pool.Release(reader);
            }
        }

        /// <summary>
        /// Build caches usign the specified configuration.
        /// The caches are returned in a <see cref="CacheMap"/> 
        /// </summary>
        /// <param name="cacheConfiguration">A dictionary mapping <see cref="CacheType"/> values 
        /// to an <see cref="ICacheOptions"/> object. This specifies the <see cref="ICacheBuilder"/> 
        /// and size parameter to use when constructing a cache of the associated type.
        /// </param>
        /// <returns>a <see cref="CacheMap"/> containing the created caches</returns>
        private static CacheMap BuildCaches(Dictionary<CacheType, ICacheOptions> cacheConfiguration)
        {
            CacheMap caches = new CacheMap();

            foreach(var configuration in cacheConfiguration)
            {
                if (configuration.Value.Builder != null)
                {
                    switch (configuration.Key)
                    {
                        case CacheType.StringsCache:
                            caches.StringCache = configuration.Value.Builder.Build<int, AsciiString>(
                                configuration.Value.Size);
                            break;
                        case CacheType.NodesCache:
                            caches.NodeCache = configuration.Value.Builder.Build<int, Entities.Node>(
                                configuration.Value.Size);
                            break;
                        case CacheType.ValuesCache:
                            caches.ValueCache = configuration.Value.Builder.Build<int, Value>(
                                configuration.Value.Size);
                            break;
                        case CacheType.ProfilesCache:
                            caches.ProfileCache = configuration.Value.Builder.Build<int, Entities.Profile>(
                                configuration.Value.Size);
                            break;
                        case CacheType.SignaturesCache:
                            caches.SignatureCache = configuration.Value.Builder.Build<int, Signature>(
                                configuration.Value.Size);
                            break;
                        default:
                            break;
                    }
                }
            }

            return caches;
        }

        #endregion
    }
}
