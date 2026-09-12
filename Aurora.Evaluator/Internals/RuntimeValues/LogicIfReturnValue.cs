using Aurora.Core;

namespace Aurora.Evaluator.Internals.RuntimeValues;

public class LogicIfReturnValue(bool value) : BaseValue(value)
{
    public override RuntimeObject GetAsRuntimeObject()
    {
        return new RuntimeObject(this, Builtins.LogicIfReturn);
    }

    public bool GetValue(SourceLocation? location) => base.GetValue<bool>(location);
    public bool RawValue => (bool)this.Value!;
    public bool AsCSharpBool => this.RawValue;


    public static explicit operator LogicIfReturnValue(bool value) => new(value);

    public static RuntimeObject CreateObject(bool value) => new RuntimeObject(new LogicIfReturnValue(value), Builtins.LogicIfReturn);
}
