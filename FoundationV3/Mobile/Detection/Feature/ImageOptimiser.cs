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
 * This Source Code Form is “Incompatible With Secondary Licenses”, as
 * defined by the Mozilla Public License, v. 2.0.
 * ********************************************************************* */

using System.Linq;
using System.IO;
using System.Reflection;
using System.Web;
using System;
using System.Drawing;
using FiftyOne.Foundation.Mobile.Configuration;
using FiftyOne.Foundation.Mobile.Detection.Entities;

namespace FiftyOne.Foundation.Mobile.Detection.Feature
{
    internal class ImageOptimiser
    {
        /// <summary>
        /// Set to true if eTags aren't supported due to the version of IIS.
        /// </summary>
        private static bool _eTagNotSupported = false;

        /// <summary>
        /// The auto string, a flag which means client code should have found the optimised sizes.
        /// The server cannot know what are the optimal sizes, so if it is seen here the defaultAuto
        /// size is used instead.
        /// </summary>
        private const string AUTO_STRING = "auto";

        /// <summary>
        /// Set a property in the application state to quickly determine if image optimisation
        /// is supported by the data set.
        /// </summary>
        /// <param name="application"></param>
        internal static void Init(HttpApplicationState application)
        {
            application["51D_ImageOptimiser"] = new bool?(Manager.ImageOptimisation.Enabled &&
                WebProvider.ActiveProvider != null);
            EventLog.Debug(String.Format("Image Optimisation '{0}'", application["51D_ImageOptimiser"]));
        }

        /// <summary>
        /// Returns info concerning the empty image gif.
        /// </summary>
        /// <returns></returns>
        private static byte[] GetEmptyImage()
        {
            byte[] image = null;
            using (var src = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream(
                Constants.EmptyImageResourceName))
            {
                image = new byte[src.Length];
                src.Read(image, 0, image.Length);
            }
            return image;
        }
        internal static readonly byte[] EmptyImage = GetEmptyImage();

        /// <summary>
        /// Returns the javascript for bandwidth monitoring for the
        /// requesting device.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        internal static string GetJavascript(HttpContext context)
        {
            var javascript = GetJavascriptValues(context.Request);
            return javascript != null && javascript.Count == 1 ?
                javascript[0].ToString() : null;
        }

        private static string WidthParameter
        {
            get
            {
                return Manager.ImageOptimisation.WidthParam;
            }
        }

        private static string HeightParameter
        {
            get
            {
                return Manager.ImageOptimisation.HeightParam;
            }
        }

        /// <summary>
        /// Returns the javascript for the feature.
        /// </summary>
        /// <param name="request">Request the javascript is needed for</param>
        /// <returns>Javascript to support the feature if present</returns>
        private static Values GetJavascriptValues(HttpRequest request)
        {
            Values values = null;
            var match = WebProvider.GetMatch(request);
            if (match != null)
            {
                var javascript = match["JavascriptImageOptimiser"];
                if (javascript != null && javascript.Count > 0)
                {
                    values = javascript;
                }
            }
            return values;
        }

        /// <summary>
        /// Determines if the feature is enabled based on the information
        /// written to the application when initialised.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        internal static bool GetIsEnabled(HttpContext context)
        {
            var enabled = context.Application["51D_ImageOptimiser"] as bool?;
            return enabled.HasValue && enabled.Value;
        }

        /// <summary>
        /// Determines if the feature is enabled based on the information
        /// written to the application when initialised and the presence of 
        /// image optimiser java script in the detected device results.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        internal static bool GetIsJavaScriptEnabled(HttpContext context)
        {
            var enabled = context.Application["51D_ImageOptimiser"] as bool?;
            if (enabled.HasValue && enabled.Value)
            {
                // If the image optimiser javascript is present for this device then it's enabled.
                return GetJavascriptValues(context.Request) != null;
            }
            return false;
        }

        /// <summary>
        /// Register a script to be run once the page has finished being loaded.
        /// </summary>
        /// <param name="page">Web page which supports client script registration</param>
        internal static bool AddScript(System.Web.UI.Page page)
        {
            if (GetIsJavaScriptEnabled(HttpContext.Current))
            {
                page.ClientScript.RegisterStartupScript(
                    typeof(ImageOptimiser),
                    "FODIO",
                    string.Format("new FODIO('{0}', '{1}');", WidthParameter, HeightParameter),
                    true);
                return true;
            }
            return false;
        }

