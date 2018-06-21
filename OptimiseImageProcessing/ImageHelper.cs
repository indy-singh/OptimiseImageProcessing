using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace OptimiseImageProcessing
{
    public static class ImageHelper
    {
        public static Image Scale(Image sourceImage, int width, int height)
        {
            if (sourceImage.Height < height && sourceImage.Width < width)
                return new Bitmap(sourceImage.Width, sourceImage.Height);

            var xRatio = (double)sourceImage.Width / width;
            var yRatio = (double)sourceImage.Height / height;
            var ratio = Math.Max(xRatio, yRatio);
            var nnx = (int)Math.Floor(sourceImage.Width / ratio);
            var nny = (int)Math.Floor(sourceImage.Height / ratio);
            return new Bitmap(nnx, nny);
        }

        public static void TransformImage(Graphics graphics, Image scaledImage, Image originalImage)
        {
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            var scaledImageRect = new Rectangle(0, 0, scaledImage.Width, scaledImage.Height);
            var originalImageRect = new Rectangle(0, 0, originalImage.Width, originalImage.Height);
            graphics.DrawImage(originalImage, scaledImageRect, originalImageRect, GraphicsUnit.Pixel);
        }
    }
}