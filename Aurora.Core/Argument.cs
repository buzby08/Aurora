namespace Aurora.Core;

public class Argument
{
    public readonly int Id = IdGenerator.GenerateId("Argument");

    public WordToken? Identifier { get; }
    public Ast[] Value { get; }

    public Argument(IEnumerable<Ast> value) : this(null!, value)
    {
    }

    public Argument(WordToken identifier, IEnumerable<Ast> value)
    {
        Ast[] valueArr = value.ToArray();
        this.Identifier = identifier;
        this.Value = valueArr;

        if (valueArr.Length == 0)
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
