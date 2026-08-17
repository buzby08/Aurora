using Aurora.Internals;

namespace Aurora;

internal class AstRework(RuntimeContext context)
{
    private TokenListItem? Action { get; set; }
    private List<ArgumentRework>? Arguments { get; set; }
    private RuntimeObject? Target { get; set; }
    private int LineNumber { get; set; }
    private int ColumnNumber { get; set; }
    private RuntimeContext Context { get; set; } = context;
    public bool IsEmpty => this.Action is null && this.Arguments is null && this.Target is null;
    public bool NoNameWithOtherValues => this.Action is null && (this.Arguments is not null || this.Target is not null);
    public bool IsValid => !this.IsEmpty && !this.NoNameWithOtherValues;

    public void AddAction(TokenListItem name)
    {
        if (this.Action is not null)
            Errors.AlwaysThrow(new SystemError("Cannot redefine an ast name"),
                this.Context,
                this.ColumnNumber);

        this.Action = name;
        UpdatePosition(name.LinePosition, name.StartCharPosition);
    }

    public void AddTarget(RuntimeObject target)
    {
        if (this.Target is not null)
            Errors.AlwaysThrow(new SystemError("Cannot redefine an ast target"),
                this.Context,
                this.ColumnNumber);

        this.Target = target;
    }

    public void AddArgs(List<ArgumentRework> args)
    {
        if (this.Arguments is not null)
            Errors.AlwaysThrow(new SystemError("Cannot redefine an ast args"),
                this.Context,
                this.ColumnNumber);

        this.Arguments = args;
    }

    private void UpdatePosition(int line, int column)
    {
        if (line < this.LineNumber)
            Errors.AlwaysThrow(new SystemError("Line numbers cannot decrease"), this.Context,
                position: this.ColumnNumber);

        if (column < this.ColumnNumber && line == this.LineNumber)
            Errors.AlwaysThrow(new SystemError("Column numbers cannot decrease unless moving onto the next line"),
                this.Context,
                position: this.ColumnNumber);

        LineNumber = line;
        ColumnNumber = column;
    }

    public override string ToString()
    {
        string asString =
            $"AST: Target: `{this.Target?.ToString() ?? "?"}`, Action: `{this.Action?.Token?.Value?.ToString() ?? "?"}`, " +
            $"  Args: ";

        if (Arguments is null) return asString + "null";

        asString += "[";
        foreach (ArgumentRework arg in Arguments)
        {
            string valueAsString = string.Join(',', arg.Value.ConvertAll(ast => ast.ToString()));
            asString +=
                $"\n    Name: {arg.Identifier?.AsString() ?? "?"}, Value: {valueAsString}";
        }

        return asString + "  ]";
    }
}
