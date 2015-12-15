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
using FiftyOne.Foundation.Mobile.Detection.Entities.Headers;

namespace FiftyOne.Foundation.Mobile.Detection.Entities.Memory
{
    internal class MemoryIntegerList : ISimpleList
    {
        #region Fields

        internal readonly Header Header;

        /// <summary>
        /// Array of items contained in the list.
        /// </summary>
        protected internal readonly int[] _array;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a new instance of <see cref="MemoryIntegerList"/>.
        /// </summary>
        /// <param name="reader">
        /// Reader connected to the source data structure and positioned to 
        /// start reading.
        /// </param>
        internal MemoryIntegerList(Reader reader)
        {
            Header = new Header(reader);
            _array = new int[Header.Count];
        }

        #endregion

        #region Methods

        /// <summary>
        /// Reads the list into memory.
        /// </summary>
        /// <param name="reader">
        /// Reader connected to the source data structure and positioned to 
        /// start reading.
        /// </param>
        internal void Read(Reader reader)
        {
            for (int index = 0; index < Header.Count; index++)
            {
                _array[index] = reader.ReadInt32();
            }
        }

        #endregion

        #region Overridden Methods

        /// <summary>
        /// Accessor for the fixed list.
        /// </summary>
        /// <param name="index">
        /// The index of the entity to be returned from the list.
        /// </param>
        /// <returns>
        /// Entity at the index requested.
        /// </returns>
        public int this[int index]
        {
            get { return _array[index]; }
        }

        /// <summary>
        /// An enumerable that can return a range of T between index
        /// and the count provided.
        /// </summary>
        /// <param name="index">
        /// First index of the range required.
        /// </param>
        /// <param name="count">
        /// Number of elements to return.
        /// </param>
        /// <returns>
        /// An enumerator for the list.
        /// </returns>
        public IList<int> GetRange(int index, int count)
        {
            return new ArrayRange<int>(index, count, _array);
        }

        public int Count
        {
            get { return _array.Length; }
        }

        public void Dispose()
        {
            // Nothing to do in memory list.
        }

        #endregion
    }
}
