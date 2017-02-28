using FiftyOne.Foundation.Mobile.Detection.Entities;
using FiftyOne.Foundation.Mobile.Detection.Entities.Stream;
using System.Collections;
using System.Collections.Generic;

namespace FiftyOne.Foundation.Mobile.Detection
{

    /// <summary>
    /// Implementation of IReadOnlyList for Streams
    /// </summary>
    /// <typeparam name="T">type of entity</typeparam>
    /// <typeparam name="D">type of dataset</typeparam>
    public class StreamList<T, D> : IReadonlyList<T> 
        where T : BaseEntity
        where D : IStreamDataSet
    {
        private DataSetBuilder.EntityLoader<T, D> _loader;

        /// <summary>
        /// StreamList constructor
        /// </summary>
        /// <param name="loader">
        /// The loader used to get 
        /// values for the list
        /// </param>
        public StreamList(DataSetBuilder.EntityLoader<T, D> loader)
        {
            _loader = loader;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public T this[int i]
        {
            get { return _loader.Load(i); }
        }

        /// <summary>
        /// The total number of items in the list
        /// (i.e. not just those currently loaded into memory)
        /// </summary>
        public int Count
        {
            get { return _loader.GetHeader().Count; }
        }

        /// <summary>
        /// Get an enumerator that will read through 
        /// the list sequentially.
        /// </summary>
        /// <returns>
        /// An enumerator that will read through 
        /// the list sequentially.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            // the item number
            int count = 0;
            // the position in the file or the item number (as above)
            // depending on whether the entity is fixed or variable size
            int position = 0;
            // number of elements
            int total = _loader.GetHeader().Count;

            while (count < total)
            {
                T result = this[position];
                count++;
                position = _loader.NextPosition(position, result);
                yield return result;
            }
        }

        /// <summary>
        /// Get an enumerator that will read through 
        /// the list sequentially.
        /// </summary>
        /// <returns>
        /// An enumerator that will read through 
        /// the list sequentially.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Dispose of the list
        /// </summary>
        public void Dispose() { }
    };
}
