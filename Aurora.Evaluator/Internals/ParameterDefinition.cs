namespace Aurora.Evaluator.Internals;

public class ParameterDefinition(string name, RuntimeType type, bool nullable = false, RuntimeObject? defaultValue = null)
{
    public string Name { get; } = name;
    public RuntimeType Type { get; } = type;
    public bool Nullable { get; } = false;
    public RuntimeObject? DefaultValue { get; } = defaultValue;
}