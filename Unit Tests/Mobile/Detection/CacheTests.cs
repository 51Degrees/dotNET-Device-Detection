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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using FiftyOne.Foundation.Mobile.Detection;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FiftyOne.Foundation.Mobile.Detection.Caching;

namespace FiftyOne.Tests.Unit.Mobile.Detection
{
    [TestClass]
    public class CacheTests
    {
        /// <summary>
        /// Used to load missing items into the cache.
        /// </summary>
        /// <typeparam name="K">Key type</typeparam>
        /// <typeparam name="V">Value type</typeparam>
        public class CacheLoader<K, V> : IValueLoader<K,V>
        {
            /// <summary>
            /// Source of data to be used by the loader.
            /// </summary>
            private readonly IDictionary<K, V> _source;
            
            /// <summary>
            /// Number of times items have been fetched from
            /// the source.
            /// </summary>
            internal int Fetches
            {
                get { return _fetches; }
            }
            private int _fetches = 0;

            internal CacheLoader(IDictionary<K, V> source)
            {
                _source = source;
            }

            public V Load(K key)
            {
                Interlocked.Increment(ref _fetches);
                return _source[key];
            }
        }

        [TestMethod]
        [TestCategory("Unit"), TestCategory("Cache")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Cache_Null()
        {
            var source = GetRandomStringKeys(1);
            var loader = new CacheLoader<string, string>(source);
            var cache = new LruCache<string, string>(source.Count, loader);
            Console.WriteLine(cache[null]);
        }

        [TestMethod]
        [TestCategory("Unit"), TestCategory("Cache")]
        public void Cache_Single()
        {
            var source = GetNumericKeys(1);
            var loader = new CacheLoader<int, string>(source);
            var cache = new LruCache<int, string>(source.Count, loader);
            Assert.IsTrue(cache[0] == source[0]);
            Assert.IsTrue(cache.Misses == 1);
        }

        [TestMethod]
        [TestCategory("Unit"), TestCategory("Cache")]
        public void Cache_SingleNull()
        {
            var source = GetNumericKeys(1);
            var loader = new CacheLoader<int, string>(source);
            var cache = new LruCache<int, string>(source.Count, loader);
            try { Assert.IsNull(cache[1]); }
            catch(KeyNotFoundException)
            {
                // This is the expected result.
            }
            Assert.IsTrue(cache.Misses == 1);
        }

        [TestMethod]
        [TestCategory("Unit"), TestCategory("Cache")]
        public void Cache_NumericFull()
        {
            ValidateCache(GetNumericKeys(10000));
        }

        [TestMethod]
        [TestCategory("Unit"), TestCategory("Cache")]
        public void Cache_StringFull()
        {
            ValidateCache(GetRandomStringKeys(10000));
        }

        [TestMethod]
        [TestCategory("Unit"), TestCategory("Cache")]
        public void Cache_Performance()
        {
            var source = GetRandomStringKeys(1000000);
            var loader = new CacheLoader<string, string>(source);
            var cache = new LruCache<string, string>(source.Count, loader);
            var fill = ProcessSource(source, cache);
            var retrieve = ProcessSource(source, cache);
            Console.WriteLine(
                "Cache fill time of '{0}' milliseconds.",
                fill.TotalMilliseconds);
            Console.WriteLine(
                "Cache retrieve time of '{0}' milliseconds.",
                retrieve.TotalMilliseconds);
            Console.WriteLine(
                "Cache performance increase of '{0:#.00}' times.",
                fill.TotalMilliseconds / retrieve.TotalMilliseconds);
            Assert.IsTrue(
                retrieve.TotalMilliseconds < fill.TotalMilliseconds / 3, 
                "It should be at least 3 times quicker to retrieve cached " +
                "items than add them in this test.");
        }
        
        /// <summary>
        /// In parallel threads processes the source against the cache.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="cache"></param>
        /// <returns></returns>
        private static TimeSpan ProcessSource(IDictionary<string, string> source, LruCache<string, string> cache)
        {
            var start = DateTime.UtcNow;
            Parallel.ForEach(source, item =>
            {
                Assert.IsTrue(cache[item.Key].Equals(item.Value));
            });
            return DateTime.UtcNow - start;
        }

        /// <summary>
        /// Takes a source of cacheable items and tests the cache statistics
        /// using fixed loading and retrieval patterns.
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="source"></param>
        private static void ValidateCache<K,V>(IDictionary<K, V> source) where V : IEquatable<V>
        {
            var loader = new CacheLoader<K, V>(source);
            var cache = new LruCache<K, V>(source.Count / 2, loader);

            // Fill the cache with half of the values.
            foreach (var item in source.Take(source.Count / 2))
            {
                var expectedLast = cache._linkedList.Last;
                Assert.IsTrue(cache[item.Key].Equals(item.Value));
                Assert.IsTrue(cache._linkedList.First.Value.Key.Equals(item.Key));
                Assert.IsTrue(expectedLast == null || expectedLast == cache._linkedList.Last);
            }
            Assert.IsTrue(cache.Misses == loader.Fetches);
            Assert.IsTrue(cache.Misses == source.Count / 2);
            Assert.IsTrue(cache.Requests == source.Count / 2);

            // Check all the values are returned from the cache.
            foreach (var item in source.Take(source.Count / 2))
            {
                var expectedLast = cache._linkedList.Last.Previous;
                Assert.IsTrue(cache[item.Key].Equals(item.Value));
                Assert.IsTrue(cache._linkedList.First.Value.Key.Equals(item.Key));
                Assert.IsTrue(expectedLast == cache._linkedList.Last);
            }
            Assert.IsTrue(cache.Misses == loader.Fetches);
            Assert.IsTrue(cache.Misses == source.Count / 2);
            Assert.IsTrue(cache.Requests == source.Count);

            // Now use the 2nd half of the source to push out all 
            // the first half.
            foreach (var item in source.Skip(source.Count / 2))
            {
                var expectedLast = cache._linkedList.Last.Previous;
                Assert.IsTrue(cache[item.Key].Equals(item.Value));
                Assert.IsTrue(cache._linkedList.First.Value.Key.Equals(item.Key));
                Assert.IsTrue(expectedLast == cache._linkedList.Last);
            }
            Assert.IsTrue(cache.Misses == loader.Fetches);
            Assert.IsTrue(cache.Misses == source.Count);
            Assert.IsTrue(cache.Requests == source.Count * 1.5);

            // Still using the 2nd half of the source retrieve all
            // the values again. They should come from the cache.
            foreach (var item in source.Skip(source.Count / 2))
            {
                var expectedLast = cache._linkedList.Last.Previous;
                Assert.IsTrue(cache[item.Key].Equals(item.Value));
                Assert.IsTrue(cache._linkedList.First.Value.Key.Equals(item.Key));
                Assert.IsTrue(expectedLast == cache._linkedList.Last);
            }
            Assert.IsTrue(cache.Misses == loader.Fetches);
            Assert.IsTrue(cache.Misses == source.Count);
            Assert.IsTrue(cache.Requests == source.Count * 2);

            // Check that the 1st half of the source is now fetched
            // again and are not already in the cache.
            foreach (var item in source.Take(source.Count / 2))
            {
                var expectedLast = cache._linkedList.Last.Previous;
                Assert.IsTrue(cache[item.Key].Equals(item.Value));
                Assert.IsTrue(cache._linkedList.First.Value.Key.Equals(item.Key));
                Assert.IsTrue(expectedLast == cache._linkedList.Last);
            }
            Assert.IsTrue(cache.Misses == loader.Fetches);
            Assert.IsTrue(cache.Misses == source.Count * 1.5);
            Assert.IsTrue(cache.Requests == source.Count * 2.5);

            // Go through in random order and check there are no cache 
            // missed.
            foreach (var item in source.Take(source.Count / 2).OrderBy(i => 
                Guid.NewGuid()))
            {
                var expectedLast = cache._linkedList.Last;
                Assert.IsTrue(cache[item.Key].Equals(item.Value));
                Assert.IsTrue(cache._linkedList.First.Value.Key.Equals(item.Key));
                Assert.IsTrue(expectedLast.Value.Key.Equals(item.Key) ||
                    expectedLast == cache._linkedList.Last);
            }
            Assert.IsTrue(cache.Misses == loader.Fetches);
            Assert.IsTrue(cache.Misses == source.Count * 1.5);
            Assert.IsTrue(cache.Requests == source.Count * 3);
        }

        /// <summary>
        /// Creates a list of numeric values from 0 to size.
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        private static IDictionary<int, string> GetNumericKeys(int size)
        {
            var source = new Dictionary<int, string>(size);
            for (int i = 0; i < size; i++)
            {
                source.Add(i, i.ToString());
            }
            return source;
        }

        /// <summary>
        /// Creates a list of random values keyed on a unique string key.
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        private static IDictionary<string, string> GetRandomStringKeys(int size)
        {
            var source = new Dictionary<string, string>(size);
            for (int i = 0; i < size; i++)
            {
                source.Add(
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString());
            }
            return source;
        }
    }
}
