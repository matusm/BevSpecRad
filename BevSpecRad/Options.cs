using CommandLine;

namespace BevSpecRad
{
    internal class Options
    {
        [Option('n', "number", Default = 10, Required = false, HelpText = "Number of spectra per acquisition.")]
        public int Nsamples { get; set; }

        [Option('t', "inttime", Default = 0.001, Required = false, HelpText = "Integration time in seconds.")]
        public double IntTime { get; set; }

        [Option("comment", Default = "---", Required = false, HelpText = "User supplied comment text.")]
        public string UserComment { get; set; }

        [Option("fwport", Default = "COM3", Required = false, HelpText = "Filter wheel serial port.")] // photometry lab, COM1 for development computer
        public string FwPort { get; set; }

        [Option("basepath", Default = @"C:\temp\BevSpecIrrad", Required = false, HelpText = "Base path for result directories.")]
        public string BasePath { get; set; }

        [Option("logfile", Default = @"BevSpecIrradLog.txt", Required = false, HelpText = "File name for logging.")]
        public string LogFileName { get; set; }

        [Option('s', "spectrometer", Default = 1, Required = false, HelpText = "Spectrometer type (see doc for usage).")]
        public int SpecType { get; set; }
        // 1: Thorlabs CCT
        // 2: Thorlabs CCS
        // 3: USB2000

        [Option("control", Default = false, Required = false, HelpText = "Perform control measurement after calibration")]
        public bool Control { get; set; }

        [Value(0, MetaName = "InputPath", Required = false, HelpText = "Standard lamp calibration filename")]
        public string InputPath { get; set; }

        [Value(1, MetaName = "OutputPath", Required = false, HelpText = "Result filename including path")]
        public string OutputPath { get; set; }

    }
}