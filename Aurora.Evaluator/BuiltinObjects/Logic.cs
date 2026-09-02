using Aurora.Evaluator.Internals;

namespace Aurora.Evaluator.BuiltinObjects;

internal class Logic : RuntimeObject
{
    public Logic()
    {
        this.Type = Builtins.Logic;
    }

    public override bool Equals(RuntimeObject other)
    {
        return false;
    }
}
