using Aurora.Core;

namespace Aurora.Evaluator.Internals.RuntimeValues;

public class BlockRuntimeValue(IEnumerable<IEnumerable<Ast>> value) : BaseRuntimeValue(ConvertToArray(value))
{
    public override RuntimeObject GetAsRuntimeObject()
    {
        return new RuntimeObject(this, Builtins.Block);
    }

    public Ast[][] GetValue(SourceLocation? location) => base.GetValue<Ast[][]>(location)!;
    public Ast[][] RawValue => (Ast[][])this.Value!;

    public static explicit operator BlockRuntimeValue(Ast[][] value) => new(value);

    public static RuntimeObject CreateObject(Ast[][] value) => new RuntimeObject(new BlockRuntimeValue(value), Builtins.Block);

    private static Ast[][] ConvertToArray(IEnumerable<IEnumerable<Ast>> value) => value.Select(x => x.ToArray()).ToArray();
}
