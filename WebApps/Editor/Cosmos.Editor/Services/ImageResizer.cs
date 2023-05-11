using Cosmos.Cms.Models;
using System;

namespace Cosmos.Cms.Services
{
    /// <summary>
    /// Image resize utility
    /// </summary>
    public class ImageResizer
    {
        /// <summary>
        /// Calculate image resize
        /// </summary>
        /// <param name="originalSize"></param>
        /// <param name="targetSize"></param>
        /// <returns></returns>
        public ImageSizeModel Resize(ImageSizeModel originalSize, ImageSizeModel targetSize)
        {
            var aspectRatio = originalSize.Width / (float)originalSize.Height;
            var width = targetSize.Width;
            var height = targetSize.Height;

            if (originalSize.Width > targetSize.Width || originalSize.Height > targetSize.Height)
            {
                if (aspectRatio > 1)
                    height = (int)(targetSize.Height / aspectRatio);
                else
                    width = (int)(targetSize.Width * aspectRatio);
            }
            else
            {
                width = originalSize.Width;
                height = originalSize.Height;
            }

            return new ImageSizeModel
            {
                Width = Math.Max(width, 1),
                Height = Math.Max(height, 1)
            };
        }
    }
}