        internal static void EmptyImageResponse(HttpContext context)
        {
            var hash = Convert.ToBase64String(Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId.ToByteArray());
            if (hash == context.Request.Headers["If-None-Match"])
            {
                EventLog.Debug("Image processor responding with empty image 304 code");

                // The response hasn't changed so respond with a 304.
                context.Response.StatusCode = 304;
            }
            else
            {
                EventLog.Debug("Image processor responding with empty image");

                // No eTag so respond with the image.
                context.Response.ContentType = "image/gif";
                context.Response.StatusCode = 200;
                context.Response.Cache.SetCacheability(HttpCacheability.Public);
                context.Response.Cache.SetExpires(WebProvider.ActiveProvider.DataSet.NextUpdate);
                context.Response.Cache.VaryByParams.IgnoreParams = false;
                if (_eTagNotSupported == false)
                {
                    try
                    {
                        context.Response.Headers["ETag"] = hash;
                    }
                    catch (PlatformNotSupportedException)
                    {
                        _eTagNotSupported = true;
                    }
                }
                context.Response.AppendHeader("Content-Length", Feature.ImageOptimiser.EmptyImage.Length.ToString());
                context.Response.OutputStream.Write(
                    Feature.ImageOptimiser.EmptyImage, 0, Feature.ImageOptimiser.EmptyImage.Length);
            }
            context.ApplicationInstance.CompleteRequest();
        }

        internal static void OptimisedImageResponse(HttpContext context)
        {
            var size = GetRequiredSize(context);

            if (size.Width == 0 && size.Height == 0)
            {
                var match = WebProvider.GetMatch(context.Request);

                // Get the screen width and height.
                int value;
                if (match["ScreenPixelsWidth"] != null &&
                    match["ScreenPixelsWidth"].Count > 0 &&
                    int.TryParse(match["ScreenPixelsWidth"][0].ToString(), out value))
                    size.Width = value;
                if (match["ScreenPixelsHeight"] != null &&
                    match["ScreenPixelsHeight"].Count > 0 &&
                    int.TryParse(match["ScreenPixelsHeight"][0].ToString(), out value))
                    size.Height = value;

                // Use the larger of the two values as the width as there is no
                // way of knowing if the device is in landscape or portrait
                // orientation.
                size.Width = Math.Max(size.Width, size.Height);
                size.Height = 0;
            }

            // Ensure the size is not larger than the maximum parameters.
            ResolveSize(ref size);

            if (size.Width > 0 || size.Height > 0)
            {
                // Get the files and paths involved in the caching.
                var cachedFileVirtualPath = Image.Support.GetCachedResponseFile(context.Request, size);
                var cachedFilePhysicalPath = context.Server.MapPath(cachedFileVirtualPath);
                var cachedFile = new FileInfo(cachedFilePhysicalPath);
                var sourceFile = context.Server.MapPath(context.Request.AppRelativeCurrentExecutionFilePath);

                EventLog.Debug(String.Format(
                    "Image processor is responding with image '{0}' of width '{1}' and height '{2}'",
                    sourceFile,
                    size.Width,
                    size.Height));

                // If the cached file doesn't exist or is out of date
                // then create a new cached file and serve this in response
                // to the request by rewriting the requested URL to the 
                // static file.
                if (cachedFile.Exists == false ||
                    cachedFile.LastWriteTimeUtc < File.GetLastWriteTimeUtc(sourceFile))
                {
                    // Check the directory exists?
                    if (cachedFile.Directory.Exists == false)
                        Directory.CreateDirectory(cachedFile.DirectoryName);

                    // Shrink the image to the cache file path. Use a bit depth of 32 pixels
                    // as the image cache is not aware of the specific devices bit depth, only
                    // the requested screen size.
                    var processor = new Image.Processor(32);
                    var source = new FileInfo(sourceFile);
                    processor.Width = size.Width;
                    processor.Height = size.Height;
                    using (var cachedStream = new MemoryStream())
                    {
                        try
                        {
                            // Make sure the source stream is closed when the shrinking
                            // process has been completed.
                            using (var sourceStream = source.OpenRead())
                            {
                                processor.Shrink(
                                    sourceStream,
                                    cachedStream);
                            }

                            // Some times the GDI can produce larger files than the original.
                            // Check for this to ensure the image is smaller.
                            if (cachedStream.Length < source.Length)
                            {
                                // Use the cache stream for the new file.
                                using (var fileStream = File.Create(cachedFilePhysicalPath))
                                {
                                    cachedStream.Position = 0;
                                    cachedStream.CopyTo(fileStream);
                                }
                            }
                            else
                            {
                                // Copy the original file into the cache to avoid doing
                                // this again in the future.
                                source.CopyTo(cachedFilePhysicalPath);
                            }
                        }
                        catch (Exception ex)
                        {
                            EventLog.Warn(String.Format(
                                "Source file '{0}' generated exception '{1}' shrinking to '{2}' by '{3}'.",
                                sourceFile,
                                ex,
                                processor.Width,
                                processor.Height));
                        }
                    }
                }
                if (File.Exists(cachedFilePhysicalPath))
                    context.RewritePath(cachedFileVirtualPath, true);
                else
                    context.RewritePath(context.Request.AppRelativeCurrentExecutionFilePath);
            }
        }

