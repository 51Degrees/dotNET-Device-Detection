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
    /// the need to refetch them from the underlying storage mechanisim.
    /// </summary>
    /// <para>
    /// The Least Recently Used (LRU) cache is used. LRU cache keeps track of what
    /// was used when in order to discard the least recently used items first.
    /// Every time a cache item is used the "age" of the item used is updated.
    /// </para>
    /// <para>
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
        #region Fields

        /// <summary>
        /// Used to synchronise access to the the dictionary and linked
        /// list in the function of the cache.
        /// </summary>
        private readonly object _writeLock = new object();

        /// <summary>
        /// Loader used to fetch items not in the cache.
        /// </summary>
        private IValueLoader<K, V> _loader;

        /// <summary>
        /// Hash map of keys to item values.
        /// </summary>
        private readonly ConcurrentDictionary<K, LinkedListNode<KeyValuePair<K, V>>> _dictionary;

        /// <summary>
        /// Linked list of items in the cache.
        /// </summary>
        /// <remarks>
        /// Marked internal as checked as part of the unit tests.
        /// </remarks>
        internal readonly LinkedList<KeyValuePair<K,V>> _linkedList;

        /// <summary>
        /// The number of items the cache lists should have capacity for.
        /// </summary>
        internal int CacheSize;

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
                LinkedListNode<KeyValuePair<K,V>> node, newNode = null;
                if (_dictionary.TryGetValue(key, out node) == false)
                {
                    // Get the item fresh from the loader before trying
                    // to write the item to the cache.
                    Interlocked.Increment(ref _misses);
                    newNode = new LinkedListNode<KeyValuePair<K, V>>(
                        new KeyValuePair<K, V>(key, loader.Load(key)));
                    
                    lock (_writeLock)
                    {
                        // If the node has already been added to the dictionary
                        // then get it, otherwise add the one just fetched.
                        node = _dictionary.GetOrAdd(key, newNode);
                    
                        // If the node got from the dictionary is the new one
                        // just fetched then it needs to be added to the linked
                        // list.
                        if (node == newNode)
                        {
                            added = true;
                            _linkedList.AddFirst(node);

                            // Check to see if the cache has grown and if so remove
                            // the last element.
                            RemoveLeastRecent();
                        }
                    }
                }

                // The item is in the dictionary. Check it's still in the list
                // and if so them move it to the head of the linked list.
                if (added == false)
                {
                    lock (_writeLock)
                    {
                        if (node.List != null)
                        {
                            _linkedList.Remove(node);
                            _linkedList.AddFirst(node);
                        }
                    }
                }
                return node.Value.Value;
            }
        }
        
        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a new instance of the cache.
        /// </summary>
        /// <param name="cacheSize">The number of items to store in the cache</param>
        internal LruCache(int cacheSize) 
        {
            CacheSize = cacheSize;
            _dictionary = new ConcurrentDictionary<K, LinkedListNode<KeyValuePair<K, V>>>(
                Environment.ProcessorCount, cacheSize);
            _linkedList = new LinkedList<KeyValuePair<K, V>>();
        }
        
        /// <summary>
        /// Constructs a new instance of the cache.
        /// </summary>
        /// <param name="cacheSize">The number of items to store in the cache</param>
        /// <param name="loader">Loader used to fetch items not in the cache</param>
        internal LruCache(int cacheSize, IValueLoader<K,V> loader) : this (cacheSize)
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

        #region Methods

        /// <summary>
        /// Set the value loader that will be used to load items on a cache miss
        /// </summary>
        /// <param name="loader"></param>
        public void SetValueLoader(IValueLoader<K,V> loader)
        {
            _loader = loader;
        }
        
        /// <summary>
        /// Removes the last item in the cache if the cache size is reached.
        /// </summary>
        private void RemoveLeastRecent()
        {
            if (_linkedList.Count > CacheSize)
            {
                LinkedListNode<KeyValuePair<K, V>> lastNode;
                _dictionary.TryRemove(_linkedList.Last.Value.Key, out lastNode);
                _linkedList.Remove(lastNode);

                Debug.Assert(_linkedList.Count == CacheSize, String.Format(
                    "The linked list has '{0}' elements but should contain '{1}'.",
                    _linkedList.Count,
                    CacheSize));
                Debug.Assert(_dictionary.Count == CacheSize, String.Format(
                    "The dictionary has '{0}' elements but should contain '{1}'.",
                    _dictionary.Count,
                    CacheSize));
            }
        }

        /// <summary>
        /// Resets the stats for the cache.
        /// </summary>
        public void ResetCache()
        {
            _linkedList.Clear();
            _dictionary.Clear();
            _misses = 0;
            _requests = 0;
        }
        
        #endregion
    }
}
