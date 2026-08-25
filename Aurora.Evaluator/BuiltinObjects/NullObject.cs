using Aurora.Internals;

namespace Aurora.BuiltinMethods;

internal class NullObject : RuntimeObject
{
    public NullObject()
    {
        Type = Builtins.Null;
    }
    
    public override bool Equals(RuntimeObject other)
    {
        return other is NullObject;
    }
}