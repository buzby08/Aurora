using Aurora.Internals;
using RuntimeObject = Aurora.Internals.RuntimeObject;
using Type = Aurora.Internals.Type;

namespace Aurora.BuiltinMethods;

internal class BooleanObject : RuntimeObject
{
    public bool Value;

    public BooleanObject(bool value)
    {
        Value = value;
        Type = Builtins.Boolean;
    }
}