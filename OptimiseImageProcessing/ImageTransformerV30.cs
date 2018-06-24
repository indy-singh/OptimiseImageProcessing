using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using Microsoft.IO;

namespace OptimiseImageProcessing
{
    /// <summary>
    /// Stats:-
    ///     Took: 7,688 ms
    ///     Allocated: 125,739 kb
    ///     Peak Working Set: 71,140 kb
    ///     Gen 0 collections: 29
    ///     Gen 1 collections: 2
    ///     Gen 2 collections: 1
    ///
    /// dotTrace:-
    ///     Total RAM: 152 MB
    ///     SOH:       150 MB
    ///     LOH:       1.6 MB
    /// </summary>
    public class ImageTransformerV30 : IImageTransformer
    {
        private readonly RecyclableMemoryStreamManager _streamManager;

        public ImageTransformerV30()
        {
            _streamManager = new RecyclableMemoryStreamManager();
        }

        public void Transform(string url)
        {
            var request = WebRequest.CreateHttp(url);
            request.Timeout = 10000;
            request.ReadWriteTimeout = 10000;
            request.AllowReadStreamBuffering = false;
            request.AllowWriteStreamBuffering = false;

            MemoryStream borrowedStream;

            using (var response = request.GetResponse())
            {
                if (response.ContentLength == -1) // Means that content length is NOT sent back by the third party server
                {
                    borrowedStream = _streamManager.GetStream(url); // then we let the stream manager had this 
                }
                else
                {
                    borrowedStream = _streamManager.GetStream(url, (int)response.ContentLength); // otherwise we borrow a stream with the exact size
                }

                int bufferSize;

                if (response.ContentLength == -1 || response.ContentLength > 81920)
                {
                    bufferSize = 81920;
                }
                else
                {
                    bufferSize = (int) response.ContentLength;
                }

                // close the http response stream asap, we only need the contents, we don't need to keep it open
                using (var responseStream = response.GetResponseStream())
                {
                    responseStream.CopyTo(borrowedStream, bufferSize);
                }
            }

            using (borrowedStream)
            using (var originalImage = Image.FromStream(borrowedStream))
            using (var scaledImage = ImageHelper.Scale(originalImage, 320, 240))
            using (var graphics = Graphics.FromImage(scaledImage))
            {
                ImageHelper.TransformImage(graphics, scaledImage, originalImage);
                // upload scaledImage to AWS S3 in production, in the test harness write to disk

                using (var fileStream = File.Create(@"..\..\v30.jpg"))
                {
                    scaledImage.Save(fileStream, ImageFormat.Jpeg);
                }
            }
        }
    }
}