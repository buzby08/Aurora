using Aurora.Evaluator.Internals;

namespace Aurora.Evaluator.BuiltinObjects;

internal class TerminalObject : RuntimeObject
{
    public string? ErrorMessage;

    public TerminalObject(string? errorMessage)
    {
        this.ErrorMessage = errorMessage;
        this.Type = Builtins.Terminal;
    }

    public override bool Equals(RuntimeObject other)
    {
        return other is TerminalObject;
    }
}