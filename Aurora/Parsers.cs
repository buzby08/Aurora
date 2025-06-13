namespace Aurora;

internal static class Parsers
{
   public static Dictionary<string, Token?> ParseArgs(List<Token> positionals, Dictionary<string, Token> keywords, 
       Dictionary<string, Type> expectedArguments, List<string> positionalOrder, bool provideExtras = false)
    {
        Dictionary<string, Token?> result = new();
    
        HashSet<string> processedPositionalArgs = [];
        HashSet<string> processedKeywordArgs = [];
    
        foreach (string arg in expectedArguments.Keys)
        {
            int positionalIndex = positionalOrder.IndexOf(arg);
            Type expectedType = expectedArguments[arg];
    
            Token? positionalValue = positionals.ElementAtOrDefault(positionalIndex);
            Token? keywordValue = keywords.GetValueOrDefault(arg);
            Token? actualValue = null;
    
            if (positionalValue is not null && keywordValue is not null)
            {
                actualValue = positionalValue;
                Errors.RaiseError(
                    new ArgumentSurplusError($"Ignoring positional value for '{arg}' (using keyword instead)"));
            }
    
            actualValue ??= positionalValue;
            actualValue ??= keywordValue;
    
            if (actualValue is not null && !Match.CheckType(actualValue, expectedType))
            {
                Errors.RaiseError(
                    new TypeMismatchError($"Parameter '{arg}' expected to be of the type '{expectedType}'"));
            }
    
            result[arg] = actualValue;
    
            if (positionalValue is not null)
                processedPositionalArgs.Add(arg);
            if (keywordValue is not null)
                processedKeywordArgs.Add(arg);
        }
    
        if (provideExtras)
        {
            for (int i = 0; i < positionals.Count; i++)
            {
                if (!processedPositionalArgs.Contains(positionalOrder[i]))
                {
                    result[$"extra_positional_{i}"] = positionals[i];
                }
            }
    
            foreach (var keyword in keywords.Keys)
            {
                if (!processedKeywordArgs.Contains(keyword))
                {
                    result[keyword] = keywords[keyword];
                }
            }
        }
    
        return result;
    }

}