namespace Aurora.Commands;

internal static class Float
{
    public static FloatToken Create(List<Token> positionals, Dictionary<string, Token> keywords)
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
            Errors.AlwaysThrow(new ArgumentDeficitError("Missing required argument 'name' in Float.create"));

        FloatToken variable = new FloatToken();

        if (value is not null)
            variable.Initialise(value.ValueAsFloat);

        Aurora.Variables.RegisterVariable(name.ValueAsString, variable);
        return variable;
    }
}