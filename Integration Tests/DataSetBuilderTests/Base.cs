/**
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited.
 * Copyright (c) 2015 51Degrees Mobile Experts Limited, 5 Charlotte Close,
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
 
 using FiftyOne.Foundation.Mobile.Detection;
using FiftyOne.Foundation.Mobile.Detection.Caching;
using FiftyOne.Foundation.Mobile.Detection.Entities;
using FiftyOne.Foundation.Mobile.Detection.Factories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Caching;
using System.Threading;
using static FiftyOne.Foundation.Mobile.Detection.DataSetBuilder;
using File = System.IO.File;

namespace FiftyOne.Tests.Integration.DataSetBuilderTests
{
    [TestClass]
    public abstract class Base<T> where T : HasCaches<T>
    {
        protected abstract string DataFile { get; }

        protected abstract IndirectDataSet BuildDataset(DataSetBuilder builder);

        protected abstract T InitBuilder();

        /// <summary>
        /// Check that the un-configured builder returns a dataset that does 
        /// not use caches and does not delete the data file
        /// </summary>
        public void DataSetBuilder_UnconfiguredCaches()
        {
            // Arange

            // Act
            using (IndirectDataSet dataset =
                BuildDataset(InitBuilder()))
            {
                // Assert
                Assert.IsNull(dataset.CacheMap.NodeCache, "Expected node cache to be null");
                Assert.IsNull(dataset.CacheMap.ProfileCache, "Expected profile cache to be null");
                Assert.IsNull(dataset.CacheMap.SignatureCache, "Expected signature cache to be null");
                Assert.IsNull(dataset.CacheMap.StringCache, "Expected string cache to be null");
                Assert.IsNull(dataset.CacheMap.ValueCache, "Expected value cache to be null");
            }
            Assert.IsTrue(File.Exists(DataFile), "Data file has been deleted when it should not have been");
        }
        
        /// <summary>
        /// Check that the builder using default caches returns a dataset that 
        /// uses LruCaches
        /// </summary>
        public void DataSetBuilder_DefaultCaches()
        {
            // Arange

            // Act
            using (IndirectDataSet dataset =
                BuildDataset(
                    InitBuilder()
                    .ConfigureDefaultCaches()))
            {
                // Assert
                Assert.IsInstanceOfType(dataset.CacheMap.NodeCache, typeof(LruCache<int,Node>));
                Assert.IsInstanceOfType(dataset.CacheMap.ProfileCache, typeof(LruCache<int,Profile>));
                Assert.IsInstanceOfType(dataset.CacheMap.SignatureCache, typeof(LruCache<int,Signature>));
                Assert.IsInstanceOfType(dataset.CacheMap.StringCache, typeof(LruCache<int,AsciiString>));
                Assert.IsInstanceOfType(dataset.CacheMap.ValueCache, typeof(LruCache<int,Value>));
            }
        }
        
        /// <summary>
        /// Check that the dataset built using the DataSetBuilder with default caches
        /// contains the same data as that built using the MemoryFactory
        /// </summary>
        public void DataSetBuilder_Compare()
        {
            // Arange
            string tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".tmp");
            File.Copy(DataFile, tempFile);

            try
            {
                // Act
                using (IndirectDataSet fileDataset =
                    BuildDataset(
                        InitBuilder()
                        .ConfigureDefaultCaches()))
                {
                    using (DataSet memoryDataset = MemoryFactory.Create(tempFile))
                    {
                        // Assert
                        Assert.IsTrue(Utils.CompareDataSets(fileDataset, memoryDataset),
                            "Data loaded by DataSetBuilder does not match that loaded by MemoryFactory from the same file");
                    }
                }
            }
            finally
            {
                // tidy up
                if (File.Exists(tempFile)) { File.Delete(tempFile); }
            }
        }
        
        /// <summary>
        /// Check that the dataset built using the DataSetBuilder with custom caches
        /// contains the same data as that built using the MemoryFactory
        /// </summary>
        public void DataSetBuilder_CompareCustomCaches()
        {
            // Arange
            string tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".tmp");
            File.Copy(DataFile, tempFile);

            try
            {
                // Act
                using (IndirectDataSet fileDataset =
                    BuildDataset(
                        InitBuilder()
                        .ConfigureCachesFromCacheSet(new CustomCacheSet())))
                {
                    using (DataSet memoryDataset = MemoryFactory.Create(tempFile))
                    {
                        // Assert
                        Assert.IsTrue(Utils.CompareDataSets(fileDataset, memoryDataset),
                            "Data loaded by DataSetBuilder does not match that loaded by MemoryFactory from the same file");
                    }
                }
            }
            finally
            {
                // tidy up
                if (File.Exists(tempFile)) { File.Delete(tempFile); }
            }
        }

        #region Protected inner classes
        protected class CustomCacheSet : ICacheSet
        {
            private static ICacheBuilder memoryCacheBuilder = new MemoryCacheBuilder();
            private static ICacheBuilder lruBuilder = new LruCacheBuilder();

            private Dictionary<CacheType, ICacheOptions> _config = new Dictionary<CacheType, ICacheOptions>()
            {
                { CacheType.StringsCache, new CacheOptions() { Builder = memoryCacheBuilder, Size = 5000 } },
                { CacheType.NodesCache, new CacheOptions() { Builder = lruBuilder, Size = 15000 } },
                { CacheType.ValuesCache, new CacheOptions() { Builder = lruBuilder, Size = 5000 } },
                { CacheType.ProfilesCache, new CacheOptions() { Builder = null } },
                { CacheType.SignaturesCache, new CacheOptions() { Builder = memoryCacheBuilder, Size = 500 } }
            };

            public Dictionary<CacheType, ICacheOptions> GetCacheConfiguration()
            {
                return _config;
            }
        }

        protected class MemoryCacheBuilder : ICacheBuilder
        {
            public ICache<K, V> Build<K, V>(int cacheSize)
            {
                return new MemoryCacheWrapper<K, V>(cacheSize);
            }
        }

        protected class MemoryCacheWrapper<K, V> : IPutCache<K, V>
        {
            private MemoryCache _cache;
            private CacheItemPolicy _policy;

            private int _maxSize;

            private long _requests;
            private long _misses;

            public MemoryCacheWrapper(int maxSize)
            {
                _cache = new MemoryCache("testCache");
                _policy = new CacheItemPolicy();

                _maxSize = maxSize;
            }

            public V this[K key]
            {
                get
                {
                    Interlocked.Increment(ref _requests);
                    return (V)_cache.Get(key.ToString());
                }
            }

            public void Put(K key, V value)
            {
                Interlocked.Increment(ref _misses);
                _cache.Add(key.ToString(), value, _policy);
            }

            public double PercentageMisses
            {
                get
                {
                    return _misses / _requests;
                }
            }

            public void Dispose()
            {
                _cache.Dispose();
            }

            public void ResetCache()
            {
                _cache.Trim(100);
            }
        }
        #endregion
    }
}
