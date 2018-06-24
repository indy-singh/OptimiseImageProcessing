using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;

namespace OptimiseImageProcessing
{
    /// <summary>
    /// Stats:-
    ///     Took: 10,297 ms
    ///     Allocated: 851,894 kb
    ///     Peak Working Set: 96,276 kb
    ///     Gen 0 collections: 184
    ///     Gen 1 collections: 101
    ///     Gen 2 collections: 101
    ///
    /// dotTrace:-
    ///     Total RAM: 901 MB
    ///     SOH:       410 MB
    ///     LOH:       491 MB
    /// </summary>
    public class ImageTransformerV2 : IImageTransformer
    {
        public void Transform(string url)
        {
            var request = WebRequest.CreateHttp(url);
            request.Timeout = 10000;
            request.ReadWriteTimeout = 10000;

            using (var response = request.GetResponse())
            using (var responseStream = response.GetResponseStream())
            {
                using (var originalImage = Image.FromStream(responseStream))
                using (var scaledImage = ImageHelper.Scale(originalImage, 320, 240))
                using (var graphics = Graphics.FromImage(scaledImage))
                {
                    ImageHelper.TransformImage(graphics, scaledImage, originalImage);

                    // upload scaledImage to AWS S3 in production, in the test harness write to disk

                    using (var fileStream = File.Create(@"..\..\v2.jpg"))
                    {
                        scaledImage.Save(fileStream, ImageFormat.Jpeg);
                    }
                }
            }
        }
    }
}