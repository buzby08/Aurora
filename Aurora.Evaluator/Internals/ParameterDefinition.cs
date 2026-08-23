namespace Aurora.Internals;

internal class ParameterDefinition(string name, Type type, bool nullable = false, RuntimeObject? defaultValue = null)
{
    public string Name { get; } = name;
    public Type Type { get; } = type;
    public bool Nullable { get; } = false;
    public RuntimeObject? DefaultValue { get; } = defaultValue;
}