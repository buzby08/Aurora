using Aurora.Evaluator.BuiltinObjects;
using Aurora.Evaluator.Internals.RuntimeValues;

namespace Aurora.Evaluator.Internals.InternalMethods;

public static class Logic
{
    public static RuntimeObject If(RuntimeContext context)
    {
        BooleanRuntimeValue conditionObject = context.GetParam("condition").GetBooleanValue();
        BlockRuntimeValue blockObject = context.GetParam("block").GetBlockValue();

        if (!conditionObject)
            return LogicIfReturnRuntimeValue.CreateObject(false);

        ExecuteBlock(blockObject, context);
        return LogicIfReturnRuntimeValue.CreateObject(true);
    }

    public static RuntimeObject Else(RuntimeObject self, RuntimeContext context)
    {
        LogicIfReturnRuntimeValue selfAsLogicIf = self.GetLogicIfReturnValue();
        BlockRuntimeValue blockObject = context.GetParam("block").GetBlockValue();

        if (selfAsLogicIf.AsCSharpBool)
            return selfAsLogicIf.GetAsRuntimeObject();

        ExecuteBlock(blockObject, context);
        return LogicIfReturnRuntimeValue.CreateObject(true);
    }

    private static void ExecuteBlock(BlockRuntimeValue blockValue, RuntimeContext context)
    {
        RuntimeContext blockContext = context.CreateChild(context.CallSiteLocation);
        using Evaluator evaluator = Evaluator.CreateChild(blockContext, Evaluator.EvaluatorState.Block);
        evaluator.EvaluateMultipleExpressions(blockValue.RawValue);
    }
}
