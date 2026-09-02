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

        using Evaluator evaluator = Evaluator.CreateChild(context);
        evaluator.EvaluateWhile(conditionArg.Value, blockObject);


        return new UnitObject();

    }

    public static UnitObject Break()
    {
        Evaluator.ExecuteBreakLoop();
        return new UnitObject();
    }

    public static UnitObject Continue()
    {
        Evaluator.ExecuteContinueLoop();
        return new UnitObject();
    }
}
