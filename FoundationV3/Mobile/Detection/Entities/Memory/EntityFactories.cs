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

using FiftyOne.Foundation.Mobile.Detection.Factories;
using FiftyOne.Foundation.Mobile.Detection.Readers;

namespace FiftyOne.Foundation.Mobile.Detection.Entities.Memory
{
    internal class NodeMemoryFactoryV31 : NodeFactory
    {
        protected override Entities.Node Construct(DataSet dataSet, int offset, Reader reader)
        {
            return new NodeV31(dataSet, offset, reader);
        }

        /// <summary>
        /// Returns the length of the <see cref="NodeV31"/> entity provided.
        /// </summary>
        /// <param name="entity">An entity of type Node who length is required</param>
        /// <returns>The number of bytes used to store the node</returns>
        internal override int GetLength(Entities.Node entity)
        {
            return BaseLength + 
                sizeof(int) + // Length of the ranked signatures count number
                (entity.Children.Length * NodeFactoryShared.NodeIndexLengthV31) +
                (entity.NumericChildren.Length * NodeNumericIndexLength) +
                (entity.RankedSignatureCount * sizeof(int));
        }
    }

    internal class NodeMemoryFactoryV32 : NodeFactory
    {
        protected override Entities.Node Construct(DataSet dataSet, int offset, Reader reader)
        {
            return new NodeV32(dataSet, offset, reader);
        }

        /// <summary>
        /// Returns the length of the <see cref="NodeV32"/> entity provided.
        /// </summary>
        /// <param name="entity">An entity of type Node who length is required</param>
        /// <returns>The number of bytes used to store the node</returns>
        internal override int GetLength(Entities.Node entity)
        {
            return BaseLength +
                sizeof(ushort) + // Length of the ranked signatures count number
                (entity.Children.Length * NodeFactoryShared.NodeIndexLengthV32) +
                (entity.NumericChildren.Length * NodeNumericIndexLength) +
                // If the ranked signature count is zero then nothing follows. If it's
                // great than 0 then the next 4 bytes are the index of the first signature.
                (entity.RankedSignatureCount == 0 ? 0 : sizeof(int));
        }
    }

    internal class ProfileMemoryFactory : ProfileFactory
    {
        protected override Entities.Profile Construct(DataSet dataSet, int offset, Reader reader)
        {
            return new Entities.Memory.Profile(dataSet, offset, reader);
        }
    }
}
