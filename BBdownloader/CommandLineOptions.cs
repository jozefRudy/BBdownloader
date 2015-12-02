using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;


namespace BBdownloader
{
    class CommandLineOptions
    {
        [Option("noDownload", HelpText = "Do not download data from bloomberg")]
        public bool NoDownload { get; set; }

        [Option("noUpload", HelpText = "Do not upload data to server")]
        public bool NoUpload { get; set; }

        [Option("dir", DefaultValue = "data", HelpText = "Directory for saving data")]
        public string Dir { get; set; }

        [Option("settings", DefaultValue = "settings.cfg", HelpText = "Settings file name")]
        public string Settings { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this, current => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}
