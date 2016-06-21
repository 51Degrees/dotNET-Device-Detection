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

namespace FiftyOne.Foundation.Mobile.Detection.Entities
{
    /// <summary>
    /// Base class used by all node indexes containing common functionality.
    /// </summary>
    internal abstract class NodeIndexBase : BaseEntity<DataSet>
    {
        #region Fields

        /// <summary>
        /// The node offset which relates to this sequence of characters.
        /// </summary>
        internal readonly int RelatedNodeOffset;

        #endregion

        #region Properties

        /// <summary>
        /// The node this index relates to.
        /// </summary>
        /// <remarks>
        /// This property will not store a reference to the node if one
        /// does not exist. This is needed so that root nodes can be stored
        /// when used in stream mode without maintaining a reference to the 
        /// entire tree. Without this important design choice stream mode will
        /// not garbage collect unused nodes.
        /// </remarks>
        internal Node Node
        {
            get
            {
                if (_node != null)
                    return _node;

                if (DataSet.Nodes is Memory.MemoryBaseList<Node, DataSet>)
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

                return DataSet.Nodes[RelatedNodeOffset];
            }
        }
        private Node _node;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a new instance of <see cref="NodeIndex"/>.
        /// </summary>
        /// <param name="dataSet">
        /// The data set the node is contained within.
        /// </param>
        /// <param name="index">
        /// The index of this object in the Node.
        /// </param>
        /// <param name="relatedNodeOffset">
        /// The offset in the list of nodes to the node the index relates to.
        /// </param>
        internal NodeIndexBase(
            DataSet dataSet,
            int index,
            int relatedNodeOffset)
            : base(dataSet, index)
        {
            this.RelatedNodeOffset = relatedNodeOffset;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Called after the entire data set has been loaded to ensure 
        /// any further initialisation steps that require other items in
        /// the data set can be completed.
        /// </summary>
        internal virtual void Init()
        {
            if (_node == null)
            {
                _node = DataSet.Nodes[RelatedNodeOffset];
            }
        }

        #endregion
    }
}
