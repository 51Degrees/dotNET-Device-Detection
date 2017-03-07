using FiftyOne.Foundation.Mobile.Detection.Caching;
using FiftyOne.Foundation.Mobile.Detection.Entities.Headers;
using FiftyOne.Foundation.Mobile.Detection.Entities.Stream;
using System;

namespace FiftyOne.Foundation.Mobile.Detection.Factories
{
    /// <summary>
    /// Static factory class used to create 
    /// <see cref="DataSetBuilder.EntityLoader{V, D}"/> objects 
    /// </summary>
    public static class EntityLoaderFactory
    {
        /// <summary>
        /// Helper to create an appropriate loader for a cached list given the 
        /// cache type.
        /// </summary>
        /// <typeparam name="T">the type</typeparam>
        /// <typeparam name="D">the dataset type</typeparam>
        /// <param name="header">the header defining the list this will create 
        /// the loader for</param>
        /// <param name="cache">the cache, or null</param>
        /// <param name="dataset">the dataset</param>
        /// <param name="factory">the factory for the type</param>
        /// <returns>an entity loader</returns>
        public static DataSetBuilder.EntityLoader<T, D> GetLoaderFor<T, D>(
            Header header,
            ICache<int, T> cache,
            D dataset,
            BaseEntityFactory<T, D> factory)
            where D : IStreamDataSet
        {
            DataSetBuilder.EntityLoader<T, D> loader;
            if (cache == null)
            {
                loader = new DataSetBuilder.EntityLoader<T, D>(header, dataset, factory);
            }
            else if (cache is LruCache<int, T>)
            {
                loader = new DataSetBuilder.LruEntityLoader<T, D>(header, dataset, factory, (LruCache<int, T>)cache);
            }
            else if (cache is IPutCache<int, T>)
            {
                loader = new DataSetBuilder.CachedEntityLoader<T, D>(header, dataset, factory, (IPutCache<int, T>)cache);
            }
            else
            {
                throw new ArgumentException("cache must be null or an implementation of LruCache or IPutCache", "cache");
            }
            return loader;
        }
    }
}
