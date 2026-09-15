using Aurora.Core;

namespace Aurora.Evaluator.Internals.RuntimeValues;

public class BooleanRuntimeValue(bool value) : BaseRuntimeValue(value)
{
    public override RuntimeObject GetAsRuntimeObject()
    {
        return new RuntimeObject(this, Builtins.Boolean);
    }

    public bool GetValue(SourceLocation? location) => base.GetValue<bool>(location);
    public bool RawValue => (bool)this.Value!;

    public bool AsCSharpBool => this.RawValue;

    public static explicit operator BooleanRuntimeValue(bool value) => new(value);
    public StringRuntimeValue ToStringValue() => new(this.RawValue ? "true" : "false");

    public static implicit operator bool(BooleanRuntimeValue value) => value.AsCSharpBool;

    public static RuntimeObject CreateObject(bool value) => new RuntimeObject(new BooleanRuntimeValue(value), Builtins.Boolean);
}
