namespace Aurora.Core;

public class ArgumentRework
{
    public readonly int Id = IdGenerator.GenerateId("ArgumentRework");

    public WordToken? Identifier { get; }
    public List<AstRework> Value { get; }

    public ArgumentRework(List<AstRework> value) : this(null!, value)
    {
    }

    public ArgumentRework(WordToken identifier, List<AstRework> value)
    {
        this.Identifier = identifier;
        this.Value = value;

        if (value.Count == 0)
            Errors.AlwaysThrow(new SystemError("Argument must have at least one value"),
                null);
    }

    public SourceLocation? GetSourceLocation()
    {
        if (this.Identifier is not null)
            return this.Identifier.StartLocation;

        return this.Value.FirstOrDefault()?.GetSourceLocation();
    }

    public override string ToString()
    {
        string valueAsString = string.Join(", ", this.Value.Select(x => x.ToString()));
        if (this.Identifier is not null)
            return $"Argument([#{this.Id}] Identifier: {this.Identifier.ValueAsString}, Value: {valueAsString})";

        return $"Argument([#{this.Id}] Value: {valueAsString})";
    }
}
