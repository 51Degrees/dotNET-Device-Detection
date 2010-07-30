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
 * Mobile Experts Limited are Copyright (C) 2009 - 2010. All Rights Reserved.
 * 
 * Contributor(s):
 *     James Rosewell <james@51degrees.mobi>
 * 
 * ********************************************************************* */

using System;
using System.Collections.Generic;
using System.Collections;
using System.Threading;

namespace FiftyOne
{
    internal class Cache<Value> : Cache<string, Value> 
    {
        internal Cache(int timeout) : base(timeout) { }
    }

    internal class Cache<Key,Value>
    {
        private Dictionary<Key, Value> _internalCache = null;
        private Dictionary<Key, DateTime> _lastAccessed = null;

        private int _timeout = 0;
        // The last time this process serviced the cache file.
        private static DateTime _nextServiceTime = DateTime.MinValue;

        internal Cache(int timeout)
        {
            _internalCache = new Dictionary<Key, Value>();
            _lastAccessed = new Dictionary<Key, DateTime>();
            _timeout = timeout;
        }

        internal protected bool GetTryParse(Key key, out Value value)
        {
            bool result = false;
            if (key != null)
            {
                lock (this)
                {
                    result = _internalCache.TryGetValue(key, out value);
                    if (result == true)
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

        internal protected bool Contains(Key key)
        {
            bool result = false;
            if (key != null)
            {
                lock (this)
                {
                    result = _internalCache.ContainsKey(key);
                    if (result == true)
                        _lastAccessed[key] = DateTime.UtcNow;
                }
            }
            return result;
        }

        internal protected Value this[Key key]
        {
            get
            {
                Value value;
                lock (this)
                {
                    if (_internalCache.TryGetValue(key, out value) == true)
                        _lastAccessed[key] = DateTime.UtcNow;
                }
                CheckIfServiceRequired();
                return value;
            }
            set
            {
                lock (this)
                {
                    if (Contains(key) == true)
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

        private void CheckIfServiceRequired()
        {
            if (_nextServiceTime < DateTime.UtcNow && _internalCache.Count > 0)
            {
                // Set the next service time to a date far in the future
                // to prevent another thread being started.
                _nextServiceTime = DateTime.MaxValue;
                ThreadPool.QueueUserWorkItem(ServiceCache, DateTime.UtcNow.AddMinutes(-_timeout));
            }
        }

        private void ServiceCache(object purgeDate)
        {
            Queue<Key> purgeKeys = new Queue<Key>();

            // Obtain a list of the keys to be purged.
            lock (this)
            {
                foreach (Key key in _lastAccessed.Keys)
                {
                    if ((DateTime)_lastAccessed[key] < (DateTime)purgeDate)
                        purgeKeys.Enqueue(key);
                }
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
                            if ((DateTime)_lastAccessed[key] < (DateTime)purgeDate)
                            {
                                _lastAccessed.Remove(key);
                                _internalCache.Remove(key);
                            }
                        }
                    }
                }
                GC.Collect();
            }

            // Set the next service time to one minute from now.
            _nextServiceTime = DateTime.UtcNow.AddMinutes(1);
        }

        internal protected void Remove(Key key)
        {
            if (_internalCache.ContainsKey(key))
                _internalCache.Remove(key);
        }
    }
}
