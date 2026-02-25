namespace Aurora;

internal class Evaluator
{
    public Evaluator(string text)
    {
        Tokeniser.Text = text;
    }

    public List<Ast> ParseTokenList()
    {
        TokenList tokens = Tokeniser.GetAllTokens();

        return ParseTokenList(tokens);
    }

    public static List<Ast> ParseTokenList(TokenList tokens)
    {
        if (tokens.Count == 0) return [];

        List<Ast> asts = [];

        Ast currentAst = new();
        bool isTarget = true;

        int count = 0;

        while (count < tokens.Count)
        {
            TokenListItem tokenItem = tokens[count++];

            if (isTarget && tokenItem.Token is DotToken)
            {
                isTarget = false;
                continue;
            }

            if (!isTarget && tokenItem.Token is DotToken)
            {
                asts.Add(currentAst);
                currentAst = new Ast();
                isTarget = false;
                continue;
            }

            if (tokenItem.Token is not WordToken && tokenItem.Token is not DotToken &&
                tokenItem.Token is not BracketToken)
                Errors.AlwaysThrow(new UnexpectedTokenError($"`{tokenItem.AsString}` was not expected"),
                    position: tokenItem.StartCharPosition);

            if (isTarget && tokenItem.Token is WordToken)
            {
                currentAst.Target = tokenItem.AsString;
                continue;
            }

            if (!isTarget && tokenItem.Token is WordToken)
            {
                currentAst.Name = tokenItem.AsString;
                continue;
            }

            if (!isTarget && tokenItem.AsString == "(")
            {
                // This should parse the args, then set the count to exactly where the argument parser finished 
                //      The count should then be the index after the arguments closing bracket
                ArgumentParsingReturnResult argumentParsingReturnResult = Argument.Parse(tokens[count..]);
                List<Argument> arguments = argumentParsingReturnResult.Arguments;
                currentAst.Arguments = arguments;

                count = argumentParsingReturnResult.EndTokenIndex + 1;
            }
        }
        
        asts.Add(currentAst);

        return asts;
    }
    
    private Tokeniser Tokeniser { get; } = new Tokeniser();
}