        private static Size GetRequiredSize(HttpContext context)
        {
            int width = 0;
            int height = 0;

            if (String.IsNullOrEmpty(context.Request.QueryString[WidthParameter]) == false &&
                int.TryParse(context.Request.QueryString[WidthParameter], out width) == false)
            {
                if (context.Request.QueryString[WidthParameter] == AUTO_STRING)
                {
                    width = Manager.ImageOptimisation.DefaultAuto;
                }
                else
                {
                    width = Manager.ImageOptimisation.MaxWidth;
                }
            }

            if (String.IsNullOrEmpty(context.Request.QueryString[HeightParameter]) == false &&
                int.TryParse(context.Request.QueryString[HeightParameter], out height) == false)
            {
                if (context.Request.QueryString[HeightParameter] == AUTO_STRING)
                {
                    height = Manager.ImageOptimisation.DefaultAuto;
                }
                else
                {
                    height = Manager.ImageOptimisation.MaxHeight;
                }
            }

            return new Size(width, height);
        }
        
        private static void ResolveSize(ref Size size)
        {
            // Check that a size has been requested.
            if (size.Width > 0 || size.Height > 0)
            {
                // Check that no dimensions are above the specifed max.
                if (size.Height > MaxHeight || size.Width > MaxWidth)
                {
                    if (size.Height > size.Width)
                    {
                        ResolveWidth(ref size);
                        ResolveHeight(ref size);
                    }
                    else
                    {
                        ResolveHeight(ref size);
                        ResolveWidth(ref size);
                    }
                }

                // Use the factor to adjust the size.
                size.Height = Factor * (int)Math.Floor((double)size.Height / Factor);
                size.Width = Factor * (int)Math.Floor((double)size.Width / Factor);

                // If the image is 0x0 after factoring then set the width to
                // the factor so that a little image gets returned.
                if (size.Height == 0 && size.Width == 0)
                    size.Width = Factor;
            }
        }

        /// <summary>
        /// Adjust the height of the image so that it is not larger than the
        /// maximum allowed height.
        /// </summary>
        private static void ResolveHeight(ref Size size)
        {
            if (size.Height > MaxHeight)
            {
                var ratio = (double)MaxHeight / (double)size.Height;
                size.Height = MaxHeight;
                size.Width = (int)((double)size.Height * ratio);
            }
        }

        /// <summary>
        /// Adjust the width of the image so that it is not larger than the
        /// maximum allowed width.
        /// </summary>
        private static void ResolveWidth(ref Size size)
        {
            if (size.Width > MaxWidth)
            {
                var ratio = (double)MaxWidth / (double)size.Width;
                size.Width = MaxWidth;
                size.Height = (int)((double)size.Height * ratio);
            }
        }

        private static int MaxWidth
        {
            get
            {
                if (Manager.ImageOptimisation.Enabled && Manager.ImageOptimisation.MaxWidth != 0)
                    return Manager.ImageOptimisation.MaxWidth;
                return int.MaxValue;
            }
        }

        private static int MaxHeight
        {
            get
            {
                if (Manager.ImageOptimisation.Enabled && Manager.ImageOptimisation.MaxHeight != 0)
                    return Manager.ImageOptimisation.MaxHeight;
                return int.MaxValue;
            }
        }

        private static int Factor
        {
            get
            {
                if (Manager.ImageOptimisation.Enabled)
                    return Manager.ImageOptimisation.Factor;
                return 1;
            }
        }
    }
}
