/* *********************************************************************
 * The contents of this file are subject to the Mozilla Public License 
 * Version 1.1 (the "License"); you may not use this file except in 
 * compliance with the License. You may obtain a copy of the License at 
 * http://www.mozilla.org/MPL/
 * 
 * Software distributed under the License is distributed on an "AS IS" 
 * basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 * See the License for the specific language governing rights and 
 * limitations under the License.
 *
 * The Original Code is named .NET Mobile API, first released under 
 * this licence on 11th March 2009.
 * 
 * The Initial Developer of the Original Code is owned by 
 * 51 Degrees Mobile Experts Limited. Portions created by 51 Degrees 
 * Mobile Experts Limited are Copyright (C) 2009 - 2011. All Rights Reserved.
 * 
 * Contributor(s):
 *     James Rosewell <james@51degrees.mobi>
 * 
 * ********************************************************************* */

#region Usings

using System;
using System.Collections.Generic;
using System.Threading;

#endregion

#if VER4
using System.Linq;
using System.Threading.Tasks;
#elif VER2

#endif

namespace FiftyOne
{
    /// <summary>
    /// Used to create a cache object of type Value with a string key.
    /// </summary>
    /// <typeparam name="Value">Type of object to hold in the cache.</typeparam>
    public class Cache<Value> : Cache<string, Value>
    {
        /// <summary>
        /// Constructs a class of the type Cache&lt;Value&gt; using the
        /// timeout value in minutes provided.
        /// </summary>
        /// <param name="timeout">Minimum number of minutes to hold items in the cache for.</param>
        public Cache(int timeout) : base(timeout)
        {
        }
    }

    /// <summary>
    /// A cache object used to store key value pairs until a timeout has expired.
    /// </summary>
    /// <typeparam name="Key">Type of the key for the class.</typeparam>
    /// <typeparam name="Value">Type of the value for the class.</typeparam>
    public class Cache<Key, Value>
    {
        #region Fields

        private DateTime _nextServiceTime = DateTime.MinValue;
        private readonly Dictionary<Key, Value> _internalCache;
        private readonly Dictionary<Key, DateTime> _lastAccessed;

        // The last time this process serviced the cache file.
        private readonly int _timeout;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs the cache clearing key value pairs after the timeout period 
        /// specified in minutes.
        /// </summary>
        /// <param name="timeout">Number of minutes to hold items in the cache for.</param>
        public Cache(int timeout)
        {
            _internalCache = new Dictionary<Key, Value>();
            _lastAccessed = new Dictionary<Key, DateTime>();
            _timeout = timeout;
        }

        #endregion

        #region Internal Members

        /// <summary>
        /// Removes the specified key from the cache.
        /// </summary>
        /// <param name="key">Key to be removed.</param>
        protected internal void Remove(Key key)
        {
            if (_internalCache.ContainsKey(key))
                _internalCache.Remove(key);
        }

        /// <summary>
        /// Returns the value associated with the key.
        /// </summary>
        /// <param name="key">Key of the value being requested.</param>
        /// <returns>Value or null if not found.</returns>
        public Value this[Key key]
        {
            get
            {
                Value value;
                lock (this)
                {
                    if (_internalCache.TryGetValue(key, out value))
                        _lastAccessed[key] = DateTime.UtcNow;
                }
                CheckIfServiceRequired();
                return value;
            }
            set
            {
                lock (this)
                {
                    if (Contains(key))
                    {
                        _internalCache[key] = value;
                    }
                    else
                    {
                        _internalCache.Add(key, value);
                        _lastAccessed[key] = DateTime.UtcNow;
                    }
                }
            }
        }

        /// <summary>
        /// If the key exists in the cache then provide the value in the
        /// value parameter.
        /// </summary>
        /// <param name="key">Key of the value to be retrieved.</param>
        /// <param name="value">Set to the associated value if found.</param>
        /// <returns>True if the key was found in the list, otherwise false.</returns>
        public bool GetTryParse(Key key, out Value value)
        {
            bool result = false;
            if (key != null)
            {
                lock (this)
                {
                    result = _internalCache.TryGetValue(key, out value);
                    if (result)
                        _lastAccessed[key] = DateTime.UtcNow;
                }
            }
            else
            {
                value = default(Value);
            }
            CheckIfServiceRequired();
            return result;
        }

        /// <summary>
        /// Determines if the key is available in the cache.
        /// </summary>
        /// <param name="key">Key to be checked.</param>
        /// <returns>True if the key is found, otherwise false.</returns>
        protected internal bool Contains(Key key)
        {
            bool result = false;
            if (key != null)
            {
                lock (this)
                {
                    result = _internalCache.ContainsKey(key);
                    if (result)
                        _lastAccessed[key] = DateTime.UtcNow;
                }
            }
            return result;
        }

        #endregion

        #region Private Members

        /// <summary>
        /// If the time has passed the point another check of the cache is needed 
        /// start a thread to check the cache.
        /// </summary>
        private void CheckIfServiceRequired()
        {
            if (_nextServiceTime >= DateTime.UtcNow || _internalCache.Count <= 0) return;
            
            // Set the next service time to a date far in the future
            // to prevent another thread being started.
            _nextServiceTime = DateTime.MaxValue;
#if VER4
            Task.Factory.StartNew(() => ServiceCache(DateTime.UtcNow.AddMinutes(-_timeout)));
#elif VER2
            ThreadPool.QueueUserWorkItem(ServiceCache, DateTime.UtcNow.AddMinutes(-_timeout));
#endif
        }

        /// <summary>
        /// The main method of the thread to service the cache. Checks for old items
        /// and removes them.
        /// </summary>
        /// <param name="purgeDate">The date before which items should be removed.</param>
        private void ServiceCache(object purgeDate)
        {
            Queue<Key> purgeKeys = new Queue<Key>();

            // Obtain a list of the keys to be purged.
            lock (this)
            {
#if VER4
                foreach (Key key in
                    _lastAccessed.Keys.Where(key => (DateTime) _lastAccessed[key] < (DateTime) purgeDate))
                {
                    purgeKeys.Enqueue(key);
                }
#elif VER2
                foreach (Key key in _lastAccessed.Keys)
                {
                    if (_lastAccessed[key] < (DateTime) purgeDate)
                        purgeKeys.Enqueue(key);
                }
#endif
            }

            // Remove the keys from the lists.
            if (purgeKeys.Count > 0)
            {
                while (purgeKeys.Count > 0)
                {
                    Key key = purgeKeys.Dequeue();
                    if (key != null)
                    {
                        lock (this)
                        {
                            if (_lastAccessed[key] < (DateTime) purgeDate)
                            {
                                _lastAccessed.Remove(key);
                                _internalCache.Remove(key);
                            }
                        }
                    }
                }
            }

            // Set the next service time to one minute from now.
            _nextServiceTime = DateTime.UtcNow.AddMinutes(1);
        }

        #endregion
    }
}