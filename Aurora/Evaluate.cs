using System.Diagnostics;

namespace Aurora;

internal static class Evaluate
{
    public static Token SingleLine(List<Token> tokens)
    {
        string lineAsOutput = PrettyPrint.TokenList(tokens, output: false);
        GlobalVariables.LOGGER.Verbose($"Evaluating line: {lineAsOutput}");
        AST ast = GenerateAst(tokens);
        return ast.Evaluate();
    }

    public static AST GenerateAst(List<Token> tokens)
    {
        AST ast = new AST();
        
        int index = 0;
        while (index < tokens.Count)
        {
            Token currentToken = tokens[index];
            AST.AstItemTypes itemType = ast.ItemType;
            
            index++;

            GlobalVariables.LOGGER.Verbose($"Generating AST: currentToken = {currentToken.ValueAsString}, itemType = {itemType}");

            if (itemType == AST.AstItemTypes.None)
            {
                GlobalVariables.LOGGER.Verbose("AST itemType is None, converting to literal");
                ast.ItemValue = currentToken;
                continue;
            }

            if (itemType == AST.AstItemTypes.Literal && currentToken.Equals(SeparatorToken.DotToken))
            {
                GlobalVariables.LOGGER.Verbose("AST itemType is literal and currentToken is '.'");
                ast.ClassWaitingForAccess = true;
                continue;
            }

            if (itemType == AST.AstItemTypes.ClassReference && ast.ClassWaitingForAccess is true &&
                currentToken.Type == WordToken.TokenType)
            {
                GlobalVariables.LOGGER.Verbose(
                    "AST itemType is classReference and classWaitingForAccess and currentToken is word");
                ast.ClassWaitingForAccess = false;
                ast.ClassAttributeName = currentToken.ValueAsString;
                continue;
            }

            if (itemType == AST.AstItemTypes.AttributeAccess && ast.ClassWaitingForAccess == false &&
                currentToken.Equals(BracketToken.OpenNormal))
            {
                GlobalVariables.LOGGER.Verbose(
                    "AST itemType is AttributeAccess and classWaitingForAccess is false and currentToken is bracket open");
                ast.ClassMethodName = ast.ClassAttributeName;
                ast.ClassAttributeName = null;
                ast.CloseBracketFound = false;
                ast.ClassArguments = new List<AST>();
                (List<List<Token>> allArgs, int endIndex) asArguments = SeparateIntoArguments(tokens, index);

                index = asArguments.endIndex;
                foreach (AST argumentAst in asArguments.allArgs.Select(GenerateAst))
                {
                    ast.ClassArguments.Add(argumentAst);
                }
                
                ast.CloseBracketFound = true;
            }

            if (itemType != AST.AstItemTypes.Literal && itemType != AST.AstItemTypes.Invalid)
            {
                GlobalVariables.LOGGER.Verbose("AST itemType is not literal: evaluating ast to create it as a literal");
                Token evaluatedResponse = ast.Evaluate();
                ast.ResetValues();
                ast.ItemValue = evaluatedResponse;
            }

            if (itemType == AST.AstItemTypes.Literal && currentToken.Equals(EqualsToken.TokenEquals))
            {
                GlobalVariables.LOGGER.Verbose("AST itemType is literal, and currentToken is '='");
                ast.KeywordValue = ast.ItemValue?.AsString();
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
                allArgs.Add(currentArg);
                currentArg.Clear();
            }

            if (currentToken.Equals(BracketToken.OpenNormal))
            {
                bracketDepth++;
                currentArg.Add(currentToken);
            }

            if (currentToken.Equals(BracketToken.CloseNormal) && bracketDepth > 1)
            {
                bracketDepth--;
                currentArg.Add(currentToken);
                continue;
            }

            if (currentToken.Equals(BracketToken.CloseNormal) && bracketDepth == 1)
            {
                allArgs.Add(currentArg);
                return (allArgs, index + 1);
            }
        }

        if (!requireCloseBracket)
        {
            allArgs.Add(currentArg);
            return (allArgs, index + 1);
        }
        
        Errors.AlwaysThrow(new UnclosedDelimiterError("Missing close bracket in expression"));
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