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

    /// <summary>
    /// Encapsulates a byte array containing the uncompressed
    /// data structures used by the data set.
    /// </summary>
    internal class SourceMemory : SourceBase
    {
        #region Fields

        /// <summary>
        /// The buffer containing the source data.
        /// </summary>
        private readonly byte[] _buffer;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates the source from the byte array provided.
        /// </summary>
        /// <param name="buffer">Byte array source of the data</param>
        internal SourceMemory(byte[] buffer)
        {
            _buffer = buffer;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a new stream from the data source.
        /// </summary>
        /// <returns>A freshly opened stream to the data source</returns>
        internal override System.IO.Stream CreateStream()
        {
            return new MemoryStream(_buffer);
        }

        #endregion
    }

    internal abstract class SourceFileBase : SourceBase
    {
        #region Fields

        /// <summary>
        /// The file containing the source data.
        /// </summary>
        protected readonly FileInfo _fileInfo;

        #endregion

        #region Constructor

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName">File source of the data</param>
        internal SourceFileBase(string fileName)
        {
            _fileInfo = new FileInfo(fileName);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Delete the file if it's a temporary file and it 
        /// still exists.
        /// </summary>
        protected void DeleteFile()
        {
            if (".tmp".Equals(_fileInfo.Extension) &&
                _fileInfo.Exists)
            {
                try
                {
                    _fileInfo.Delete();
                }
                catch (IOException)
                {
                    // Do nothing as the cause is likely to be because the
                    // file is in use by another process.
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// Encapsulates either a file containing the uncompressed
    /// data structures used by the data set.
    /// </summary>
    internal class SourceFile : SourceFileBase
    {
        #region Constructors

        /// <summary>
        /// Creates the source from the file provided.
        /// </summary>
        /// <param name="fileName">File source of the data</param>
        internal SourceFile(string fileName)
            : base(fileName)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a new stream from the data source.
        /// </summary>
        /// <returns>A freshly opened stream to the data source</returns>
        internal override System.IO.Stream CreateStream()
        {
            return _fileInfo.OpenRead();
        }

        /// <summary>
        /// Closes any file references and then checks
        /// to delete the file.
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            DeleteFile();
        }

        #endregion
    }

    /// <summary>
    /// Encapsulates a file containing an uncompressed data structure.
    /// </summary>
    internal class SourceMemoryMappedFile : SourceFileBase
    {
        #region Fields

        /// <summary>
        /// The memory mapped file to use as the source.
        /// </summary>
        private readonly MemoryMappedFile _file;

        /// <summary>
        /// Controls the security of the memory mapped file.
        /// </summary>
        private readonly MemoryMappedFileSecurity _security;

        /// <summary>
        /// Stream connected to the underlying file.
        /// </summary>
        private readonly FileStream _fileStream;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates the source from the file provided.
        /// </summary>
        /// <param name="fileName">File source of the data</param>
        internal SourceMemoryMappedFile(string fileName) : base(fileName)
        {
            _security = new MemoryMappedFileSecurity();

            // The mapname must not be the same as the file name.
            var mapName = String.Format(
                "{0}-{1}",
                GetType().Name,
                new FileInfo(fileName).Name);

            _fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            _file = MemoryMappedFile.CreateFromFile(
                _fileStream,
                mapName,
                _fileInfo.Length,
                MemoryMappedFileAccess.Read,
                _security,
                HandleInheritability.Inheritable,
                true);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a new stream from the data source.
        /// </summary>
        /// <returns>A freshly opened stream to the data source</returns>
        internal override System.IO.Stream CreateStream()
        {
            return _file.CreateViewStream(0, _fileInfo.Length, MemoryMappedFileAccess.Read);
        }

        /// <summary>
        /// Closes any file references and then checks
        /// to delete the file.
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            _file.Dispose();
            _fileStream.Dispose();
            DeleteFile();
        }
        
        #endregion
    }
}
