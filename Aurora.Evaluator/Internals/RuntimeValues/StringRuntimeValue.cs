using Aurora.Core;

namespace Aurora.Evaluator.Internals.RuntimeValues;

public class StringRuntimeValue(string value) : BaseRuntimeValue(value)
{
    public string GetValue(SourceLocation? location) => base.GetValue<string>(location);
    public int Length => ((string)this.Value!).Length;
    public string RawValue => (string)this.Value!;

    public string this[int index] => this.RawValue[index].ToString();
    public string this[int start, int end] => this.RawValue[start..end];

    public static StringRuntimeValue operator +(StringRuntimeValue self, StringRuntimeValue other) => new(self.RawValue + other.RawValue);
    public static StringRuntimeValue operator +(StringRuntimeValue self, string other) => new(self.RawValue + other);

    public IntRuntimeValue GetIndexOf(StringRuntimeValue value) => new IntRuntimeValue(this.RawValue.IndexOf(value.RawValue, StringComparison.Ordinal));
    public BooleanRuntimeValue Contains(StringRuntimeValue value) => new BooleanRuntimeValue(this.RawValue.Contains(value.RawValue, StringComparison.Ordinal));

    public string ToCSharpString() => this.RawValue;

    public static explicit operator StringRuntimeValue (string value) => new (value);

    public static RuntimeObject CreateObject(string value) => new RuntimeObject(new StringRuntimeValue(value), Builtins.String);

    public override RuntimeObject GetAsRuntimeObject()
    {
        return new RuntimeObject(this, Builtins.String);
    }
}
