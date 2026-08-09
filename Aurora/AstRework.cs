using Aurora.Internals;

namespace Aurora;

internal class AstRework(RuntimeContext context)
{
    private TokenListItem? Name { get; set; }
    private List<Argument>? Arguments { get; set; }
    private RuntimeObject? Target { get; set; }
    private int LineNumber { get; set; }
    private int ColumnNumber { get; set; }
    private RuntimeContext Context { get; set; } = context;
    public bool IsEmpty => this.Name is null && this.Arguments is null && this.Target is null;
    public bool NoNameWithOtherValues => this.Name is null && (this.Arguments is not null || this.Target is not null);
    public bool IsValid => !this.IsEmpty && !this.NoNameWithOtherValues;

    public void AddName(TokenListItem name)
    {
        if (this.Name is not null)
            Errors.AlwaysThrow(new SystemError("Cannot redefine an ast name"),
                this.Context,
                this.ColumnNumber);

        this.Name = name;
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

    public void AddArgs(List<Argument> args)
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
}
