using Aurora.Internals;
using RuntimeObject = Aurora.Internals.RuntimeObject;

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