using Math = System.Math;

namespace Aurora.Commands;

internal static class Math
{
    private static bool IsInteger(Token token)
    {
        try
        {
            return token.ValueAsInt.Value == token.ValueAsFloat.Value;
        }
        catch (NotImplementedException)
        {
            return false;
        }
    }

    public static Token Pow(List<Token> positionals, Dictionary<string, Token> keywords, List<Ast> raw)
    {
        Dictionary<string, List<Type>> expectedTokens = new()
        {
            { "a", [typeof(IntegerToken), typeof(FloatToken)] },
            { "b", [typeof(IntegerToken), typeof(FloatToken)] }
        };
        List<string> positionalOrder = ["a", "b"];

        Dictionary<string, Token?>
            arguments = Parsers.ParseArgs(positionals, keywords, expectedTokens, positionalOrder);

        Token? a = arguments.GetValueOrDefault("a");
        Token? b = arguments.GetValueOrDefault("b");

        if (a is null)
            Errors.AlwaysThrow(new ArgumentDeficitError("Missing required argument 'a' in Math.pow"));

        if (b is null)
            Errors.AlwaysThrow(new ArgumentDeficitError("Missing required argument 'b' in Math.pow"));

        bool aIsInteger = IsInteger(a);
        bool bIsInteger = IsInteger(b);

        if (aIsInteger && bIsInteger)
            return new IntegerToken().Initialise(CustomInt.Pow(a.ValueAsInt, b.ValueAsInt));

        return new FloatToken().Initialise(CustomFloat.Pow(a.ValueAsFloat, b.ValueAsFloat));
    }

    public static Token Abs(List<Token> positionals, Dictionary<string, Token> keywords, List<Ast> raw)
    {
        Dictionary<string, List<Type>> expectedTokens = new()
        {
            { "x", [typeof(IntegerToken), typeof(FloatToken)] }
        };
        List<string> positionalOrder = ["x"];

        Dictionary<string, Token?>
            arguments = Parsers.ParseArgs(positionals, keywords, expectedTokens, positionalOrder);

        Token? x = arguments.GetValueOrDefault("x");

        if (x is null)
            Errors.AlwaysThrow(new ArgumentDeficitError("Missing required argument 'x' in Math.abs"));

        return IsInteger(x)
            ? (Token)new IntegerToken().Initialise(new CustomInt(System.Math.Abs((dynamic)x.ValueAsInt.Value)))
            : (Token)new FloatToken().Initialise(new CustomFloat(System.Math.Abs((dynamic)x.ValueAsFloat.Value)));
    }

    public static Token Round(List<Token> positionals, Dictionary<string, Token> keywords, List<Ast> raw)
    {
        Dictionary<string, List<Type>> expectedTokens = new()
        {
            { "x", [typeof(IntegerToken), typeof(FloatToken)] },
            { "nPoints", [typeof(IntegerToken)] }
        };
        List<string> positionalOrder = ["x", "nPoints"];

        Dictionary<string, Token?>
            arguments = Parsers.ParseArgs(positionals, keywords, expectedTokens, positionalOrder);

        Token? x = arguments.GetValueOrDefault("x");
        Token nPoints = arguments.GetValueOrDefault("nPoints") ?? new IntegerToken().Initialise("0");

        if (x is null)
            Errors.AlwaysThrow(new ArgumentDeficitError("Missing required argument 'x' in Math.round"));

        if (nPoints.ValueAsInt < new CustomInt(0) || nPoints.ValueAsInt > new CustomInt(15))
            Errors.AlwaysThrow(new OutOfRangeError("nDigits must be between 0 and 15 (inclusive) in Math.round"));

        return new FloatToken().Initialise(new CustomFloat(System.Math.Round((dynamic)x.ValueAsFloat.Value,
            (dynamic)nPoints.ValueAsInt.Value)));
    }
}