using Bev.Instruments.ArraySpectrometer.Abstractions;
using Bev.Instruments.Thorlabs.Ctt;
using Bev.Instruments.Thorlabs.FW;
using BevSpecRad.Abstractions;
using BevSpecRad.Domain;
using BevSpecRad.Helpers;
using At.Matus.OpticalSpectrumLib;
using System;
using Bev.Instruments.Thorlabs.Ccs;
using Bev.Instruments.ArraySpectrometer.Domain;
using Bev.Instruments.OceanOptics.Usb2000;

namespace BevSpecRad
{
    public enum FilterPosition
    {
        FilterA = 1,
        FilterB = 2,
        FilterC = 3,
        FilterD = 4,
        Blank = 5,
        Closed = 6
    }

    public partial class Program
    {
        private static IArraySpectrometer spectro;
        private static IShutter shutter;
        private static IFilterWheel filterWheel;
        private static EventLogger eventLogger;

        // Run() is the new Main() after parsing command-line arguments
        private static void Run(Options options)
        {
            // instantiate instruments and logger
            eventLogger = new EventLogger(options.BasePath, options.LogFileName);
            filterWheel = new NullFilterWheel();
            //filterWheel = new MotorFilterWheel(options.FwPort);
            switch (options.SpecType)
            {
                case 1: // CCT
                    spectro = new ThorlabsCct();
                    shutter = new CctShutter((ThorlabsCct)spectro);
                    break;
                case 2: // CCS
                    spectro = new ThorlabsCcs(ProductID.CCS100, "M00928408");
                    shutter = new FilterWheelShutter(filterWheel, (int)FilterPosition.Closed);
                    break;
                case 3: // USB2000
                    spectro = new OceanOpticsUsb2000();
                    shutter = new FilterWheelShutter(filterWheel, (int)FilterPosition.Closed);
                    break;
                default:
                    break;
            }

            eventLogger.LogEvent($"Program: {GetAppNameAndVersion()}");
            eventLogger.LogEvent($"User comment: {options.UserComment}");
            LogSetupInfo();

            // TODO: read calibration data from file for standard lamp
            string fileName = "SN7-1108_FEL1000_2022.csv";
            var standardLampSpectrum = SpectralReader.ReadSpectrumFromCsv(System.IO.Path.Combine(options.BasePath, fileName));
            eventLogger.LogEvent($"Standard lamp calibration spectrum read from file: {fileName} ({standardLampSpectrum.GetMinimumWavelength()} nm - {standardLampSpectrum.GetMaximumWavelength()} nm)");

            // ask user to set up everything
            Console.WriteLine("Testing shutter and filter wheel");
            filterWheel.GoToPosition((int)FilterPosition.Closed);
            filterWheel.GoToPosition((int)FilterPosition.Blank); // Ensuring filter wheel is at position 5 also for manual filterwheels
            shutter.Close();
            shutter.Open();
            Console.WriteLine();

            UIHelper.WriteMessageAndWait(
                "==============================================================\n" +
                "Please set up the STANDARD LAMP and press any key to continue.\n" +
                "==============================================================\n");

            #region Define all datafields
            // integration times for each filter
            double intTimeA = options.IntTime;
            double intTimeB = options.IntTime;
            double intTimeC = options.IntTime;
            double intTimeD = options.IntTime;
            double intTime0 = options.IntTime   ;
            // spectra for standard lamp
            IOpticalSpectrum specStdA;
            IOpticalSpectrum specStdB;
            IOpticalSpectrum specStdC;
            IOpticalSpectrum specStdD;
            IOpticalSpectrum specStd0;
            // spectra for unknown lamp (DUT)
            IOpticalSpectrum specDutA;
            IOpticalSpectrum specDutB;
            IOpticalSpectrum specDutC;
            IOpticalSpectrum specDutD;
            IOpticalSpectrum specDut0;
            // control spectra for dark current subtraction
            IOpticalSpectrum specControlA;
            IOpticalSpectrum specControlB;
            IOpticalSpectrum specControlC;
            IOpticalSpectrum specControlD;
            IOpticalSpectrum specControl0;
            #endregion

            #region Get optimal integration times for each filter
            if (UIHelper.SkipAction("to determine optimal integration times for each filter (otherwise, the provided integration time will be used)"))
            {
                eventLogger.LogEvent("Skipping optimal integration time determination as per user request.");
                intTimeA = options.IntTime;
                intTimeB = options.IntTime;
                intTimeC = options.IntTime;
                intTimeD = options.IntTime;
                intTime0 = options.IntTime;
            }
            else
            {
                filterWheel.GoToPosition((int)FilterPosition.FilterA);
                intTimeA = spectro.GetOptimalExposureTime();
                eventLogger.LogEvent($"Optimal integration time for filter A: {intTimeA} s");
                filterWheel.GoToPosition((int)FilterPosition.FilterB);
                intTimeB = spectro.GetOptimalExposureTime();
                eventLogger.LogEvent($"Optimal integration time for filter B: {intTimeB} s");
                filterWheel.GoToPosition((int)FilterPosition.FilterC);
                intTimeC = spectro.GetOptimalExposureTime();
                eventLogger.LogEvent($"Optimal integration time for filter C: {intTimeC} s");
                filterWheel.GoToPosition((int)FilterPosition.FilterD);
                intTimeD = spectro.GetOptimalExposureTime();
                eventLogger.LogEvent($"Optimal integration time for filter D: {intTimeD} s");
                filterWheel.GoToPosition((int)FilterPosition.Blank);
                intTime0 = spectro.GetOptimalExposureTime();
                eventLogger.LogEvent($"Optimal integration time for blank filter: {intTime0} s");
            }
            #endregion

            #region Measure Standard Lamp
            // Now perform measurements on standard lamp
            specStdA = PerformABBAMeasurement((int)FilterPosition.FilterA, intTimeA, options.Nsamples);
            specStdB = PerformABBAMeasurement((int)FilterPosition.FilterB, intTimeB, options.Nsamples);
            specStdC = PerformABBAMeasurement((int)FilterPosition.FilterC, intTimeC, options.Nsamples);
            specStdD = PerformABBAMeasurement((int)FilterPosition.FilterD, intTimeD, options.Nsamples);
            specStd0 = PerformABBAMeasurement((int)FilterPosition.Blank, intTime0, options.Nsamples);

            specStdA.SaveSpectrumAsCsv(eventLogger.LogDirectory, "1_RawSpecStdA.csv");
            specStdB.SaveSpectrumAsCsv(eventLogger.LogDirectory, "1_RawSpecStdB.csv");
            specStdC.SaveSpectrumAsCsv(eventLogger.LogDirectory, "1_RawSpecStdC.csv");
            specStdD.SaveSpectrumAsCsv(eventLogger.LogDirectory, "1_RawSpecStdD.csv");
            specStd0.SaveSpectrumAsCsv(eventLogger.LogDirectory, "1_RawSpecStd0.csv");
            #endregion

            #region Measure DUT Lamp
            UIHelper.WriteMessageAndWait("\n==============================================================\n" +
                "Please set up the DUT LAMP and press any key to continue.\n" +
                "==============================================================");

            specDutA = PerformABBAMeasurement((int)FilterPosition.FilterA, intTimeA, options.Nsamples);
            specDutB = PerformABBAMeasurement((int)FilterPosition.FilterB, intTimeB, options.Nsamples);
            specDutC = PerformABBAMeasurement((int)FilterPosition.FilterC, intTimeC, options.Nsamples);
            specDutD = PerformABBAMeasurement((int)FilterPosition.FilterD, intTimeD, options.Nsamples);
            specDut0 = PerformABBAMeasurement((int)FilterPosition.Blank, intTime0, options.Nsamples);

            specDutA.SaveSpectrumAsCsv(eventLogger.LogDirectory, "2_RawSpecDutA.csv");
            specDutB.SaveSpectrumAsCsv(eventLogger.LogDirectory, "2_RawSpecDutB.csv");
            specDutC.SaveSpectrumAsCsv(eventLogger.LogDirectory, "2_RawSpecDutC.csv");
            specDutD.SaveSpectrumAsCsv(eventLogger.LogDirectory, "2_RawSpecDutD.csv");
            specDut0.SaveSpectrumAsCsv(eventLogger.LogDirectory, "2_RawSpecDut0.csv");

            #endregion

            #region Take Control Spectra
            // Now take control spectra (dark spectra) for all integration times
            Console.WriteLine("\nMeasurements on lamps done. Lamp can be shut down. Taking control (dark) spectra.");
            specControlA = PerformABBAControlMeasurement(intTimeA, options.Nsamples);
            specControlB = PerformABBAControlMeasurement(intTimeB, options.Nsamples);
            specControlC = PerformABBAControlMeasurement(intTimeC, options.Nsamples);
            specControlD = PerformABBAControlMeasurement(intTimeD, options.Nsamples);
            specControl0 = PerformABBAControlMeasurement(intTime0, options.Nsamples);

            specControlA.SaveSpectrumAsCsv(eventLogger.LogDirectory, "3_ControlSpecA.csv");
            specControlB.SaveSpectrumAsCsv(eventLogger.LogDirectory, "3_ControlSpecB.csv");
            specControlC.SaveSpectrumAsCsv(eventLogger.LogDirectory, "3_ControlSpecC.csv");
            specControlD.SaveSpectrumAsCsv(eventLogger.LogDirectory, "3_ControlSpecD.csv");
            specControl0.SaveSpectrumAsCsv(eventLogger.LogDirectory, "3_ControlSpec0.csv");

            var statControlA = specControlA.GetSignalStatistics();
            var statControlB = specControlB.GetSignalStatistics();
            var statControlC = specControlC.GetSignalStatistics();
            var statControlD = specControlD.GetSignalStatistics();
            var statControl0 = specControl0.GetSignalStatistics();

            Console.WriteLine();
            eventLogger.LogEvent($"Control Spectrum Stats Filter A: {statControlA.AverageValue:F6} +- {statControlA.StandardDeviation:F6}");
            eventLogger.LogEvent($"Control Spectrum Stats Filter B: {statControlB.AverageValue:F6} +- {statControlB.StandardDeviation:F6}");
            eventLogger.LogEvent($"Control Spectrum Stats Filter C: {statControlC.AverageValue:F6} +- {statControlC.StandardDeviation:F6}");
            eventLogger.LogEvent($"Control Spectrum Stats Filter D: {statControlD.AverageValue:F6} +- {statControlD.StandardDeviation:F6}");
            eventLogger.LogEvent($"Control Spectrum Stats Filter 0: {statControl0.AverageValue:F6} +- {statControl0.StandardDeviation:F6}");

            #endregion

            #region Calculate some stuff
            Console.WriteLine("Evaluating signal ratio ...");

            // Quotient spectra DUT/STD per filter (after dark subtraction)
            var ratioA = SpecMath.Ratio(specDutA, specStdA);
            var ratioB = SpecMath.Ratio(specDutB, specStdB);
            var ratioC = SpecMath.Ratio(specDutC, specStdC);
            var ratioD = SpecMath.Ratio(specDutD, specStdD);
            var ratio0 = SpecMath.Ratio(specDut0, specStd0);

            ratioA.SaveSpectrumAsCsv(eventLogger.LogDirectory, "4_ratioA.csv");
            ratioB.SaveSpectrumAsCsv(eventLogger.LogDirectory, "4_ratioB.csv");
            ratioC.SaveSpectrumAsCsv(eventLogger.LogDirectory, "4_ratioC.csv");
            ratioD.SaveSpectrumAsCsv(eventLogger.LogDirectory, "4_ratioD.csv");
            ratio0.SaveSpectrumAsCsv(eventLogger.LogDirectory, "4_ratio0.csv");

            // Apply bandpass masks to each ratio spectrum
            // the cut-off wavelengths are determined using actual spectra
            // they are hard coded here for simplicity
            var maskedRatioA = ratioA.ApplyBandpassMask(180, 464, 10, 10);
            var maskedRatioB = ratioB.ApplyBandpassMask(464, 545, 10, 10);
            var maskedRatioC = ratioC.ApplyBandpassMask(545, 658, 10, 10);
            var maskedRatioD = ratioD.ApplyBandpassMask(658, 2000, 10, 10);

            maskedRatioA.SaveSpectrumAsCsv(eventLogger.LogDirectory, "4_maskedRatioA.csv");
            maskedRatioB.SaveSpectrumAsCsv(eventLogger.LogDirectory, "4_maskedRatioB.csv");
            maskedRatioC.SaveSpectrumAsCsv(eventLogger.LogDirectory, "4_maskedRatioC.csv");
            maskedRatioD.SaveSpectrumAsCsv(eventLogger.LogDirectory, "4_maskedRatioD.csv");

            // Sum all masked ratio spectra
            var combinedRatio = SpecMath.Add(maskedRatioA, maskedRatioB, maskedRatioC, maskedRatioD);
            combinedRatio.SaveSpectrumAsCsv(eventLogger.LogDirectory, "5_combinedRatio.csv");

            eventLogger.LogEvent("Signal ratio evaluation done.");

            double fromWl = 350;
            double toWl = 700;
            double stepWl = 1;

            eventLogger.LogEvent($"Resampling and final calibration ({fromWl} - {toWl} @ {stepWl}) nm");
            var correctedLampRatioResampled = combinedRatio.ResampleSpectrum(fromWl, toWl, stepWl);
            var simpleLampRatioResampled = ratio0.ResampleSpectrum(fromWl, toWl, stepWl);
            var standardLampResampled = standardLampSpectrum.ResampleSpectrum(fromWl, toWl, stepWl);
            var calibratedSpectrumCorrected = SpecMath.Multiply(correctedLampRatioResampled, standardLampResampled);
            correctedLampRatioResampled.SaveSpectrumAsCsv(eventLogger.LogDirectory, "6_correctedLampRatio_resampled.csv");
            simpleLampRatioResampled.SaveSpectrumAsCsv(eventLogger.LogDirectory, "6_simpleLampRatio_resampled.csv");
            calibratedSpectrumCorrected.SaveSpectrumAsCsv(eventLogger.LogDirectory, "6_calibratedSpectrum_corrected.csv");
            standardLampResampled.SaveSpectrumAsCsv(eventLogger.LogDirectory, "6_standardLamp_resampled.csv");
            #endregion

            Console.WriteLine();
            filterWheel.GoToPosition(5);
            eventLogger.Close();
        }
    }
}
