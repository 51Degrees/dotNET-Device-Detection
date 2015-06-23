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
using System.IO.MemoryMappedFiles;

namespace FiftyOne.Foundation.Mobile.Detection.Entities.Stream
{
    /// <summary>
    /// Encapsulates a file containing an uncompressed data structure.
    /// </summary>
    internal class SourceMemoryMappedFile : SourceFileBase
    {
        #region Fields

        /// <summary>
        /// The memory mapped file to use as the source.
        /// </summary>
        private readonly MemoryMappedFile _mapped;

        /// <summary>
        /// Used to ensure that only one memory mapped source can be 
        /// created at a time.
        /// </summary>
        private static readonly object _createLock = new object();

        #endregion

        #region Constructors

        /// <summary>
        /// Creates the source from the file provided.
        /// </summary>
        /// <param name="fileName">File source of the data</param>
        internal SourceMemoryMappedFile(string fileName) : base(fileName)
        {
            // The mapname must not be the same as the file name.
            var mapName = String.Format(
                "{0}-{1}",
                GetType().Name,
                _fileInfo.Name);

            // Ensure only one memory mapped file source is created at a time
            // to ensure that any checks for an existing file can not occur at
            // the same time.
            lock (_createLock)
            {
                try
                {
                    // Try opening an existing memory mapped file incase one is
                    // already available. This will reduce the number of open
                    // memory mapped files.
                    _mapped = MemoryMappedFile.OpenExisting(mapName, MemoryMappedFileRights.Read, HandleInheritability.Inheritable);
                }
                catch (Exception)
                {
                    // An existing memory mapped file could not be used. Use a new 
                    // one connected to the same underlying file.
                    _mapped = MemoryMappedFile.CreateFromFile(
                        _fileInfo.FullName,
                        FileMode.Open,
                        mapName,
                        _fileInfo.Length,
                        MemoryMappedFileAccess.Read);
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a new stream from the data source.
        /// </summary>
        /// <returns>A freshly opened stream to the data source</returns>
        internal override System.IO.Stream CreateStream()
        {
            return _mapped.CreateViewStream(0, _fileInfo.Length, MemoryMappedFileAccess.Read);
        }

        /// <summary>
        /// Closes any file references and then checks
        /// to delete the file.
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            _mapped.Dispose();
            DeleteFile();
        }
        
        #endregion
    }
}
