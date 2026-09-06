using Aurora.Evaluator.Internals;

namespace Aurora.Evaluator.BuiltinObjects;

internal class LoopObject : RuntimeObject
{
    public LoopObject()
    {
        this.Type = Builtins.Loop;
    }

    public override bool Equals(RuntimeObject other)
    {
        // Todo: Make this throw an error
        return false;
    }
}
