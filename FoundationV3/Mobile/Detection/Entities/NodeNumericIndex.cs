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

namespace FiftyOne.Foundation.Mobile.Detection.Entities
{
    /// <summary>
    /// Represents a child of a node with a numeric value rather than
    /// character values. Used to support the Numeric matching method
    /// if an exact match can't be found.
    /// </summary>
    internal class NodeNumericIndex : BaseEntity
    {
        #region Properties

        /// <summary>
        /// The numeric value of the index.
        /// </summary>
        internal int Value
        {
            get { return base.Index; }
        }

        /// <summary>
        /// The node offset which relates to this sequence of characters.
        /// </summary>
        internal readonly int RelatedNodeOffset;

        /// <summary>
        /// The node the numeric index relates to.
        /// </summary>
        internal Node Node
        {
            get
            {
                if (_node == null)
                {
                    lock (this)
                    {
                        if (_node == null)
                        {
                            _node = DataSet.Nodes[RelatedNodeOffset];
                        }
                    }
                }
                return _node;
            }
        }
        private Node _node;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a new instance of <see cref="NodeNumericIndex"/>
        /// </summary>
        /// <param name="dataSet">
        /// The data set the node is contained within
        /// </param>
        /// <param name="value">
        /// The value of the numeric index. Added to it's index field.
        /// </param>
        /// <param name="relatedNodeOffset">
        /// The offset in the list of nodes to the node the index relates to
        /// </param>
        internal NodeNumericIndex(DataSet dataSet, short value, int relatedNodeOffset) 
            : base (dataSet, value)
        {
            RelatedNodeOffset = relatedNodeOffset;
        }

        #endregion
    }
}
