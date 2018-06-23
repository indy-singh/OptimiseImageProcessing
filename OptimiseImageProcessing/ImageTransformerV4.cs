using System.IO;
using System.Net;
using Microsoft.IO;
using PhotoSauce.MagicScaler;

namespace OptimiseImageProcessing
{
    /// <summary>
    /// Stats:-
    ///     Took: 297 ms
    ///     Allocated: 20,740 kb
    ///     Peak Working Set: 34,700 kb
    ///     Gen 0 collections: 4
    ///     Gen 1 collections: 1
    ///     Gen 2 collections: 0
    ///
    /// dotTrace:-
    ///     Total RAM: 22 MB
    ///     SOH:       19 MB
    ///     LOH:       2.8 MB
    /// </summary>
    public class ImageTransformerV4 : IImageTransformer
    {
        private readonly RecyclableMemoryStreamManager _streamManager;
        private bool _written;

        public ImageTransformerV4()
        {
            _streamManager = new RecyclableMemoryStreamManager();
            _written = false;
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

            borrowedStream.Position = 0L;

            using (borrowedStream)
            {
                // upload scaledImage to AWS S3
               
                if (_written == false)
                {
                    using (var fileStream = File.Create(@"..\..\v4.jpg"))
                    {
                        MagicImageProcessor.ProcessImage(borrowedStream, fileStream, new ProcessImageSettings()
                        {
                            Width = 320,
                            Height = 240,
                            ResizeMode = CropScaleMode.Max,
                            SaveFormat = FileFormat.Jpeg,
                            JpegQuality = 70,
                        });
                    }

                    _written = true;
                }
            }
        }
    }
}