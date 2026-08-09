using Aurora.Internals;

namespace Aurora;

internal class ParserRework
{
    public required RuntimeContext Context { get; init; }
    public string Code { get; private set; }
    private TokenList Tokens { get; set; }
    private List<List<AstRework>> Asts { get; set; } = [];
    private List<List<AstRework>> Expressions { get; set; } = [];
    private List<AstRework> CurrentExpression { get; set; } = [];
    private int CurrentIndex { get; set; }
    private int FinalIndex { get; set; }


    public void ParseCode(string code)
    {
        this.Code = code;
        Tokenizer tokenizer = new Tokenizer
        {
            Text = code,
        };

        this.Tokens = tokenizer.GetAllTokens();
    }

    public void ParseTokens()
    {
        if (Tokens == null)
            throw new Exception("Tokens not initialized");
    }

    private void ParseExpression()
    {
        AstRework currentAst = new(this.Context);

        while (this.CurrentIndex < this.Expressions.Count)
        {
            TokenListItem tokenListItem = this.Tokens[this.CurrentIndex];
            this.FinalIndex = this.CurrentIndex;

            this.CurrentIndex++;

            if (tokenListItem.Token is DotToken)
            {
                this.CurrentExpression.Add(currentAst);
                currentAst = new AstRework(this.Context);
                continue;
            }

            if (tokenListItem.Token is EofToken)
            {
                HandleEofToken(tokenListItem, currentAst);
                return;
            }

            if (tokenListItem.Token is EoLToken && !currentAst.IsValid) continue;

            if (tokenListItem.Token is EoLToken)
            {
                this.HandleEolTokenWhenValid(tokenListItem, currentAst);
                currentAst = new AstRework(this.Context);
                continue;
            }

            // Todo: Handle bracket open
            // Todo: Handle block open

            currentAst.AddName(tokenListItem);
        }
    }

    private void HandleEofToken(TokenListItem item, AstRework currentAst)
    {
        if (!currentAst.IsValid)
            Errors.AlwaysThrow(new EofError(), this.Context, item.StartCharPosition);

        this.CurrentExpression.Add(currentAst);
    }

    private void HandleEolTokenWhenValid(TokenListItem item, AstRework currentAst)
    {
        Token nextToken = GetTokenAtIndex(this.CurrentIndex + 1);

        if (nextToken is not DotToken)
        {
            this.CurrentExpression.Add(currentAst);
            this.Expressions.Add(this.CurrentExpression);
            this.CurrentExpression.Clear();
            return;
        }

        this.CurrentExpression.Add(currentAst);
    }

    private void HandleOpenBracket(TokenListItem item, AstRework currentAst)
    {
        var argumentParseResult = Argument.Parse(this.Tokens[this.CurrentIndex..], this.Context);
        // Todo: continue: check notion. Make ^ compatible with EOL tokens.
    }

    // Todo: Create handleBlockOpen function

    private Token GetTokenAtIndex(int index)
    {
        if (index < 0 || index >= this.Tokens.Count) return new EofToken();
        return this.Tokens.ElementAtOrDefault(index).Token;
    }
}
