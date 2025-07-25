using System.Collections.Immutable;

namespace Aurora.Commands;

internal static class Boolean
{
    public static ImmutableList<string> ValidOptionStyles = ["word", "char", "number", "binary"];

    public static BooleanToken Create(List<Token> positionals, Dictionary<string, Token> keywords)
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

        BooleanToken variable = new BooleanToken();

        if (value is not null)
            variable.Initialise(value.ValueAsString.ToLower());

        Aurora.Variables.RegisterVariable(name.ValueAsString, variable);
        return variable;
    }

    public static StringToken ToStyle(List<Token> positionals, Dictionary<string, Token> keywords)
    {
        Dictionary<string, Type> expectedArguments = new()
        {
            { "variable", typeof(WordToken) }, { "optionStyle", typeof(StringToken) }
        };
        List<string> positionalOrder = ["variable", "optionStyle"];
        Dictionary<string, Token?> arguments =
            Parsers.ParseArgs(positionals, keywords, expectedArguments, positionalOrder);

        Token? variable = arguments.GetValueOrDefault("variable");
        Token? optionStyle = arguments.GetValueOrDefault("optionStyle");

        if (variable is null)
            Errors.AlwaysThrow(new ArgumentDeficitError("Missing required argument 'variable' in Boolean.ToStyle"));

        if (variable.Type != BooleanToken.TokenType)
            Errors.AlwaysThrow(
                new ArgumentTypeMismatchError(
                    $"Boolean.toStyle expected variable of type boolean, not {variable.Type}"));

        if (optionStyle is null)
            Errors.AlwaysThrow(new ArgumentDeficitError("Missing required argument 'optionStyle' in Boolean.ToStyle"));

        string actualOptionStyle = optionStyle.ValueAsString;

        if (!ValidOptionStyles.Contains(actualOptionStyle))
            Errors.AlwaysThrow(
                new UnsupportedOperationError($"Cannot convert boolean with option style `{actualOptionStyle}`"));

        string? initialiser = actualOptionStyle switch
        {
            "word" => variable.ValueAsBool ? "true" : "false",
            "char" => variable.ValueAsBool ? "y" : "n",
            "number" => variable.ValueAsBool ? "1" : "2",
            "binary" => variable.ValueAsBool ? "1" : "0",
            _ => null
        };

        if (initialiser is null)
            Errors.AlwaysThrow(
                new UnsupportedOperationError($"Cannot convert boolean with option style `{actualOptionStyle}`"));

        return new StringToken().Initialise(initialiser, withoutQuotes: true);
    }
}