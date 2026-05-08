using Aurora.Internals;

namespace Aurora.BuiltinMethods;

internal class TerminalObject : RuntimeObject
{
    public string? ErrorMessage;

    public TerminalObject(string? errorMessage)
    {
        this.ErrorMessage = errorMessage;
        this.Type = Builtins.Terminal;
    }
}