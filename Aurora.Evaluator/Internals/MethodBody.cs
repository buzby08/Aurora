using Aurora.Core;

namespace Aurora.Evaluator.Internals;

public delegate RuntimeObject MethodBody(
    RuntimeObject self,
    Dictionary<string, RawMethodArgument> args,
    RuntimeContext context);

public class RawMethodArgument(string name, Ast[] value)
{
    public string Name = name;
    public Ast[] Value = value;
}
