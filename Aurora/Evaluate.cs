using System.Diagnostics;

namespace Aurora;

internal static class Evaluate
{
    private static Ast ParseGroupedTokens(List<List<Token>> tokens)
    {
        List<Ast> asts = [];
        foreach (List<Token> tokenSet in tokens)
        {
            Token? firstElement = tokenSet.ElementAtOrDefault(0);
            if (firstElement is not null && firstElement.Equals(BracketToken.OpenNormal))
            {
                int count = tokenSet.Count;
                int endIndex = tokenSet[^1].Equals(BracketToken.CloseNormal) ? count - 1 : count - 2;
                List<List<Token>> tempTokenList = SegmentTokens(tokenSet[1..endIndex]);
                Ast tempAst = ParseGroupedTokens(tempTokenList);

                if (tokenSet[count - 1] is OperatorToken operatorToken)
                {
                    tempAst.OperatorToken = operatorToken;
                }

                asts.Add(tempAst);
                continue;
            }

            Ast ast = GenerateAst(tokenSet);
            asts.Add(ast);
        }

        Ast previousAst = asts.ElementAtOrDefault(0) ?? new Ast();
        for (int i = 1; i < asts.Count; i++)
        {
            if (previousAst.OperatorToken is null)
            {
                return previousAst;
            }

            Ast currentAst = asts[i];
            previousAst.Combine(currentAst);
        }

        return previousAst;
    }

    private static List<List<Token>> SegmentTokens(List<Token> tokens)
    {
        List<List<Token>> all = [];
        List<Token> current = [];
        int bracketDepth = 0;

        foreach (Token token in tokens)
        {
            if (token.Equals(BracketToken.OpenNormal))
            {
                bracketDepth++;
            }

            if (token.Equals(BracketToken.CloseNormal))
            {
                bracketDepth--;
            }

            if (token.Type == OperatorToken.TokenType && bracketDepth <= 0)
            {
                current.Add(token);
                all.Add(current.ToList());
                current.Clear();
                continue;
            }

            current.Add(token);
        }

        all.Add(current.ToList());
        current.Clear();

        return all;
    }

    public static Token SingleLine(List<Token> tokens)
    {
        string lineAsOutput = PrettyPrint.TokenList(tokens, output: false);
        GlobalVariables.LOGGER.Verbose($"Evaluating line: {lineAsOutput}");

        List<List<Token>> tokenList = SegmentTokens(tokens);
        Ast ast = ParseGroupedTokens(tokenList);
        return ast.Evaluate();
    }

    public static Ast GenerateAst(List<Token> tokens)
    {
        Ast ast = new Ast();

        GlobalVariables.LOGGER.Verbose(
            $"Begin Generating Ast: Line = `{PrettyPrint.TokenList(tokens, output: false)}`");

        int index = 0;
        while (index < tokens.Count)
        {
            Token currentToken = tokens[index];
            Ast.AstItemTypes itemType = ast.ItemType;

            index++;

            GlobalVariables.LOGGER.Verbose(
                $"Generating Ast: currentToken = {currentToken.ValueAsString}, itemType = {itemType}");

            if (itemType == Ast.AstItemTypes.None)
            {
                GlobalVariables.LOGGER.Verbose("Ast itemType is None, converting to literal");
                ast.ItemValue = currentToken;
                continue;
            }

            if (itemType == Ast.AstItemTypes.Literal && currentToken is OperatorToken operatorToken)
            {
                ast.OperatorToken = operatorToken;
                continue;
            }

            if (itemType == Ast.AstItemTypes.Literal && currentToken.Equals(SeparatorToken.DotToken))
            {
                GlobalVariables.LOGGER.Verbose("Ast itemType is literal and currentToken is '.'");
                ast.ClassWaitingForAccess = true;
                continue;
            }

            if (itemType == Ast.AstItemTypes.ClassReference && ast.ClassWaitingForAccess is true &&
                currentToken.Type == WordToken.TokenType)
            {
                GlobalVariables.LOGGER.Verbose(
                    "Ast itemType is classReference and classWaitingForAccess and currentToken is word");
                ast.ClassWaitingForAccess = null;
                ast.ClassName = ast.ItemValue?.ValueAsString;
                ast.ItemValue = null;
                ast.ClassAttributeName = currentToken.ValueAsString;
                continue;
            }

            if (itemType == Ast.AstItemTypes.AttributeAccess && currentToken.Equals(BracketToken.OpenNormal))
            {
                GlobalVariables.LOGGER.Verbose(
                    "Ast itemType is AttributeAccess and currentToken is bracket open");
                ast.ClassMethodName = ast.ClassAttributeName;
                ast.ClassAttributeName = null;
                ast.CloseBracketFound = false;
                ast.ClassArguments = new List<Ast>();
                (List<List<Token>> allArgs, int endIndex) asArguments = SeparateIntoArguments(tokens, index);

                index = asArguments.endIndex;

                foreach (List<Token> argTokens in asArguments.allArgs)
                {
                    List<List<Token>> segmentedTokens = SegmentTokens(argTokens);
                    Ast groupedToken = ParseGroupedTokens(segmentedTokens);
                    ast.ClassArguments.Add(groupedToken);
                }

                ast.CloseBracketFound = true;
            }

            if (itemType != Ast.AstItemTypes.Literal && itemType != Ast.AstItemTypes.Invalid)
            {
                GlobalVariables.LOGGER.Verbose("Ast itemType is not literal: evaluating ast to create it as a literal");
                Token evaluatedResponse = ast.Evaluate();
                string? keywordValue = ast.KeywordValue;
                ast.ResetValues();
                ast.KeywordValue = keywordValue;
                ast.ItemValue = evaluatedResponse;
            }

            if (itemType == Ast.AstItemTypes.Literal && currentToken.Equals(EqualsToken.TokenEquals))
            {
                GlobalVariables.LOGGER.Verbose("Ast itemType is literal, and currentToken is '='");
                ast.KeywordValue = ast.ItemValue?.ValueAsString;
                ast.ItemValue = null;
            }
        }

        return ast;
    }

