namespace Aurora.Evaluator.Internals.RuntimeValues;

public class BooleanOutputStyleRuntimeValue(BooleanOutputStyleRuntimeValue.Style value) : BaseRuntimeValue(value)
{
    public enum Style
    {
        Word,
        YesNo,
        Char,
        Binary,
        OnOff,
    }

    public Style RawValue => (Style)this.Value!;

    public Style AsCSharpStyle => this.RawValue;


    public static BooleanRuntimeValue ReadWordOption() => ReadWord("true", "false");

    public static BooleanRuntimeValue ReadYesNo() => ReadWord("yes", "no");

    public static BooleanRuntimeValue ReadOnOff() => ReadWord("on", "off");

    private static BooleanRuntimeValue ReadImmediateChar(char trueOption, char falseOption)
    {
        char? result = null;

        while (result is null || result != trueOption && result != falseOption)
        {
            Console.WriteLine($"Please input either {trueOption} or {falseOption}");
            result = Console.ReadKey().KeyChar;
        }
        Console.Write(Environment.NewLine);

        return new BooleanRuntimeValue(result == trueOption);
    }

    private static BooleanRuntimeValue ReadWord(string trueOption, string falseOption)
    {
        string? result = null;

        while (result is null || result != trueOption && result != falseOption)
        {
            Console.WriteLine($"Please input either {trueOption} or {falseOption}");
            result = Console.ReadLine()?.ToLower();
        }

        return new BooleanRuntimeValue(result == trueOption);
    }

    public static BooleanRuntimeValue ReadChar(bool immediate)
    {
        if (immediate)
            return ReadImmediateChar('y', 'n');

        return ReadWord("y", "n");
    }
    public static BooleanRuntimeValue ReadBinary(bool immediate)
    {
        if (immediate)
            return ReadImmediateChar('1', '0');

        return ReadWord("1", "0");
    }

    public override RuntimeObject GetAsRuntimeObject()
    {
        return new RuntimeObject(this, Builtins.BooleanOutputStyles);
    }

    public static RuntimeObject CreateObject(Style value) => new RuntimeObject(new BooleanOutputStyleRuntimeValue(value), Builtins.BooleanOutputStyles);
}
