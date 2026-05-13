using Aurora.Internals;

namespace Aurora.BuiltinMethods;

internal class OptionalObject : RuntimeObject
{
    public readonly RuntimeObject? Value;
    public bool HasValue => Value is not null;
    public bool CreatedFromEmpty { get; }

    public OptionalObject(RuntimeObject? value, bool createdFromEmpty = false)
    {
        Value = value;
        Type = Builtins.Optional;
        CreatedFromEmpty = createdFromEmpty;
    }
    
    public override bool Equals(RuntimeObject other)
    {
        if (other is not OptionalObject optionalObject)
            return false;

        if (this.Value is null ^ optionalObject.Value is null) return false;

        if (this.Value is null && optionalObject.Value is null) return true;

        return this.Value!.Equals(optionalObject.Value!);
    }
}