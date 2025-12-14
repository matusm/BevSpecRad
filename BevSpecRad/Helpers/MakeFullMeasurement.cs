using At.Matus.OpticalSpectrumLib;
using BevSpecRad.Helpers;
using System;

namespace BevSpecRad
{
    public partial class Program
    {
        internal static IOpticalSpectrum PerformABBAMeasurement(int filterIdx, double integrationTime, int nSamples)
        {
            Console.WriteLine();
            spectro.SetIntegrationTime(integrationTime);
            filterWheel.GoToPosition(filterIdx);
            eventLogger.LogEvent($"ABBA sequence, filter index {filterIdx}, integration time {spectro.GetIntegrationTime()} s, {nSamples} samples.");
            MeasuredOpticalSpectrum signal = new MeasuredOpticalSpectrum(spectro.Wavelengths);
            MeasuredOpticalSpectrum dark = new MeasuredOpticalSpectrum(spectro.Wavelengths);

            // first A of ABBA Measurement Sequence
            shutter.Open();
            OnCallUpdateSpectrum(signal, nSamples, "first A of ABBA");
            // first B of ABBA Measurement Sequence
            shutter.Close();
            OnCallUpdateSpectrum(dark, nSamples, "first B of ABBA");
            // second B of ABBA Measurement Sequence
            shutter.Close();
            OnCallUpdateSpectrum(dark, nSamples, "second B of ABBA");
            // second A of ABBA Measurement Sequence
            shutter.Open();
            OnCallUpdateSpectrum(signal, nSamples, "second A of ABBA");

            OpticalSpectrum correctedSignal = SpecMath.Subtract(signal, dark);
            // TODO: update metadata of correctedSignal to indicate ABBA correction
            return correctedSignal;
        }

        internal static IOpticalSpectrum PerformABBAControlMeasurement(double integrationTime, int nSamples)
        {
            Console.WriteLine();
            spectro.SetIntegrationTime(integrationTime);
            eventLogger.LogEvent($"ABBA sequence, control with closed shutter, integration time {spectro.GetIntegrationTime()} s, {nSamples} samples.");
            MeasuredOpticalSpectrum signal = new MeasuredOpticalSpectrum(spectro.Wavelengths);
            MeasuredOpticalSpectrum dark = new MeasuredOpticalSpectrum(spectro.Wavelengths);

            shutter.Close();
            // first A of ABBA Measurement Sequence
            OnCallUpdateSpectrum(signal, nSamples, "first A of ABBA");
            // first B of ABBA Measurement Sequence
            OnCallUpdateSpectrum(dark, nSamples, "first B of ABBA");
            // second B of ABBA Measurement Sequence
            OnCallUpdateSpectrum(dark, nSamples, "second B of ABBA");
            // second A of ABBA Measurement Sequence
            OnCallUpdateSpectrum(signal, nSamples, "second A of ABBA");
            shutter.Open();
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
