using Aurora.Evaluator.BuiltinObjects;

namespace Aurora.Evaluator.Internals.InternalMethods;

public static class Logic
{
    public static LogicIfReturnObject If(RuntimeContext context)
    {
        BooleanObject conditionObject = context.GetParam<BooleanObject>("condition");
        BlockObject blockObject = context.GetParam<BlockObject>("block");

        if (conditionObject.Value is false)
            return new LogicIfReturnObject(conditionExecuted: false);

        ExecuteBlock(blockObject, context);
        return new LogicIfReturnObject(conditionExecuted: true);
    }

    public static LogicIfReturnObject Else(RuntimeObject self, RuntimeContext context)
    {
        LogicIfReturnObject selfAsLogicIf = (LogicIfReturnObject)self;
        BlockObject blockObject = context.GetParam<BlockObject>("block");

        if (selfAsLogicIf.ConditionExecuted)
            return selfAsLogicIf;

        ExecuteBlock(blockObject, context);
        return new LogicIfReturnObject(conditionExecuted: true);
    }

    private static void ExecuteBlock(BlockObject blockObject, RuntimeContext context)
    {
        Evaluator evaluator = new(context.CreateChild(context.CallSiteLocation));
        evaluator.EvaluateMultipleExpressions(blockObject.Value);
    }
}
