using System.Collections.Immutable;

namespace Aurora.Commands;

internal static class Terminal
{
    public static StringToken Color { get; private set; } =
        new StringToken().Initialise(Colors.White, withoutQuotes: true, interpolate: false);

    public static Token Clear(List<Token> positionals, Dictionary<string, Token> keywords)
    {
        if (positionals.Count > 0 || keywords.Count > 0)
            Errors.RaiseError(new ArgumentSurplusError("Terminal.clear does not take any arguments"));

        Console.Clear();
        GlobalVariables.LOGGER.Debug("(Screen Cleared)");

        return new NullToken();
    }

    public static Token Write(List<Token> positionals, Dictionary<string, Token> keywords)
    {
        Token endCharToken =
            keywords.GetValueOrDefault("end", new StringToken().Initialise("\n", withoutQuotes: true));
        if (endCharToken is not StringToken)
            Errors.RaiseError(new TypeMismatchError("Terminal.Write 'end' argument requests a string"));

        GlobalVariables.LOGGER.Debug($"positionals = {PrettyPrint.TokenList(positionals, output: false)}");

        string endChar = endCharToken.ValueAsString;

        GlobalVariables.LOGGER.Debug($"endChar = {GlobalVariables.ReprString(endChar)}");

        foreach (Token positional in positionals)
        {
            string message = positional.ValueAsString;
            if (positional is WordToken wordToken)
            {
                GlobalVariables.LOGGER.Verbose($"All variables: {Aurora.Variables.UserDefined.Keys}");
                message = Aurora.Variables.GetVariable(wordToken.ValueAsString).ValueAsString;
            }

            Writer.AddToQueue(message);
        }

        Writer.AddToQueue(endChar);
        Writer.PushToStream();

        return new NullToken();
    }

    public static StringToken Read(List<Token> positionals, Dictionary<string, Token> keywords)
    {
        Dictionary<string, Token?> arguments = Parsers.ParseArgs(positionals, keywords,
            new Dictionary<string, Type> { { "message", typeof(StringToken) }, { "default", typeof(StringToken) } },
            ["message", "default"]);

        Token message = arguments.GetValueOrDefault("message") ??
                        new StringToken().Initialise(string.Empty, withoutQuotes: true);
        GlobalVariables.LOGGER.Debug($"Got message parameter as {GlobalVariables.ReprString(message.ValueAsString)}");

        Token? defaultParam = arguments.GetValueOrDefault("default");

        Console.Write(message.ValueAsString);
        string input = Console.ReadLine() ?? string.Empty;
        GlobalVariables.LOGGER.Debug($"(Read from screen) {input}");

        if (string.IsNullOrEmpty(input) && defaultParam is not null)
            return new StringToken().Initialise(defaultParam.ValueAsString, withoutQuotes: true, interpolate: false);

        return new StringToken().Initialise(input, withoutQuotes: true, interpolate: false);
    }

    public static IntegerToken ReadInt(List<Token> positionals, Dictionary<string, Token> keywords)
    {
        Dictionary<string, Token?> arguments = Parsers.ParseArgs(positionals, keywords,
            new Dictionary<string, Type>
            {
                { "message", typeof(StringToken) }, { "min", typeof(IntegerToken) }, { "max", typeof(IntegerToken) }
            },
            ["message", "min", "max"]);

        Token message = arguments.GetValueOrDefault("message") ??
                        new StringToken().Initialise(string.Empty, withoutQuotes: true);
        GlobalVariables.LOGGER.Debug($"Got message parameter as {GlobalVariables.ReprString(message.ValueAsString)}");

        Token? minimum = arguments.GetValueOrDefault("min");
        Token? maximum = arguments.GetValueOrDefault("max");

        if (minimum is not null && minimum.Type != IntegerToken.TokenType)
            Errors.AlwaysThrow(
                new ArgumentTypeMismatchError($"Parameter min expected an Integer, not `{minimum.Type}`"));

        if (maximum is not null && maximum.Type != IntegerToken.TokenType)
            Errors.AlwaysThrow(
                new ArgumentTypeMismatchError($"Parameter min expected an Integer, not `{maximum.Type}`"));


        readIntGetUserValueStart:

        Console.Write(message.ValueAsString);
        string input = Console.ReadLine() ?? string.Empty;
        GlobalVariables.LOGGER.Debug($"(Read from screen) {input}");

        if (!CustomInt.TryParse(input, out var result))
        {
            Writer.AddToQueue("Please enter a valid integer\n");
            Writer.PushToStream();
            goto readIntGetUserValueStart;
        }

        if (minimum is not null && result! < minimum.ValueAsInt)
        {
            Writer.AddToQueue($"Please enter a value greater than or equal to {minimum.ValueAsInt}\n");
            Writer.PushToStream();
            goto readIntGetUserValueStart;
        }

        if (maximum is not null && result! > maximum.ValueAsInt)
        {
            Writer.AddToQueue($"Please enter a value less than or equal to {maximum.ValueAsInt}\n");
            Writer.PushToStream();
            goto readIntGetUserValueStart;
        }

        if (result is null)
        {
            Writer.AddToQueue("Please enter a valid integer\n");
            Writer.PushToStream();
            goto readIntGetUserValueStart;
        }

        return new IntegerToken().Initialise(result);
    }

