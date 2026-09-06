using Aurora.Evaluator.Internals;
using Type = Aurora.Evaluator.Internals.Type;

namespace Aurora.Evaluator.BuiltinObjects;

public class ArrayObject : RuntimeObject
{
    public RuntimeObject[] Value;
    public Type ArrayType;

    public static Func<RuntimeObject, RuntimeContext, RuntimeObject> LengthGetter =>
        (obj, _) => new IntObject(((ArrayObject)obj).Value.Length);

    public ArrayObject(RuntimeObject[] value)
    {
        this.Value = value;
        this.ArrayType = value.First().Type;
        Type = Builtins.Array;
    }

    public override bool Equals(RuntimeObject other)
    {
        if (other is not ArrayObject arrayObject)
            return false;

        return Value.SequenceEqual(arrayObject.Value);
    }
}
