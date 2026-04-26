using Aurora.Internals;
using RuntimeObject = Aurora.Internals.RuntimeObject;

namespace Aurora.BuiltinMethods;

internal class TypeObject : RuntimeObject
{
    public TypeObject()
    {
        this.Type = Builtins.Type;
    }
}