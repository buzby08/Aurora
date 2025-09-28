using System.Collections.Immutable;

namespace Aurora.Commands;

internal static class Boolean
{
    public static ImmutableList<string> ValidOptionStyles = ["word", "char", "number", "binary"];

    public static Token WordOptionStyle = new StringToken().Initialise("word", withoutQuotes: true);
    public static Token CharOptionStyle = new StringToken().Initialise("char", withoutQuotes: true);
    public static Token NumberOptionStyle = new StringToken().Initialise("number", withoutQuotes: true);
    public static Token BinaryOptionStyle = new StringToken().Initialise("binary", withoutQuotes: true);

    public static BooleanToken Create(List<Token> positionals, Dictionary<string, Token> keywords,
        List<Ast> raw)
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

    public static StringToken ToStyle(List<Token> positionals, Dictionary<string, Token> keywords,
        List<Ast> raw)
    {
        Dictionary<string, Type> expectedArguments = new()
        {
            { "value", typeof(WordToken) }, { "optionStyle", typeof(StringToken) }
        };
        List<string> positionalOrder = ["value", "optionStyle"];
        Dictionary<string, Token?> arguments =
            Parsers.ParseArgs(positionals, keywords, expectedArguments, positionalOrder);

        Token? value = arguments.GetValueOrDefault("value");
        Token? optionStyle = arguments.GetValueOrDefault("optionStyle");

        if (value is null)
            Errors.AlwaysThrow(new ArgumentDeficitError("Missing required argument 'variable' in Boolean.ToStyle"));

        if (value.Type != BooleanToken.TokenType)
            Errors.AlwaysThrow(
                new ArgumentTypeMismatchError(
                    $"Boolean.toStyle expected variable of type boolean, not {value.Type}"));

        if (optionStyle is null)
            Errors.AlwaysThrow(new ArgumentDeficitError("Missing required argument 'optionStyle' in Boolean.ToStyle"));

        string actualOptionStyle = optionStyle.ValueAsString;

        if (!ValidOptionStyles.Contains(actualOptionStyle))
            Errors.AlwaysThrow(
                new UnsupportedOperationError($"Cannot convert boolean with option style `{actualOptionStyle}`"));

        string? initialiser = actualOptionStyle switch
        {
            "word" => value.ValueAsBool ? "true" : "false",
            "char" => value.ValueAsBool ? "y" : "n",
            "number" => value.ValueAsBool ? "1" : "2",
            "binary" => value.ValueAsBool ? "1" : "0",
            _ => null
        };

        if (initialiser is null)
            Errors.AlwaysThrow(
                new UnsupportedOperationError($"Cannot convert boolean with option style `{actualOptionStyle}`"));

        return new StringToken().Initialise(initialiser, withoutQuotes: true);
    }
}