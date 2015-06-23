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
using System.Linq;
using System.Text;

namespace FiftyOne.Foundation.Mobile.Detection.Entities.Stream
{
    /// <summary>
    /// Profile entity with stream specific data access implementation.
    /// </summary>
    public class Profile : Entities.Profile
    {
        #region Fields

        /// <summary>
        /// The position in the data set for the start of the value indexes.
        /// </summary>
        private readonly long _position;

        /// <summary>
        /// The number of value indexes in the entity.
        /// </summary>
        private readonly int _valueIndexesCount;

        /// <summary>
        /// The number of signature indexes in the entity.
        /// </summary>
        private readonly int _signatureIndexesCount;

        /// <summary>
        /// Reference to the pool from the dataset.
        /// </summary>
        private readonly Pool _pool;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a new instance of the <see cref="Profile"/>
        /// </summary>
        /// <param name="dataSet">
        /// The data set whose profile list the profile will be contained within
        /// </param>
        /// <param name="offset">
        /// The offset position in the data structure to the profile</param>
        /// <param name="reader">
        /// Reader connected to the source data structure and positioned to start reading
        /// </param>
        internal Profile(
            DataSet dataSet,
            int offset,
            BinaryReader reader)
            : base(dataSet, offset, reader)
        {
            _pool = dataSet.Pool;
            _valueIndexesCount = reader.ReadInt32();
            _signatureIndexesCount = reader.ReadInt32();
            _position = reader.BaseStream.Position;
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Array of value indexes associated with the profile.
        /// </summary>
        protected internal override int[] ValueIndexes
        {
            get 
            { 
                if (_valueIndexes == null)
                {
                    lock(this)
                    {
                        if (_valueIndexes == null)
                        {
                            var reader = _pool.GetReader();
                            try
                            {
                                reader.BaseStream.Position = _position;
                                _valueIndexes = BaseEntity.ReadIntegerArray(reader, _valueIndexesCount);
                            }
                            finally
                            {
                                _pool.Release(reader);
                            }
                        }
                    }
                }
                return _valueIndexes;
            }
        }
        private int[] _valueIndexes;

        /// <summary>
        /// Array of signature indexes associated with the profile.
        /// </summary>
        protected internal override int[] SignatureIndexes
        {
            get 
            {
                if (_signatureIndexes == null)
                {
                    lock (this)
                    {
                        if (_signatureIndexes == null)
                        {
                            var reader = _pool.GetReader();
                            try
                            {
                                reader.BaseStream.Position = _position + (_valueIndexesCount * sizeof(int));
                                _signatureIndexes = BaseEntity.ReadIntegerArray(reader, _signatureIndexesCount);
                            }
                            finally
                            {
                                _pool.Release(reader);
                            }
                        }
                    }
                }
                return _signatureIndexes;
            }
        }
        private int[] _signatureIndexes;

        #endregion
    }
}
