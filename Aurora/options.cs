using CommandLine;

namespace Aurora
{
    public class Options
    {
        [Value(0, MetaName = "file", Required = true, HelpText = "Input file to be processed")]
        public required string FilePath { get; set; }

        [Option("version", HelpText = "Display version information")]
        public bool Version { get; set; }

        [Option('v', "verbose", Default = false, HelpText = "Prints verbose log messages")]
        public bool Verbose { get; set; }

        [Option('d', "debug", Default = false, HelpText = "Prints debug log messages")]
        public bool Debug { get; set; }

        [Option('w', "warn", Default = false, HelpText = "Prints warning log messages")]
        public bool Warning { get; set; }

        [Option("no-console", Default = false, HelpText = "Suppresses log messages to the terminal")]
        public bool NoConsole { get; set; }

        [Option("logfile", Default = "aurora.LOG", HelpText = "The file to output the log messages to")]
        public string? LogFile { get; set; }

        [Option('s', "strict", Default = false, HelpText = "Only displays the log messages specified from flags")]
        public bool Strict { get; set; }

        [Option("config-file", Default = null, HelpText = "The config file for the aurora interpreter")]
        public string? ConfigFile { get; set; }

        public Options()
        {
        }
    }
}