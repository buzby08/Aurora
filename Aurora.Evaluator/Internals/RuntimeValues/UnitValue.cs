namespace Aurora.Evaluator.Internals.RuntimeValues;

public class UnitValue() : BaseValue(null)
{
    public static RuntimeObject CreateObject() => new RuntimeObject(new UnitValue(), Builtins.Unit);

    public override RuntimeObject GetAsRuntimeObject()
    {
        return CreateObject();
    }
}
