using CommandLine;
using CommandLine.Text;

namespace bloomberg_downloader
{
    internal class CommandLineOptions
    {
        [Option('d', "dateId", HelpText = "dateId = 20131018")]
        public int? DateId { get; set; }

        [Option('h', "host", DefaultValue = "localhost", HelpText = "bloomberg host = localhost")]
        public string Host { get; set; }

        [Option('p', "port", DefaultValue = 3194, HelpText = "bloomberg port = 3194")]
        public int Port { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this,
              current => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}