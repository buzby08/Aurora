using Aurora.Core;

namespace Aurora.Evaluator.Internals.RuntimeValues;

public class ArrayValue(RuntimeObject[] value) : BaseValue(value)
{
    public override RuntimeObject GetAsRuntimeObject()
    {
        return new RuntimeObject(this, Builtins.Array);
    }

    public RuntimeObject[] GetValue(SourceLocation? location) => base.GetValue<RuntimeObject[]>(location)!;
    public RuntimeObject[] RawValue => (RuntimeObject[])this.Value!;

    public IntValue Length => new IntValue(this.RawValue.Length);
    public TypeValue Type => new TypeValue(this.RawValue.First().GetRuntimeType());


    public static explicit operator ArrayValue(RuntimeObject[] value) => new(value);
    public RuntimeObject this[int index] => this.RawValue[index];

    public StringValue ToStringValue() => new($"Array<type: {this.Type.Name}, length: {this.Length}>");

    public static RuntimeObject CreateObject(RuntimeObject[] value) => new RuntimeObject(new ArrayValue(value), Builtins.Array);
}
