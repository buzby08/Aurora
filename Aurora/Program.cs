using System.Reflection;
using Aurora.BuiltinMethods;
using Aurora.Internals;
using CommandLine;
using CommandLine.Text;

namespace Aurora;

public static class Program
{
    private static string[] ReadCode(string filePath, RuntimeContext context)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            Errors.RaiseError(new FileNotFoundError("Please provide a file path to execute"));
        }

        if (!File.Exists(filePath))
        {
            Errors.RaiseError(new FileNotFoundError($"The file - {filePath} - was not found"));
        }

        if (!filePath.EndsWith(".aur"))
        {
            Logs.Warning("Aurora code should be written in an aurora file (ending with .aur).");
        }

        context.Create("__SCRIPT__", new StringObject(filePath));
        return File.ReadAllLines(filePath);
    }

    public static void ApplyOptions(bool noConsole, bool debug, bool verbose, bool warning, bool strict,
        bool inlineStackTrace, bool disableEasterEggs, string? logFile)
    {
        if (noConsole)
        {
            Logs.NoConsole = true;
        }

        if (debug)
        {
            Logs.AllowDebug = true;
            Logs.Debug("Debug messages enabled");

            verbose = !strict || verbose;
        }

        if (verbose)
        {
            Logs.AllowVerbose = true;
            Logs.Verbose("Verbose messages enabled");

            warning = !strict || warning;
        }

        if (warning)
        {
            Logs.AllowWarning = true;
            Logs.Warning("Warning messages enabled");
        }

        if (inlineStackTrace)
        {
            InternalVariables.InlineStackTrace = true;
            Logs.Warning("Inline stack trace messages enabled");
        }

        if (disableEasterEggs)
        {
            InternalVariables.EasterEggs = false;
            Logs.Warning("Easter eggs disabled");
        }

        if (!string.IsNullOrEmpty(logFile))
        {
            Logs.LogFilePath = logFile;
        }
    }

    private static void RunOptionsAndReturnExitCode(Options opts)
    {
        if (opts.Version)
        {
            var version = Assembly
                .GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                .InformationalVersion ?? "Unknown";

            Console.WriteLine($"Aurora version {version}");
            Environment.Exit(0);
        }

        InternalVariables.CodeFilePath = opts.FilePath;

        if (opts.FilePath == "nothing")
        {
            Console.WriteLine("You’ve run nothing. And yet... something happened. Think about it.");
            Environment.Exit(-1);
        }

        if (opts.FilePath == "missing.aur")
        {
            Errors.AlwaysThrow(new FileNotFoundError("404: File intentionally not found."));
        }

        Errors.ConfigFilePath = string.IsNullOrEmpty(opts.ConfigFile) ? Errors.ConfigFilePath : opts.ConfigFile;

        InternalVariables.StrictFlagMode = opts.Strict;

        if (!string.IsNullOrEmpty(opts.ConfigFile))
        {
            // UserConfiguration.ApplyConfiguration(opts.ConfigFile);
        }

        ApplyOptions(opts.NoConsole, opts.Debug, opts.Verbose, opts.Warning, opts.Strict,
            opts.InlineStackTrace, opts.DisableEasterEggs, opts.LogFile);
    }

    private static void HandleParseError(IEnumerable<Error> errs)
    {
        errs = errs.ToList();

        if (errs.IsHelp() || errs.IsVersion())
        {
            Environment.Exit(0);
        }

        foreach (var err in errs)
        {
            Logs.ForceLog($"System Error: {err}");
        }

        Errors.RaiseError(new ConfigurationError("The system encountered an error it could not handle",
            user: false));
    }

    private static void AttachBuiltinsToGlobalContext()
    {
        InternalVariables.GlobalContext.Create("Type", Builtins.Type);
        InternalVariables.GlobalContext.Create("Null", Builtins.Null);
        InternalVariables.GlobalContext.Create("Unit", Builtins.Unit);
        InternalVariables.GlobalContext.Create("Int", Builtins.Int);
        InternalVariables.GlobalContext.Create("Float", Builtins.Float);
        InternalVariables.GlobalContext.Create("String", Builtins.String);
        InternalVariables.GlobalContext.Create("Boolean", Builtins.Boolean);
        InternalVariables.GlobalContext.Create("Terminal", Builtins.Terminal);
        InternalVariables.GlobalContext.Create("BooleanOutputStyles", Builtins.BooleanOutputStyles);
        InternalVariables.GlobalContext.Create("Optional", Builtins.Optional);
    }

    public static void Main(string[] args)
    {
#if TESTING
        Test.isTesting = true;
        Test.Main();
        Environment.Exit(0);
#endif

#if OWL
        Owl.Show();
        Environment.Exit(0);
#endif

        if (args.Contains("--supercalifragalisticexpialidocious"))
        {
            MaryPoppins.Supercalifragalisticexpialidocious();
        }

        if (args.Contains("--teapot"))
        {
            Console.WriteLine("418: Im a teapot");
            Environment.Exit(418);
        }

        if (args.Contains("--help-me"))
        {
            Console.WriteLine("It looks like you're trying to code. Would you like assistance from Clippy?");
            Environment.Exit(-1);
        }

        if (args.Contains("--praise"))
        {
            Console.WriteLine(
                "You're doing amazing. Your code isn't perfect, but neither is the moon, and it still controls the tides.");
            Environment.Exit(0);
        }

        try
        {
            Builtins.InitialiseTypes();
            AttachBuiltinsToGlobalContext();

            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(RunOptionsAndReturnExitCode)
                .WithNotParsed(HandleParseError);

            string[] code = ReadCode(InternalVariables.CodeFilePath, InternalVariables.GlobalContext);
            InternalVariables.Code = code;

            InternalVariables.LineNumber = 0;
            Evaluator.EvaluateAllCode(code, InternalVariables.GlobalContext);

            Errors.OutputWarningsAndExit();
        }
        catch (Exception e)
        {
            string fullError =
                $"\nError message: {e.Message}\n"
                + "--"
                + $"Stack trace:\n{e.StackTrace}\n"
                + "--\n"
                + $"Source: {e.Source}\n"
                + $"TargetSite: {e.TargetSite}\n";

            Errors.Log("System Error", fullError);
            Errors.RaiseError(
                new SystemError(InternalVariables.InlineStackTrace ? fullError : e.Message));
        }
    }
}

// namespace Aurora;
//
// public static class Program
// {
//     public static int LineNumber = 1;
//
//     public static void Main()
//     {
//         
//     }
// }