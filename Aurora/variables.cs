using System.Diagnostics;
using Aurora.Commands;

namespace Aurora;

internal static class Variables
{
    private static string _owner = "Aurora.Variables";

    public static Dictionary<string, Token> UserDefined =>
        Memory.Get("UserDefined", _owner)?.Value ?? new Dictionary<string, Token>();

    public static Dictionary<string, Token> SystemDefined =>
        Memory.Get("SystemDefined", _owner)?.Value ?? new Dictionary<string, Token>();

    private static void AddSystemVariable(string name, Token value)
    {
        Dictionary<string, Token> systemDefined = SystemDefined;
        systemDefined.TryAdd(name, value);
        Memory.Update("SystemDefined", _owner, systemDefined);
    }

    public static void ModifySystemVariable(string name, Token newValue)
    {
        Dictionary<string, Token> systemDefined = SystemDefined;
        systemDefined[name] = newValue;
        Memory.Update("SystemDefined", _owner, systemDefined);
    }

    public static void RegisterSystemVariables()
    {
        StringToken platform = new StringToken().Initialise(
            System.Runtime.InteropServices.RuntimeInformation.OSDescription,
            withoutQuotes: true, interpolate: false);
        StringToken cwd =
            new StringToken().Initialise(Directory.GetCurrentDirectory(), withoutQuotes: true, interpolate: false);
        IntegerToken lineNumber = new IntegerToken().Initialise(0);
        StringToken user = new StringToken().Initialise(Environment.UserName, withoutQuotes: true, interpolate: false);
        StringToken systemHome =
            new StringToken().Initialise(Environment.SystemDirectory, withoutQuotes: true, interpolate: false);
        StringToken userHome = new StringToken().Initialise(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), withoutQuotes: true, interpolate: false);
        StringToken lang = new StringToken().Initialise(System.Globalization.CultureInfo.CurrentCulture.Name,
            withoutQuotes: true, interpolate: false);
        StringToken banana = new StringToken().Initialise("🍌", withoutQuotes: true, interpolate: false);

        AddSystemVariable("__PLATFORM__", platform);
        AddSystemVariable("__CWD__", cwd);
        AddSystemVariable("__LINE_NUMBER__", lineNumber);
        AddSystemVariable("__USER__", user);
        AddSystemVariable("__SYSTEM_HOME__", systemHome);
        AddSystemVariable("__USER_HOME__", userHome);
        AddSystemVariable("__LANG__", lang);
        AddSystemVariable("banana", banana);
    }

    public static bool IsVariable(string x) => UserDefined.ContainsKey(x) || SystemDefined.ContainsKey(x);

    public static Token GetVariable(string x, Token? defaultValue = null)
    {
        if (SystemDefined.TryGetValue(x, out var variable) || UserDefined.TryGetValue(x, out variable))
        {
            return variable;
        }

        return defaultValue ?? Errors.AlwaysThrow<Token>(new VarNotDefinedError($"The variable '{x}' is not defined."));
    }

    private static bool AddUserVariable(string name, Token value)
    {
        Dictionary<string, Token> userDefined = UserDefined;

        bool result = userDefined.TryAdd(name, value);
        Memory.Update("UserDefined", _owner, userDefined);
        return result;
    }

    public static void RegisterVariable(string name, Token value)
    {
        if (GlobalVariables.EasterEggs && name.Contains("banana", StringComparison.CurrentCultureIgnoreCase))
            GlobalVariables.LOGGER.Warning("Variable names containing 'banana' are slippery: proceed with caution.");

        if (AddUserVariable(name, value))
            return;

        Token currentVariable = GetVariable(name);
        if (currentVariable.Type != value.Type)
            Errors.AlwaysThrow(new TypeMismatchError($"`{value.Type}` is not assignable to `{currentVariable.Type}`."));

        UserDefined[name] = value;
    }

    public static void InitialiseVariables()
    {
        MemoryItem systemDefined = new MemoryItem
        {
            Owner = _owner,
            Name = "SystemDefined",
            Value = new Dictionary<string, Token>()
        };

        MemoryItem userDefined = new MemoryItem
        {
            Owner = _owner,
            Name = "UserDefined",
            Value = new Dictionary<string, Token>()
        };

        Memory.Save(systemDefined);
        Memory.Save(userDefined);
        RegisterSystemVariables();
    }
}