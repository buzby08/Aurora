using Aurora.Evaluator.Internals;

namespace Aurora.Evaluator.BuiltinObjects;

public class ArrayObject : RuntimeObject
{
    public RuntimeObject[] Value;

    public ArrayObject(RuntimeObject[] value)
    {
        this.Value = value;
        Type = Builtins.Array;
    }

    public override bool Equals(RuntimeObject other)
    {
        if (other is not ArrayObject arrayObject)
            return false;

        return Value.SequenceEqual(arrayObject.Value);
    }
}
