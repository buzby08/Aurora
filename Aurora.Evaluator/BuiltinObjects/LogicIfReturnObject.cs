using Aurora.Evaluator.Internals;

namespace Aurora.Evaluator.BuiltinObjects;

public class LogicIfReturnObject : RuntimeObject
{
    public bool ConditionExecuted;

    public LogicIfReturnObject(bool conditionExecuted)
    {
        this.Type = Builtins.LogicIfReturn;
        this.ConditionExecuted = conditionExecuted;
    }

    public override bool Equals(RuntimeObject other)
    {
        return false;
    }
}
