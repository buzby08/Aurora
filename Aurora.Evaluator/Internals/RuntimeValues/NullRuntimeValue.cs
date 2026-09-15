namespace Aurora.Evaluator.Internals.RuntimeValues;

public class NullRuntimeValue() : BaseRuntimeValue(null)
{
    public static RuntimeObject CreateObject() => new RuntimeObject(new NullRuntimeValue(), Builtins.Null);

    public override RuntimeObject GetAsRuntimeObject()
    {
        return CreateObject();
    }
}
