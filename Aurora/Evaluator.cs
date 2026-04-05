using System.Diagnostics;
using Aurora.Internals;

namespace Aurora;

internal class Evaluator
{
    public Evaluator(string text)
    {
        this.Tokenizer.Text = text;
    }

    public static void EvaluateAllCode(string[] code, RuntimeContext context)
    {
        foreach (string s in code)
        {
            InternalVariables.LineNumber += 1;
            
            if (InternalVariables.LinesToDebug.Contains<int>((int)InternalVariables.LineNumber!))
                Debugger.Break();
            
            if (string.IsNullOrWhiteSpace(s))
                continue;
            
            Evaluator evaluator = new(s);
            AstList astList = evaluator.ParseTokenList();
            EvaluateAstList(astList, context);
        }
    }

    public AstList ParseTokenList()
    {
        TokenList tokens = this.Tokenizer.GetAllTokens();

        return ParseTokenList(tokens);
    }

    public static AstList ParseTokenList(TokenList tokens)
    {
        if (tokens.Count == 0) return [];

        AstList asts = [];

        Ast currentAst = new()
        {
            IsALiteral = true
        };
        bool isTarget = true;

        int count = 0;

        while (count < tokens.Count)
        {
            // Todo: Set Ast as literal when dotToken has not been found as name is a token that can become a runtime 
            //  object.
            TokenListItem tokenItem = tokens[count++];

            if (isTarget && tokenItem.Token is DotToken)
            {
                isTarget = false;
                currentAst.IsALiteral = false;
                continue;
            }

            if (!isTarget && tokenItem.Token is DotToken)
            {
                asts.Add(currentAst);
                currentAst = new Ast
                {
                    IsALiteral = false
                };
                isTarget = false;
                continue;
            }

            if (tokenItem.Token is not WordToken && tokenItem.Token is not StringToken &&
                tokenItem.Token is not DotToken &&
                tokenItem.Token is not BracketToken
                && tokenItem.Token is not NumberToken)
                Errors.AlwaysThrow(new UnexpectedTokenError($"`{tokenItem.AsString}` was not expected"),
                    position: tokenItem.StartCharPosition);

            if (isTarget && tokenItem.Token is WordToken or StringToken or NumberToken)
            {
                currentAst.Target = tokenItem;
                continue;
            }

            if (!isTarget && tokenItem.Token is WordToken)
            {
                currentAst.Name = tokenItem;
                continue;
            }

            if (!isTarget && tokenItem.AsString == "(")
            {
                // This should parse the args, then set the count to exactly where the argument parser finished 
                //      The count should then be the index after the arguments closing bracket
                ArgumentParsingReturnResult argumentParsingReturnResult = Argument.Parse(tokens[count..]);
                List<Argument> arguments = argumentParsingReturnResult.Arguments;
                currentAst.Arguments = arguments;

                count += argumentParsingReturnResult.NumberOfTokensChecked;
            }
        }

        if (currentAst.IsALiteral && currentAst.Target is not null)
        {
            currentAst.Name = currentAst.Target.Value;
            currentAst.Target = null;
        }

        asts.Add(currentAst);

        return asts;
    }

    public static RuntimeObject EvaluateAstList(AstList asts, RuntimeContext context)
    {
        RuntimeObject? result = null;
        foreach (Ast currentAst in asts)
        {
            result = currentAst.Evaluate(context, result);
        }

        if (result is null) /* Todo: This is being hit when current code in code.aur is being run. Figure out why, as passing 3 values should
                                be allowed*/
            Errors.AlwaysThrow(new SystemError($"Could not parse ast list where ast list is empty."));

        return result;
    }

    public static RuntimeObject ExecuteMethodAst(List<List<Ast>> body, RuntimeObject self,
        RuntimeContext context)
    {
        // Todo: Implement
        return new RuntimeObject();
    }


    private Tokenizer Tokenizer { get; } = new Tokenizer();
}