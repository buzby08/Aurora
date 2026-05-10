using Aurora.Internals;
using RuntimeObject = Aurora.Internals.RuntimeObject;

namespace Aurora.BuiltinMethods;

internal class IntObject : RuntimeObject
{
    public int Value;

    public IntObject(int value)
    {
        Value = value;
        Type = Builtins.Int;
    }

    public IntObject(string value)
    {
        bool isAnInt = int.TryParse(value, out int intValue);
        if (!isAnInt)
            Errors.AlwaysThrow(new SystemError($"`{value}` is not an integer."), null);

        Value = intValue;
        Type = Builtins.Int;
    }
}