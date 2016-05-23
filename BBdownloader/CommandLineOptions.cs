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

        [Option("fieldInfoDir", DefaultValue = "FieldInfo", HelpText = "Directory for field info")]
        public string FieldInfoDir { get; set; }

        [Option("settings", DefaultValue = "settings.cfg", HelpText = "Settings file name")]
        public string Settings { get; set; }

        [Option("logging", DefaultValue = "log.txt", HelpText = "Full logfile path")]
        public string LogFile { get; set; }

        [OptionList("startDate", DefaultValue = new string[] { "2005", "1", "1" }, HelpText = "Start downloading from", Separator = '.', Required = false)]
        public IList<string> startDate { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this, current => HelpText.DefaultParsingErrorsHandler(this, current));
        }

        public override string  ToString()
        {
            return "options: --noDownload:" + NoDownload.ToString() + " --noUpload:" + NoUpload.ToString() + " --dir:" + Dir.ToString() + " --settings:" + Settings.ToString() + " --logging:" + LogFile.ToString();
        }

    }
}