    public static FloatToken ReadFloat(List<Token> positionals, Dictionary<string, Token> keywords)
    {
        Dictionary<string, Token?> arguments = Parsers.ParseArgs(positionals, keywords,
            new Dictionary<string, Type>
            {
                { "message", typeof(StringToken) }, { "min", typeof(FloatToken) }, { "max", typeof(FloatToken) }
            },
            ["message", "min", "max"]);

        Token message = arguments.GetValueOrDefault("message") ??
                        new StringToken().Initialise(string.Empty, withoutQuotes: true);
        GlobalVariables.LOGGER.Debug($"Got message parameter as {GlobalVariables.ReprString(message.ValueAsString)}");

        Token? minimum = arguments.GetValueOrDefault("min");
        Token? maximum = arguments.GetValueOrDefault("max");

        if (minimum is not null && minimum.Type != FloatToken.TokenType)
            Errors.AlwaysThrow(
                new ArgumentTypeMismatchError($"Parameter min expected an Float, not `{minimum.Type}`"));

        if (maximum is not null && maximum.Type != FloatToken.TokenType)
            Errors.AlwaysThrow(
                new ArgumentTypeMismatchError($"Parameter min expected an Float, not `{maximum.Type}`"));


        readIntGetUserValueStart:

        Console.Write(message.ValueAsString);
        string input = Console.ReadLine() ?? string.Empty;
        GlobalVariables.LOGGER.Debug($"(Read from screen) {input}");

        if (!CustomFloat.TryParse(input, out var result))
        {
            Writer.AddToQueue("Please enter a valid float\n");
            Writer.PushToStream();
            goto readIntGetUserValueStart;
        }

        if (minimum is not null && result! < minimum.ValueAsFloat)
        {
            Writer.AddToQueue($"Please enter a value greater than or equal to {minimum.ValueAsFloat}\n");
            Writer.PushToStream();
            goto readIntGetUserValueStart;
        }

        if (maximum is not null && result! > maximum.ValueAsFloat)
        {
            Writer.AddToQueue($"Please enter a value less than or equal to {maximum.ValueAsFloat}\n");
            Writer.PushToStream();
            goto readIntGetUserValueStart;
        }

        if (result is null)
        {
            Writer.AddToQueue("Please enter a valid integer\n");
            Writer.PushToStream();
            goto readIntGetUserValueStart;
        }

        return new FloatToken().Initialise(result);
    }

    public static BooleanToken ReadBoolean(List<Token> positionals, Dictionary<string, Token> keywords)
    {
        Dictionary<string, Type> expectedArguments = new()
            { { "message", typeof(StringToken) }, { "optionStyle", typeof(StringToken) } };
        List<string> positionalOrder = ["message", "optionStyle"];
        Dictionary<string, Token?> arguments =
            Parsers.ParseArgs(positionals, keywords, expectedArguments, positionalOrder);

        Token message = arguments.GetValueOrDefault("message") ??
                        new StringToken().Initialise(string.Empty, withoutQuotes: true);
        GlobalVariables.LOGGER.Debug($"Got message parameter as {GlobalVariables.ReprString(message.ValueAsString)}");

        Token? optionStyle = arguments.GetValueOrDefault("optionStyle");
        string actualOptionStyle = optionStyle?.ValueAsString ?? "word";

        if (!Boolean.ValidOptionStyles.Contains(actualOptionStyle))
            Errors.AlwaysThrow(
                new UnsupportedOperationError($"Cannot read boolean with option style '{actualOptionStyle}'"));

        Writer.AddToQueue(message.ValueAsString);
        Writer.PushToStream();

        bool result = actualOptionStyle switch
        {
            "word" => ReadBoolOptionWord(),
            "number" => ReadBoolOptionNumber(),
            "char" => ReadBoolOptionChar(),
            "binary" => ReadBoolOptionBinary(),
            _ => false
        };


        return new BooleanToken().Initialise(result.ToString().ToLower());
    }

    private static bool ReadBoolOptionWord()
    {
        const string message = "Please enter either 'true' or 'false'\n";

        readBoolGetUserValueWordStart:
        Writer.AddToQueue(message);
        Writer.PushToStream();

        string input = Console.ReadLine() ?? string.Empty;

        switch (input)
        {
            case "true":
                return true;
            case "false":
                return false;
            default:
                Writer.AddToQueue("Please enter a valid option...\n");
                goto readBoolGetUserValueWordStart;
        }
    }

    private static bool ReadBoolOptionChar()
    {
        const string message = "Please enter either 'y' or 'n'\n";

        readBoolGetUserValueCharStart:
        Writer.AddToQueue(message);
        Writer.PushToStream();

        char input = Console.ReadKey().KeyChar;
        Writer.AddToQueue("\n");
        Writer.PushToStream();

        switch (input)
        {
            case 'y':
                return true;
            case 'n':
                return false;
            default:
                Writer.AddToQueue("Please enter a valid option...\n");
                goto readBoolGetUserValueCharStart;
        }
    }

    private static bool ReadBoolOptionNumber()
    {
        const string message = "Please enter either '1' for true, or '2' for false\n";

        readBoolGetUserValueNumberStart:
        Writer.AddToQueue(message);
        Writer.PushToStream();

        char input = Console.ReadKey().KeyChar;
        Writer.AddToQueue("\n");
        Writer.PushToStream();

        switch (input)
        {
            case '1':
                return true;
            case '2':
                return false;
            default:
                Writer.AddToQueue("Please enter a valid option...\n");
                goto readBoolGetUserValueNumberStart;
        }
    }

    private static bool ReadBoolOptionBinary()
    {
        const string message = "Please enter either '1' or '0'\n";

        readBoolGetUserValueBinaryStart:
        Writer.AddToQueue(message);
        Writer.PushToStream();

        char input = Console.ReadKey().KeyChar;
        Writer.AddToQueue("\n");
        Writer.PushToStream();

        switch (input)
        {
            case '1':
                return true;
            case '0':
                return false;
            default:
                Writer.AddToQueue("Please enter a valid option...\n");
                goto readBoolGetUserValueBinaryStart;
        }
    }
}