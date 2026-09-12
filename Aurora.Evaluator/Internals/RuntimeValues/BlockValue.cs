using Aurora.Core;

namespace Aurora.Evaluator.Internals.RuntimeValues;

public class BlockValue(IEnumerable<IEnumerable<Ast>> value) : BaseValue(ConvertToArray(value))
{
    public override RuntimeObject GetAsRuntimeObject()
    {
        return new RuntimeObject(this, Builtins.Block);
    }

    public Ast[][] GetValue(SourceLocation? location) => base.GetValue<Ast[][]>(location)!;
    public Ast[][] RawValue => (Ast[][])this.Value!;

    public static explicit operator BlockValue(Ast[][] value) => new(value);

    public static RuntimeObject CreateObject(Ast[][] value) => new RuntimeObject(new BlockValue(value), Builtins.Block);

    private static Ast[][] ConvertToArray(IEnumerable<IEnumerable<Ast>> value) => value.Select(x => x.ToArray()).ToArray();
}
