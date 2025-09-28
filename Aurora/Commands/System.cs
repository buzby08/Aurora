using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Aurora.Commands;

internal static class SystemCommands
{
    [DoesNotReturn]
    public static EofToken Exit(List<Token> positionals, Dictionary<string, Token> keywords, List<Ast> raw)
    {
        if (positionals.Count > 0)
            Errors.AlwaysThrow(new ArgumentSurplusError("System.exit does not take any positional arguments."));

        Dictionary<string, Type> expectedArguments = new()
        {
            { "statusCode", typeof(IntegerToken) },
            { "message", typeof(StringToken) }
        };

        Dictionary<string, Token?> arguments = Parsers.ParseArgs(positionals, keywords, expectedArguments, []);

        Token statusCode = arguments.GetValueOrDefault("statusCode") ?? new IntegerToken().Initialise(0);
        Token message = arguments.GetValueOrDefault("message") ?? new StringToken().Initialise("", withoutQuotes: true);


        Console.ForegroundColor = ConsoleColor.DarkRed;
        Writer.AddToQueue(message.ValueAsString);
        Writer.PushToStream();
        Console.ResetColor();
        Environment.Exit((int)statusCode.ValueAsInt.Value);
        return new EofToken();
    }

    [DoesNotReturn]
    public static EofToken Restart(List<Token> positionals, Dictionary<string, Token> keywords, List<Ast> raw)
    {
        Dictionary<string, Type> expectedArguments = new()
        {
            { "clearMemory", typeof(BooleanToken) }
        };
        Dictionary<string, Token?> arguments =
            Parsers.ParseArgs(positionals, keywords, expectedArguments, ["clearMemory"]);

        Token clearMemory = arguments.GetValueOrDefault("clearMemory") ?? new BooleanToken().Initialise(true);

        GlobalVariables.IncrementRecursionDepth();

        string[] code = GlobalVariables.Code;
        if (clearMemory.ValueAsBool)
        {
            Memory.Clear();
            Program.InitialiseMembers();
        }

        GlobalVariables.Code = code;
        GlobalVariables.LineNumber = 0;

        GlobalVariables.Evaluator.AllCode(GlobalVariables.Code);
        Environment.Exit(0);
        return new EofToken();
    }

    [DoesNotReturn]
    public static EofToken Reload(List<Token> positionals, Dictionary<string, Token> keywords, List<Ast> raw)
    {
        Dictionary<string, Type> expectedArguments = new()
        {
            { "clearMemory", typeof(BooleanToken) }
        };
        Dictionary<string, Token?> arguments =
            Parsers.ParseArgs(positionals, keywords, expectedArguments, ["clearMemory"]);

        Token clearMemory = arguments.GetValueOrDefault("clearMemory") ?? new BooleanToken().Initialise(true);

        GlobalVariables.IncrementRecursionDepth();

        string codeFilePath = GlobalVariables.CodeFilePath;
        
        Debugger.Break();
        if (clearMemory.ValueAsBool)
        {
            Memory.Clear();
            Program.InitialiseMembers();
            GlobalVariables.CodeFilePath = codeFilePath;
        }

        GlobalVariables.LineNumber = 0;
        
        string[] code = Program.ReadCode(codeFilePath);
        GlobalVariables.Code = code;
        GlobalVariables.Evaluator.AllCode(GlobalVariables.Code);
        Environment.Exit(0);
        return new EofToken();
    }
}