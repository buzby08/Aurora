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
}