namespace Aurora.Commands;

internal static class VariablesCommands
{
    // public static Token Create(List<Token> positionals, Dictionary<string, Token> keywords)
    // {
    //     Dictionary<string, Token?> arguments = Parsers.ParseArgs(positionals, keywords,
    //         new Dictionary<string, Type>() { { "type", typeof(StringToken) } }, ["type"], 
    //         provideExtras: true);
    //     
    //     Token? type = arguments.GetValueOrDefault("type");
    //     if (type is null)
    //     {
    //         Errors.RaiseError(new ArgumentDeficitError("Missing required argument type for Variables.Create"),
    //             alwaysThrow: true);
    //         throw new UnreachableException();
    //     }
    //     
    //     
    // }
}