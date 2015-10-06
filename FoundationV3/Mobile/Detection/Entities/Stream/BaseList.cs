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
using System.IO;
using System.Collections.Generic;
using System.Threading;
using FiftyOne.Foundation.Mobile.Detection.Entities.Headers;
using FiftyOne.Foundation.Mobile.Detection.Factories;
using FiftyOne.Foundation.Mobile.Detection.Readers;

namespace FiftyOne.Foundation.Mobile.Detection.Entities.Stream
{
    /// <summary>
    /// <para>
    /// Lists can be stored as a set of related objects entirely within memory, or 
    /// the relevent objects loaded as required from a file or other permanent store
    /// as required.
    /// </para>
    /// </summary>
    /// <remarks>
    /// Delegate methods are used to create new instances of items to add to the list
    /// in order to avoid creating many inherited list classes for each 
    /// <see cref="BaseEntity"/> type.
    /// </remarks>
    /// <remarks>
    /// Should not be referenced directly.
    /// </remarks>
    /// <typeparam name="T">The type of <see cref="BaseEntity"/> the list will contain</typeparam>
    public abstract class BaseList<T> where T : BaseEntity
    {
        #region Fields
        
        /// <summary>
        /// Information about the data structure the list is associated with.
        /// </summary>
        internal readonly Header Header;

        /// <summary>
        /// Factory used to create new instances of the entity.
        /// </summary>
        internal readonly BaseEntityFactory<T> EntityFactory;

        /// <summary>
        /// The dataset which contains the list.
        /// </summary>
        protected internal readonly DataSet _dataSet;

        #endregion

        #region Properties

        /// <summary>
        /// The number of items in the list.
        /// </summary>
        public int Count
        {
            get { return Header.Count; }
        }

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Used to create a new entity of type T when an item is required from the list.
        /// </summary>
        /// <param name="key">
        /// The offset position in the data structure to the entity to be returned from the list,
        /// or the index of the entity to be returned from the list.
        /// </param>
        /// <param name="reader">
        /// Reader connected to the source data structure and positioned to start reading
        /// </param>
        /// <returns>A new instance of type T</returns>
        internal abstract T CreateEntity(int key, Reader reader);

        #endregion

        #region Constructor
        
        /// <summary>
        /// Constructs a new instance of <see cref="BaseList{T}"/> ready to read entities from 
        /// the source.
        /// </summary>
        /// <param name="dataSet">Dataset being created</param>
        /// <param name="reader">Reader used to initialise the header only</param>
        /// <param name="entityFactory">Used to create new instances of the entity</param>
        internal BaseList(
            DataSet dataSet, 
            Reader reader,
            BaseEntityFactory<T> entityFactory)
        {
            _dataSet = dataSet;    
            Header = new Header(reader);
            EntityFactory = entityFactory;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Retrieves the entity at the offset or index requested.
        /// </summary>
        /// <param name="key">Index or offset of the entity required</param>
        /// <returns>A new instance of the entity at the offset or index</returns>
        public virtual T this[int key]
        {
            get
            {
                T item;
                var reader = _dataSet.Pool.GetReader();
                try
                {
                    item = CreateEntity(key, reader);
                }
                finally
                {
                    _dataSet.Pool.Release(reader);
                }
                return item;
            }
        }
       
        #endregion
    }
}
