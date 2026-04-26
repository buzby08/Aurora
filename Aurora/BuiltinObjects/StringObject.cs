using Aurora.Internals;
using RuntimeObject = Aurora.Internals.RuntimeObject;
using Type = Aurora.Internals.Type;

namespace Aurora.BuiltinMethods;

internal class StringObject : RuntimeObject
{
    public string Value;

    public StringObject(string value)
    {
        this.Value = value;
        Type = Builtins.String;
    }
}