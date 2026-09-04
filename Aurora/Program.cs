#define TESTING
using System.Reflection;
using Aurora.Core;
using Aurora.Parser;
using Aurora.Evaluator;
using Aurora.Evaluator.Internals;

namespace Aurora;

public static class Program
{
    private static string ReadCode(string filePath /*, RuntimeContext context*/)
    {
        if (string.IsNullOrEmpty(filePath))
            Errors.RaiseError(new FileNotFoundError("Please provide a file path to execute"),
                null);

        if (!File.Exists(filePath))
            Errors.RaiseError(new FileNotFoundError($"The file - {filePath} - was not found"),
                null);

        if (!filePath.EndsWith(".aur"))
            InternalVariables.GlobalLogger.Warning(
                "Aurora code should be written in an aurora file (ending with .aur).");

        // context.Create("__SCRIPT__", new StringObject(filePath));
        return File.ReadAllText(filePath);
    }

    private static void HandleArgumentEasterEggs(string[] args)
    {
        if (CommandLineArguments.DisableEasterEggs) return;

        if (args.Contains("--supercalifragalisticexpialidocious")) MaryPoppins.Supercalifragalisticexpialidocious();

        if (args.Contains("--teapot"))
        {
            Console.WriteLine("418: Im a teapot");
            Environment.Exit(418);
        }

        if (args.Contains("--help-me"))
        {
            Console.WriteLine("It looks like you're trying to code. Would you like assistance from Clippy?");
            Environment.Exit(0);
        }

        if (args.Contains("--praise"))
        {
            Console.WriteLine(
                "You're doing amazing. Your code isn't perfect, but neither is the moon, and it still controls the tides.");
            Environment.Exit(0);
        }
    }

    private static void AttachBuiltinsToGlobalContext(RuntimeContext globalContext)
    {
        foreach (FieldInfo property in typeof(Builtins).GetFields())
        {
            // if (property.GetType() != typeof(RuntimeObject))
            //     continue;

            globalContext.Create(property.Name, (RuntimeObject)property.GetValue(null)!, null);
        }
    }

    public static void Main(string[] args)
    {
#if OWL
        Owl.Show();
        Environment.Exit(0);
#endif

        Logger logger = new("Program.cs");

        try
        {
            CommandLineArguments.HandleArgs(args);
            string filePath = CommandLineArguments.File!;

            HandleArgumentEasterEggs(args);

            RuntimeContext.CreateGlobalContext(filePath);
            Builtins.InitialiseTypes();
            AttachBuiltinsToGlobalContext(RuntimeContext.GlobalContext!);

            string code = ReadCode(filePath /*, InternalVariables.GlobalContext*/);
            InternalVariables.Code = code;

            Tokenizer tokenizer = new()
            {
                Text = code,
                FilePath = filePath,
            };

#if TESTING

            Parser.Parser parser = new(tokenizer);
            List<List<Ast>> expressions = parser.Parse();

            Evaluator.Evaluator evaluator = new(RuntimeContext.GlobalContext!);
            evaluator.EvaluateMultipleExpressions(expressions);

            // Todo: Add test and fix nested blocks. E.g. Logic.if(true; {Logic.if(true; {Terminal.writeLine("Hey)})})

            logger.Info("Program finished.");
#endif

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
                new SystemError("SE_001" + (InternalVariables.InlineStackTrace ? fullError : e.Message)),
                null);
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
