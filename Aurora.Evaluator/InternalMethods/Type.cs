using Aurora.BuiltinMethods;
using Aurora.Internals;

namespace Aurora.InternalMethods;

internal static class Type
{
    public static UnitObject Create(RuntimeObject self, Dictionary<string, RawMethodArgument> args,
                                    RuntimeContext context)
    {
        Internals.Type targetType = (Internals.Type)self;

        foreach (var (_, rawVar) in args)
        {
            RuntimeObject variableObject = Evaluator.EvaluateAstList(rawVar.Value, context.Parent!);

            if (variableObject.Type != targetType)
                Errors.AlwaysThrow(
                    new TypeMismatchError(
                        $"{targetType.Name}.create requires `{targetType.Name}`, not `{variableObject.Type.Name}`"),
                    context, position: rawVar.ValuePosition);

            context.Parent!.Create(rawVar.Name, variableObject);
        }

        return new UnitObject();
    }

    public static UnitObject Set(RuntimeObject self, Dictionary<string, RawMethodArgument> args, RuntimeContext context)
    {
        Internals.Type targetType = (Internals.Type)self;

        foreach (var (_, rawVar) in args)
        {
            RuntimeObject variableObject = Evaluator.EvaluateAstList(rawVar.Value, context.Parent!);
            if (variableObject.Type != targetType)
                Errors.AlwaysThrow(
                    new TypeMismatchError(
                        $"{targetType.Name}.set requires `{targetType.Name}`, not `{variableObject.Type.Name}`"),
                    context, position: rawVar.ValuePosition);

            context.Set(rawVar.Name, variableObject);
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
        RuntimeObject other = context.Get("other");

        return new BooleanObject(self.Equals(other));
    }
}