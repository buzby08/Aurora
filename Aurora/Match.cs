using System.Reflection;
namespace Aurora;

internal static class Match
{
    public static List<Token> SplitToTokens(string line)
    {
        Interpreter interpreter = new()
        {
            Text = line
        };
        return interpreter.GetAllTokens();
    }

    public static bool CheckType(Token a, Type b)
    {
        string? otherTokenType = b.GetField("TokenType", BindingFlags.Static | BindingFlags.Public)?.GetValue(null) as string;
        if (otherTokenType is null) throw new MissingFieldException("Other token does not have the required field");
        return a.Type == otherTokenType || otherTokenType == BaseToken.TokenType;
    }

    public static bool MatchTokens(List<Token> tokens, List<(List<Type>, int)> sequence)
    {
        int tokenIndex = 0;
        int sequenceIndex = 0;

        while (sequenceIndex < sequence.Count)
        {
            (List<Type> expected, int count) = sequence[sequenceIndex];

            bool isFixedCount = count >= 1;
            bool isGreedy = count == -1;
            bool isOptional = count == 0;

            if (isFixedCount)
            {
                for (int _ = 0; _ < count; _++)
                {
                    if (tokenIndex >= tokens.Count) return false;

                    if (!expected.Any(e => CheckType(tokens[tokenIndex], e))) return false;

                    tokenIndex++;
                }
            }
            else if (isOptional)
            {
                bool tokenLengthValid = tokenIndex < tokens.Count;
                if (!tokenLengthValid) return false;
                bool tokenFound = expected.Any(e => CheckType(tokens[tokenIndex], e));
                if (tokenLengthValid && tokenFound) tokenIndex++;
            }
            else if (isGreedy)
            {
                int greedyStart = tokenIndex;
                bool tokenIndexValid = tokenIndex < tokens.Count;
                
                while (
                    tokenIndexValid
                    && expected.Any(e => CheckType(tokens[tokenIndex], e))
                )
                {
                    tokenIndex++;
                    tokenIndexValid = tokenIndex < tokens.Count;
                }

                for (int backtrack = tokenIndex; backtrack > greedyStart; backtrack--)
                {
                    List<Token> remainingTokens = tokens[backtrack..];
                    List<(List<Type>, int)> remainingSequence = sequence[(sequenceIndex+1)..];

                    if (MatchTokens(remainingTokens, remainingSequence)) return true;
                }

                return false;
            }

            sequenceIndex++;
        }

        return tokenIndex == tokens.Count;
    }

    public static bool ExactMatch(List<Token> tokens, List<Token> expectedTokens)
    {
        if (tokens.Count != expectedTokens.Count) return false;
        for (int i = 0; i < tokens.Count; i++)
        {
            var currentToken = tokens[i];
            var expectedToken = expectedTokens[i];
            
            if (!expectedToken.Equals(currentToken)) return false;
        }
        
        return true;
    }
    
}