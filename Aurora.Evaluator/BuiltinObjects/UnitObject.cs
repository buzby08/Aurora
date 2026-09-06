using Aurora.Evaluator.Internals;

namespace Aurora.Evaluator.BuiltinObjects;

public class UnitObject : RuntimeObject
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