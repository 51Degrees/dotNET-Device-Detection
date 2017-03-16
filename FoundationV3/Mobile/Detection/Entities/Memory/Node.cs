/* *********************************************************************
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited. 
 * Copyright © 2017 51Degrees Mobile Experts Limited, 5 Charlotte Close,
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

namespace FiftyOne.Foundation.Mobile.Detection.Entities.Memory
{
    /// <summary>
    /// All data is loaded into memory when the entity is constructed.
    /// </summary>
    internal abstract class Node : Entities.Node
    {
        #region Constructors

        /// <summary>
        /// Constructs a new instance of <see cref="Node"/>.
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
        internal Node(
            DataSet dataSet,
            int offset,
            BinaryReader reader)
            : base(dataSet, offset, reader)
        {
        }

        #endregion  
        
        #region Overrides

        /// <summary>
        /// An array of all the numeric children.
        /// </summary>
        protected internal override NodeNumericIndex[] NumericChildren
        {
            get { return _numericChildren; }
        }
        protected NodeNumericIndex[] _numericChildren;

        #endregion
    }
}
