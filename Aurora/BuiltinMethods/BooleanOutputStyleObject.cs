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
}