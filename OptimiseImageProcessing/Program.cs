using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace OptimiseImageProcessing
{
    public class Program
    {
        private const string Url = "https://upload.wikimedia.org/wikipedia/commons/b/b9/Pizigani_1367_Chart_1MB.jpg";

        public static void Main(string[] args)
        {
            AppDomain.MonitoringIsEnabled = true;

            var dict = new Dictionary<string, Action>()
            {
                ["1"] = () =>
                {
                    Console.WriteLine("Version 1");
                    var transformer = new ImageTransformerV1();
                    Transform(transformer, Url);
                },
                ["2"] = () =>
                {
                    Console.WriteLine("Version 2");
                    var transformer = new ImageTransformerV2();
                    Transform(transformer, Url);
                },
                ["30"] = () =>
                {
                    Console.WriteLine("Version 30");
                    var transformer = new ImageTransformerV30();
                    Transform(transformer, Url);
                },
                ["40"] = () =>
                {
                    Console.WriteLine("Version 40");
                    var transformer = new ImageTransformerV40();
                    Transform(transformer, Url);
                }
            };

#if DEBUG
            dict["1"]();
            Environment.Exit(0);
#endif

            if (args.Length == 1 && dict.ContainsKey(args[0]))
            {
                dict[args[0]]();
            }
            else
            {
                Console.WriteLine("Incorrect parameters");
                Environment.Exit(1);
            }

            Console.WriteLine($"Took: {AppDomain.CurrentDomain.MonitoringTotalProcessorTime.TotalMilliseconds:#,###} ms");
            Console.WriteLine($"Allocated: {AppDomain.CurrentDomain.MonitoringTotalAllocatedMemorySize / 1024:#,#} kb");
            Console.WriteLine($"Peak Working Set: {Process.GetCurrentProcess().PeakWorkingSet64 / 1024:#,#} kb");

            for (var index = 0; index <= GC.MaxGeneration; index++)
            {
                Console.WriteLine($"Gen {index} collections: {GC.CollectionCount(index)}");
            }

            Console.WriteLine(Environment.NewLine);
        }

        private static void Transform(IImageTransformer imageTransformer, string url)
        {
            for (int i = 0; i < 100; i++)
            {
                //Console.WriteLine(i);
                imageTransformer.Transform(url);
            }
        }
    }

    public interface IImageTransformer
    {
        void Transform(string url);
    }
}
