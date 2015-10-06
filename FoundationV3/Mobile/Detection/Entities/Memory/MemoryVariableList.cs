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

using System.IO;
using FiftyOne.Foundation.Mobile.Detection.Factories;
using FiftyOne.Foundation.Mobile.Detection.Readers;
using System.Collections.Generic;

namespace FiftyOne.Foundation.Mobile.Detection.Entities.Memory
{
    /// <summary>
    /// A readonly list of variable length entity types held in memory.
    /// </summary>
    /// <para>
    /// Entities in the underlying data structure are either fixed length where the 
    /// data that represents them always contains the same number of bytes, or variable
    /// length where the number of bytes to represent the entity varies.
    /// </para>
    /// <para>
    /// This class uses the offset of the first byte of the entities data in the underlying
    /// data structure in the accessor. As such the list isn't being used as a traditional
    /// list because items are not retrieved by their index in the list, but by there offset
    /// in the underlying data structure.
    /// </para>
    /// <remarks>
    /// The constructor will read the header information about the underlying data structure
    /// and the entities are added to the list when the Read method is called.
    /// </remarks>
    /// <remarks>
    /// The class supports source stream that do not support seeking.
    /// </remarks>
    /// <remarks>
    /// Should not be referenced directly.
    /// </remarks>
    /// <typeparam name="T">The type of <see cref="BaseEntity"/> the list will contain</typeparam>
    public class MemoryVariableList<T> : MemoryBaseList<T>, IReadonlyList<T> where T : BaseEntity
    {
        #region Constructor
        
        /// <summary>
        /// Constructs a new instance of <see cref="MemoryVariableList{T}"/>
        /// </summary>
        /// <param name="dataSet">The <see cref="DataSet"/> being created</param>
        /// <param name="reader">Reader connected to the source data structure and positioned to start reading</param>
        /// <param name="entityFactory">Used to create new instances of the entity</param>
        internal MemoryVariableList(
            DataSet dataSet, 
            Reader reader,
            BaseEntityFactory<T> entityFactory)
            : base(dataSet, reader, entityFactory)
        {
        }

        #endregion

        #region Overriden Methods

        /// <summary>
        /// Reads the list into memory.
        /// </summary>
        /// <param name="reader">
        /// Reader connected to the source data structure and positioned to start reading
        /// </param>
        internal override void Read(Reader reader)
        {
            var offset = 0;
            for (int index = 0; index < Header.Count; index++)
            {
                T entity = (T)EntityFactory.Create(_dataSet, offset, reader);
                _array[index] = entity;
                offset += EntityFactory.GetLength(entity);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Uses a divide and conquer method to search the ordered list of entities
        /// that are held in memory.
        /// </summary>
        /// <param name="offset">
        /// The offset position in the data structure to the entity to be returned from the list
        /// </param>
        /// <returns>Entity at the offset requested</returns>
        private int BinarySearch(int offset)
        {
            var lower = 0;
            var upper = Count - 1;

            while (lower <= upper)
            {
                var middle = lower + (upper - lower) / 2;
                var comparisonResult = _array[middle].Index.CompareTo(offset);
                if (comparisonResult == 0)
                    return middle;
                else if (comparisonResult > 0)
                    upper = middle - 1;
                else
                    lower = middle + 1;
            }

            return -1;
        }

        /// <summary>
        /// Accessor for the fixed list.
        /// </summary>
        /// <param name="offset">
        /// The offset position in the data structure to the entity to be returned from the list
        /// </param>
        /// <remarks>
        /// As all the entities are held in memory and in ascending order of offset a 
        /// BinarySearch can be used to determine the one that relates to the given offset
        /// rapidly.
        /// </remarks>
        /// <returns>Entity at the offset requested</returns>
        public T this[int offset]
        {
            get 
            {
                var index = BinarySearch(offset);
                if (index >= 0)
                    return _array[index];
                return null;
            }
        }
        
        #endregion
    }
}
