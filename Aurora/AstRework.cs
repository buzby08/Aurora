using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Aurora.Internals;

namespace Aurora;

internal class AstRework
{
    public readonly int Id = IdGenerator.GenerateId("AstRework");
    private TokenListItem? Action { get; set; }
    private IImmutableList<ArgumentRework>? Arguments { get; set; }
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

    public void AddArgs(IImmutableList<ArgumentRework> args)
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


        this.BlockValue = blockValue;
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
        bool useColor = !Debugger.IsAttached;
        int colorCode = Random.Shared.Next(1, 231);
        string color = useColor ? $"\e[38;5;{colorCode}m" : "";
        string resetColor = useColor ? "\e[0m" : "";
        List<string> messages = [];

        string asString =
            $"{color}AST([#{this.Id}] ";

        if (Target is not null) messages.Add($"{color}Target: {this.Target}");
        if (Action is not null) messages.Add($"{color}Action: {this.Action?.Token?.Value}");

        if (Arguments is not null)
        {
            List<string> args = Arguments.Select(x => x.ToString()).ToList();
            messages.Add($"{color}[" + string.Join($"{color} ,", args) + $"{color}]");
        }

        if (BlockValue is not null) messages.Add($"{color}Block: {BlockValue.Count()} expressions");

        asString += string.Join($"{color}, ", messages);

        return asString + $" {color}){resetColor}";
    }
}
