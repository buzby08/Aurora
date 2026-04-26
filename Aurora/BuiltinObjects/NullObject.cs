using Aurora.Internals;
using Type = Aurora.Internals.Type;

namespace Aurora.BuiltinMethods;

internal class NullObject : RuntimeObject
{
    public NullObject()
    {
        Type = Builtins.Null;
    }
}