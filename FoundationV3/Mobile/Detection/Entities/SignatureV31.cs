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

using System.Linq;
using FiftyOne.Foundation.Mobile.Detection.Readers;
using System.Collections.Generic;

namespace FiftyOne.Foundation.Mobile.Detection.Entities
{
    /// <summary>
    /// Signature of a User-Agent in version 3.1 data format.
    /// </summary>
    public sealed class SignatureV31 : Signature
    {
        #region Internal Properties

        /// <summary>
        /// List of the node offsets the signature relates to ordered
        /// by offset of the node.
        /// </summary>
        internal override IList<int> NodeOffsets
        {
            get
            {
                return _nodeOffsets;
            }
        }
        private readonly IList<int> _nodeOffsets;
        
        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the rank, where a lower number means the signature is more 
        /// popular, of the signature compared to other signatures.
        /// </summary>
        /// <remarks>
        /// As the property uses the ranked signature indexes list to obtain 
        /// the rank it will be comparatively slow compared to other methods 
        /// the firs time the property is accessed.
        /// </remarks>
        public override int Rank
        {
            get
            {
                if (_rank == null)
                {
                    lock(this)
                    {
                        if (_rank == null)
                        {
                            _rank = GetSignatureRank();
                        }
                    }
                }
                return _rank.Value;
            }
        }
        internal int? _rank;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a new instance of <see cref="SignatureV31"/>.
        /// </summary>
        /// <param name="dataSet">
        /// The data set the signature is contained within.
        /// </param>
        /// <param name="index">
        /// The index in the data structure to the signature.
        /// </param>
        /// <param name="reader">
        /// Reader connected to the source data structure and positioned to 
        /// start reading.
        /// </param>
        internal SignatureV31(
            DataSet dataSet,
            int index,
            Reader reader)
            : base(dataSet, index, reader)
        {
            _nodeOffsets = ReadOffsets(dataSet, reader, dataSet.SignatureNodesCount);
        }

        #endregion

        #region Methods

        /// <summary>
        /// The number of characters in the signature.
        /// </summary>
        /// <returns>
        /// The number of characters in the signature.
        /// </returns>
        internal override int GetSignatureLength()
        {
            var lastNode = DataSet.Nodes[NodeOffsets[NodeOffsets.Count - 1]];
            return lastNode.Position + lastNode.Length + 1;
        }

        /// <summary>
        /// Gets the signature rank by iterating through the list of signature 
        /// ranks.
        /// </summary>
        /// <returns>
        /// Rank compared to other signatures starting at 0.
        /// </returns>
        private int GetSignatureRank()
        {
            for (var rank = 0; rank < DataSet.RankedSignatureIndexes.Count; rank++)
            {
                if (DataSet.RankedSignatureIndexes[rank] == this.Index)
                {
                    return rank;
                }
            }
            return int.MaxValue;
        }

        #endregion
    }
}
