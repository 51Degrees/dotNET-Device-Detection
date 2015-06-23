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

using System;
using FiftyOne.Foundation.Mobile.Detection.Readers;

namespace FiftyOne.Foundation.Mobile.Detection.Entities.Stream
{
    /// <summary>
    /// Providers the base for a data source containing the uncompressed
    /// data structures used by the data set.
    /// </summary>
    /// <remarks>
    /// Must be disposed to ensure that the readers are closed and any resources
    /// free for other uses.
    /// </remarks>
    internal abstract class SourceBase : IDisposable
    {
        #region Fields

        /// <summary>
        /// List of binary readers opened against the data source. 
        /// </summary>
        private readonly System.Collections.Generic.List<Reader> _readers = 
            new System.Collections.Generic.List<Reader>();

        #endregion

        #region Abstract Members

        /// <summary>
        /// Creates a new stream from the data source.
        /// </summary>
        /// <returns>A freshly opened stream to the data source</returns>
        internal abstract System.IO.Stream CreateStream();

        #endregion

        #region Methods

        /// <summary>
        /// Creates a new reader and stores a reference to it.
        /// </summary>
        /// <returns>A reader open for read access to the stream</returns>
        internal Reader CreateReader()
        {
            var reader = new Reader(CreateStream());
            lock (_readers)
            {
                _readers.Add(reader);
            }
            return reader;
        }

        /// <summary>
        /// Releases the reference to memory and forces garbage collection.
        /// </summary>
        public virtual void Dispose()
        {
            lock (_readers)
            {
                foreach (var reader in _readers)
                {
                    reader.Dispose();
                }
                _readers.Clear();
            }
        }

        #endregion
    }
}
