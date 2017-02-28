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

using System.Collections.Generic;
using FiftyOne.Foundation.Mobile.Detection.Entities.Headers;
using FiftyOne.Foundation.Mobile.Detection.Factories;
using FiftyOne.Foundation.Mobile.Detection.Readers;

namespace FiftyOne.Foundation.Mobile.Detection.Entities.Stream
{
    internal class IntegerList : ISimpleList
    {
        #region Fields

        /// <summary>
        /// Information about the data structure the list is associated with.
        /// </summary>
        private readonly Header _header;

        /// <summary>
        /// The dataset which contains the list.
        /// </summary>
        protected internal readonly IndirectDataSet _dataSet;
        
        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a new instance of <see cref="IntegerList"/> ready to 
        /// read entities from the source.
        /// </summary>
        /// <param name="dataSet">
        /// Dataset being created.
        /// </param>
        /// <param name="reader">
        /// Reader used to initialise the header only.
        /// </param>
        internal IntegerList(
            IndirectDataSet dataSet,
            Reader reader)
        {
            _header = new Header(reader);
            _dataSet = dataSet;
        }

        #endregion
        
        #region Methods

        /// <summary>
        /// Returns the value in the list at the index provided.
        /// </summary>
        /// <param name="index">Index of the value required.</param>
        /// <returns>Value at the index requested</returns>
        public int this[int index]
        {
            get
            {
                var result = 0;
                var reader = _dataSet.Pool.GetReader();
                try
                {
                    reader.BaseStream.Position =
                        _header.StartPosition + (sizeof(int) * index);
                    result = reader.ReadInt32();
                }
                finally
                {
                    _dataSet.Pool.Release(reader);
                }
                return result;
            }
        }

        /// <summary>
        /// Returns the values in the list starting at the index provided.
        /// </summary>
        /// <param name="index">
        /// First index of the range required.
        /// </param>
        /// <param name="count">
        /// Number of elements to return.
        /// </param>
        /// <returns>
        /// A list of the items in the range requested.
        /// </returns>
        public IList<int> GetRange(int index, int count)
        {
            var result = new int[count];
            var reader = _dataSet.Pool.GetReader();
            try
            {
                reader.BaseStream.Position =
                    _header.StartPosition + (sizeof(int) * index);
                for (int i = 0; i < count; i++)
                {
                    result[i] = reader.ReadInt32();
                }
            }
            finally
            {
                _dataSet.Pool.Release(reader);
            }
            return result;
        }

        /// <summary>
        /// The number of items in the list.
        /// </summary>
        public int Count
        {
            get { return _header.Count; }
        }

        #endregion
    }
}
