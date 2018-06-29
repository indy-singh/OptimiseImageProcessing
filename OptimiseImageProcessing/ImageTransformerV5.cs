using System.IO;
using System.Net;
using Microsoft.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Transforms;
using SixLabors.Primitives;

namespace OptimiseImageProcessing
{
    /// <summary>
    /// Stats:-
    ///     Took: 30,328 ms
    ///     Allocated: 125,191 kb
    ///     Peak Working Set: 80,876 kb
    ///     Gen 0 collections: 17
    ///     Gen 1 collections: 4
    ///     Gen 2 collections: 2
    ///
    /// dotTrace:-
    ///     Total RAM: 125 MB
    ///     SOH:        64 MB
    ///     LOH:        61 MB
    /// </summary>
    public class ImageTransformerV5 : IImageTransformer
    {
        private readonly RecyclableMemoryStreamManager _streamManager;

        public ImageTransformerV5()
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

            MemoryStream instream;

            using (var response = request.GetResponse())
            {
                if (response.ContentLength == -1) // Means that content length is NOT sent back by the third party server
                {
                    instream = _streamManager.GetStream(url); // then we let the stream manager had this 
                }
                else
                {
                    instream = _streamManager.GetStream(url, (int)response.ContentLength); // otherwise we borrow a stream with the exact size
                }

                int bufferSize;

                if (response.ContentLength == -1 || response.ContentLength > 81920)
                {
                    bufferSize = 81920;
                }
                else
                {
                    bufferSize = (int)response.ContentLength;
                }

                // close the http response stream asap, we only need the contents, we don't need to keep it open
                using (var responseStream = response.GetResponseStream())
                {
                    responseStream.CopyTo(instream, bufferSize);
                }
            }

            instream.Position = 0L;

            using (instream)
            {
                // upload scaledImage to AWS S3 in production, in the test harness write to disk

                using (var outStream = File.Create(@"..\..\v5.jpg"))
                using (var image = Image.Load(instream, out var format))
                {
                    image.Mutate(c => c.Resize(new ResizeOptions
                    {
                        Size = new Size(width: 320, height: 240),
                        Mode = ResizeMode.Max
                    }));
                    image.Save(outStream, format);
                }
            }
        }
    }
}