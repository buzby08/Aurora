namespace Aurora.Commands;

internal static class StringClass
{
    public static StringToken Create(List<Token> positionals, Dictionary<string, Token> keywords)
    {
        Dictionary<string, Type> expectedArguments = new()
        {
            { "name", typeof(WordToken) }, { "value", typeof(IntegerToken) }
        };
        List<string> positionalOrder = ["name", "value"];

        Dictionary<string, Token?> arguments =
            Parsers.ParseArgs(positionals, keywords, expectedArguments, positionalOrder);

        Token? name = arguments.GetValueOrDefault("name");
        Token? value = arguments.GetValueOrDefault("value");

        if (name == null)
            Errors.AlwaysThrow(new ArgumentDeficitError("Missing required argument 'name' in String.create"));

        StringToken variable = new StringToken();

        if (value is not null)
            variable.Initialise(value.ValueAsString, withoutQuotes: true);

        Aurora.Variables.RegisterVariable(name.ValueAsString, variable);
        return variable;
    }
}