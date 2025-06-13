#define OWL
using CommandLine;
namespace Aurora;

internal static class Program
{
    private static string[] ReadCode(string filePath)
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
            GlobalVariables.LOGGER.Warning("Aurora code should be written in an aurora file (ending with .aur).");
        }
        
        Variables.SYSTEM_DEFINED.Add("__SCRIPT__", new StringToken().Initialise(filePath, withoutQuotes: true));
        return File.ReadAllLines(filePath);
    }

    public static void ApplyOptions(bool noConsole, bool debug, bool verbose, bool warning, bool strict)
    {
        if (noConsole)
        {
            GlobalVariables.LOGGER.NoConsole = true;
        }

        if (debug)
        {
            GlobalVariables.LOGGER.AllowDebug = true;
            GlobalVariables.LOGGER.Debug("Debug messages enabled");

            verbose = !strict || verbose;
        }

        if (verbose)
        {
            GlobalVariables.LOGGER.AllowVerbose = true;
            GlobalVariables.LOGGER.Verbose("Verbose messages enabled");

            warning = !strict || warning;
        }

        if (warning)
        {
            GlobalVariables.LOGGER.AllowWarning = true;
            GlobalVariables.LOGGER.Warning("Warning messages enabled");
        }
    }

    private static void RunOptionsAndReturnExitCode(Options opts)
    {
        GlobalVariables.CodeFilePath = opts.FilePath;

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
        
        GlobalVariables.StrictFlagMode = opts.Strict;
        if (!string.IsNullOrEmpty(opts.LogFile))
        {
            GlobalVariables.LOGGER.LogFilePath = opts.LogFile;
        }
        
        if (!string.IsNullOrEmpty(opts.ConfigFile))
        {
            UserConfiguration.ApplyConfiguration(opts.ConfigFile);
        }

        ApplyOptions(opts.NoConsole, opts.Debug, opts.Verbose, opts.Warning, opts.Strict);            
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
            GlobalVariables.LOGGER.ForceLog($"System Error: {err}");
        }

        Errors.RaiseError(new ConfigurationError("The system encountered an error it could not handle",
            user: false));
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
        
        
        Classes.RegisterSystemClasses();
        
        try
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(RunOptionsAndReturnExitCode)
                .WithNotParsed(HandleParseError);
            
            string[] code = ReadCode(GlobalVariables.CodeFilePath);
            Evaluate.AllCode(code);
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
                new SystemError(e.Message));
        }
        
        
    }
}
