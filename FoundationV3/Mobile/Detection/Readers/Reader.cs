using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace FiftyOne.Foundation.Mobile.Detection.Readers
{
    /// <summary>
    /// Used to provide extra features to the standard binary reader
    /// to reduce the number of objects created for garbage collection 
    /// to handle.
    /// </summary>
    internal class Reader : System.IO.BinaryReader
    {
        /// <summary>
        /// A list of integers used to create arrays when the number of elements
        /// are unknown prior to commencing reading.
        /// </summary>
        internal readonly List<int> List = new List<int>();

        /// <summary>
        /// Constructs a new instance of reader from the stream.
        /// </summary>
        /// <param name="stream"></param>
        internal Reader(Stream stream) : base(stream) { }
    }
}