    public static (List<List<Token>> allArgs, int endIndex) SeparateIntoArguments(List<Token> tokens, int index,
        bool requireCloseBracket = true)
    {
        int bracketDepth = 1;
        List<List<Token>> allArgs = [];
        List<Token> currentArg = [];

        while (index < tokens.Count)
        {
            Token currentToken = tokens[index];

            if (bracketDepth == 1 && currentToken.Equals(SeparatorToken.SemiColonToken))
            {
                allArgs.Add(currentArg.ToList());
                currentArg.Clear();
                index++;
                continue;
            }

            if (currentToken.Equals(BracketToken.OpenNormal))
            {
                bracketDepth++;
            }

            if (currentToken.Equals(BracketToken.CloseNormal) && bracketDepth > 1)
            {
                bracketDepth--;
                currentArg.Add(currentToken);
                index++;
                continue;
            }

            if (currentToken.Equals(BracketToken.CloseNormal) && bracketDepth <= 1)
            {
                if (currentArg.Count > 0)
                    allArgs.Add(currentArg);
                return (allArgs, index + 1);
            }

            currentArg.Add(currentToken);
            index++;
        }

        if (!requireCloseBracket)
        {
            allArgs.Add(currentArg);
            return (allArgs, index + 1);
        }

        Errors.AlwaysThrow(
            new UnclosedDelimiterError(
                $"Missing close bracket in expression {PrettyPrint.TokenList(tokens, output: false)}"));
        throw new UnreachableException();
    }

    public static void AllCode(string[] code)
    {
        Interpreter interpreter = new Interpreter();

        GlobalVariables.LineNumber = 0;
        foreach (string line in code)
        {
            GlobalVariables.LineNumber++;

            if (line == string.Empty) continue;

            if (GlobalVariables.LineNumber == 42)
            {
                GlobalVariables.LOGGER.Debug("[Line 42] You have discovered the meaning of life. Use it wisely.");
            }

            if (line.Contains("if (life == hard)", StringComparison.CurrentCultureIgnoreCase))
                GlobalVariables.LOGGER.ForceConsoleLog("Suggestion: Have you tried turning it off and on again",
                    addLineNumber: true);

            interpreter.Text = line;

            SingleLine(interpreter.GetAllTokens());
        }
    }

    public static List<List<Token>> SplitListForArgs(List<Token> tokens, Token tokenToSplit)
    {
        List<List<Token>> splitLists = [];

        int startIndex = 0;
        int currentIndex = 0;

        while (currentIndex < tokens.Count)
        {
            Token currentToken = tokens.ElementAt(currentIndex);
            if (currentToken.Equals(tokenToSplit))
            {
                if (Test.isTesting)
                    Console.WriteLine($"Adding where startIndex: {startIndex}, currentIndex: {currentIndex}");

                List<Token> partialList = tokens[startIndex..currentIndex];
                splitLists.Add(partialList);
                startIndex = currentIndex + 1;
            }

            currentIndex++;
        }

        List<Token> finalPartialList = tokens[startIndex..currentIndex];
        splitLists.Add(finalPartialList);

        return splitLists;
    }
}