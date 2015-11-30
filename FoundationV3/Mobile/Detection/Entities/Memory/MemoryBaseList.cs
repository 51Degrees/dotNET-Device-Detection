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
using System.Collections.Generic;
using System;
using FiftyOne.Foundation.Mobile.Detection.Entities.Headers;
using FiftyOne.Foundation.Mobile.Detection.Factories;
using FiftyOne.Foundation.Mobile.Detection.Readers;

namespace FiftyOne.Foundation.Mobile.Detection.Entities.Memory
{
    /// <summary>
    /// <para>
    /// Lists can be stored as a set of related objects entirely within memory, 
    /// or as the relevant objects loaded as required from a file or other 
    /// permanent store.
    /// </para>
    /// </summary>
    /// <para>
    /// This class provides base functions for lists implemented in memory 
    /// using arrays of type T. 
    /// </para>
    /// <remarks>
    /// Delegate methods are used to create new instances of items to add to 
    /// the list in order to avoid creating many inherited list classes for 
    /// each <see cref="BaseEntity"/> type.
    /// </remarks>
    /// <remarks>
    /// The data is held in the private readonly variable _listArray.
    /// </remarks>
    /// <remarks>
    /// Should not be referenced directly.
    /// </remarks>
    /// <typeparam name="T">
    /// The type of <see cref="BaseEntity"/> the list will contain
    /// </typeparam>
    public abstract class MemoryBaseList<T> : IEnumerable<T>
    {
        #region Fields

        /// <summary>
        /// Information about the data structure the list is associated with.
        /// </summary>
        internal readonly Header Header;

        /// <summary>
        /// Method used to create a new instance of an item in the list.
        /// </summary>
        internal readonly BaseEntityFactory<T> EntityFactory;

        /// <summary>
        /// The dataset which contains the list.
        /// </summary>
        protected internal readonly DataSet _dataSet;

        /// <summary>
        /// Array of items contained in the list.
        /// </summary>
        protected internal readonly T[] _array; 

        #endregion

        #region Properties

        /// <summary>
        /// The number of entities the list contains.
        /// </summary>
        public int Count
        {
            get { return _array.Length; }
        }

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Reads all the records to be added to the list.
        /// </summary>
        /// <param name="reader">
        /// Reader connected to the source data structure and positioned to start reading
        /// </param>
        internal abstract void Read(Reader reader);

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a new instance of <see cref="MemoryBaseList{T}"/>. 
        /// The Read method needs to be called following construction to read 
        /// all the entities which form the list before the list can be used.
        /// </summary>
        /// <param name="dataSet">
        /// Dataset being created.
        /// </param>
        /// <param name="reader">
        /// Reader used to initialise the header only.
        /// </param>
        /// <param name="entityFactory">
        /// Used to create new instances of the entity.
        /// </param>
        internal MemoryBaseList(DataSet dataSet, Reader reader, BaseEntityFactory<T> entityFactory)
        {
            Header = new Header(reader);
            _array = new T[Header.Count];
            _dataSet = dataSet;
            EntityFactory = entityFactory;
        }

        #endregion

        #region Destructor

        /// <summary>
        /// Ensures any resources used by the list are disposed.
        /// </summary>
        ~MemoryBaseList()
        {
            Dispose(false);
        }

        /// <summary>
        /// Disposes of any resources used by the list.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Disposes of any resources used by the list.
        /// </summary>
        /// <param name="disposing">
        /// True if the calling method is Dispose, false for the finaliser.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            GC.SuppressFinalize((object)this);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// An enumerator to the array of items.
        /// </summary>
        /// <returns>
        /// An enumerator to the array of items.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            foreach (var entity in _array)
                yield return entity;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
        #endregion
    }
}
