namespace Aurora.Evaluator.Internals.RuntimeValues;

public class NullValue() : BaseValue(null)
{
    public static RuntimeObject CreateObject() => new RuntimeObject(new NullValue(), Builtins.Null);

    public override RuntimeObject GetAsRuntimeObject()
    {
        return CreateObject();
    }
}
