/* *********************************************************************
 * The contents of this file are subject to the Mozilla Public License 
 * Version 1.1 (the "License"); you may not use this file except in 
 * compliance with the License. You may obtain a copy of the License at 
 * http://www.mozilla.org/MPL/
 * 
 * Software distributed under the License is distributed on an "AS IS" 
 * basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 * See the License for the specific language governing rights and 
 * limitations under the License.
 *
 * The Original Code is named .NET Mobile API, first released under 
 * this licence on 11th March 2009.
 * 
 * The Initial Developer of the Original Code is owned by 
 * 51 Degrees Mobile Experts Limited. Portions created by 51 Degrees 
 * Mobile Experts Limited are Copyright (C) 2009 - 2011. All Rights Reserved.
 * 
 * Contributor(s):
 *     James Rosewell <james@51degrees.mobi>
 * 
 * ********************************************************************* */

#if VER4
using System.Linq;
#endif

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

        internal static int GetBitsPerPixel(long colors)
        {
#if VER4
            foreach (ColorsToBitsPerPixel current in _colorTable.Where(current => colors <= current.Colors))
            {
                return current.BitsPerPixel;
            }
#elif VER2
            foreach (ColorsToBitsPerPixel current in _colorTable)
            {
                if (colors <= current.Colors)
                    return current.BitsPerPixel;
            }
#endif
            return 8;
        }

        internal static string GetContentType(ImageFormat format)
        {
            if (format != null && _contentTypes.ContainsKey(format))
                return _contentTypes[format];
            return null;
        }

        #endregion
    }
}