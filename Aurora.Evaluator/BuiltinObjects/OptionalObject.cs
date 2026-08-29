using Aurora.Evaluator.Internals;

namespace Aurora.Evaluator.BuiltinObjects;

internal class OptionalObject : RuntimeObject
{
    public readonly RuntimeObject? Value;
    public bool HasValue => this.Value is not null;
    public bool CreatedFromEmpty { get; }

    public OptionalObject(RuntimeObject? value, bool createdFromEmpty = false)
    {
        this.Value = value;
        this.Type = Builtins.Optional;
        this.CreatedFromEmpty = createdFromEmpty;
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