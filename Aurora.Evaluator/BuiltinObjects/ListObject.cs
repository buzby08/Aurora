using Aurora.Evaluator.Internals;

namespace Aurora.Evaluator.BuiltinObjects;

public class ListObject : RuntimeObject
{
    public List<RuntimeObject> Value;
    public RuntimeType ListType;

    public static Func<RuntimeObject, RuntimeContext, RuntimeObject> LengthGetter =>
        (obj, _) => new IntObject(((ListObject)obj).Value.Count);

    public ListObject(List<RuntimeObject> value)
    {
        this.Value = value;
        this.ListType = value.First().Type;
        Type = Builtins.List.Value;
    }

    public IntObject Length => new(Value.Count);

    public override bool Equals(RuntimeObject other)
    {
        if (other is not ListObject listObject)
            return false;

        return Value.SequenceEqual(listObject.Value);
    }
}
