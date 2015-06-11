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

using System.Collections.Generic;
using System.IO;
using System;
using FiftyOne.Foundation.Mobile.Detection.Readers;

namespace FiftyOne.Foundation.Mobile.Detection.Entities.Stream
{
    /// <summary>
    /// As multiple threads need to read from the <see cref="SourceBase"/> concurrently
    /// this class provides a mechanisim for readers to be recycled across threads
    /// and requests.
    /// </summary>
    /// <para>
    /// Used by the <see cref="BaseList{T}"/> to provide multiple readers for the
    /// list.
    /// </para>
    /// <remarks>
    /// The <see cref="DataSet"/> must be disposed of to ensure the readers
    /// in the pool are closed.
    /// </remarks>
    internal class Pool : IDisposable
    {
        #region Fields

        /// <summary>
        /// List of readers available to be used.
        /// </summary>
        private readonly Queue<Reader> _readers = new Queue<Reader>();

        /// <summary>
        /// A pool of file readers to use to read data from the file.
        /// </summary>
        private readonly SourceBase Source;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a new pool of readers for <see cref="SourceBase"/> provided.
        /// </summary>
        /// <param name="source">The data source for the list</param>
        internal Pool(SourceBase source)
        {
            Source = source;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns a reader to the temp file for exclusive use.
        /// Release method must be called to return the reader to 
        /// the pool when finished.
        /// </summary>
        /// <returns>Reader open and ready to read from the temp file</returns>
        internal Reader GetReader()
        {
            lock(_readers)
            {
                if (_readers.Count > 0)
                    return _readers.Dequeue();
            }
            return Source.CreateReader();
        }

        /// <summary>
        /// Returns the reader to the pool to be used by another
        /// process later.
        /// </summary>
        /// <param name="reader">Reader open and ready to read from the temp file</param>
        internal void Release(Reader reader)
        {
            lock (_readers)
            {
                _readers.Enqueue(reader);
            }
        }

        /// <summary>
        /// Disposes of the source ensuring all the readers
        /// are also closed.
        /// </summary>
        public void Dispose()
        {
            Source.Dispose();
        }

        #endregion
    }
}
