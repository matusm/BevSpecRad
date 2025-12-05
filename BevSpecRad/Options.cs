using CommandLine;

namespace BevSpecRad
{
    internal class Options
    {
        // Supports: -n 5  (also provides --number as an additional alias)
        [Option('n', "number", Default = 5, Required = false, HelpText = "Number of spectra per acquisition.")]
        public int N { get; set; }

        // Supports: --comment "text"
        [Option("comment", Required = false, HelpText = "User supplied comment text.")]
        public string UserComment { get; set; }

        [Value(0, MetaName = "InputPath", Required = false, HelpText = "Input filename including path")]
        public string InputPath { get; set; }

        [Value(1, MetaName = "OutputPath", Required = false, HelpText = "Result filename including path")]
        public string OutputPath { get; set; }
    }
}