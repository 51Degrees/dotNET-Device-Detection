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

namespace FiftyOne.Foundation.Mobile.Detection.Entities
{
    /// <summary>
    /// Signature of a User-Agent in version 3.2 data format.
    /// </summary>
    public sealed class SignatureV32 : Signature
    {
        #region Fields

        /// <summary>
        /// The number of nodes associated with the signature.
        /// </summary>
        private readonly byte NodeCount;

        /// <summary>
        /// The index in the <see cref="DataSet.SignatureNodeOffsets"/> list of the first
        /// node associated with this signature.
        /// </summary>
        private readonly int FirstNodeOffsetIndex;

        /// <summary>
        /// Flags used to provide extra details about the signature.
        /// </summary>
        private readonly byte Flags;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the rank, where a lower number means the signature is more 
        /// popular, of the signature compared to other signatures.
        /// </summary>
        public override int Rank
        {
            get
            {
                return _rank;
            }
        }
        private readonly int _rank;

        #endregion

        #region Internal Properties

        /// <summary>
        /// List of the node offsets the signature relates to ordered by offset 
        /// of the node.
        /// </summary>
        internal override int[] NodeOffsets
        {
            get
            {
                if (_nodeOffsets == null)
                {
                    lock(this)
                    {
                        if (_nodeOffsets == null)
                        {
                            var range = DataSet.SignatureNodeOffsets.GetRange(FirstNodeOffsetIndex, NodeCount);
                            var enumerator = range.GetEnumerator();
                            var nodeOffsets = new int[NodeCount];
                            var i = 0;
                            while (enumerator.MoveNext())
                            {
                                nodeOffsets[i] = enumerator.Current.Value;
                                i++;
                            }
                            _nodeOffsets = nodeOffsets;
                        }
                    }
                }
                return _nodeOffsets;
            }
        }
        private int[] _nodeOffsets;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a new instance of <see cref="SignatureV32"/>.
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
        internal SignatureV32(
            DataSet dataSet,
            int index,
            Reader reader)
            : base(dataSet, index, reader)
        {
            NodeCount = reader.ReadByte();
            FirstNodeOffsetIndex = reader.ReadInt32();
            _rank = reader.ReadInt32();
            Flags = reader.ReadByte();
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
            var lastNode = DataSet.Nodes[DataSet.SignatureNodeOffsets[NodeCount + FirstNodeOffsetIndex - 1].Value];
            return lastNode.Position + lastNode.Length + 1;
        }

        #endregion
    }
}
