using System.Diagnostics;

namespace Aurora;

internal class Evaluate
{
    private readonly Interpreter interpreter = new Interpreter();
    private string[]? _allCode;

    private Ast ParseGroupedTokens(List<List<Token>> tokens)
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
                    tempAst.OperatorToken = operatorToken;

                if (tokenSet[count - 1] is ComparisonToken comparisonToken)
                    tempAst.ComparisonToken = comparisonToken;

                asts.Add(tempAst);
                continue;
            }

            Ast ast = GenerateAst(tokenSet);
            asts.Add(ast);
        }

        Ast previousAst = asts.ElementAtOrDefault(0) ?? new Ast();
        for (int i = 1; i < asts.Count; i++)
        {
            if (previousAst.OperatorToken is null && previousAst.ComparisonToken is null)
            {
                return previousAst;
            }

            Ast currentAst = asts[i];
            previousAst.Combine(currentAst);
        }

        return previousAst;
    }

    private List<List<Token>> SegmentTokens(List<Token> tokens)
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

            if (token.Type is OperatorToken.TokenType or ComparisonToken.TokenType && bracketDepth <= 0)
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

    public Token? SingleLine(List<Token> tokens, bool highestLevel = false)
    {
        string lineAsOutput = PrettyPrint.TokenList(tokens, output: false);
        GlobalVariables.LOGGER.Verbose($"Evaluating line: {lineAsOutput}");

        List<List<Token>> tokenList = SegmentTokens(tokens);
        Ast ast = ParseGroupedTokens(tokenList);
        if (highestLevel && ast.ItemType != Ast.AstItemTypes.ConditionAndBlock)
            GlobalVariables.PreviousIfIsTrue = null;
        return ast.Evaluate();
    }

    public Ast GenerateAst(List<Token> tokens)
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

            if (itemType == Ast.AstItemTypes.Literal && currentToken is ComparisonToken comparisonToken)
            {
                ast.ComparisonToken = comparisonToken;
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

            if (itemType == Ast.AstItemTypes.Literal && currentToken.Equals(BracketToken.OpenNormal))
            {
                ast = ParseConditionalStatement(ast, tokens, index);
                return ast;
            }

            if (itemType != Ast.AstItemTypes.Literal && itemType != Ast.AstItemTypes.Invalid)
            {
                GlobalVariables.LOGGER.Verbose("Ast itemType is not literal: evaluating ast to create it as a literal");
                Token? evaluatedResponse = ast.Evaluate();
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

    public Ast ParseConditionalStatement(Ast ast, List<Token> tokens, int index)
    {
        int normalBracketIndex = 1;
        List<Token> condition = [];
        while (index < tokens.Count)
        {
            Token currentToken = tokens[index];
            if (currentToken.Equals(BracketToken.OpenNormal))
                normalBracketIndex++;

            if (normalBracketIndex > 1 && currentToken.Equals(BracketToken.CloseNormal))
                normalBracketIndex--;

            if (normalBracketIndex <= 1 && currentToken.Equals(BracketToken.CloseNormal))
                break;

            condition.Add(currentToken);
            index++;
        }

        index++;
        ast.Condition = condition;

        if (index >= tokens.Count)
            Errors.AlwaysThrow(new InvalidSyntaxError("Conditionals require a code body, defined using curly braces."));

        Token nextToken = tokens[index];
        index++;
        List<string> codeBody = [];
        bool curlyBracketFound = false;

        if (!nextToken.Equals(BracketToken.OpenCurly))
        {
            string currentLine = GetCodeFromLine((int)GlobalVariables.LineNumber!)!;
            int currentIndex = tokens.TakeWhile((t, i) => i < index).Sum(t => t.ValueLength);
            codeBody.Add(currentLine[..currentIndex]);
            curlyBracketFound = true;
        }

        int curlyBracketDepth = 1;
        List<Token> currentTokens = tokens[index..].ToList();
        int currentLineNumber = (int)GlobalVariables.LineNumber!;

        while (!curlyBracketFound)
        {
            int currentIndex = 0;
            List<Token> tokensToAdd = [];
            int? curlyBracketIndex = null;

            while (currentIndex < currentTokens.Count)
            {
                Token currentToken = currentTokens[currentIndex];
                currentIndex++;
                if (currentToken.Equals(BracketToken.OpenCurly))
                    curlyBracketDepth++;

                if (curlyBracketDepth <= 1 && currentToken.Equals(BracketToken.CloseCurly))
                {
                    curlyBracketFound = true;
                    curlyBracketIndex = currentIndex;
                    break;
                }

                if (curlyBracketDepth > 1 && currentToken.Equals(BracketToken.CloseCurly))
                    curlyBracketDepth--;

                tokensToAdd.Add(currentToken);
            }

            if (tokensToAdd.Count > 0)
            {
                codeBody.Add(GetCodeFromLine(currentLineNumber)!);
            }

            MarkAsComment(currentLineNumber);

            string? code = GetCodeFromLine(++currentLineNumber);
            if (code is null)
                break;

            currentTokens = GetTokensFromCode(code);
        }

        if (!curlyBracketFound)
            Errors.AlwaysThrow(
                new UnclosedDelimiterError("Missing closing curly bracket in conditional statement"));

        ast.CodeBlock = codeBody;
        return ast;
    }

    public (List<List<Token>> allArgs, int endIndex) SeparateIntoArguments(List<Token> tokens, int index,
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

    public string? GetCodeFromLine(int lineNumber)
    {
        return _allCode?.ElementAtOrDefault(lineNumber - 1);
    }

    public List<Token> GetTokensFromCode(string code)
    {
        interpreter.Text = code;
        return interpreter.GetAllTokens();
    }

    private void MarkAsComment(int lineNumber)
    {
        if (_allCode is null) return;
        if (lineNumber - 1 > _allCode.Length) return;

        _allCode[lineNumber - 1] = "//" + _allCode[lineNumber - 1];
    }

    private string RemoveComments(string code)
    {
        for (int i = 0; i < code.Length; i++)
        {
            try
            {
                if (code[i] == '/' && code[i + 1] == '/')
                    return code[..i];
            }
            catch
            {
                return code;
            }
        }

        return code;
    }

    public void AllCode(string[] code)
    {
        _allCode = code.ToArray();

        GlobalVariables.LineNumber = 0;

        int index = 0;
        while (index < _allCode.Length)
        {
            string line = _allCode[index];
            GlobalVariables.LineNumber = index + 1;
            index++;

            string actualLine = RemoveComments(line);

            if (GlobalVariables.LinesToDebug.Contains(GlobalVariables.LineNumber ?? 0))
                Debugger.Break();

            if (actualLine == string.Empty) continue;

            if (GlobalVariables.EasterEggs && GlobalVariables.LineNumber == 42)
            {
                GlobalVariables.LOGGER.Debug("[Line 42] You have discovered the meaning of life. Use it wisely.");
            }

            if (GlobalVariables.EasterEggs &&
                actualLine.Contains("if (life == hard)", StringComparison.CurrentCultureIgnoreCase))
                GlobalVariables.LOGGER.ForceConsoleLog("Suggestion: Have you tried turning it off and on again",
                    addLineNumber: true);

            interpreter.Text = actualLine;


            SingleLine(interpreter.GetAllTokens(), highestLevel: true);
        }
    }
}