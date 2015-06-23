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

using System;
using System.Collections.Generic;
using System.IO;
using FiftyOne.Foundation.Mobile.Detection.Factories;
using FiftyOne.Foundation.Mobile.Detection.Readers;

namespace FiftyOne.Foundation.Mobile.Detection.Entities.Stream
{
    /// <summary>
    /// A readonly list of fixed length entity types held on persistent storage rather
    /// than in memory.
    /// </summary>
    /// <para>
    /// Entities in the underlying data structure are either fixed length where the 
    /// data that represents them always contains the same number of bytes, or variable
    /// length where the number of bytes to represent the entity varies.
    /// </para>
    /// <para>
    /// This class uses the index of the entity in the accessor. The list is typically
    /// used by entities that need to be found quickly using a divide and conquer 
    /// algorithm.
    /// </para>
    /// <remarks>
    /// The constructor will read the header information about the underlying data structure.
    /// The data for each entity is only loaded when requested via the accessor. A cache is used
    /// to avoid creating duplicate objects when requested multiple times.
    /// </remarks>
    /// <remarks>
    /// Data sources which don't support seeking can not be used. Specifically compressed data 
    /// structures can not be used with these lists.
    /// </remarks>
    /// <remarks>
    /// Should not be referenced directly.
    /// </remarks>
    /// <typeparam name="T">The type of <see cref="BaseEntity"/> the list will contain</typeparam>
    public class FixedList<T> : BaseList<T>, IFixedList<T> where T : BaseEntity
    {
        #region Properties

        /// <summary>
        /// The number of items in the list.
        /// </summary>
        public int Count
        {
            get { return Header.Count; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a new instance of <see cref="FixedList{T}"/>
        /// </summary>
        /// <param name="dataSet">The <see cref="DataSet"/> being created</param>
        /// <param name="reader">Reader connected to the source data structure and positioned to start reading</param>
        /// <param name="entityFactory">Used to create new instances of the entity</param>
        /// <param name="cacheSize">Number of items in list to have capacity to cache</param>
        internal FixedList(
            DataSet dataSet, 
            Reader reader, 
            BaseEntityFactory<T> entityFactory,
            int cacheSize)
            : base(dataSet, reader, entityFactory, cacheSize)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a new entity of type T.
        /// </summary>
        /// <param name="index">The index of the entity being created</param>
        /// <param name="reader">
        /// Reader connected to the source data structure and positioned to start reading
        /// </param>
        /// <returns>A new entity of type T at the index provided</returns>
        internal override T CreateEntity(int index, Reader reader)
        {
            reader.BaseStream.Position = Header.StartPosition + (EntityFactory.GetLength() * index);
            return (T)EntityFactory.Create(_dataSet, index, reader);
        }

        /// <summary>
        /// An enumerator for the list between the range provided.
        /// </summary>
        /// <returns>An enumerator for the list</returns>
        public IEnumerable<T> GetRange(int index, int count)
        {
            var reader = _dataSet.Pool.GetReader();
            try
            {
                reader.BaseStream.Position = Header.StartPosition + (EntityFactory.GetLength() * index);
                for (int i = 0; i < count; i++)
                {
                    yield return (T)EntityFactory.Create(_dataSet, index, reader);
                }
            }
            finally
            {
                _dataSet.Pool.Release(reader);
            }
        }

        /// <summary>
        /// An enumeration for the underlying list.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            foreach (var item in GetRange(0, Count))
            {
                yield return item;
            }
        }

        /// <summary>
        /// An enumeration for the underlying list.
        /// </summary>
        /// <returns></returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach (var item in GetRange(0, Count))
            {
                yield return item;
            }
        }
        
        #endregion
    }
}
