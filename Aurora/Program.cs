#define TESTING
using Aurora.BuiltinMethods;
using Aurora.Internals;

namespace Aurora;

public static class Program
{
    private static string ReadCode(string filePath, RuntimeContext context)
    {
        if (string.IsNullOrEmpty(filePath))
            Errors.RaiseError(new FileNotFoundError("Please provide a file path to execute"),
                InternalVariables.GlobalContext);

        if (!File.Exists(filePath))
            Errors.RaiseError(new FileNotFoundError($"The file - {filePath} - was not found"),
                InternalVariables.GlobalContext);

        if (!filePath.EndsWith(".aur"))
            Logs.Warning("Aurora code should be written in an aurora file (ending with .aur).");

        context.Create("__SCRIPT__", new StringObject(filePath));
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
        InternalVariables.GlobalContext.Create("Math", Builtins.Math);
    }

    public static void Main(string[] args)
    {
#if OWL
        Owl.Show();
        Environment.Exit(0);
#endif

        try
        {
            Builtins.InitialiseTypes();
            AttachBuiltinsToGlobalContext();

            CommandLineArguments.HandleArgs(args);

            HandleArgumentEasterEggs(args);

            InternalVariables.CodeFilePath = CommandLineArguments.File!;

            InternalVariables.GlobalContext.FileName = InternalVariables.CodeFilePath;
            InternalVariables.GlobalContext.ShowInStackTrace = false;

            string code = ReadCode(InternalVariables.CodeFilePath, InternalVariables.GlobalContext);
            InternalVariables.Code = code;

            InternalVariables.LineNumber = 0;

#if TESTING

            string testCode = File.ReadAllText("code.aur");

            ParserRework parser = new(testCode);
            List<List<AstRework>> expressions = parser.Parse();

            foreach (List<AstRework> expression in expressions)
            {
                Console.WriteLine("----------------------------------------");
                foreach (AstRework ast in expression)
                    Console.WriteLine(ast.ToString());
            }


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
                InternalVariables.GlobalContext);
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
