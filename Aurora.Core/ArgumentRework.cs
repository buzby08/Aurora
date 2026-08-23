namespace Aurora.Core;

public class ArgumentRework(WordToken identifier, List<AstRework> value)
{
    public readonly int Id = IdGenerator.GenerateId("ArgumentRework");
    public WordToken? Identifier { get; init; } = identifier;
    public List<AstRework> Value { get; init; } = value;

    public ArgumentRework(List<AstRework> value) : this(null!, value)
    {
    }

    public override string ToString()
    {
        string valueAsString = string.Join(", ", this.Value.Select(x => x.ToString()));
        if (this.Identifier is not null)
            return $"Argument([#{this.Id}] Identifier: {this.Identifier.ValueAsString}, Value: {valueAsString})";

        return $"Argument([#{this.Id}] Value: {valueAsString})";
    }
}
