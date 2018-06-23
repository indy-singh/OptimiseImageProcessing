using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;

namespace OptimiseImageProcessing
{
    /// <summary>
    /// Stats:-
    ///     Took: 7,844 ms
    ///     Allocated: 527,246 kb
    ///     Peak Working Set: 69,436 kb
    ///     Gen 0 collections: 100
    ///     Gen 1 collections: 100
    ///     Gen 2 collections: 100
    ///
    /// dotTrace:-
    ///     Total RAM: 547 MB
    ///     SOH:       159 MB
    ///     LOH:       388 MB
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
            using (var originalImage = Image.FromStream(responseStream))
            using (var scaledImage = ImageHelper.Scale(originalImage, 320, 240))
            using (var graphics = Graphics.FromImage(scaledImage))
            {
                ImageHelper.TransformImage(graphics, scaledImage, originalImage);

                // upload scaledImage to AWS S3

                using (var fileStream = File.Create(@"..\..\v2.jpg"))
                {
                    scaledImage.Save(fileStream, ImageFormat.Jpeg);
                }
            }
        }
    }
}