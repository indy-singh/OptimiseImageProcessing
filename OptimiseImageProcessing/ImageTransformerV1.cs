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
    public class ImageTransformerV1 : IImageTransformer
    {
        private bool _written;

        public ImageTransformerV1()
        {
            _written = false;
        }

        public void Transform(string url)
        {
            var bytes = GetImageFromUrl(url);

            if (CanCreateImageFrom(bytes) == false)
            {
                return;
            }

            using (var stream = new MemoryStream(bytes))
            using (var originalImage = Image.FromStream(stream))
            using (var scaledImage = ImageHelper.Scale(originalImage, 320, 240))
            using (var graphics = Graphics.FromImage(scaledImage))
            {
                ImageHelper.TransformImage(graphics, scaledImage, originalImage);

                // upload scaledImage to AWS S3

                if (_written == false)
                {
                    using (var fileStream = File.Create(@"..\..\v1.jpg"))
                    {
                        scaledImage.Save(fileStream, ImageFormat.Jpeg);
                    }

                    _written = true;
                }
            }
        }

        private static bool CanCreateImageFrom(byte[] bytes)
        {
            try
            {
                using (var stream = new MemoryStream(bytes))
                {
                    Image.FromStream(stream);
                }
            }
            catch (ArgumentException)
            {
                return false;
            }

            return true;
        }

        private static byte[] GetImageFromUrl(string url)
        {
            byte[] data;
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Timeout = 10000;
            request.ReadWriteTimeout = 10000;

            using (var response = request.GetResponse())
            using (var responseStream = response.GetResponseStream())
            using (var memoryStream = new MemoryStream())
            {
                int count;
                do
                {
                    var buf = new byte[1024];
                    count = responseStream.Read(buf, 0, 1024);
                    memoryStream.Write(buf, 0, count);
                } while (responseStream.CanRead && count > 0);

                data = memoryStream.ToArray();
            }

            return data;
        }
    }
}