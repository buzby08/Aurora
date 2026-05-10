using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;

namespace Aurora;

public static class CommandLineArguments
{
    public static string? File;
    public static bool Verbose;
    public static bool Debug;
    public static bool Warn;
    public static bool NoConsole;
    public static string? ConfigFile;
    public static bool Strict;
    public static bool DisableEasterEggs;
    public static string? LogFile;


    static CommandLineArguments()
    {
        Argument<string> fileArg = new("file")
        {
            Description = "Input file to run",
        };

        Option<bool> verboseOption = new("--verbose", "-v")
        {
            Description = "Prints verbose log messages",
        };

        Option<bool> debugOption = new("--debug", "-d")
        {
            Description = "Prints debug log messages",
        };

        Option<bool> warnOption = new("--warn", "-w")
        {
            Description = "Prints warning log messages",
        };

        Option<bool> noConsoleOption = new("--no-console")
        {
            Description = "Suppresses log messages to the console",
        };

        Option<string> logFileOption = new("--log-file")
        {
            Description = "The file to output log messages to",
        };

        Option<bool> strictOption = new("--strict", "-s")
        {
            Description = "Only displays the log messages specified from flags",
        };

        Option<bool> disableEasterEggsOption = new("--disable-easter-eggs")
        {
            Description = "Disables the Easter eggs hidden within the Aurora interpreter",
        };

        Option<string> configFileOption = new("--config-file")
        {
            Description = "The configuration file for the aurora interpreter",
        };

        RootCommand rootCommand = new("Aurora interpreter")
        {
            fileArg,
            verboseOption,
            debugOption,
            warnOption,
            noConsoleOption,
            logFileOption,
            strictOption,
            disableEasterEggsOption,
            configFileOption,
        };

        rootCommand.SetAction(Handle);

        RootCommand = rootCommand;
    }

    public static void HandleArgs(string[] args)
    {
        int returnVal = RootCommand.Parse(args).Invoke();

        if (!_hasBeenHandled)
            Environment.Exit(returnVal);
    }

    private static readonly RootCommand RootCommand;
    private static bool _hasBeenHandled;

    private static void Handle(ParseResult parseResult)
    {
        _hasBeenHandled = true;

        File = parseResult.GetRequiredValue<string>("file");
        Verbose = parseResult.GetValue<bool>("--verbose");
        Debug = parseResult.GetValue<bool>("--debug");
        Warn = parseResult.GetValue<bool>("--warn");
        NoConsole = parseResult.GetValue<bool>("--no-console");
        ConfigFile = parseResult.GetValue<string>("--config-file");
        Strict = parseResult.GetValue<bool>("--strict");
        LogFile = parseResult.GetValue<string>("--log-file");
        DisableEasterEggs = parseResult.GetValue<bool>("--disable-easter-eggs");

        if (!DisableEasterEggs && File == "nothing")
        {
            Console.WriteLine("You've run nothing. And yet... something happened. Think about it.");
            Environment.Exit(-1);
        }

        if (!DisableEasterEggs && File == "missing.aur")
        {
            Errors.AlwaysThrow(new FileNotFoundError("404: File intentionally not found."),
                InternalVariables.GlobalContext);
        }

        Errors.ConfigFilePath = string.IsNullOrEmpty(ConfigFile) ? Errors.ConfigFilePath : ConfigFile;

        InternalVariables.StrictFlagMode = Strict;

        if (!string.IsNullOrEmpty(ConfigFile))
        {
            // UserConfiguration.ApplyConfiguration(ConfigFile);
        }

        if (NoConsole) Logs.NoConsole = true;

        if (Debug)
        {
            Logs.AllowDebug = true;
            Logs.Debug("Debug messages enabled");

            Verbose = !Strict || Verbose;
        }

        if (Verbose)
        {
            Logs.AllowVerbose = true;
            Logs.Verbose("Verbose messages enabled");

            Warn = !Strict || Warn;
        }

        if (Warn)
        {
            Logs.AllowWarning = true;
            Logs.Warning("Warning messages enabled");
        }

        if (DisableEasterEggs)
        {
            InternalVariables.EasterEggs = false;
            Logs.Warning("Easter eggs disabled");
        }

        if (!string.IsNullOrEmpty(LogFile)) Logs.LogFilePath = LogFile;
    }
}

// public class Options
// {
//     [Value(0, MetaName = "file", Required = true, HelpText = "Input file to be processed")]
//     public required string FilePath { get; set; }
// 
//     [Option("version", HelpText = "Display version information")]
//     public bool Version { get; set; }
// 
//     [Option('v', "verbose", Default = false, HelpText = "Prints verbose log messages")]
//     public bool Verbose { get; set; }
// 
//     [Option('d', "debug", Default = false, HelpText = "Prints debug log messages")]
//     public bool Debug { get; set; }
// 
//     [Option('w', "warn", Default = false, HelpText = "Prints warning log messages")]
//     public bool Warning { get; set; }
// 
//     [Option("no-console", Default = false, HelpText = "Suppresses log messages to the terminal")]
//     public bool NoConsole { get; set; }
// 
//     [Option("logfile", HelpText = "The file to output the log messages to")]
//     public string? LogFile { get; set; }
// 
//     [Option('s', "strict", Default = false, HelpText = "Only displays the log messages specified from flags")]
//     public bool Strict { get; set; }
// 
//     [Option("inline-stack-trace", Default = false, HelpText = "Prints inline stack traces for system errors")]
//     public bool InlineStackTrace { get; set; }
// 
//     [Option("disable-easter-eggs", Default = false,
//         HelpText = "Disables the easter eggs hidden within the Aurora interpreter")]
//     public bool DisableEasterEggs { get; set; }
// 
//     [Option("config-file", Default = null, HelpText = "The config file for the aurora interpreter")]
//     public string? ConfigFile { get; set; }
// }