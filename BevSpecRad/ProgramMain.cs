using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace BevSpecRad
{
    public partial class Program
    {
        static void Main(string[] args)
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            Parser parser = new Parser(with => with.HelpWriter = null);
            ParserResult<Options> parserResult = parser.ParseArguments<Options>(args);
            parserResult
                .WithParsed<Options>(options => Run(options))
                .WithNotParsed(errs => DisplayHelp(parserResult, errs));
        }

        private static void DisplayHelp<T>(ParserResult<T> result, IEnumerable<Error> errs)
        {
            string appName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            HelpText helpText = HelpText.AutoBuild(result, h =>
            {
                h.AutoVersion = false;
                h.AdditionalNewLineAfterOption = false;
                h.AddPreOptionsLine("\nProgram to ...");
                h.AddPreOptionsLine("");
                h.AddPreOptionsLine($"Usage: {appName} InputPath [OutPath] [options]");
                return HelpText.DefaultParsingErrorsHandler(result, h);
            }, e => e);
            Console.WriteLine(helpText);
        }

        private static void LogSetupInfo()
        {
            eventLogger.WriteLine("=== Instrument Information ===");
            eventLogger.Write(UIHelper.FormatSpectrometerInfo(spectro));
            eventLogger.WriteLine(UIHelper.FormatShutterInfo(shutter));
            eventLogger.Write(UIHelper.FormatFilterWheelInfo(filterWheel));
            eventLogger.WriteLine("==============================");
        }

        private static void DisplaySetupInfo()
        {
            Console.WriteLine(UIHelper.FormatSpectrometerInfo(spectro));
            Console.WriteLine(UIHelper.FormatShutterInfo(shutter));
            Console.WriteLine(UIHelper.FormatFilterWheelInfo(filterWheel));
        }

    }
}
