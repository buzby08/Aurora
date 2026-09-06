using Aurora.Evaluator.Internals;

namespace Aurora.Evaluator.BuiltinObjects;

internal class BooleanObject : RuntimeObject
{
    public bool Value;

    public BooleanObject(bool value)
    {
        this.Value = value;
        Type = Builtins.Boolean;
    }

    public override bool Equals(RuntimeObject other)
    {
        if (other is BooleanObject booleanObject)
            return this.Value == booleanObject.Value;

        return false;
    }
}