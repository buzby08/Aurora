using Aurora.Core;
using Aurora.Evaluator.BuiltinObjects;
using Aurora.Evaluator.Internals.RuntimeValues;

namespace Aurora.Evaluator.Internals.InternalMethods;

internal static class Type
{
    public static RuntimeObject Create(RuntimeObject self, Dictionary<string, RawMethodArgument> args,
                                    RuntimeContext context)
    {
        TypeObject targetType = (TypeObject)self;

        if (targetType.IsStatic)
            Errors.AlwaysThrow(new UnsupportedOperationError($"{targetType.Name} is static and cannot be instantiated"),
                context.CallSiteLocation);

        foreach (var (_, rawVar) in args)
        {
            using Evaluator evaluator = Evaluator.CreateChild(context.Parent!);
            RuntimeObject variableObject = evaluator.EvaluateExpressionForValue(rawVar.Value);

            if (!variableObject.IsInstanceOf(targetType))
                Errors.AlwaysThrow(
                    new TypeMismatchError(
                        $"{targetType.Name}.create requires `{targetType.Name}`, not `{variableObject.GetInstanceName()}`"),
                    null /* Todo: Try add a better source value*/);

            context.Parent!.Create(rawVar.Name, variableObject, context.CallSiteLocation);
        }

        return UnitRuntimeValue.CreateObject();
    }

    public static RuntimeObject Set(RuntimeObject self, Dictionary<string, RawMethodArgument> args, RuntimeContext context)
    {
        TypeObject targetType = (TypeObject)self;

        foreach (var (_, rawVar) in args)
        {
            using Evaluator evaluator = Evaluator.CreateChild(context.Parent!);
            RuntimeObject variableObject = evaluator.EvaluateExpressionForValue(rawVar.Value);
            if (!variableObject.IsInstanceOf(targetType))
                Errors.AlwaysThrow(
                    new TypeMismatchError(
                        $"{targetType.Name}.set requires `{targetType.Name}`, not `{variableObject.GetInstanceName()}`"),
                    null /* Todo: Try add a better source value*/);

            context.Set(rawVar.Name, variableObject, context.CallSiteLocation);
        }

        return UnitRuntimeValue.CreateObject();
    }

    public static RuntimeObject ToString(RuntimeObject self)
    {
        if (self.IsInstanceOf(Builtins.Type))
            return StringRuntimeValue.CreateObject($"<{self.GetInstanceName()} {self.GetTypeValue().Name}>");

        return StringRuntimeValue.CreateObject($"Object<{self.GetInstanceName()}>");
    }

    public static RuntimeObject Equals(RuntimeObject self, RuntimeContext context)
    {
        RuntimeObject other = context.GetParam("other");

        return BooleanRuntimeValue.CreateObject(self.Equals(other));
    }
}
