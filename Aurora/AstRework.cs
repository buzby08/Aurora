using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Aurora.Internals;

namespace Aurora;

internal class AstRework
{
    private TokenListItem? Action { get; set; }
    private List<ArgumentRework>? Arguments { get; set; }
    private RuntimeObject? Target { get; set; }
    private IEnumerable<IEnumerable<AstRework>>? BlockValue { get; set; }
    private int LineNumber { get; set; }
    private int ColumnNumber { get; set; }
    public bool IsEmpty => this.Action is null && this.Arguments is null && this.Target is null;
    public bool NoNameWithOtherValues => this.Action is null && (this.Arguments is not null || this.Target is not null);
    public bool IsValid => !this.IsEmpty && !this.NoNameWithOtherValues;

    public void AddAction(TokenListItem name)
    {
        if (this.Action is not null)
            this.ThrowError(new SystemError("Cannot redefine an ast name"));

        this.Action = name;
        UpdatePosition(name.LinePosition, name.StartCharPosition);
    }

    public void AddTarget(RuntimeObject target)
    {
        if (this.Target is not null)
            this.ThrowError(new SystemError("Cannot redefine an ast target"));

        this.Target = target;
    }

    public void AddArgs(List<ArgumentRework> args)
    {
        if (this.Arguments is not null)
            this.ThrowError(new SystemError("Cannot redefine an ast args"));

        this.Arguments = args;
    }

    public void AddBlockValue(IEnumerable<IEnumerable<AstRework>> blockValue)
    {
        if (this.BlockValue is not null)
            this.ThrowError(new SystemError("Cannot redefine an ast block value"));

        if (this.Target is not null || this.Action is not null || this.Arguments is not null)
            this.ThrowError(new SystemError("Cannot add a block value to an ast with other values"));
    }

    private void UpdatePosition(int line, int column)
    {
        if (line < this.LineNumber)
            this.ThrowError(new SystemError("Line numbers cannot decrease"));

        if (column < this.ColumnNumber && line == this.LineNumber)
            this.ThrowError(new SystemError("Column numbers cannot decrease unless moving onto the next line"));

        LineNumber = line;
        ColumnNumber = column;
    }

    [DoesNotReturn]
    private void ThrowError(ErrorTypes error)
    {
        Errors.AlwaysThrow(error, InternalVariables.GlobalContext);
        throw new UnreachableException();
    }

    public override string ToString()
    {
        List<string> messages = [];
        string asString =
            $"AST: ";

        if (Target is not null) messages.Add($"Target: {this.Target}");
        if (Action is not null) messages.Add($"Action: {this.Action?.Token?.Value}");

        if (Arguments is not null)
        {
            string argumentMessage = "Arguments: ";

            argumentMessage += "[";
            foreach (ArgumentRework arg in Arguments)
            {
                string valueAsString = string.Join(',', arg.Value.ConvertAll(ast => ast.ToString()));
                argumentMessage +=
                    $"\n    Name: {arg.Identifier?.AsString() ?? "?"}, Value: {valueAsString}";
            }

            argumentMessage += "  ]";

            messages.Add(argumentMessage);
        }

        if (BlockValue is not null) messages.Add($"Block: {BlockValue.Count()} expressions");

        asString += string.Join(',', messages);

        return asString;
    }
}
