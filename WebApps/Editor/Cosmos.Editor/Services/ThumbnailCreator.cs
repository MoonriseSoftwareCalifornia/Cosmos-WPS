﻿using Cosmos.Cms.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace Cosmos.Cms.Services
{
    /// <summary>
    /// Image thumbnail creator
    /// </summary>
    public class ThumbnailCreator
    {
        private static readonly IDictionary<string, ImageFormat> ImageFormats = new Dictionary<string, ImageFormat>
        {
            {"image/png", ImageFormat.Png},
            {"image/gif", ImageFormat.Gif},
            {"image/jpeg", ImageFormat.Jpeg}
        };

        private readonly ImageResizer resizer;

        /// <summary>
        /// Constructor
        /// </summary>
        public ThumbnailCreator()
        {
            resizer = new ImageResizer();
        }

        /// <summary>
        /// Create thumbnail
        /// </summary>
        /// <param name="source"></param>
        /// <param name="desiredSize"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public byte[] Create(Stream source, ImageSizeModel desiredSize, string contentType)
        {
            using (var image = Image.FromStream(source))
            {
                var originalSize = new ImageSizeModel
                {
                    Height = image.Height,
                    Width = image.Width
                };

                var size = resizer.Resize(originalSize, desiredSize);

                using (var thumbnail = new Bitmap(size.Width, size.Height))
                {
                    ScaleImage(image, thumbnail);

                    using (var memoryStream = new MemoryStream())
                    {
                        thumbnail.Save(memoryStream, ImageFormats[contentType]);

                        return memoryStream.ToArray();
                    }
                }
            }
        }

        /// <summary>
        /// Create filkl
        /// </summary>
        /// <param name="source"></param>
        /// <param name="desiredSize"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public byte[] CreateFill(Stream source, ImageSizeModel desiredSize, string contentType)
        {
            using (var image = Image.FromStream(source))
            {
                using (var memoryStream = new MemoryStream())
                {
                    FixedSize(image, desiredSize.Width, desiredSize.Height, true)
                        .Save(memoryStream, ImageFormats[contentType]);
                    return memoryStream.ToArray();
                }
            }
        }

        private void ScaleImage(Image source, Image destination)
        {
            using (var graphics = Graphics.FromImage(destination))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

                graphics.DrawImage(source, 0, 0, destination.Width, destination.Height);
            }
        }


        private Image FixedSize(Image imgPhoto, int Width, int Height, bool needToFill)
        {
            var sourceWidth = imgPhoto.Width;
            var sourceHeight = imgPhoto.Height;
            var sourceX = 0;
            var sourceY = 0;
            var destX = 0;
            var destY = 0;

            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;

            nPercentW = Width / (float)sourceWidth;
            nPercentH = Height / (float)sourceHeight;
            if (!needToFill)
            {
                if (nPercentH < nPercentW)
                    nPercent = nPercentH;
                else
                    nPercent = nPercentW;
            }
            else
            {
                if (nPercentH > nPercentW)
                {
                    nPercent = nPercentH;
                    destX = (int)Math.Round((Width -
                                              sourceWidth * nPercent) / 2);
                }
                else
                {
                    nPercent = nPercentW;
                    destY = (int)Math.Round((Height -
                                              sourceHeight * nPercent) / 2);
                }
            }

            if (nPercent > 1)
                nPercent = 1;

            var destWidth = (int)Math.Round(sourceWidth * nPercent);
            var destHeight = (int)Math.Round(sourceHeight * nPercent);

            var bmPhoto = new Bitmap(
                destWidth <= Width ? destWidth : Width,
                destHeight < Height ? destHeight : Height,
                PixelFormat.Format32bppRgb);

            var grPhoto = Graphics.FromImage(bmPhoto);
            grPhoto.Clear(Color.White);
            grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;
            grPhoto.CompositingQuality = CompositingQuality.HighQuality;
            grPhoto.SmoothingMode = SmoothingMode.AntiAlias;

            grPhoto.DrawImage(imgPhoto,
                new Rectangle(destX, destY, destWidth, destHeight),
                new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
                GraphicsUnit.Pixel);

            grPhoto.Dispose();
            return bmPhoto;
        }
    }
}