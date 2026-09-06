using Aurora.Core;
using Aurora.Evaluator.Internals;

namespace Aurora.Evaluator.BuiltinObjects;

internal class IntObject : RuntimeObject
{
    public int Value;

    public IntObject(int value)
    {
        this.Value = value;
        Type = Builtins.Int;
    }

    public IntObject(string value)
    {
        bool isAnInt = int.TryParse(value, out int intValue);
        if (!isAnInt)
            Errors.AlwaysThrow(new SystemError($"`{value}` is not an integer."), null); // Todo: Try add a source value

        this.Value = intValue;
        Type = Builtins.Int;
    }

    public override bool Equals(RuntimeObject other)
    {
        if (other is IntObject intObject)
            return this.Value == intObject.Value;

        return false;
    }
}