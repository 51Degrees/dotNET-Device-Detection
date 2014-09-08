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

using System.IO;
using System;
using FiftyOne.Foundation.Mobile.Detection.Readers;

namespace FiftyOne.Foundation.Mobile.Detection.Entities.Stream
{
    /// <summary>
    /// Encapsulates either a byte array or a file containing the uncompressed
    /// data structures used by the data set.
    /// </summary>
    /// <remarks>
    /// Must be disposed to ensure that the readers are closed and the file
    /// free for other uses. Does not need to be disposed if a byte array is
    /// used.
    /// </remarks>
    internal class Source : IDisposable
    {
        #region Fields

        /// <summary>
        /// The file containing the source data.
        /// </summary>
        private readonly string _fileName;

        /// <summary>
        /// The buffer containing the source data.
        /// </summary>
        private readonly byte[] _buffer;

        // List of binary readers opened against the file.
        private readonly System.Collections.Generic.List<Reader> _readers = 
            new System.Collections.Generic.List<Reader>();

        #endregion

        #region Constructors

        /// <summary>
        /// Creates the source from the file provided.
        /// </summary>
        /// <param name="fileName">File source of the data</param>
        internal Source(string fileName)
        {
            _fileName = fileName;
        }

        /// <summary>
        /// Creates the source from the byte array provided.
        /// </summary>
        /// <param name="buffer">Byte array source of the data</param>
        internal Source(byte[] buffer)
        {
            _buffer = buffer;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a new stream from the data source.
        /// </summary>
        /// <returns>A freshly opened stream to the data source</returns>
        private System.IO.Stream CreateStream()
        {
            if (_buffer != null)
                return new MemoryStream(_buffer);
            return File.OpenRead(_fileName);
        }

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
        /// Closes any file references.
        /// </summary>
        public void Dispose()
        {
            if (_readers != null)
            {
                foreach (var reader in _readers)
                    reader.Close();
            }

            // If the file is a temporary one ensure it's deleted.
            if (_fileName.EndsWith("tmp"))
            {
                try
                {
                    File.Delete(_fileName);
                }
                catch (IOException ex)
                {
                    EventLog.Info(String.Format(
                        "Exception '{0}' deleting temporary file '{1}'.",
                        ex.Message,
                        _fileName));
                }
            }

        }

        #endregion
    }
}
