using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Aurora.Core;

public class Ast
{
    public readonly int Id = IdGenerator.GenerateId("Ast");

    private Token? Action { get; set; }
    private IImmutableList<Arguement>? Arguments { get; set; }

    private IEnumerable<IEnumerable<Ast>>? BlockValue { get; set; }

    public void AddAction(Token name)
    {
        if (this.Action is not null)
            this.ThrowError(new SystemError("Cannot redefine an ast name"));

        if (this.BlockValue is not null)
            this.ThrowError(new SystemError("Cannot add an action to an ast with a block value"));

        this.Action = name;
    }

    public void AddArgs(IImmutableList<Arguement> args)
    {
        if (this.Arguments is not null)
            this.ThrowError(new SystemError("Cannot redefine an ast's arguments"));

        if (this.Action is null)
            this.ThrowError(new SystemError("Cannot add arguments to an ast without an action"));

        this.Arguments = args;
    }

    public void AddBlockValue(IEnumerable<IEnumerable<Ast>> blockValue)
    {
        if (this.BlockValue is not null)
            this.ThrowError(new SystemError("Cannot redefine an ast block value"));

        if (this.Action is not null || this.Arguments is not null)
            this.ThrowError(new SystemError("Cannot add a block value to an ast with other values"));


        this.BlockValue = blockValue;
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

        if (this.Action is not null) messages.Add($"{color}Action: {this.Action?.Value}");

        if (this.Arguments is not null)
        {
            List<string> args = this.Arguments.Select(x => x.ToString()).ToList();
            messages.Add($"{color}[" + string.Join($"{color} ,", args) + $"{color}]");
        }

        if (this.BlockValue is not null) messages.Add($"{color}Block: {this.BlockValue.Count()} expressions");

        asString += string.Join($"{color}, ", messages);

        return asString + $" {color}){resetColor}";
    }

    public SourceLocation? GetSourceLocation()
    {
        if (Action is not null)
            return Action?.StartLocation;

        if (BlockValue is not null)
            return BlockValue.FirstOrDefault()?.FirstOrDefault()?.GetSourceLocation();

        return null;
    }

    [DoesNotReturn]
    private void ThrowError(ErrorTypes error)
    {
        Errors.AlwaysThrow(error, this.GetSourceLocation());
        throw new UnreachableException();
    }
}
