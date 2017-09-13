/* *********************************************************************
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited. 
 * Copyright © 2017 51Degrees Mobile Experts Limited, 5 Charlotte Close,
 * Caversham, Reading, Berkshire, United Kingdom RG4 7BY
 * 
 * This Source Code Form is the subject of the following patents and patent 
 * applications, owned by 51Degrees Mobile Experts Limited of 5 Charlotte
 * Close, Caversham, Reading, Berkshire, United Kingdom RG4 7BY: 
 * European Patent No. 2871816;
 * European Patent Application No. 17184134.9;
 * United States Patent Nos. 9,332,086 and 9,350,823; and
 * United States Patent Application No. 15/686,066.
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

using FiftyOne.Foundation.Mobile.Detection.Factories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FiftyOne.Foundation.Mobile.Detection.Entities.Stream
{
    /// <summary>
    /// Represents a <see cref="NodeV31"/> which can be used with the 
    /// Stream data set. NumericChidren and RankedSignatureIndexes are not loaded
    /// into memory when the entity is constructed, they're only loaded from the
    /// data source when requested.
    /// </summary>
    internal class NodeV31 : Node
    {
        #region Constructors

        /// <summary>
        /// Constructs a new instance of <see cref="NodeV31"/>.
        /// </summary>
        /// <param name="dataSet">
        /// The data set the node is contained within.
        /// </param>
        /// <param name="offset">
        /// The offset in the data structure to the node.
        /// </param>
        /// <param name="reader">
        /// Reader connected to the source data structure and positioned to 
        /// start reading.
        /// </param>
        internal NodeV31(
            IStreamDataSet dataSet,
            int offset,
            BinaryReader reader)
            : base(dataSet, offset, reader)
        {
            _rankedSignatureCount = reader.ReadInt32();
            _children = NodeFactoryShared.ReadNodeIndexesV31(
                (FiftyOne.Foundation.Mobile.Detection.Entities.DataSet)dataSet, 
                reader, 
                (int)((long)offset + reader.BaseStream.Position - this._nodeStartStreamPosition), 
                (int)_childrenCount);
            _position = reader.BaseStream.Position;
        }

        #endregion     
       
        #region Overrides

        /// <summary>
        /// An array of the ranked signature indexes for the node.
        /// </summary>
        internal override IList<int> RankedSignatureIndexes
        {
            get 
            {
                if (_rankedSignatureIndexes == null)
                {
                    lock(this)
                    {
                        if (_rankedSignatureIndexes == null)
                        {
                            var reader = _pool.GetReader();
                            try
                            {
                                reader.BaseStream.Position = 
                                    _position + ((sizeof(short) + sizeof(int)) * NumericChildrenCount);
                                _rankedSignatureIndexes = 
                                    Utils.ReadIntegerArray(reader, RankedSignatureCount);
                            }
                            finally
                            {
                                _pool.Release(reader);
                            }
                        }
                    }
                }
                return _rankedSignatureIndexes;
            }
        }
        private int[] _rankedSignatureIndexes;
        
        #endregion
    }
}
