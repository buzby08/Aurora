namespace Aurora.Core;

public class Arguement
{
    public readonly int Id = IdGenerator.GenerateId("Arguement");

    public WordToken? Identifier { get; }
    public List<Ast> Value { get; }

    public Arguement(List<Ast> value) : this(null!, value)
    {
    }

    public Arguement(WordToken identifier, List<Ast> value)
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
