namespace Aurora.Internals;

internal class ParameterDefinition(string name, Type type, RuntimeObject? defaultValue = null)
{
    public string Name { get; } = name;
    public Type Type { get; } = type;
    public RuntimeObject? DefaultValue { get; } = defaultValue;
}