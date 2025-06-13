namespace Aurora;

internal static class PrettyPrint
{
    public static string? List(object list, bool output = true)
    {
        string? message = string.Empty;
        
        if (list is not string and IEnumerable<object> collection)
        {
            message += "[";
            bool first = true;
    
            foreach (var item in collection)
            {
                if (!first) message += ", ";
                message += List(item, output: false);
                first = false;
            }

            message += "]";
        }
        else
        {
            message = list.ToString();
        }
        
        if (output)
            Console.WriteLine(message);
        
        return message;
    }

    public static string TokenList(List<Token> list, bool output = true)
    {
        string asString = string.Empty;

        foreach (Token token in list)
        {
            asString += token.ValueAsString;
        }

        if (output)
            Console.WriteLine(asString);
        return asString;
    }
}
