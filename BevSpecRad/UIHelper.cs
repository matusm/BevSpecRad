using Bev.Instruments.ArraySpectrometer.Abstractions;
using Bev.Instruments.Thorlabs.FW;
using BevSpecRad.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BevSpecRad
{
    public static class UIHelper
    {
        public static void WriteMessageAndWait(string message)
        {
            Console.WriteLine(message);
            Console.ReadKey(true); // true = do not display the key pressed
        }

        public static void DisplaySpectrometerInfo(IArraySpectrometer spectro)
        {
            Console.Write(FormatSpectrometerInfo(spectro)); 
        }

        public static string FormatSpectrometerInfo(IArraySpectrometer spectro)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Instrument Manufacturer:  {spectro.InstrumentManufacturer}");
            sb.AppendLine($"Instrument Type:          {spectro.InstrumentType}");
            sb.AppendLine($"Instrument Serial Number: {spectro.InstrumentSerialNumber}");
            sb.AppendLine($"Firmware Revision:        {spectro.InstrumentFirmwareVersion}");
            sb.AppendLine($"Min Wavelength:           {spectro.MinimumWavelength:F2} nm");
            sb.AppendLine($"Max Wavelength:           {spectro.MaximumWavelength:F2} nm");
            sb.AppendLine($"Pixel Number:             {spectro.Wavelengths.Length}");
            sb.AppendLine($"Integration Time:         {spectro.GetIntegrationTime()} s");
            return sb.ToString();
        }

        public static void DisplayShutterInfo(IShutter shutter)
        {
            Console.WriteLine($"Shutter Name:             {shutter.Name}");
        }

        public static void DisplayFilterWheelInfo(IFilterWheel filterWheel)
        {
            Console.WriteLine($"Filter Wheel Name:        {filterWheel.Name}");
            Console.WriteLine($"Number of Positions:      {filterWheel.FilterCount}");
        }
    }
}
