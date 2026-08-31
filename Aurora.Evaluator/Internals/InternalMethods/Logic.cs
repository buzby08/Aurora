using Aurora.Evaluator.BuiltinObjects;

namespace Aurora.Evaluator.Internals.InternalMethods;

public static class Logic
{
    public static UnitObject If(RuntimeContext context)
    {
        BooleanObject conditionObject = context.GetParam<BooleanObject>("condition");
        BlockObject blockObject = context.GetParam<BlockObject>("block");

        if (conditionObject.Value is false)
            return new UnitObject();

        Evaluator evaluator = new(context.CreateChild(context.CallSiteLocation));
        evaluator.EvaluateMultipleExpressions(blockObject.Value);
        return new UnitObject();
    }
}
