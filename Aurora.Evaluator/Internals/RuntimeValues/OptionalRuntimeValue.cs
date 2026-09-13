using Aurora.Core;

namespace Aurora.Evaluator.Internals.RuntimeValues;

public class OptionalRuntimeValue(RuntimeObject? value) : BaseRuntimeValue(value)
{
    public RuntimeObject? GetValue(SourceLocation? location) => base.GetValue<RuntimeObject>(location);
    public bool HasValue => this.Value != null;
    public RuntimeObject? RawValue => (RuntimeObject?)this.Value;

    public static RuntimeObject CreateObject(RuntimeObject? value) => new RuntimeObject(new OptionalRuntimeValue(value), Builtins.Optional);

    public override RuntimeObject GetAsRuntimeObject()
    {
        return new RuntimeObject(this, Builtins.Optional);
    }
}
