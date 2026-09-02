using Aurora.Core;
using Aurora.Evaluator.BuiltinObjects;

namespace Aurora.Evaluator.Internals.InternalMethods;

internal static class Type
{
    public static UnitObject Create(RuntimeObject self, Dictionary<string, RawMethodArgument> args,
                                    RuntimeContext context)
    {
        Internals.Type targetType = (Internals.Type)self;

        foreach (var (_, rawVar) in args)
        {
            using Evaluator evaluator = Evaluator.CreateChild(context.Parent!);
            RuntimeObject variableObject = evaluator.EvaluateExpressionForValue(rawVar.Value);

            if (variableObject.Type != targetType)
                Errors.AlwaysThrow(
                    new TypeMismatchError(
                        $"{targetType.Name}.create requires `{targetType.Name}`, not `{variableObject.Type.Name}`"),
                    null /* Todo: Try add a better source value*/);

            context.Parent!.Create(rawVar.Name, variableObject, context.CallSiteLocation);
        }

        return new UnitObject();
    }

    public static UnitObject Set(RuntimeObject self, Dictionary<string, RawMethodArgument> args, RuntimeContext context)
    {
        Internals.Type targetType = (Internals.Type)self;

        foreach (var (_, rawVar) in args)
        {
            using Evaluator evaluator = Evaluator.CreateChild(context.Parent!);
            RuntimeObject variableObject = evaluator.EvaluateExpressionForValue(rawVar.Value);
            if (variableObject.Type != targetType)
                Errors.AlwaysThrow(
                    new TypeMismatchError(
                        $"{targetType.Name}.set requires `{targetType.Name}`, not `{variableObject.Type.Name}`"),
                    null /* Todo: Try add a better source value*/);

            context.Set(rawVar.Name, variableObject, context.CallSiteLocation);
        }

        return new UnitObject();
    }

    public static StringObject ToString(RuntimeObject self)
    {
        if (self is Internals.Type selfType)
            return new StringObject($"<{self.Type.Name} {selfType.Name}>");

        return new StringObject($"Object<{self.Type.Name}>");
    }

    public static BooleanObject Equals(RuntimeObject self, RuntimeContext context)
    {
        RuntimeObject other = context.GetParam("other");

        return new BooleanObject(self.Equals(other));
    }
}
