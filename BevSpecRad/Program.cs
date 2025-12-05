using Bev.Instruments.ArraySpectrometer.Abstractions;
using Bev.Instruments.Thorlabs.Ctt;
using Bev.Instruments.Thorlabs.FW;
using BevSpecRad.Abstractions;
using BevSpecRad.Domain;
using SpectralFilterRegistry;
using System;
using System.IO;

namespace BevSpecRad
{
    public partial class Program
    {
        private static IArraySpectrometer spectro;
        private static IShutter shutter;
        private static IFilterWheel filterWheel;
        private static StreamWriter logFile;

        // Run() is the new Main() after parsing command-line arguments
        private static void Run(Options options)
        {
            spectro = new ThorlabsCct();
            shutter = new CctShutter((ThorlabsCct)spectro);
            filterWheel = new ManualFilterWheel();

            UIHelper.DisplaySpectrometerInfo(spectro);
            UIHelper.DisplayShutterInfo(shutter);
            UIHelper.DisplayFilterWheelInfo(filterWheel);
            Console.WriteLine();

            FilterRegistry filterRegistry = new FilterRegistry(@"C:\Users\User\source\repos\BevSpecRad\SpectralFilterRegistry\appsettings.json");
            Console.WriteLine($"Filter registry contains {filterRegistry.Count} filters.");
            for (int i = 1; i <= filterRegistry.Count; i++)
            {
                var filter = filterRegistry.GetFilter(i);
                Console.WriteLine(filter);
            }




            // Use parsed options here
            if (!string.IsNullOrEmpty(options.UserComment))
            {
                Console.WriteLine(options.UserComment);
            }

            if (options.N != 0)
            {
                Console.WriteLine(options.N);
            }
        }
    }
}
