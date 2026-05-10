using Aurora.Internals;

namespace Aurora.BuiltinMethods;

internal class NullObject : RuntimeObject
{
    public NullObject()
    {
        Type = Builtins.Null;
    }
}