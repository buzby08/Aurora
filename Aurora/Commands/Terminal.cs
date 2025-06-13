namespace Aurora.Commands;

internal static class Terminal
{
    public static StringToken Color { get; private set; } = new StringToken().Initialise(Colors.White);
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
            Console.Write(positional.ValueAsString);
            GlobalVariables.LOGGER.Debug($"(Written To Screen) {positional.ValueAsString}");
        }
        
        Console.Write(endChar);
        GlobalVariables.LOGGER.Debug($"(Written To Screen) {endChar}");
        
        return new NullToken();
    }

    public static StringToken Read(List<Token> positionals, Dictionary<string, Token> keywords)
    {
        Dictionary<string, Token?> arguments = Parsers.ParseArgs(positionals, keywords,
            new Dictionary<string, Type> { { "message", typeof(StringToken) } }, ["message"]);

        Token message = arguments.GetValueOrDefault("message") ??
                        new StringToken().Initialise(string.Empty, withoutQuotes: true);
        GlobalVariables.LOGGER.Debug($"Got message parameter as {GlobalVariables.ReprString(message.ValueAsString)}");
        
        Console.Write(message.ValueAsString);
        string input = Console.ReadLine() ?? string.Empty;
        GlobalVariables.LOGGER.Debug($"(Read from screen) {input}");
        
        return new StringToken().Initialise(input, withoutQuotes: true, interpolate: false);
    }

}