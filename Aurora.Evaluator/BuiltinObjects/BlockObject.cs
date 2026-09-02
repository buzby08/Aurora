using Aurora.Core;
using Aurora.Evaluator.Internals;

namespace Aurora.Evaluator.BuiltinObjects;

public class BlockObject : RuntimeObject
{
    public Ast[][] Value;

    public BlockObject(IEnumerable<IEnumerable<Ast>> value)
    {
        this.Value = this.ConvertToArray(value);
        Type = Builtins.Block;
    }

    private Ast[][] ConvertToArray(IEnumerable<IEnumerable<Ast>> value) => value.Select(x => x.ToArray()).ToArray();

    public override bool Equals(RuntimeObject other)
    {
        return false;
    }
}
