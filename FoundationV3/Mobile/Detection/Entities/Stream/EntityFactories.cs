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
using FiftyOne.Foundation.Mobile.Detection.Readers;

namespace FiftyOne.Foundation.Mobile.Detection.Entities.Stream
{
    internal class StreamAsciiStringFactory : BaseAsciiStringFactory<IStreamDataSet>
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
        public override AsciiString Create(IStreamDataSet dataSet, int offset, Reader reader)
        {
            return new AsciiString(dataSet, offset, reader);
        }
    }
    
    /// <summary>
    /// Factory used to create stream <see cref="Entities.Node"/> entities.
    /// </summary>
    internal class NodeStreamFactoryV31 : NodeFactoryV31<IStreamDataSet>
    {
        /// <summary>
        /// Pool for the corresponding data set used to get readers.
        /// </summary>
        protected readonly Pool _pool;

        /// <summary>
        /// Constructs a new instance of <see cref="NodeStreamFactoryV31"/>.
        /// </summary>
        /// <param name="pool">
        /// Pool from the data set to be used when creating 
        /// new <see cref="Entities.Node"/> entities.
        /// </param>
        internal NodeStreamFactoryV31(Pool pool) 
        {
            _pool = pool;
        }

        /// <summary>
        /// Constructs a new <see cref="Entities.Stream.NodeV31"/> entity from 
        /// the offset provided.
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
        /// <returns>
        /// A new <see cref="Entities.Node"/> entity from the data set.
        /// </returns>
        protected override Entities.Node Construct(IStreamDataSet dataSet, int offset, Reader reader)
        {
            return new Entities.Stream.NodeV31(dataSet, offset, reader);
        }
    }

    /// <summary>
    /// Factory used to create stream <see cref="Entities.Node"/> entities.
    /// </summary>
    internal class NodeStreamFactoryV32 : NodeFactoryV32<IStreamDataSet>
    {
        /// <summary>
        /// Pool for the corresponding data set used to get readers.
        /// </summary>
        protected readonly Pool _pool;
        
        /// <summary>
        /// Constructs a new instance of <see cref="NodeStreamFactoryV32"/>.
        /// </summary>
        /// <param name="pool">
        /// Pool from the data set to be used when creating new 
        /// <see cref="Entities.Node"/> entities
        /// </param>
        internal NodeStreamFactoryV32(Pool pool)
        {
            _pool = pool;
        }

        /// <summary>
        /// Constructs a new <see cref="Entities.Stream.NodeV32"/> entity from 
        /// the offset provided.
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
        /// <returns>
        /// A new <see cref="Entities.Node"/> entity from the data set.
        /// </returns>
        protected override Entities.Node Construct(IStreamDataSet dataSet, int offset, Reader reader)
        {
            return new Entities.Stream.NodeV32(dataSet, offset, reader);
        }
    }

    /// <summary>
    /// Factory used to create stream <see cref="Entities.Profile"/> entities.
    /// </summary>
    internal class ProfileStreamFactory : ProfileFactory<IStreamDataSet>
    {
        /// <summary>
        /// Pool for the corresponding data set used to get readers.
        /// </summary>
        private readonly Pool _pool;

        /// <summary>
        /// Constructs a new instance of <see cref="ProfileStreamFactory"/>.
        /// </summary>
        /// <param name="pool">
        /// Pool from the data set to be used when creating 
        /// new <see cref="Entities.Profile"/> entities.
        /// </param>
        internal ProfileStreamFactory(Pool pool)
        {
            _pool = pool;
        }

        /// <summary>
        /// Constructs a new <see cref="Entities.Profile"/> entity from the 
        /// offset provided.
        /// </summary>
        /// <param name="dataSet">
        /// The data set the profile is contained within.
        /// </param>
        /// <param name="offset">
        /// The offset in the data structure to the profile.
        /// </param>
        /// <param name="reader">
        /// Reader connected to the source data structure and positioned to 
        /// start reading.
        /// </param>
        /// <returns>
        /// A new <see cref="Entities.Profile"/> entity from the data set.
        /// </returns>
        protected override Entities.Profile Construct(IStreamDataSet dataSet, int offset, Reader reader)
        {
            return new Entities.Stream.Profile(dataSet, offset, reader);
        }
    }
}
