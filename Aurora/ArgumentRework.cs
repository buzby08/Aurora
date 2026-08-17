namespace Aurora;

internal class ArgumentRework(WordToken identifier, List<AstRework> value)
{
    public WordToken? Identifier { get; init; } = identifier;
    public List<AstRework> Value { get; init; } = value;

    public ArgumentRework(List<AstRework> value) : this(null!, value)
    {
    }
}
