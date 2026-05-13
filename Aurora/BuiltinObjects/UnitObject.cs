using Aurora.Internals;

namespace Aurora.BuiltinMethods;

internal class UnitObject : RuntimeObject
{
    public UnitObject()
    {
        this.Type = Builtins.Unit;
    }

    public override bool Equals(RuntimeObject other)
    {
        return other is UnitObject;
    }
}