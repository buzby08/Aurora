using Aurora.Evaluator.BuiltinObjects;
using Aurora.Evaluator.Internals.RuntimeValues;

namespace Aurora.Evaluator.Internals.InternalMethods;

public static class Logic
{
    public static RuntimeObject If(RuntimeContext context)
    {
        BooleanValue conditionObject = context.GetParam("condition").GetBooleanValue();
        BlockValue blockObject = context.GetParam("block").GetBlockValue();

        if (!conditionObject)
            return LogicIfReturnValue.CreateObject(false);

        ExecuteBlock(blockObject, context);
        return LogicIfReturnValue.CreateObject(true);
    }

    public static RuntimeObject Else(RuntimeObject self, RuntimeContext context)
    {
        LogicIfReturnValue selfAsLogicIf = self.GetLogicIfReturnValue();
        BlockValue blockObject = context.GetParam("block").GetBlockValue();

        if (selfAsLogicIf.AsCSharpBool)
            return selfAsLogicIf.GetAsRuntimeObject();

        ExecuteBlock(blockObject, context);
        return LogicIfReturnValue.CreateObject(true);
    }

    private static void ExecuteBlock(BlockValue blockValue, RuntimeContext context)
    {
        RuntimeContext blockContext = context.CreateChild(context.CallSiteLocation);
        using Evaluator evaluator = Evaluator.CreateChild(blockContext, Evaluator.EvaluatorState.Block);
        evaluator.EvaluateMultipleExpressions(blockValue.RawValue);
    }
}
