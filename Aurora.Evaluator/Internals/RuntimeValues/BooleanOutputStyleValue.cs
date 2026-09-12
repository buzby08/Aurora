namespace Aurora.Evaluator.Internals.RuntimeValues;

public class BooleanOutputStyleValue(BooleanOutputStyleValue.Style value) : BaseValue(value)
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


    public static BooleanValue ReadWordOption() => ReadWord("true", "false");

    public static BooleanValue ReadYesNo() => ReadWord("yes", "no");

    public static BooleanValue ReadOnOff() => ReadWord("on", "off");

    private static BooleanValue ReadImmediateChar(char trueOption, char falseOption)
    {
        char? result = null;

        while (result is null || result != trueOption && result != falseOption)
        {
            Console.WriteLine($"Please input either {trueOption} or {falseOption}");
            result = Console.ReadKey().KeyChar;
        }
        Console.Write(Environment.NewLine);

        return new BooleanValue(result == trueOption);
    }

    private static BooleanValue ReadWord(string trueOption, string falseOption)
    {
        string? result = null;

        while (result is null || result != trueOption && result != falseOption)
        {
            Console.WriteLine($"Please input either {trueOption} or {falseOption}");
            result = Console.ReadLine()?.ToLower();
        }

        return new BooleanValue(result == trueOption);
    }

    public static BooleanValue ReadChar(bool immediate)
    {
        if (immediate)
            return ReadImmediateChar('y', 'n');

        return ReadWord("y", "n");
    }
    public static BooleanValue ReadBinary(bool immediate)
    {
        if (immediate)
            return ReadImmediateChar('1', '0');

        return ReadWord("1", "0");
    }

    public override RuntimeObject GetAsRuntimeObject()
    {
        return new RuntimeObject(this, Builtins.BooleanOutputStyles);
    }

    public static RuntimeObject CreateObject(Style value) => new RuntimeObject(new BooleanOutputStyleValue(value), Builtins.BooleanOutputStyles);
}
