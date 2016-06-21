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
using System;

namespace FiftyOne.Foundation.Mobile.Detection.Entities.Memory
{
    /// <summary>
    /// A readonly list of variable length entity types held in memory.
    /// </summary>
    /// <para>
    /// Entities in the underlying data structure are either fixed length where 
    /// the data that represents them always contains the same number of bytes, 
    /// or variable length where the number of bytes to represent the entity 
    /// varies.
    /// </para>
    /// <para>
    /// This class uses the offset of the first byte of the entities data in 
    /// the underlying data structure in the accessor. As such the list isn't 
    /// being used as a traditional list because items are not retrieved by 
    /// their index in the list, but by there offset in the underlying data 
    /// structure.
    /// </para>
    /// <remarks>
    /// The constructor will read the header information about the underlying 
    /// data structure and the entities are added to the list when the Read 
    /// method is called.
    /// </remarks>
    /// <remarks>
    /// The class supports source stream that do not support seeking.
    /// </remarks>
    /// <remarks>
    /// Should not be referenced directly.
    /// </remarks>
    /// <typeparam name="T">
    /// The type of item the list will contain.
    /// </typeparam>
    /// <typeparam name="D">
    /// The type of the shared data set the item is contained within.
    /// </typeparam>
    public class MemoryVariableList<T, D> : MemoryBaseList<T, D>, IReadonlyList<T> 
        where T : IComparable<int>
    {
        #region Classes

        private class SearchVariableList : SearchBase<T, int, T[]>
        {
            private readonly T[] _array;

            internal SearchVariableList(T[] array)
            {
                _array = array;
            }

            internal int BinarySearch(int offset)
            {
                return base.BinarySearchBase(_array, offset);
            }

            protected override int GetCount(T[] array)
            {
                return array.Length;
            }

            protected override T GetValue(T[] array, int index)
            {
                return array[index];
            }

            protected override int CompareTo(T item, int offset)
            {
                return item.CompareTo(offset);
            }
        }

        #endregion

        #region Fields

        /// <summary>
        /// Used to search for items in the list.
        /// </summary>
        private SearchVariableList _search;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a new instance of <see cref="MemoryVariableList{T,D}"/>.
        /// </summary>
        /// <param name="dataSet">
        /// The <see cref="DataSet"/> being created.
        /// </param>
        /// <param name="reader">
        /// Reader connected to the source data structure and positioned to 
        /// start reading.
        /// </param>
        /// <param name="entityFactory">
        /// Used to create new instances of the entity.
        /// </param>
        internal MemoryVariableList(
            D dataSet, 
            Reader reader,
            BaseEntityFactory<T, D> entityFactory)
            : base(dataSet, reader, entityFactory)
        {
        }

        #endregion

        #region Overriden Methods

        /// <summary>
        /// Reads the list into memory.
        /// </summary>
        /// <param name="reader">
        /// Reader connected to the source data structure and positioned to 
        /// start reading.
        /// </param>
        internal override void Read(Reader reader)
        {
            var offset = 0;
            var startPos = (int)reader.BaseStream.Position;
            for (int index = 0; index < Header.Count; index++)
            {
                _array[index] = EntityFactory.Create(_dataSet, offset, reader);
                offset = (int)reader.BaseStream.Position - startPos;
            }
            _search = new SearchVariableList(_array);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Accessor for the fixed list.
        /// </summary>
        /// <param name="offset">
        /// The offset position in the data structure to the entity to be 
        /// returned from the list.
        /// </param>
        /// <remarks>
        /// As all the entities are held in memory and in ascending order of 
        /// offset a BinarySearch can be used to determine the one that relates 
        /// to the given offset rapidly.
        /// </remarks>
        /// <returns>
        /// Entity at the offset requested.
        /// </returns>
        public T this[int offset]
        {
            get 
            {
                var index = _search.BinarySearch(offset);
                if (index >= 0)
                    return _array[index];
                return default(T);
            }
        }
        
        #endregion
    }
}
