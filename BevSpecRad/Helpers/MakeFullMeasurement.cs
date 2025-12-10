using At.Matus.OpticalSpectrumLib;
using BevSpecRad.Helpers;
using System;

namespace BevSpecRad
{
    public partial class Program
    {
        internal static IOpticalSpectrum PerformABBAMeasurement(int filterIdx, double integrationTime, int nSamples)
        {
            eventLogger.LogEvent($"Starting ABBA measurement with filter index {filterIdx}, integration time {integrationTime}s, {nSamples} samples per measurement.");
            MeasuredOpticalSpectrum signal = new MeasuredOpticalSpectrum(spectro.Wavelengths);
            MeasuredOpticalSpectrum dark = new MeasuredOpticalSpectrum(spectro.Wavelengths);
            spectro.SetIntegrationTime(integrationTime);
            Console.WriteLine($"\nABBA sequence with filter index {filterIdx}");
            filterWheel.GoToPosition(filterIdx);

            // first A of ABBA Measurement Sequence
            shutter.Open();
            OnCallUpdateSpectrum(signal, nSamples, "A1 of ABBA");
            // first B of ABBA Measurement Sequence
            shutter.Close();
            OnCallUpdateSpectrum(dark, nSamples, "B1 of ABBA");
            // second B of ABBA Measurement Sequence
            shutter.Close();
            OnCallUpdateSpectrum(dark, nSamples, "B2 of ABBA");
            // second A of ABBA Measurement Sequence
            shutter.Open();
            OnCallUpdateSpectrum(signal, nSamples, "A2 of ABBA");

            OpticalSpectrum correctedSignal = SpecMath.Subtract(signal, dark);
            // TODO: update metadata of correctedSignal to indicate ABBA correction
            return correctedSignal;
        }

        internal static void OnCallUpdateSpectrum(MeasuredOpticalSpectrum spectrum, int numberSamples, string message)
        {
            Console.WriteLine($"Measurement of {message}...");
            ConsoleProgressBar consoleProgressBar = new ConsoleProgressBar();
            for (int i = 0; i < numberSamples; i++)
            {
                spectrum.UpdateSignal(spectro.GetIntensityData());
                consoleProgressBar.Report(i + 1, numberSamples);
            }
        }

    }
}
