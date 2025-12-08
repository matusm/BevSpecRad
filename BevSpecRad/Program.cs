using Bev.Instruments.ArraySpectrometer.Abstractions;
using Bev.Instruments.Thorlabs.Ctt;
using Bev.Instruments.Thorlabs.FW;
using BevSpecRad.Abstractions;
using BevSpecRad.Domain;
using BevSpecRad.Helpers;
using SpectralFilterRegistry;
using At.Matus.OpticalSpectrumLib;
using System;
using System.IO;

namespace BevSpecRad
{
    public partial class Program
    {
        private static IArraySpectrometer spectro;
        private static IShutter shutter;
        private static IFilterWheel filterWheel;
        private static FilterRegistry filterRegistry;
        private static EventLogger eventLogger;

        // Run() is the new Main() after parsing command-line arguments
        private static void Run(Options options)
        {
            // instantiate instruments and logger
            eventLogger = new EventLogger(options.BasePath);
            spectro = new ThorlabsCct();
            shutter = new CctShutter((ThorlabsCct)spectro);
            filterWheel = new ManualFilterWheel();
            filterRegistry = new FilterRegistry(@"C:\Users\User\source\repos\BevSpecRad\SpectralFilterRegistry\appsettings.json");

            LogSetupInfo();
            DisplaySetupInfo();
            // ask user to set up everything
            filterWheel.GoToPosition(5); // Ensuring filter wheel is at position 5 also for manual filterwheels
            UIHelper.WriteMessageAndWait("==============================================================\nPlease set up the STANDARD LAMP and press any key to continue.\n==============================================================");

            #region Define all datafields
            // integration times for each filter
            double intTimeA = options.IntTime;
            double intTimeB = options.IntTime;
            double intTimeC = options.IntTime;
            double intTimeD = options.IntTime;
            double intTimeBlank = options.IntTime   ;
            // spectra for standard lamp
            IOpticalSpectrum specStdA;
            IOpticalSpectrum specStdB;
            IOpticalSpectrum specStdC;
            IOpticalSpectrum specStdD;
            IOpticalSpectrum specStdBlank;
            // spectra for unknown lamp (DUT)
            IOpticalSpectrum specDutA;
            IOpticalSpectrum specDutB;
            IOpticalSpectrum specDutC;
            IOpticalSpectrum specDutD;
            IOpticalSpectrum specDutBlank;
            // control spectra for dark current subtraction
            IOpticalSpectrum specControlA;
            IOpticalSpectrum specControlB;
            IOpticalSpectrum specControlC;
            IOpticalSpectrum specControlD;
            IOpticalSpectrum specControlBlank;
            #endregion

            #region Get optimal integration times for each filter
            if (UIHelper.SkipAction("to determine optimal integration times for each filter (otherwise, the provided integration time will be used)"))
            {
                eventLogger.LogEvent("Skipping optimal integration time determination as per user request.");
                intTimeA = options.IntTime;
                intTimeB = options.IntTime;
                intTimeC = options.IntTime;
                intTimeD = options.IntTime;
                intTimeBlank = options.IntTime;
            }
            else
            {
                filterWheel.GoToPosition(1);
                intTimeA = ((ThorlabsCct)spectro).GetOptimalExposureTime();
                eventLogger.LogEvent($"Optimal integration time for filter A: {intTimeA} s");
                Console.WriteLine($"Optimal integration time for filter A: {intTimeA} s");
                filterWheel.GoToPosition(2);
                intTimeB = ((ThorlabsCct)spectro).GetOptimalExposureTime();
                eventLogger.LogEvent($"Optimal integration time for filter B: {intTimeB} s");
                Console.WriteLine($"Optimal integration time for filter B: {intTimeB} s");
                filterWheel.GoToPosition(3);
                intTimeC = ((ThorlabsCct)spectro).GetOptimalExposureTime();
                eventLogger.LogEvent($"Optimal integration time for filter C: {intTimeC} s");
                Console.WriteLine($"Optimal integration time for filter C: {intTimeC} s");
                filterWheel.GoToPosition(4);
                intTimeD = ((ThorlabsCct)spectro).GetOptimalExposureTime();
                eventLogger.LogEvent($"Optimal integration time for filter D: {intTimeD} s");
                Console.WriteLine($"Optimal integration time for filter D: {intTimeD} s");
                filterWheel.GoToPosition(5);
                intTimeBlank = ((ThorlabsCct)spectro).GetOptimalExposureTime();
                eventLogger.LogEvent($"Optimal integration time for blank filter: {intTimeBlank} s");
                Console.WriteLine($"Optimal integration time for blank filter: {intTimeBlank} s");
            }
            #endregion

            #region Standard Lamp
            // Now perform measurements on standard lamp
            specStdA = PerformABBAMeasurement(1, intTimeA, options.Nsamples);
            specStdB = PerformABBAMeasurement(2, intTimeB, options.Nsamples);
            specStdC = PerformABBAMeasurement(3, intTimeC, options.Nsamples);
            specStdD = PerformABBAMeasurement(4, intTimeD, options.Nsamples);
            specStdBlank = PerformABBAMeasurement(5, intTimeBlank, options.Nsamples);
            #endregion

            #region DUT Lamp
            UIHelper.WriteMessageAndWait("\n==============================================================\nPlease set up the DUT LAMP and press any key to continue.\n==============================================================");

            specDutA = PerformABBAMeasurement(1, intTimeA, options.Nsamples);
            specDutB = PerformABBAMeasurement(2, intTimeB, options.Nsamples);
            specDutC = PerformABBAMeasurement(3, intTimeC, options.Nsamples);
            specDutD = PerformABBAMeasurement(4, intTimeD, options.Nsamples);
            specDutBlank = PerformABBAMeasurement(5, intTimeBlank, options.Nsamples);
            #endregion

            #region Control Spectra
            // Now take control spectra (dark spectra) for all integration times
            Console.WriteLine("\nMeasurements on lamps done. Lamp can be shut down. Taking control (dark) spectra.");
            specControlA = PerformABBAMeasurement(6, intTimeA, options.Nsamples);
            specControlB = PerformABBAMeasurement(6, intTimeB, options.Nsamples);
            specControlC = PerformABBAMeasurement(6, intTimeC, options.Nsamples);
            specControlD = PerformABBAMeasurement(6, intTimeD, options.Nsamples);
            specControlBlank = PerformABBAMeasurement(6, intTimeBlank, options.Nsamples);
            #endregion

            #region Save all rawspectra as CSV
            specStdA.SaveSpectrumAsCsv(eventLogger.LogDirectory, "SpecStdA.csv");
            specStdB.SaveSpectrumAsCsv(eventLogger.LogDirectory, "SpecStdB.csv");
            specStdC.SaveSpectrumAsCsv(eventLogger.LogDirectory, "SpecStdC.csv");
            specStdD.SaveSpectrumAsCsv(eventLogger.LogDirectory, "SpecStdD.csv");
            specStdBlank.SaveSpectrumAsCsv(eventLogger.LogDirectory, "SpecStdBlank.csv");
            specDutA.SaveSpectrumAsCsv(eventLogger.LogDirectory, "SpecDutA.csv");
            specDutB.SaveSpectrumAsCsv(eventLogger.LogDirectory, "SpecDutB.csv");
            specDutC.SaveSpectrumAsCsv(eventLogger.LogDirectory, "SpecDutC.csv");
            specDutD.SaveSpectrumAsCsv(eventLogger.LogDirectory, "SpecDutD.csv");
            specDutBlank.SaveSpectrumAsCsv(eventLogger.LogDirectory, "SpecDutBlank.csv");
            specControlA.SaveSpectrumAsCsv(eventLogger.LogDirectory, "SpecControlA.csv");
            specControlB.SaveSpectrumAsCsv(eventLogger.LogDirectory, "SpecControlB.csv");
            specControlC.SaveSpectrumAsCsv(eventLogger.LogDirectory, "SpecControlC.csv");
            specControlD.SaveSpectrumAsCsv(eventLogger.LogDirectory, "SpecControlD.csv");
            specControlBlank.SaveSpectrumAsCsv(eventLogger.LogDirectory, "SpecControlBlank.csv");
            #endregion

            Console.WriteLine();
            filterWheel.GoToPosition(6);
            eventLogger.Close();
        }
    }
}
