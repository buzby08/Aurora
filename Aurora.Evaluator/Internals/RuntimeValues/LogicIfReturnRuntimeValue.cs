using Aurora.Core;

namespace Aurora.Evaluator.Internals.RuntimeValues;

public class LogicIfReturnRuntimeValue(bool value) : BaseRuntimeValue(value)
{
    public override RuntimeObject GetAsRuntimeObject()
    {
        return new RuntimeObject(this, Builtins.LogicIfReturn);
    }

    public bool GetValue(SourceLocation? location) => base.GetValue<bool>(location);
    public bool RawValue => (bool)this.Value!;
    public bool AsCSharpBool => this.RawValue;


    public static explicit operator LogicIfReturnRuntimeValue(bool value) => new(value);

    public static RuntimeObject CreateObject(bool value) => new RuntimeObject(new LogicIfReturnRuntimeValue(value), Builtins.LogicIfReturn);
}
