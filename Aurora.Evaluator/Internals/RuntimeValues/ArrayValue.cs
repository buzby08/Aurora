using Aurora.Core;

namespace Aurora.Evaluator.Internals.RuntimeValues;

public class ArrayValue(RuntimeObject[] value) : BaseRuntimeValue(value)
{
    public override RuntimeObject GetAsRuntimeObject()
    {
        return new RuntimeObject(this, Builtins.Array);
    }

    public RuntimeObject[] GetValue(SourceLocation? location) => base.GetValue<RuntimeObject[]>(location)!;
    public RuntimeObject[] RawValue => (RuntimeObject[])this.Value!;

    public IntRuntimeValue Length => new IntRuntimeValue(this.RawValue.Length);
    public TypeRuntimeValue Type => new TypeRuntimeValue(this.RawValue.First().GetRuntimeType());


    public static explicit operator ArrayValue(RuntimeObject[] value) => new(value);
    public RuntimeObject this[int index] => this.RawValue[index];

    public StringRuntimeValue ToStringValue() => new($"Array<type: {this.Type.Name}, length: {this.Length}>");

    public static RuntimeObject CreateObject(RuntimeObject[] value) => new RuntimeObject(new ArrayValue(value), Builtins.Array);
}
