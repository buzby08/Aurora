using System.Runtime.CompilerServices;
using Aurora.Internals;

namespace Aurora;

internal class Argument(TokenList? value = null, TokenListItem? keyword = null)
{
    public TokenListItem? Keyword = keyword;
    public string? KeywordAsString => keyword?.AsString;
    public readonly TokenList Value = value ?? [];
    public int? KeywordPosition => this.Keyword?.StartCharPosition;
    public int? ValuePosition => this.Value?.Position;
    public List<string> ValueAsString => this.Value.DataAsString;
    private AstList? _cachedAst;

    public AstList ValueAsAsts(RuntimeContext context)
    {
        if (this._cachedAst is not null) return this._cachedAst;

        this._cachedAst = Evaluator.ParseTokenList(this.Value, context);
        return this._cachedAst;
    }

    private static void ThrowErrorAtEqualsSignIfNeeded(Argument argument, int characterIndex, RuntimeContext context)
    {
        if (argument.Value.Count != 1)
            Errors.AlwaysThrow(new UnexpectedTokenError(
                    $"At `{argument.Value[1].AsString}` - Expected `=` after keyword name " +
                    $"`{argument.Value[0].AsString}`"),
                context, position: characterIndex);

        if (argument.Value.First().Token is not WordToken)
            Errors.AlwaysThrow(new UnexpectedTokenError(
                    $"`{argument.Value.First().AsString}` is not a valid keyword argument"),
                context, position: characterIndex);
    }

    private static void ThrowErrorAtSemicolonIfNeeded(Argument argument, int characterIndex, RuntimeContext context)
    {
        if (argument.Value.Count == 0)
            Errors.AlwaysThrow(new UnexpectedTokenError(
                    "Unexpected token `;` - Missing argument - Expected expression before `;`"),
                context, position: characterIndex);
    }

    public static ArgumentParsingReturnResult Parse(TokenList tokens, RuntimeContext context)
    {
        if (tokens.Count == 0)
            Errors.AlwaysThrow(new SystemError("Argument parsing needs a closing bracket!"), context);
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
                    ThrowErrorAtSemicolonIfNeeded(currentArgument, tokenItem.StartCharPosition, context);
                    argumentsList.Add(currentArgument);
                    currentArgument = new Argument();
                    break;
                case "=":
                    ThrowErrorAtEqualsSignIfNeeded(currentArgument, tokenItem.StartCharPosition, context);
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