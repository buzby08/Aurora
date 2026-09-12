using Aurora.Core;

namespace Aurora.Evaluator.Internals.RuntimeValues;

public class BooleanValue(bool value) : BaseValue(value)
{
    public override RuntimeObject GetAsRuntimeObject()
    {
        return new RuntimeObject(this, Builtins.Boolean);
    }

    public bool GetValue(SourceLocation? location) => base.GetValue<bool>(location);
    public bool RawValue => (bool)this.Value!;

    public bool AsCSharpBool => this.RawValue;

    public static explicit operator BooleanValue(bool value) => new(value);
    public StringValue ToStringValue() => new(this.RawValue ? "true" : "false");

    public static implicit operator bool(BooleanValue value) => value.AsCSharpBool;

    public static RuntimeObject CreateObject(bool value) => new RuntimeObject(new BooleanValue(value), Builtins.Boolean);
}
