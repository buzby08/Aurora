using System.Diagnostics;
using Aurora.Commands;
namespace Aurora;

internal static class Variables
{
    public static Dictionary<string, Token> UserDefined = new();

    public static readonly Dictionary<string, Token> SYSTEM_DEFINED = new()
    {
        {
            "__PLATFORM__",
            new StringToken().Initialise(System.Runtime.InteropServices.RuntimeInformation.OSDescription,
                withoutQuotes: true, interpolate: false)
        },
        {
            "__CWD__",
            new StringToken().Initialise(Directory.GetCurrentDirectory(), withoutQuotes: true, interpolate: false)
        },
        { "__LINE_NUMBER__", new IntegerToken().Initialise(new CustomInt(GlobalVariables.LineNumber ?? 0)) },
        { "__USER__", new StringToken().Initialise(Environment.UserName, withoutQuotes: true, interpolate: false) },
        {
            "__SYSTEM_HOME__",
            new StringToken().Initialise(Environment.SystemDirectory, withoutQuotes: true, interpolate: false)
        },
        {
            "__USER_HOME__",
            new StringToken().Initialise(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                withoutQuotes: true, interpolate: false)
        },
        {
            "__LANG__",
            new StringToken().Initialise(System.Globalization.CultureInfo.CurrentCulture.Name, withoutQuotes: true,
                interpolate: false)
        },
        {
            "banana",
            new StringToken().Initialise("🍌", withoutQuotes: true, interpolate: false)
        }
    };
    
    public static bool IsVariable(string x) => UserDefined.ContainsKey(x) || SYSTEM_DEFINED.ContainsKey(x);

    public static Token GetVariable(string x)
    {
        if (SYSTEM_DEFINED.TryGetValue(x, out var variable) || UserDefined.TryGetValue(x, out variable))
        {
            return variable;
        }

        return Errors.AlwaysThrow<Token>(new VarNotDefinedError($"The variable '{x}' is not defined."));
    }

    public static void RegisterVariable(string name, Token value)
    {
        UserDefined.Add(name, value);
        
        if (name.Contains("banana", StringComparison.CurrentCultureIgnoreCase))
            GlobalVariables.LOGGER.Warning("Variable names containing 'banana' are slippery: proceed with caution.");
    }
}