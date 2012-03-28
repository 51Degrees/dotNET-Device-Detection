/* *********************************************************************
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0.
 * 
 * If a copy of the MPL was not distributed with this file, You can obtain
 * one at http://mozilla.org/MPL/2.0/.
 * 
 * This Source Code Form is “Incompatible With Secondary Licenses”, as
 * defined by the Mozilla Public License, v. 2.0.
 * ********************************************************************* */

#region Usings

using System.Collections.Generic;
using System.Drawing.Imaging;

#endregion

namespace FiftyOne.Foundation.Image
{
    /// <summary>
    /// Class contains static methods to support image management.
    /// </summary>
    internal static class Support
    {
        #region Private Classes

        /// <summary>
        /// Used to convert colors to bits per pixel.
        /// </summary>
        private class ColorsToBitsPerPixel
        {
            private readonly int _bitsPerPixel;
            private readonly long _colors;

            protected internal ColorsToBitsPerPixel(int bitsPerPixel, long colors)
            {
                _bitsPerPixel = bitsPerPixel;
                _colors = colors;
            }

            protected internal int BitsPerPixel
            {
                get { return _bitsPerPixel; }
            }

            protected internal long Colors
            {
                get { return _colors; }
            }
        }

        #endregion

        #region Fields

        private static List<ColorsToBitsPerPixel> _colorTable;
        private static Dictionary<ImageFormat, string> _contentTypes;

        #endregion

        #region Static Constructor & Support Methods

        static Support()
        {
            InitColorTable();
            InitContentTypes();
        }

        /// <summary>
        /// Initialises the colours lookup table.
        /// </summary>
        private static void InitColorTable()
        {
            _colorTable = new List<ColorsToBitsPerPixel>();
            AddPair(_colorTable, 1, 2);
            AddPair(_colorTable, 2, 4);
            AddPair(_colorTable, 3, 8);
            AddPair(_colorTable, 4, 16);
            AddPair(_colorTable, 5, 32);
            AddPair(_colorTable, 6, 64);
            AddPair(_colorTable, 7, 128);
            AddPair(_colorTable, 8, 256);
            AddPair(_colorTable, 9, 512);
            AddPair(_colorTable, 10, 1024);
            AddPair(_colorTable, 11, 2048);
            AddPair(_colorTable, 12, 4096);
            AddPair(_colorTable, 13, 8192);
            AddPair(_colorTable, 14, 16384);
            AddPair(_colorTable, 15, 32768);
            AddPair(_colorTable, 16, 65536);
            AddPair(_colorTable, 17, 131072);
            AddPair(_colorTable, 18, 262144);
            AddPair(_colorTable, 19, 524288);
            AddPair(_colorTable, 20, 1048576);
            AddPair(_colorTable, 21, 2097152);
            AddPair(_colorTable, 22, 4194304);
            AddPair(_colorTable, 23, 8388608);
            AddPair(_colorTable, 24, 16777216);
            AddPair(_colorTable, 25, 33554432);
            AddPair(_colorTable, 26, 67108864);
            AddPair(_colorTable, 27, 134217728);
            AddPair(_colorTable, 28, 268435456);
            AddPair(_colorTable, 29, 536870912);
            AddPair(_colorTable, 30, 1073741824);
            AddPair(_colorTable, 31, 2147483648);
            AddPair(_colorTable, 32, 4294967296);
        }

        /// <summary>
        /// Populates the lookup table with context types.
        /// </summary>
        internal static void InitContentTypes()
        {
            _contentTypes = new Dictionary<ImageFormat, string>();
            _contentTypes.Add(ImageFormat.Png, "image/png");
            _contentTypes.Add(ImageFormat.Gif, "image/gif");
            _contentTypes.Add(ImageFormat.Jpeg, "image/jpeg");
            _contentTypes.Add(ImageFormat.Bmp, "image/bmp");
            _contentTypes.Add(ImageFormat.Tiff, "image/tiff");
        }

        private static void AddPair(List<ColorsToBitsPerPixel> colorTable, int bitsPerPixel, long colors)
        {
            colorTable.Add(new ColorsToBitsPerPixel(bitsPerPixel, colors));
        }

        #endregion

        #region Static Methods

        internal static string GetContentType(ImageFormat format)
        {
            if (format != null && _contentTypes.ContainsKey(format))
                return _contentTypes[format];
            return null;
        }

        #endregion
    }
}