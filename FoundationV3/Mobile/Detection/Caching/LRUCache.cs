/* *********************************************************************
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited. 
 * Copyright © 2017 51Degrees Mobile Experts Limited, 5 Charlotte Close,
 * Caversham, Reading, Berkshire, United Kingdom RG4 7BY
 * 
 * This Source Code Form is the subject of the following patents and patent 
 * applications, owned by 51Degrees Mobile Experts Limited of 5 Charlotte
 * Close, Caversham, Reading, Berkshire, United Kingdom RG4 7BY: 
 * European Patent No. 2871816;
 * European Patent Application No. 17184134.9;
 * United States Patent Nos. 9,332,086 and 9,350,823; and
 * United States Patent Application No. 15/686,066.
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

using System.Collections.Generic;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Diagnostics;

namespace FiftyOne.Foundation.Mobile.Detection.Caching
{
    /// <summary>
    /// Many of the entities used by the detector are requested repeatably. 
    /// The cache improves memory usage and reduces strain on the garbage collector
    /// by storing previously requested entities for a short period of time to avoid 
    /// the need to refetch them from the underlying storage mechanism.
    /// </summary>
    /// <para>
    /// The Least Recently Used (LRU) cache is used. LRU cache keeps track of what
    /// was used when in order to discard the least recently used items first.
    /// Every time a cache item is used the "age" of the item used is updated.
    /// </para>
    /// <para>
    /// This implementation supports concurrency by using multiple linked lists
    /// in place of a single linked list in the original implementation.
    /// The linked list to use is assigned at random and stored in the cached
    /// item. This will generate an even set of results across the different 
    /// linked lists. The approach reduces the probability of the same linked 
    /// list being locked when used in a environments with a high degree of
    /// concurrency. If the feature is not required then the constructor should be
    /// provided with a concurrency value of 1.
    /// </para>
    /// <para>
    /// Use for User-Agent caching.
    /// For a vast majority of the real life environments a constant stream of unique 
    /// User-Agents is a fairly rare event. Usually the same User-Agent can be
    /// encountered multiple times within a fairly short period of time as the user
    /// is making a subsequent request. Caching frequently occurring User-Agents
    /// improved detection speed considerably.
    /// </para>
    /// <para>
    /// Some devices are also more popular than others and while the User-Agents for
    /// such devices may differ, the combination of components used would be very
    /// similar. Therefore internal caching is also used to take advantage of the 
    /// more frequently occurring entities.
    /// </para>
    /// <typeparam name="K">Key for the cache items</typeparam>
    /// <typeparam name="V">Value for the cache items</typeparam>
    internal class LruCache<K, V> : ILoadingCache<K, V>
    {
        #region Classes

        /// <summary>
        /// An item stored in the cache along with references to the next and
        /// previous items.
        /// </summary>
        internal class CachedItem
        {
            /// <summary>
            /// Key associated with the cached item.
            /// </summary>
            internal readonly K Key;

            /// <summary>
            /// Value of the cached item.
            /// </summary>
            internal readonly V Value;

            /// <summary>
            /// The next item in the linked list.
            /// </summary>
            internal CachedItem Next;

            /// <summary>
            /// The previous item in the linked list.
            /// </summary>
            internal CachedItem Previous;

            /// <summary>
            /// The linked list the item is part of.
            /// </summary>
            internal readonly CacheLinkedList List;

            /// <summary>
            /// Indicates that the items is valid and added to the linked list. 
            /// It is not in the process of being manipulated by another thread
            /// either being added to the list or being removed.
            /// </summary>
            internal bool IsValid;

            internal CachedItem(CacheLinkedList list, K key, V value)
            {
                List = list;
                Key = key;
                Value = value;
            }
        }

        /// <summary>
        /// A linked list used in the LruCache implementation in place of the
        /// .NET linked list. This implementation enables items to be moved 
        /// within the linked list.
        /// </summary>
        internal class CacheLinkedList
        {
            /// <summary>
            /// The cache that the list is part of.
            /// </summary>
            private readonly LruCache<K, V> _cache;

            /// <summary>
            /// The first item in the list.
            /// </summary>
            internal CachedItem First { get; private set; }

            /// <summary>
            /// The last item in the list.
            /// </summary>
            internal CachedItem Last { get; private set; }

            /// <summary>
            /// Constructs a new instance of <see cref="CacheLinkedList"/>.
            /// </summary>
            /// <param name="cache">Cache the list is included within</param>
            internal CacheLinkedList(LruCache<K, V> cache)
            {
                _cache = cache;
            }

            /// <summary>
            /// Adds a new cache item to the linked list.
            /// </summary>
            /// <param name="item"></param>
            internal void AddNew(CachedItem item)
            {
                bool added = false;
                if (item != First)
                {
                    lock (this)
                    {
                        if (item != First)
                        {
                            if (First == null)
                            {
                                // First item to be added to the queue.
                                First = item;
                                Last = item;
                            }
                            else
                            {
                                // Add this item to the head of the linked list.
                                item.Next = First;
                                First.Previous = item;
                                First = item;

                                // Set flag to indicate an item was added and if
                                // the cache is full an item should be removed.
                                added = true;
                            }

                            // Indicate the item is now ready for another thread
                            // to manipulate and is fully added to the linked list.
                            item.IsValid = true;
                        }
                    }
                }

                // Check if the linked list needs to be trimmed as the cache
                // size has been exceeded.
                if (added && _cache._dictionary.Count > _cache.CacheSize)
                {
                    lock (this)
                    {
                        if (_cache._dictionary.Count > _cache.CacheSize)
                        {
                            // Indicate that the last item is being removed from 
                            // the linked list.
                            Last.IsValid = false;

                            // Remove the item from the dictionary before 
                            // removing from the linked list.
                            CachedItem lastItem;
                            var result = _cache._dictionary.TryRemove(
                                Last.Key, 
                                out lastItem);
                            Debug.Assert(result,
                                "The last key was not in the dictionary");
                            Debug.Assert(Last == lastItem,
                                "The item removed does not match the last one");
                            Last = Last.Previous;
                            Last.Next = null;
                        }
                    }
                }
            }

            /// <summary>
            /// Set the first item in the linked list to the item provided.
            /// </summary>
            /// <param name="item"></param>
            internal void MoveFirst(CachedItem item)
            {
                if (item != First && item.IsValid == true)
                {
                    lock (this)
                    {
                        if (item != First && item.IsValid == true)
                        {
                            if (item == Last)
                            {
                                // The item is the last one in the list so is 
                                // easy to remove. A new last will need to be
                                // set.
                                Last = item.Previous;
                                Last.Next = null;
                            }
                            else
                            {
                                // The item was not at the end of the list. 
                                // Remove it from it's current position ready
                                // to be added to the top of the list.
                                item.Previous.Next = item.Next;
                                item.Next.Previous = item.Previous;
                            }

                            // Add this item to the head of the linked list.
                            item.Next = First;
                            item.Previous = null;
                            First.Previous = item;
                            First = item;
                        }
                    }
                }
            }

            /// <summary>
            /// Clears all items from the linked list.
            /// </summary>
            internal void Clear()
            {
                First = null;
                Last = null;
            }
        }

        #endregion

        #region Fields

        /// <summary>
        /// Random number generator used to determine which cache list to 
        /// place items into.
        /// </summary>
        private static readonly Random _random = new Random();

        /// <summary>
        /// Loader used to fetch items not in the cache.
        /// </summary>
        private IValueLoader<K, V> _loader;

        /// <summary>
        /// Hash map of keys to item values.
        /// </summary>
        private readonly ConcurrentDictionary<K, CachedItem> _dictionary;

        /// <summary>
        /// Linked list of items in the cache.
        /// </summary>
        /// <remarks>
        /// Marked internal as checked as part of the unit tests.
        /// </remarks>
        internal readonly CacheLinkedList[] _linkedLists;

        /// <summary>
        /// The number of items the cache lists should have capacity for.
        /// </summary>
        internal readonly int CacheSize;

        /// <summary>
        /// The number of requests made to the cache.
        /// </summary>
        internal long Requests { get { return _requests; } }
        private long _requests;

        /// <summary>
        /// The number of times an item was not available.
        /// </summary>
        internal long Misses { get { return _misses; } }
        private long _misses;

        #endregion

        #region Properties

        /// <summary>
        /// Percentage of cache misses.
        /// </summary>
        public double PercentageMisses
        {
            get
            {
                return (double)Misses / (double)Requests;
            }
        }

        /// <summary>
        /// Retrieves the value for key requested. If the key does not exist
        /// in the cache then the loader provided in the constructor is used
        /// to fetch the item.
        /// </summary>
        /// <param name="key">Key for the item required</param>
        /// <returns>An instance of the value associated with the key</returns>
        public V this[K key]
        {
            get
            {
                return this[key, _loader];
            }
        }

        /// <summary>
        /// Retrieves the value for key requested. If the key does not exist
        /// in the cache then the Fetch method is used to retrieve the value
        /// from another source.
        /// </summary>
        /// <param name="key">Key for the item required</param>
        /// <param name="loader">Loader to fetch the item from if not in the cache</param>
        /// <returns>An instance of the value associated with the key</returns>
        public V this[K key, IValueLoader<K, V> loader]
        {
            get
            {
                bool added = false;
                Interlocked.Increment(ref _requests);
                CachedItem node, newNode = null;
                if (_dictionary.TryGetValue(key, out node) == false)
                {
                    // Get the item fresh from the loader before trying
                    // to write the item to the cache.
                    Interlocked.Increment(ref _misses);
                    newNode = new CachedItem(
                        GetRandomLinkedList(),
                        key,
                        loader.Load(key));

                    // If the node has already been added to the dictionary
                    // then get it, otherwise add the one just fetched.
                    node = _dictionary.GetOrAdd(key, newNode);

                    // If the node got from the dictionary is the new one
                    // just fetched then it needs to be added to the linked
                    // list.
                    if (node == newNode)
                    {
                        // Set the node as the first item in the list.
                        newNode.List.AddNew(newNode);
                        added = true;
                    }
                }

                // The item is in the dictionary. Check it's still in the list
                // and if so them move it to the head of the linked list.
                if (added == false)
                {
                    node.List.MoveFirst(node);
                }
                return node.Value;
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a new instance of the cache.
        /// </summary>
        /// <param name="cacheSize">
        /// The number of items to store in the cache
        /// </param>
        internal LruCache(int cacheSize)
            : this(cacheSize, Environment.ProcessorCount) { }

        /// <summary>
        /// Constructs a new instance of the cache.
        /// </summary>
        /// <param name="cacheSize">
        /// The number of items to store in the cache
        /// </param>
        /// <param name="concurrency">
        /// The expected number of concurrent requests to the cache
        /// </param>
        internal LruCache(int cacheSize, int concurrency)
        {
            if (concurrency <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    "concurrency",
                    "Concurrency must be a positive integer greater than 0.");
            }
            CacheSize = cacheSize;
            _dictionary = new ConcurrentDictionary<K, CachedItem>(
                concurrency, cacheSize);
            _linkedLists = new CacheLinkedList[concurrency];
            for (int i = 0; i < _linkedLists.Length; i++)
            {
                _linkedLists[i] = new CacheLinkedList(this);
            }
        }

        /// <summary>
        /// Constructs a new instance of the cache.
        /// </summary>
        /// <param name="cacheSize">The number of items to store in the cache</param>
        /// <param name="loader">Loader used to fetch items not in the cache</param>
        internal LruCache(int cacheSize, IValueLoader<K, V> loader)
            : this(cacheSize, loader, Environment.ProcessorCount)
        {
        }

        /// <summary>
        /// Constructs a new instance of the cache.
        /// </summary>
        /// <param name="cacheSize">The number of items to store in the cache</param>
        /// <param name="loader">Loader used to fetch items not in the cache</param>
        /// <param name="concurrency">
        /// The expected number of concurrent requests to the cache
        /// </param>
        internal LruCache(int cacheSize, IValueLoader<K, V> loader, int concurrency)
            : this(cacheSize, concurrency)
        {
            SetValueLoader(loader);
        }

        #endregion

        #region Destructor

        /// <summary>
        /// Ensures the lock used to synchronise the cache is disposed.
        /// </summary>
        ~LruCache()
        {
            Dispose(false);
        }

        /// <summary>
        /// Disposes of the lock used to synchronise the cache.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Disposes of the lock used to synchronise the cache.
        /// </summary>
        /// <param name="disposing">
        /// True if the calling method is Dispose, false for the finaliser.
        /// </param>
        protected void Dispose(bool disposing)
        {
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Set the value loader that will be used to load items on a cache miss
        /// </summary>
        /// <param name="loader"></param>
        public void SetValueLoader(IValueLoader<K, V> loader)
        {
            _loader = loader;
        }

        /// <summary>
        /// Resets the stats for the cache.
        /// </summary>
        public void ResetCache()
        {
            for (int i = 0; i < _linkedLists.Length; i++)
            {
                _linkedLists[i].Clear();
            }
            _dictionary.Clear();
            _misses = 0;
            _requests = 0;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Returns a random linked list.
        /// </summary>
        /// <returns>
        /// A random linked list of the cache.
        /// </returns>
        private CacheLinkedList GetRandomLinkedList()
        {
            return _linkedLists[_random.Next(_linkedLists.Length)];
        }

        #endregion
    }
}
