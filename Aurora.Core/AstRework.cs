using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Aurora.Core;

public class AstRework
{
    public readonly int Id = IdGenerator.GenerateId("AstRework");
    private TokenListItem? Action { get; set; }
    private IImmutableList<ArgumentRework>? Arguments { get; set; }
    private IEnumerable<IEnumerable<AstRework>>? BlockValue { get; set; }
    public bool IsEmpty => this.Action is null && this.Arguments is null;
    public bool NoNameWithOtherValues => this.Action is null && this.Arguments is not null;
    public bool IsValid => !this.IsEmpty && !this.NoNameWithOtherValues;

    public void AddAction(TokenListItem name)
    {
        if (this.Action is not null)
            this.ThrowError(new SystemError("Cannot redefine an ast name"));

        this.Action = name;
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

        if (this.Action is not null || this.Arguments is not null)
            this.ThrowError(new SystemError("Cannot add a block value to an ast with other values"));


        this.BlockValue = blockValue;
    }

    [DoesNotReturn]
    private void ThrowError(ErrorTypes error)
    {
        Errors.AlwaysThrow(error, InternalVariables.GetEmptySourceLocation());
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

        if (this.Action is not null) messages.Add($"{color}Action: {this.Action?.Token?.Value}");

        if (this.Arguments is not null)
        {
            List<string> args = this.Arguments.Select(x => x.ToString()).ToList();
            messages.Add($"{color}[" + string.Join($"{color} ,", args) + $"{color}]");
        }

        if (this.BlockValue is not null) messages.Add($"{color}Block: {this.BlockValue.Count()} expressions");

        asString += string.Join($"{color}, ", messages);

        return asString + $" {color}){resetColor}";
    }
}
