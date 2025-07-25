using System.Diagnostics;

namespace Aurora.Commands;

internal static class Variables
{
    private static string GetTitleCase(string value)
    {
        string lowerValue = value.ToLower();
        string titleCaseValue = lowerValue[0].ToString().ToUpper() + lowerValue[1..];
        return titleCaseValue;
    }

    private static bool TypeMatches(Token token, string expectedType)
    {
        return GetTitleCase(token.Type) == GetTitleCase(expectedType);
    }

    private static Token GetTokenFromType(string type)
    {
        switch (GetTitleCase(type))
        {
            case "Boolean": return new BooleanToken();
            case "Integer": return new IntegerToken();
            case "String": return new StringToken();
            case "Float": return new FloatToken();
            default:
                Errors.RaiseError(new TypeMismatchError($"There is no type {type}"), alwaysThrow: true);
                throw new UnreachableException();
        }
    }

    public static Token Create(List<Token> positionals, Dictionary<string, Token> keywords)
    {
        Dictionary<string, Token?> arguments = Parsers.ParseArgs(positionals, keywords,
            new Dictionary<string, Type>() { { "type", typeof(WordToken) } },
            ["type"],
            provideExtras: true);

        Token? type = arguments.GetValueOrDefault("type");
        if (type is null)
            Errors.AlwaysThrow(new ArgumentDeficitError("Missing required argument type for Variables.Create"));

        arguments.Remove("type");

        // { "x": 5 }     { "extra_positional_1": "x" }

        if (arguments.Count <= 0)
        {
            Errors.RaiseError(new ArgumentDeficitError("Missing arguement for Variables.create: value"),
                alwaysThrow: true);
            throw new UnreachableException();
        }

        foreach (string key in arguments.Keys)
        {
            if (key.StartsWith("extra_positional"))
            {
                Aurora.Variables.RegisterVariable(arguments[key]!.ValueAsString, GetTokenFromType(type.ValueAsString));
                continue;
            }

            Token value = arguments[key] ?? GetTokenFromType(type.ValueAsString);

            if (!TypeMatches(value, type.Type))
            {
                Errors.RaiseError(
                    new TypeMismatchError($"`{value.Type}` does not match expected type of `{type.Type}`"),
                    alwaysThrow: true);
                throw new UnreachableException();
            }

            Aurora.Variables.RegisterVariable(key, value);
        }

        return new NullToken();
    }

    public static Token Edit(List<Token> positionals, Dictionary<string, Token> keywords)
    {
        if (keywords.Count > 0)
            Errors.RaiseError(new ArgumentSurplusError("Variables.edit does not take any keyword arguments"));

        Token? name = positionals.ElementAtOrDefault(0);
        Token? value = positionals.ElementAtOrDefault(1);

        if (name is null)
            Errors.AlwaysThrow(new ArgumentDeficitError("Variables.edit is missing argument 'name'"));

        if (value is null)
            Errors.AlwaysThrow(new ArgumentDeficitError("Variables.edit is missing argument 'value'"));

        Aurora.Variables.RegisterVariable(name.ValueAsString, value);

        return new NullToken();
    }
}