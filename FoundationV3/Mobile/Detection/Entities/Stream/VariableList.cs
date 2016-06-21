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

using System;
using System.Collections.Generic;
using System.IO;
using FiftyOne.Foundation.Mobile.Detection.Factories;
using FiftyOne.Foundation.Mobile.Detection.Readers;

namespace FiftyOne.Foundation.Mobile.Detection.Entities.Stream
{
    /// <summary>
    /// A read only list of variable length entity types held on persistent 
    /// storage rather than in memory.
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
    /// data structure. The data for each entity is only loaded when requested 
    /// via the accessor. A cache is used to avoid creating duplicate objects 
    /// when requested multiple times.
    /// </remarks>
    /// <remarks>
    /// Data sources which don't support seeking can not be used. Specifically 
    /// compressed data structures can not be used with these lists.
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
    public class VariableList<T, D> : CacheList<T, D>, IReadonlyList<T>
        where D : IStreamDataSet
    {
        #region Constructor

        /// <summary>
        /// Constructs a new instance of <see cref="VariableList{T, D}"/>.
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
        /// <param name="cacheSize">
        /// Number of items in list to have capacity to cache.
        /// </param>
        internal VariableList(
            D dataSet, 
            Reader reader,
            BaseEntityFactory<T, D> entityFactory,
            int cacheSize)
            : base(dataSet, reader, entityFactory, cacheSize)
        {
        }

        #endregion

        #region Destructor

        /// <summary>
        /// Needed to overcome CA1063 in Code Analysis.
        /// </summary>
        /// <param name="disposing">
        /// True if the calling method is Dispose, false for the finaliser.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a new entity of type T.
        /// </summary>
        /// <param name="offset">
        /// The offset of the entity being created.
        /// </param>
        /// <param name="reader">
        /// Reader connected to the source data structure and positioned to 
        /// start reading.
        /// </param>
        /// <returns>
        /// A new entity of type T at the offset provided.
        /// </returns>
        internal override T CreateEntity(int offset, Reader reader)
        {
            reader.BaseStream.Position = Header.StartPosition + offset;
            return (T)EntityFactory.Create(_dataSet, offset, reader);
        }

        /// <summary>
        /// An enumerator for the list.
        /// </summary>
        /// <returns>
        /// An enumerator for the list.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            var offset = 0;
            while (offset < Header.Length)
            {
                var entity = this[offset];
                yield return entity;
                offset += EntityFactory.GetLength(entity);
            }
        }

        /// <summary>
        /// An enumerator for the list.
        /// </summary>
        /// <returns>
        /// An enumerator for the list.
        /// </returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
