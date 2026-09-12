using System.Diagnostics;
using Aurora.Core;
using Aurora.Evaluator.BuiltinObjects;
using Aurora.Evaluator.Internals.RuntimeValues;

namespace Aurora.Evaluator.Internals.InternalMethods;

internal static class Loop
{
    public static RuntimeObject While(Dictionary<string, RawMethodArgument> args, RuntimeContext context)
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


        if (!body.IsInstanceOf(Builtins.Block))
        {
            Errors.AlwaysThrow(new ArgumentTypeMismatchError($"Argument 2 to {nameof(While)} must be a block"),
                context.CallSiteLocation);
            throw new UnreachableException();
        }

        using Evaluator evaluator = Evaluator.CreateChild(context);
        evaluator.EvaluateWhile(conditionArg.Value, body.GetBlockValue().RawValue);


        return UnitValue.CreateObject();

    }

    public static RuntimeObject For(Dictionary<string, RawMethodArgument> args, RuntimeContext context)
    {
        if (args.Count <= 3)
            Errors.AlwaysThrow(new ArgumentDeficitError($"{nameof(While)} requires four arguments"), context.CallSiteLocation);

        RawMethodArgument initArg = args["ARG_0"];
        RawMethodArgument conditionArg = args["ARG_1"];
        RawMethodArgument incrementArg = args["ARG_2"];
        RawMethodArgument bodyArg = args["ARG_3"];

        RuntimeObject body;

        using (Evaluator evaluatorOne = Evaluator.CreateChild(context))
        {
            body = evaluatorOne.EvaluateExpressionForValue(bodyArg.Value);
        }


        if (!body.IsInstanceOf(Builtins.Block))
        {
            Errors.AlwaysThrow(new ArgumentTypeMismatchError($"Argument 2 to {nameof(While)} must be a block"),
                context.CallSiteLocation);
            throw new UnreachableException();
        }

        using Evaluator evaluator = Evaluator.CreateChild(context);
        evaluator.EvaluateFor(initArg.Value, conditionArg.Value, incrementArg.Value, body.GetBlockValue().RawValue);

        return UnitValue.CreateObject();
    }

    public static RuntimeObject Break()
    {
        Evaluator.ExecuteBreakLoop();
        return UnitValue.CreateObject();
    }

    public static RuntimeObject Continue()
    {
        Evaluator.ExecuteContinueLoop();
        return UnitValue.CreateObject();
    }
}
