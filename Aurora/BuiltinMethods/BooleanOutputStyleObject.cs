using Aurora.Internals;

namespace Aurora.BuiltinMethods;

internal class BooleanOutputStyleObject : RuntimeObject
{
    public enum Style
    {
        Word,
        YesNo,
        Char,
        Binary,
        OnOff
    }

    public Style Value;

    public BooleanOutputStyleObject(Style value)
    {
        Value = value;
        this.Type = Builtins.BooleanOutputStyles;
    }

    public static bool ReadWordOption() => ReadWord("true", "false");

    public static bool ReadYesNo() => ReadWord("yes", "no");
    
    public static bool ReadOnOff() => ReadWord("on", "off");

    private static bool ReadImmediateChar(char trueOption, char falseOption)
    {
        char? result = null;

        while (result is null || result != trueOption && result != falseOption)
        {
            Console.WriteLine($"Please input either {trueOption} or {falseOption}");
            result = Console.ReadKey().KeyChar;
        }
        
        return result == trueOption;
    }

    private static bool ReadWord(string trueOption, string falseOption)
    {
        string? result = null;

        while (result is null || result != trueOption && result != falseOption)
        {
            Console.WriteLine($"Please input either {trueOption} or {falseOption}");
            result = Console.ReadLine()?.ToLower();
        }
        
        return result == trueOption;
    }

    public static bool ReadChar(bool immediate)
    {
        if (immediate)
            return ReadImmediateChar('y', 'n');
        
        return ReadWord("y", "n");
    }
    public static bool ReadBinary(bool immediate)
    {
        if (immediate)
            return ReadImmediateChar('1', '0');
        
        return ReadWord("1", "0");
    }
}