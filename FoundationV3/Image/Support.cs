/* *********************************************************************
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited. 
 * Copyright 2014 51Degrees Mobile Experts Limited, 5 Charlotte Close,
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
 * This Source Code Form is "Incompatible With Secondary Licenses", as
 * defined by the Mozilla Public License, v. 2.0.
 * ********************************************************************* */

#region Usings

using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System;
using System.Drawing;
using System.Text;
using System.Reflection;
using FiftyOne.Foundation.Mobile.Configuration;
using System.Security.Cryptography;
using System.Linq;

#endregion

namespace FiftyOne.Foundation.Image
{
    /// <summary>
    /// Class contains static methods to support image management.
    /// </summary>
    internal static class Support
    {
        #region Constants

        /// <summary>
        /// The number of "chunks" the image file path should be broken into.
        /// </summary>
        private const int SPLIT_COUNT = 5;

        #endregion

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

        /// <summary>
        /// Returns the size of the resulting image when scaled up or down.
        /// If one of the dimensions is zero then the image will maintain
        /// it's aspect ratio. If both dimensions are specified then
        /// the size will be the destination width and height and no
        /// scaling will be required.
        /// </summary>
        /// <param name="dstWidth">The width of the destination image.</param>
        /// <param name="dstHeight">The height of the destination image.</param>
        /// <param name="srcWidth">The width of the source image.</param>
        /// <param name="srcHeight">The height of the source image.</param>
        /// <returns>The size of the scaled image.</returns>
        internal static Size GetScaledSize(int dstWidth, int dstHeight, int srcWidth, int srcHeight)
        {
            double widthRatio = (double)dstWidth / (double)srcWidth;
            double heightRatio = (double)dstHeight / (double)srcHeight;

            Size size;

            if (widthRatio > 0 && heightRatio > 0 && widthRatio <= 1 && heightRatio <= 1)
            {
                size = new Size(
                    (int)((double)srcWidth * widthRatio),
                    (int)((double)srcHeight * heightRatio));
            }
            else if (widthRatio > 0 && heightRatio == 0 && widthRatio <= 1)
            {
                size = new Size(
                    (int)((double)srcWidth * widthRatio),
                    (int)((double)srcHeight * widthRatio));
            }
            else if (widthRatio == 0 && heightRatio > 0 && heightRatio <= 1)
            {
                size = new Size(
                    (int)((double)srcWidth * heightRatio),
                    (int)((double)srcHeight * heightRatio));
            }
            else
            {
                // Use the existing size for the image.
                size = new Size(
                    srcWidth,
                    srcHeight);
            }

            return size;
        }

        /// <summary>
        /// Uses the first two bytes of the hash code to form the directory, and then
        /// the bytes of the entire rawurl to form the file name. The extension has to 
        /// stay as the same as the request to ensure the static file handler treats the
        /// image correctly.
        /// </summary>
        /// <param name="request">The requested image</param>
        /// <param name="size">The size of the image being rendered</param>
        /// <returns></returns>
        internal static string GetCachedResponseFile(System.Web.HttpRequest request, Size size)
        {
            // Create a single array of all the relevent bytes.
            var encoded = Convert.ToBase64String(Encoding.ASCII.GetBytes(request.Path));

            return Path.Combine(
                request.ApplicationPath,
                Path.Combine(
                    "App_Data\\Cache",
                    String.Concat(
                        String.Format("{0}", size.Width),
                        "\\",
                        String.Format("{0}", size.Height),
                        "\\",
                        String.Join("\\", SplitArray(encoded).ToArray()),
                        Path.GetExtension(request.CurrentExecutionFilePath))));
        }

        private static IEnumerable<string> SplitArray(string bytes)
        {
            var iteration = 0;
            var startIndex = iteration * SPLIT_COUNT;
            while (startIndex < bytes.Length)
            {
                var length = bytes.Length - startIndex;
                yield return bytes.Substring(startIndex, length > SPLIT_COUNT ? SPLIT_COUNT : length);
                iteration++;
                startIndex = iteration * SPLIT_COUNT;
            }
        }
        
        #endregion
    }
}