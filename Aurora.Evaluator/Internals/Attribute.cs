using System.Diagnostics;
using Aurora.Core;

namespace Aurora.Evaluator.Internals;

public class Attribute(string name, RuntimeType type, Func<RuntimeObject, RuntimeContext, RuntimeObject> valueGetter)
{
    public string Name = name;
    public RuntimeType Type = type;
    public Func<RuntimeObject, RuntimeContext, RuntimeObject> ValueGetter = valueGetter;

    public RuntimeObject GetValue(
        RuntimeObject self,
        RuntimeContext context,
        SourceLocation location)
    {
        RuntimeObject value = this.ValueGetter(self, context);
        if (value.Type.IsSubclassOf(this.Type))
            return value;

        Errors.AlwaysThrow(new TypeMismatchError(
            $"Attribute `{this.Name}` should return an object of type `{this.Type.Name}`, but an object of " +
            $"type `{value.Type.Name}` was returned instead.", user: false), location);
        throw new UnreachableException();
    }
}
