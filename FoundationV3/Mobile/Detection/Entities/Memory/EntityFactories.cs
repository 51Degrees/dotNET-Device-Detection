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
using FiftyOne.Foundation.Mobile.Detection.Readers;

namespace FiftyOne.Foundation.Mobile.Detection.Entities.Memory
{
    internal class MemoryAsciiStringFactory : BaseAsciiStringFactory<DataSet>
    {
        /// <summary>
        /// Creates a new instance of <see cref="AsciiString"/>
        /// </summary>
        /// <param name="dataSet">
        /// The data set whose strings list the string is contained within
        /// </param>
        /// <param name="offset">
        /// The offset to the start of the string within the string data
        /// structure
        /// </param>
        /// <param name="reader">
        /// Binary reader positioned at the start of the AsciiString
        /// </param>
        /// <returns>A new instance of an <see cref="AsciiString"/></returns>
        public override AsciiString Create(DataSet dataSet, int offset, Reader reader)
        {
            return new AsciiString(dataSet, offset, reader);
        }
    }

    /// <summary>
    /// Factory class used to create the new instances of Node V3.1 object.
    /// Difference is in the length of the Node entity.
    /// </summary>
    internal class NodeMemoryFactoryV31 : NodeFactoryV31<DataSet>
    {
        protected override Entities.Node Construct(DataSet dataSet, int offset, Reader reader)
        {
            return new NodeV31(dataSet, offset, reader);
        }
    }

    /// <summary>
    /// Factory class used to create the new instances of Node V3.2 object.
    /// Difference is in the length of the Node entity.
    /// </summary>
    internal class NodeMemoryFactoryV32 : NodeFactoryV32<DataSet>
    {
        protected override Entities.Node Construct(DataSet dataSet, int offset, Reader reader)
        {
            return new NodeV32(dataSet, offset, reader);
        }
    }

    /// <summary>
    /// Creates Profile entities for use with memory data set.
    /// </summary>
    internal class ProfileMemoryFactory : ProfileFactory<DataSet>
    {
        protected override Entities.Profile Construct(DataSet dataSet, int offset, Reader reader)
        {
            return new Entities.Memory.Profile(dataSet, offset, reader);
        }
    }
}
