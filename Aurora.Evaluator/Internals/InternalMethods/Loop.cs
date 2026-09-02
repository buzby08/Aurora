using System.Diagnostics;
using Aurora.Core;
using Aurora.Evaluator.BuiltinObjects;

namespace Aurora.Evaluator.Internals.InternalMethods;

internal static class Loop
{
    public static UnitObject While(Dictionary<string, RawMethodArgument> args, RuntimeContext context)
    {
        if (args.Count <= 1)
            Errors.AlwaysThrow(new ArgumentDeficitError($"{nameof(While)} requires at two arguments"), context.CallSiteLocation);

        RawMethodArgument conditionArg = args["ARG_0"];
        RawMethodArgument bodyArg = args["ARG_1"];

        RuntimeObject body;

        using (Evaluator evaluatorOne = Evaluator.CreateChild(context))
        {
            body = evaluatorOne.EvaluateExpressionForValue(bodyArg.Value);
        }

        if (body is not BlockObject blockObject)
            {
                Errors.AlwaysThrow(new ArgumentTypeMismatchError($"Argument 2 to {nameof(While)} must be a block"),
                    context.CallSiteLocation);
                throw new UnreachableException();
            }

        while (EvaluateCondition(conditionArg, context))
        {
            using Evaluator evaluatorTwo = Evaluator.CreateChild(context);
            evaluatorTwo.EvaluateMultipleExpressions(blockObject.Value);
        }


        return new UnitObject();

    }

    private static bool EvaluateCondition(RawMethodArgument condition, RuntimeContext context)
    {
        using Evaluator evaluator = Evaluator.CreateChild(context);
        RuntimeObject evaluatedObject = evaluator.EvaluateExpressionForValue(condition.Value);

        if (evaluatedObject is BooleanObject booleanObject) return booleanObject.Value;

        Errors.AlwaysThrow(
            new UnsupportedOperationError($"Argument 1 to {nameof(While)} must evaluate be a boolean"),
            context.CallSiteLocation);
        throw new UnreachableException();
    }

    public static UnitObject Break(RuntimeContext context)
    {
        return new UnitObject();
    }
}
