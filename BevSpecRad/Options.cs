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

        [Option("basepath", Default = @"C:\temp\BevSpecRad", Required = false, HelpText = "Base path for result directories.")]
        public string BasePath { get; set; }

        [Value(0, MetaName = "InputPath", Required = false, HelpText = "Input filename including path")]
        public string InputPath { get; set; }

        [Value(1, MetaName = "OutputPath", Required = false, HelpText = "Result filename including path")]
        public string OutputPath { get; set; }
    }
}