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

using System.Collections.Generic;
using System.IO;
using System;
using FiftyOne.Foundation.Mobile.Detection.Readers;
using System.Threading;

namespace FiftyOne.Foundation.Mobile.Detection.Entities.Stream
{
    /// <summary>
    /// As multiple threads need to read from the <see cref="SourceBase"/> 
    /// concurrently this class provides a mechanisim for readers to be 
    /// recycled across threads and requests.
    /// </summary>
    /// <remarks>
    /// The <see cref="DataSet"/> must be disposed of to ensure the readers
    /// in the pool are closed.
    /// </remarks>
    /// <remarks>Not intended to be used directly by 3rd parties.</remarks>
    public class Pool
    {
        #region Fields
        
        /// <summary>
        /// List of readers available for use.
        /// </summary>
        private readonly Queue<Reader> _readers = new Queue<Reader>();

        /// <summary>
        /// A pool of file readers to use to read data from the file.
        /// </summary>
        private readonly SourceBase Source;

        #endregion

        #region Properties

        /// <summary>
        /// The number of readers that have been created. May not be the
        /// same as the readers in the queue as some may be in use.
        /// </summary>
        internal int ReadersCreated 
        {
            get { return _readerCount; }
        }
        private int _readerCount = 0;

        /// <summary>
        /// The number of readers in the queue.
        /// </summary>
        internal int ReadersQueued
        {
            get { return _readers.Count; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a new pool of readers for <see cref="SourceBase"/> 
        /// provided.
        /// </summary>
        /// <param name="source">
        /// The data source for the list.
        /// </param>
        public Pool(SourceBase source)
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
        /// <returns>
        /// Reader open and ready to read from the temp file.
        /// </returns>
        public Reader GetReader()
        {
            lock(_readers)
            {
                if (_readers.Count > 0)
                {
                    return _readers.Dequeue();
                }
            }
            Interlocked.Increment(ref _readerCount);
            return Source.CreateReader();
        }

        /// <summary>
        /// Returns the reader to the pool to be used by another
        /// process later.
        /// </summary>
        /// <param name="reader">
        /// Reader open and ready to read from the temp file.
        /// </param>
        public void Release(Reader reader)
        {
            lock (_readers)
            {
                _readers.Enqueue(reader);
            }
        }
        
        #endregion
    }
}
