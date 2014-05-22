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

using System.Collections.Generic;
using System;

#if VER4
using System.Collections.Concurrent;
#endif

namespace FiftyOne.Foundation.Mobile.Detection
{
    /// <summary>
    /// Used to speed the retrieval of detection results over duplicate requests.
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <typeparam name="V"></typeparam>
    internal class Cache<K, V>
    {
        /// <summary>
        /// The next time the caches should be switched.
        /// </summary>
        private DateTime _nextCacheService = DateTime.MinValue;

        /// <summary>
        /// The time between cache services.
        /// </summary>
        private readonly TimeSpan _serviceInterval;

        /// <summary>
        /// The active cache.
        /// </summary>
#if VER4
        private ConcurrentDictionary<K, V> _active;
#else
        private Dictionary<K, V> _active;
#endif

        /// <summary>
        /// The background cache.
        /// </summary>
#if VER4
        private ConcurrentDictionary<K, V> _background;
#else
        private Dictionary<K, V> _background;
#endif

        internal Cache(int serviceInterval)
        {
            _serviceInterval = new TimeSpan(0, 0, serviceInterval);
            _nextCacheService = DateTime.UtcNow + _serviceInterval;
#if VER4
            _active = new ConcurrentDictionary<K, V>();
            _background = new ConcurrentDictionary<K, V>();
#else
            _active = new Dictionary<K, V>();
            _background = new Dictionary<K, V>();
#endif
        }

        internal Cache(IEqualityComparer<K> comparer, int serviceInterval)
        {
            _serviceInterval = new TimeSpan(serviceInterval);
            _nextCacheService = DateTime.UtcNow + _serviceInterval;
#if VER4
            _active = new ConcurrentDictionary<K, V>(comparer);
            _background = new ConcurrentDictionary<K, V>(comparer);
#else
            _active = new Dictionary<K, V>(comparer);
            _background = new Dictionary<K, V>(comparer);
#endif
        }

        /// <summary>
        /// Service the cache by switching the lists if the next service
        /// time has passed.
        /// </summary>
        private void Service()
        {
            if (_nextCacheService < DateTime.UtcNow)
            {
                lock (this)
                {
                    if (_nextCacheService < DateTime.UtcNow)
                    {
                        // Switch the cache dictionaries over.
                        var tempCache = _active;
                        _active = _background;
                        _background = tempCache;

                        // Clear the background cache before continuing.
                        _background.Clear();

                        // Set the next service time.
                        _nextCacheService = DateTime.UtcNow + _serviceInterval;
                    }
                }
            }
        }

        internal bool TryGetValue(K key, out V result)
        {
#if VER4
            return _active.TryGetValue(key, out result);
#else
            lock (_active)
            {
                return _active.TryGetValue(key, out result);
            }
#endif
        }

        internal void SetActive(K key, V result)
        {
#if VER4
            _active[key] = result;
#else
            lock (_active)
            {
                _active[key] = result;
            }
#endif
        }

        internal void SetBackground(K key, V result)
        {
#if VER4
            _background[key] = result;
#else
            lock (_background)
            {
                _background[key] = result;
            }
#endif
            Service();
        }
    }
}
