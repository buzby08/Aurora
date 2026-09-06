namespace Aurora.Evaluator.Internals;

public class ParameterDefinition(string name, RuntimeType type, bool nullable = false, RuntimeObject? defaultValue = null)
{
    public string Name { get; } = name;
    public RuntimeType Type { get; } = type;
    public bool Nullable { get; } = false;
    public RuntimeObject? DefaultValue { get; } = defaultValue;

    public override bool Equals(object? obj)
    {
        if (obj is not ParameterDefinition other)
            return false;

        if (this.Name != other.Name) return false;
        if (!this.Type.Equals(other.Type)) return false;
        if (this.Nullable != other.Nullable) return false;
        if (this.DefaultValue is not null && other.DefaultValue is null) return false;
        if (!this.DefaultValue?.Equals(other.DefaultValue!) ?? false) return false;

        return true;
    }

    protected bool Equals(ParameterDefinition other)
    {
        return this.Name == other.Name && this.Type.Equals(other.Type) && this.Nullable == other.Nullable && Equals(this.DefaultValue, other.DefaultValue);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(this.Name, this.Type, this.Nullable, this.DefaultValue);
    }
}
