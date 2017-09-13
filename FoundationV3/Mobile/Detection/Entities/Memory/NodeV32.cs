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

namespace FiftyOne.Foundation.Mobile.Detection.Entities.Memory
{
    /// <summary>
    /// All data is loaded into memory when the entity is constructed.
    /// </summary>
    internal class NodeV32 : Node
    {
        #region Constructors

        /// <summary>
        /// Constructs a new instance of <see cref="NodeV32"/>.
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
        internal NodeV32(
            DataSet dataSet,
            int offset,
            BinaryReader reader)
            : base(dataSet, offset, reader)
        {
            _rankedSignatureCount = (int)reader.ReadUInt16();
            _children = 
                NodeFactoryShared.ReadNodeIndexesV32(
                    dataSet, 
                    reader, 
                    (int)((long)offset + reader.BaseStream.Position - _nodeStartStreamPosition), 
                    (int)_childrenCount);
            _numericChildren = 
                ReadNodeNumericIndexes(dataSet, reader, NumericChildrenCount);
            if (RankedSignatureCount > 0)
            {
                _nodeRankedSignatureValue = reader.ReadInt32();
            }
        }

        #endregion  
        
        #region Overrides

        /// <summary>
        /// Loads all the ranked signature indexes for the node.
        /// </summary>
        internal override void Init()
        {
            base.Init();
            if (_rankedSignatureIndexes == null)
            {
                _rankedSignatureIndexes = GetRankedSignatureIndexesAsArray();
            }
        }

        internal override IList<int> RankedSignatureIndexes
        {
            get 
            {
                if (_rankedSignatureIndexes == null)
                {
                    lock (this)
                    {
                        if (_rankedSignatureIndexes == null)
                        {
                            _rankedSignatureIndexes = GetRankedSignatureIndexesAsArray();
                        }
                    }
                }
                return _rankedSignatureIndexes; 
            }
        }
        private IList<int> _rankedSignatureIndexes;
        private int _nodeRankedSignatureValue;
                
        private IList<int> GetRankedSignatureIndexesAsArray()
        {
            IList<int> rankedSignatureIndexes = null;
            if (RankedSignatureCount == 0)
            {
                rankedSignatureIndexes = new int[] { };
            }
            else if (RankedSignatureCount == 1)
            {
                // The value of _nodeRankedSignatureIndex is the ranked signature
                // index when the node only relates to 1 signature.
                rankedSignatureIndexes = new int[] { _nodeRankedSignatureValue };
            }
            else if (RankedSignatureCount > 1)
            {
                // Where the node relates to multiple signatures the _nodeRankedSignatureIndex
                // relates to the first ranked signature index in DataSet.NodeRankedSignatureIndexes.
                rankedSignatureIndexes = DataSet.NodeRankedSignatureIndexes.GetRange(
                    _nodeRankedSignatureValue, RankedSignatureCount);
            }
            return rankedSignatureIndexes;
        }

        #endregion
    }
}
