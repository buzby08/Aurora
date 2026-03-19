using Aurora.Internals;

namespace Aurora.BuiltinMethods;

internal class UnitObject : RuntimeObject
{
    public UnitObject()
    {
        this.Type = Builtins.Unit;
    }
}