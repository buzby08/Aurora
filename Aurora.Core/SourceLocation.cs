namespace Aurora.Core;

public class SourceLocation
{
    public readonly int Id = IdGenerator.GenerateId("SourceLocation");
    public required string FilePath { get; init; }
    public required int LineNumber { get; init; }
    public required int ColumnNumber { get; init; }
    public required int Offset { get; init; }

    public override string ToString() => $"{nameof(SourceLocation)}({this.FilePath} {this.LineNumber}:{this.ColumnNumber} (offset: {this.Offset})";
}
