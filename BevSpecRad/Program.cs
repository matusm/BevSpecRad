using Bev.Instruments.ArraySpectrometer.Abstractions;
using Bev.Instruments.Thorlabs.Ctt;
using Bev.Instruments.Thorlabs.FW;
using BevSpecRad.Abstractions;
using BevSpecRad.Domain;
using BevSpecRad.Helpers;
using SpectralFilterRegistry;
using At.Matus.OpticalSpectrumLib;
using System;

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
            filterWheel = new MotorFilterWheel("COM1");
            spectro = new ThorlabsCct();
            shutter = new CctShutter((ThorlabsCct)spectro);
            filterRegistry = new FilterRegistry(@"C:\Users\Administrator\source\repos\BevSpecRad\SpectralFilterRegistry\appsettings.json");

            LogSetupInfo();
            DisplaySetupInfo();
            // ask user to set up everything
            filterWheel.GoToPosition(5); // Ensuring filter wheel is at position 5 also for manual filterwheels
            UIHelper.WriteMessageAndWait("==============================================================\nPlease set up the STANDARD LAMP and press any key to continue.\n==============================================================\n");

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

            specStdA.SaveSpectrumAsCsv(eventLogger.LogDirectory, "RawSpecStdA.csv");
            specStdB.SaveSpectrumAsCsv(eventLogger.LogDirectory, "RawSpecStdB.csv");
            specStdC.SaveSpectrumAsCsv(eventLogger.LogDirectory, "RawSpecStdC.csv");
            specStdD.SaveSpectrumAsCsv(eventLogger.LogDirectory, "RawSpecStdD.csv");
            specStdBlank.SaveSpectrumAsCsv(eventLogger.LogDirectory, "RawSpecStdBlank.csv");
            #endregion

            #region DUT Lamp
            UIHelper.WriteMessageAndWait("\n==============================================================\nPlease set up the DUT LAMP and press any key to continue.\n==============================================================");

            specDutA = PerformABBAMeasurement(1, intTimeA, options.Nsamples);
            specDutB = PerformABBAMeasurement(2, intTimeB, options.Nsamples);
            specDutC = PerformABBAMeasurement(3, intTimeC, options.Nsamples);
            specDutD = PerformABBAMeasurement(4, intTimeD, options.Nsamples);
            specDutBlank = PerformABBAMeasurement(5, intTimeBlank, options.Nsamples);

            specDutA.SaveSpectrumAsCsv(eventLogger.LogDirectory, "RawSpecDutA.csv");
            specDutB.SaveSpectrumAsCsv(eventLogger.LogDirectory, "RawSpecDutB.csv");
            specDutC.SaveSpectrumAsCsv(eventLogger.LogDirectory, "RawSpecDutC.csv");
            specDutD.SaveSpectrumAsCsv(eventLogger.LogDirectory, "RawSpecDutD.csv");
            specDutBlank.SaveSpectrumAsCsv(eventLogger.LogDirectory, "RawSpecDutBlank.csv");

            #endregion

            #region Control Spectra
            // Now take control spectra (dark spectra) for all integration times
            Console.WriteLine("\nMeasurements on lamps done. Lamp can be shut down. Taking control (dark) spectra.");
            specControlA = PerformABBAMeasurement(6, intTimeA, options.Nsamples);
            specControlB = PerformABBAMeasurement(6, intTimeB, options.Nsamples);
            specControlC = PerformABBAMeasurement(6, intTimeC, options.Nsamples);
            specControlD = PerformABBAMeasurement(6, intTimeD, options.Nsamples);
            specControlBlank = PerformABBAMeasurement(6, intTimeBlank, options.Nsamples);

            specControlA.SaveSpectrumAsCsv(eventLogger.LogDirectory, "ControlSpecA.csv");
            specControlB.SaveSpectrumAsCsv(eventLogger.LogDirectory, "ControlSpecB.csv");
            specControlC.SaveSpectrumAsCsv(eventLogger.LogDirectory, "ControlSpecC.csv");
            specControlD.SaveSpectrumAsCsv(eventLogger.LogDirectory, "ControlSpecD.csv");
            specControlBlank.SaveSpectrumAsCsv(eventLogger.LogDirectory, "ControlSpecBlank.csv");
            #endregion

            #region Calculate some stuff 

            var ratioA = SpecMath.Ratio(specDutA, specStdA);
            var ratioB = SpecMath.Ratio(specDutB, specStdB);
            var ratioC = SpecMath.Ratio(specDutC, specStdC);
            var ratioD = SpecMath.Ratio(specDutD, specStdD);
            var ratioBlank = SpecMath.Ratio(specDutBlank, specStdBlank);

            // the cut-off wavelength are determined using actual spectra
            var maskedRatioA = ratioA.ApplyBandpassMask(180, 464, 10, 10);
            var maskedRatioB = ratioB.ApplyBandpassMask(464, 545, 10, 10);
            var maskedRatioC = ratioC.ApplyBandpassMask(545, 658, 10, 10);
            var maskedRatioD = ratioD.ApplyBandpassMask(658, 2000, 10, 10);

            var temp1 = SpecMath.Add(maskedRatioA, maskedRatioB);
            var temp2 = SpecMath.Add(maskedRatioC, maskedRatioD);
            var result = SpecMath.Add(temp1, temp2);

            var resampledResult = result.ResampleSpectrum(340, 900, 1);


            ratioA.SaveSpectrumAsCsv(eventLogger.LogDirectory,"ratioA.csv");
            ratioB.SaveSpectrumAsCsv(eventLogger.LogDirectory, "ratioB.csv");
            ratioC.SaveSpectrumAsCsv(eventLogger.LogDirectory, "ratioC.csv");
            ratioD.SaveSpectrumAsCsv(eventLogger.LogDirectory, "ratioD.csv");
            ratioBlank.SaveSpectrumAsCsv(eventLogger.LogDirectory, "ratioBlank.csv");

            maskedRatioA.SaveSpectrumAsCsv(eventLogger.LogDirectory, "maskedRatioA.csv");
            maskedRatioB.SaveSpectrumAsCsv(eventLogger.LogDirectory, "maskedRatioB.csv");
            maskedRatioC.SaveSpectrumAsCsv(eventLogger.LogDirectory, "maskedRatioC.csv");
            maskedRatioD.SaveSpectrumAsCsv(eventLogger.LogDirectory, "maskedRatioD.csv");

            temp1.SaveSpectrumAsCsv(eventLogger.LogDirectory, "temp1.csv");
            temp2.SaveSpectrumAsCsv(eventLogger.LogDirectory, "temp2.csv");


            result.SaveSpectrumAsCsv(eventLogger.LogDirectory, "rawSum.csv");
            resampledResult.SaveSpectrumAsCsv(eventLogger.LogDirectory, "Result.csv");

            #endregion







            Console.WriteLine();
            filterWheel.GoToPosition(6);
            eventLogger.Close();
        }
    }
}
