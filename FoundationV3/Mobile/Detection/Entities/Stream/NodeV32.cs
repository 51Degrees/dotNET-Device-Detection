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

using FiftyOne.Foundation.Mobile.Detection.Factories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FiftyOne.Foundation.Mobile.Detection.Entities.Stream
{
    /// <summary>
    /// Represents a <see cref="Entities.Node"/> which can be used with the 
    /// Stream data set. NumericChidren and RankedSignatureIndexes are not 
    /// loaded into memory when the entity is constructed, they're only loaded 
    /// from the data source when requested.
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
                    (FiftyOne.Foundation.Mobile.Detection.Entities.DataSet)dataSet, 
                    reader, 
                    (int)((long)offset + reader.BaseStream.Position - this._nodeStartStreamPosition), 
                    (int)this._childrenCount);
            _position = reader.BaseStream.Position;
        }

        #endregion     
       
        #region Overrides

        /// <summary>
        /// A list of all the signature indexes that relate to this node.
        /// </summary>
        internal override int[] RankedSignatureIndexes
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
        private int[] _rankedSignatureIndexes;
        
        /// <summary>
        /// Gets the ranked signature indexes array for the node.
        /// </summary>
        /// <returns>
        /// An array of length _rankedSignatureCount filled with ranked 
        /// signature indexes.
        /// </returns>
        private int[] GetRankedSignatureIndexesAsArray()
        {
            int[] rankedSignatureIndexes;
            if (RankedSignatureCount == 0)
            {
                rankedSignatureIndexes = new int[0];
            }
            else
            {
                var reader = _pool.GetReader();
                try
                {
                    // Position the reader after the numeric children.
                    reader.BaseStream.Position = 
                        _position + ((sizeof(short) + sizeof(int)) * NumericChildrenCount);
                    
                    // Read the index.
                    var index = reader.ReadInt32();

                    // Set the size of the array to return.
                    rankedSignatureIndexes = new int[RankedSignatureCount];

                    if (RankedSignatureCount == 1)
                    {
                        // If the count is one then the value is the ranked signature index.
                        rankedSignatureIndexes[0] = index;    
                    }
                    else
                    {
                        // If the count is greater than one then the value is 
                        // the index of the first ranked signature index in the 
                        // merged list.
                        var range = 
                            DataSet.NodeRankedSignatureIndexes.GetRange(index, RankedSignatureCount);
                        var enumerator = range.GetEnumerator();
                        var i = 0;
                        while (enumerator.MoveNext())
                        {
                            rankedSignatureIndexes[i] = enumerator.Current.Value;
                            i++;
                        }
                    }
                }
                finally
                {
                    _pool.Release(reader);
                }
            }
            return rankedSignatureIndexes;
        }

        #endregion
    }
}
