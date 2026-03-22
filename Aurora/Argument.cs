namespace Aurora;

internal class Argument(TokenList? value = null, TokenListItem? keyword = null)
{
    public TokenListItem? Keyword = keyword;
    public readonly TokenList Value = value ?? [];
    private AstList? _cachedAst;

    public AstList ValueAsAsts()
    {
        if (this._cachedAst is not null) return this._cachedAst;

        this._cachedAst = Evaluator.ParseTokenList(this.Value);
        return this._cachedAst;
    }

    private static void ThrowErrorAtEqualsSignIfNeeded(Argument argument, int characterIndex)
    {
        if (argument.Value.Count != 1)
            Errors.AlwaysThrow(new UnexpectedTokenError(
                    $"At `{argument.Value[1].AsString}` - Expected `=` after keyword name " +
                    $"`{argument.Value[0].AsString}`"),
                position: characterIndex);

        if (argument.Value.First().Token is not WordToken)
            Errors.AlwaysThrow(new UnexpectedTokenError(
                    $"`{argument.Value.First().AsString}` is not a valid keyword argument"),
                position: characterIndex);
    }

    private static void ThrowErrorAtSemicolonIfNeeded(Argument argument, int characterIndex)
    {
        if (argument.Value.Count == 0)
            Errors.AlwaysThrow(new UnexpectedTokenError(
                    "Unexpected token `;` - Missing argument - Expected expression before `;`"),
                position: characterIndex);
    }

    public static ArgumentParsingReturnResult Parse(TokenList tokens)
    {
        if (tokens.Count == 0)
            Errors.AlwaysThrow(new SystemError("Argument parsing needs a closing bracket!"));
        int bracketDepth = 1;
        List<Argument> argumentsList = [];

        Argument currentArgument = new();
        int numberOfTokensChecked = 0;

        foreach (TokenListItem tokenItem in tokens)
        {
            numberOfTokensChecked++;

            if (bracketDepth > 1 && tokenItem.Token.ValueAsString != ")" && tokenItem.Token.ValueAsString != "(")
            {
                currentArgument.Value.AddRaw(tokenItem);
                continue;
            }

            bool isEndOfExpression = bracketDepth == 1 && tokenItem.Token.ValueAsString == ")";

            if (isEndOfExpression && currentArgument.Value.Count > 0)
            {
                argumentsList.Add(currentArgument);
                currentArgument = new Argument();
                break;
            }

            if (isEndOfExpression)
            {
                break;
            }

            switch (tokenItem.Token.ValueAsString)
            {
                case "(":
                    bracketDepth++;
                    currentArgument.Value.AddRaw(tokenItem);
                    break;
                case ")":
                    bracketDepth--;
                    currentArgument.Value.AddRaw(tokenItem);
                    break;
                case ";":
                    ThrowErrorAtSemicolonIfNeeded(currentArgument, tokenItem.StartCharPosition);
                    argumentsList.Add(currentArgument);
                    currentArgument = new Argument();
                    break;
                case "=":
                    ThrowErrorAtEqualsSignIfNeeded(currentArgument, tokenItem.StartCharPosition);
                    currentArgument.Keyword = currentArgument.Value.First();
                    currentArgument.Value.Clear();
                    break;
                default:
                    currentArgument.Value.AddRaw(tokenItem);
                    break;
            }
        }

        return new ArgumentParsingReturnResult(numberOfTokensChecked, argumentsList);
    }
}

internal struct ArgumentParsingReturnResult(int numberOfTokensChecked, List<Argument> arguments)
{
    public List<Argument> Arguments = arguments;
    public int NumberOfTokensChecked = numberOfTokensChecked;
}