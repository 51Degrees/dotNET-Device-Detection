using System;
using FiftyOne.Foundation.Mobile.Detection.Caching;

namespace Caching_Configuration
{
    /// <summary>
    /// Implementation of ICacheBuilder to build 
    /// <see cref="CustomCache{K, V}"/> 
    /// </summary>
    public class CustomCacheBuilder : ICacheBuilder
    {
        /// <summary>
        /// Build and return a <see cref="CustomCache{K, V}"/> 
        /// </summary>
        /// <typeparam name="K">
        /// The type of the key to use when accessing cache items
        /// </typeparam>
        /// <typeparam name="V">
        /// The type of the data items to store in the cache
        /// </typeparam>
        /// <param name="cacheSize">
        /// Not used by CustomCache
        /// </param>
        /// <returns>
        /// The cache
        /// </returns>
        public ICache<K, V> Build<K, V>(int cacheSize)
        {
            return new CustomCache<K, V>();
        }
    }
}
