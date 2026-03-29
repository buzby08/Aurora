using Aurora.Internals;

namespace Aurora.BuiltinMethods;

internal class OptionalObject : RuntimeObject
{
    public readonly RuntimeObject? Value;
    public bool HasValue => Value is not null;

    public OptionalObject(RuntimeObject? value)
    {
        Value = value;
        Type = Builtins.Optional;
    }
}