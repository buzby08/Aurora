using Aurora.Evaluator.Internals;

namespace Aurora.Evaluator.BuiltinObjects;

internal class TypeObject : RuntimeObject
{
    public TypeObject()
    {
        this.Type = Builtins.Type;
    }

    public override bool Equals(RuntimeObject other)
    {
        return other is TypeObject;
    }
}