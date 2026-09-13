namespace Aurora.Evaluator.Internals.RuntimeValues;

public class UnitRuntimeValue() : BaseRuntimeValue(null)
{
    public static RuntimeObject CreateObject() => new RuntimeObject(new UnitRuntimeValue(), Builtins.Unit);

    public override RuntimeObject GetAsRuntimeObject()
    {
        return CreateObject();
    }
}
