using Aurora.Core;

namespace Aurora.Evaluator.Internals.RuntimeValues;

public class TypeRuntimeValue(RuntimeType value) : BaseRuntimeValue(value)
{
    public RuntimeType GetValue(SourceLocation? location) => base.GetValue<RuntimeType>(location)!;
    public RuntimeType RawValue => (RuntimeType)this.Value!;

    public RuntimeType AsCSharpRuntimeType => this.RawValue;

    public string Name => this.RawValue.Name;

    public static RuntimeObject GetAsObject(RuntimeType value) => new(new TypeRuntimeValue(value), Builtins.Type);

    public override RuntimeObject GetAsRuntimeObject()
    {
        return new RuntimeObject(this, Builtins.Type);
    }
}
