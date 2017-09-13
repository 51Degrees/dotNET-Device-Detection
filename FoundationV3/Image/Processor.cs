/* *********************************************************************
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited. 
 * Copyright © 2017 51Degrees Mobile Experts Limited, 5 Charlotte Close,
 * Caversham, Reading, Berkshire, United Kingdom RG4 7BY
 * 
 * This Source Code Form is the subject of the following patents and patent 
 * applications, owned by 51Degrees Mobile Experts Limited of 5 Charlotte
 * Close, Caversham, Reading, Berkshire, United Kingdom RG4 7BY: 
 * European Patent No. 2871816;
 * European Patent Application No. 17184134.9;
 * United States Patent Nos. 9,332,086 and 9,350,823; and
 * United States Patent Application No. 15/686,066.
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

using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace FiftyOne.Foundation.Image
{
    internal class Processor
    {
        #region Fields

        private static readonly long _pngQuality = 80;
        private static readonly long _jpegQuality = 80;

        private Size _size = Size.Empty;
        private ImageFormat _imageFormat = ImageFormat.Png;
        private readonly int _screenBitDepth;

        #endregion

        #region Properties

        internal int ScreenBitDepth
        {
            get { return _screenBitDepth; }
        }

        internal ImageFormat Format
        {
            get { return _imageFormat; }
            set { _imageFormat = value; }
        }

        internal int Height
        {
            get { return _size.Height; }
            set { _size.Height = value; }
        }

        internal int Width
        {
            get { return _size.Width; }
            set { _size.Width = value; }
        }

        internal Size Size
        {
            get { return _size; }
            set { _size = value; }
        }

        #endregion

        #region Constructor

        internal Processor(int screenBitDepth)
        {
            _screenBitDepth = screenBitDepth;
        }

        #endregion

        #region Crop Methods

        internal void Crop(Stream inStream, Stream outStream)
        {
            using (System.Drawing.Image image = System.Drawing.Image.FromStream(inStream))
            {
                Crop(image, outStream);
            }
        }

        private void Crop(System.Drawing.Image image, Stream outStream)
        {
            using(MemoryStream ms = new MemoryStream())
            {
                if (CalculateNewSize(image) == true)
                {
                    Rectangle rect = new Rectangle(
                        (image.Width - Width) / 2,
                        (image.Height - Height) / 2,
                        Width, Height);
                    Bitmap cropped = null;
                    using (cropped = new Bitmap(Width, Height, GetPixelFormat(image)))
                    {
                        Crop(image, cropped, rect);
                        // Change the transparent color to match the source if
                        // one has been defined.
                        Color transparent = Color.White;
                        if (GetTransparentColor(image, out transparent) == true)
                            cropped.MakeTransparent(transparent);
                        Save(cropped, ms, image);
                    }
                }
                else
                    Save(image, ms, image);
                ms.WriteTo(outStream);
            }
        }

        #endregion

        #region Shrink Methods

        internal void Shrink(Stream inStream, Stream outStream)
        {
            using (System.Drawing.Image image = System.Drawing.Image.FromStream(inStream, true, true))
            {
                Shrink(image, outStream);
            }
        }

        private bool GetTransparentColor(System.Drawing.Image image, out Color color)
        {
            color = Color.Empty;
            // Check we're dealing with an alpha pallete.
            if (image.Palette.Flags == 1)
            {
                for (int i = 0; i < image.Palette.Entries.Length; i++)
                {
                    if (image.Palette.Entries[i].A == 0)
                    {
                        color = image.Palette.Entries[i];
                        return true;
                    }
                }
            }
            return false;
        }

        internal void Shrink(System.Drawing.Image image, Stream outStream)
        {
            if (outStream.CanSeek)
            {
                ShrinkToSeekStream(image, outStream);
            }
            else
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    ShrinkToSeekStream(image, ms);
                    outStream.Write(ms.ToArray(), 0, (int)ms.Length);
                }
            }
        }

        private void ShrinkToSeekStream(System.Drawing.Image image, Stream seekStream)
        {
            CalculateNewSize(image);
            using (Bitmap shrunk = new Bitmap(Width, Height, GetPixelFormat(image)))
            {
                Shrink(image, shrunk);
                Save(shrunk, seekStream, image);
            }
        }

        #endregion

        #region Save Methods

        private static int GetImageBitDepth(System.Drawing.Image image)
        {
            if (image.PixelFormat == PixelFormat.Format16bppArgb1555) return 16;
            else if (image.PixelFormat == PixelFormat.Format16bppGrayScale) return 16;
            else if (image.PixelFormat == PixelFormat.Format16bppRgb555) return 16;
            else if (image.PixelFormat == PixelFormat.Format16bppRgb565) return 16;
            else if (image.PixelFormat == PixelFormat.Format24bppRgb) return 24;
            else if (image.PixelFormat == PixelFormat.Format32bppArgb) return 32;
            else if (image.PixelFormat == PixelFormat.Format32bppPArgb) return 32;
            else if (image.PixelFormat == PixelFormat.Format32bppRgb) return 32;
            else if (image.PixelFormat == PixelFormat.Format48bppRgb) return 48;
            else if (image.PixelFormat == PixelFormat.Format64bppArgb) return 64;
            else if (image.PixelFormat == PixelFormat.Format64bppPArgb) return 64;
            else if (image.PixelFormat == PixelFormat.Format24bppRgb) return 24;
            return 8;
        }

        private PixelFormat GetPixelFormat(System.Drawing.Image image)
        {
            if (_screenBitDepth > GetImageBitDepth(image))
                return image.PixelFormat;
            if (_screenBitDepth <= 32)
                return PixelFormat.Format32bppArgb;
            return PixelFormat.Format64bppArgb;
        }

        private void SavePng(Stream outStream, System.Drawing.Image image)
        {
            ImageCodecInfo codec = GetCodec("image/png");
            if (codec != null)
            {
                EncoderParameters encoderParametersInstance = new EncoderParameters(2);
                encoderParametersInstance.Param[0] = new EncoderParameter(Encoder.Quality, _pngQuality);
                encoderParametersInstance.Param[1] = new EncoderParameter(Encoder.ColorDepth, (long)_screenBitDepth);
                image.Save(outStream, codec, encoderParametersInstance);
            }
        }

        private void SaveJpg(Stream outStream, System.Drawing.Image image)
        {
            ImageCodecInfo codec = GetCodec("image/jpeg");
            if (codec != null)
            {
                EncoderParameters encoderParametersInstance = new EncoderParameters(2);
                encoderParametersInstance.Param[0] = new EncoderParameter(Encoder.Quality, _jpegQuality);
                encoderParametersInstance.Param[1] = new EncoderParameter(Encoder.ColorDepth, (long)_screenBitDepth);
                image.Save(outStream, codec, encoderParametersInstance);
            }
        }
        
        private static ImageCodecInfo GetCodec(string mime)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            ImageCodecInfo codec = null;
            for (int i = 0; i < codecs.Length; i++)
            {
                if (codecs[i].MimeType.Equals(mime))
                {
                    codec = codecs[i];
                }
            }
            return codec;
        }

        private void Save(System.Drawing.Image image, Stream outStream, System.Drawing.Image sourceImage)
        {
            if (Format.Guid == ImageFormat.Png.Guid)
                SavePng(outStream, image);
            else if (Format.Guid == ImageFormat.Jpeg.Guid)
                SaveJpg(outStream, image);
        }

        #endregion

        #region Resize Methods

        private void Shrink(System.Drawing.Image src, Bitmap dest)
        {
            using (Graphics g = Graphics.FromImage(dest))
            {
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.CompositingQuality = CompositingQuality.GammaCorrected;

                Rectangle rect = new Rectangle(0, 0, Width, Height);
                // If the source image has a color palette using indexed colors.
                if (src.Palette.Flags == 1)
                {
                    g.FillRectangle(Brushes.Transparent, rect);
                    // Change any transparent colors to .NET's default
                    // transparent color for images.
                    ColorPalette newPalette = src.Palette;
                    for (int i = 0; i < newPalette.Entries.Length; i++)
                        if (newPalette.Entries[i].A == 0)
                            newPalette.Entries[i] = Color.FromArgb(0, 0, 0, 0);
                    src.Palette = newPalette;
                    g.CompositingMode = CompositingMode.SourceCopy;
                    g.CompositingQuality = CompositingQuality.Default;
                    g.DrawImage(src, rect, 0, 0, src.Width, src.Height, GraphicsUnit.Pixel);
                }
                else
                {
                    // No need to check the palette. Shrink the image.
                    g.CompositingMode = CompositingMode.SourceCopy;
                    g.DrawImage(src, rect, 0, 0, src.Width, src.Height, GraphicsUnit.Pixel);
                }
            }
        }

        private static void Crop(System.Drawing.Image src, Bitmap dest, Rectangle rect)
        {
            using(Graphics g = Graphics.FromImage(dest))
            {
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.CompositingMode = CompositingMode.SourceCopy;
                g.CompositingQuality = CompositingQuality.HighSpeed;
                g.DrawImage(src, 0, 0, rect, GraphicsUnit.Pixel);
            }
        }

        /// <summary>
        /// Sets the dimensions of the new image.
        /// </summary>
        /// <param name="img">The source image.</param>
        /// <returns>False if the img does not need to be scaled.</returns>
        private bool CalculateNewSize(System.Drawing.Image img)
        {
            // Get the new size of the image.
            Size newSize = Support.GetScaledSize(Width, Height, img.Width, img.Height);

            // Get the new image size.
            Width = newSize.Width;
            Height = newSize.Height;

            // Return true if the image size needs to change.
            if (newSize.Width == img.Width && newSize.Height == img.Height)
                return false;
            return true;
        }

        #endregion
    }
}