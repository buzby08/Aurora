using Aurora.Evaluator.Internals;

namespace Aurora.Evaluator.BuiltinObjects;

internal class NullObject : RuntimeObject
{
    public NullObject()
    {
        this.Type = Builtins.Null;
    }
    
    public override bool Equals(RuntimeObject other)
    {
        return other is NullObject;
    }
}