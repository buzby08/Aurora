using Aurora.Core;

namespace Aurora.Evaluator.Internals.RuntimeValues;

public class StringValue(string value) : BaseValue(value)
{
    public string GetValue(SourceLocation? location) => base.GetValue<string>(location);
    public int Length => ((string)this.Value!).Length;
    public string RawValue => (string)this.Value!;

    public string this[int index] => this.RawValue[index].ToString();
    public string this[int start, int end] => this.RawValue[start..end];

    public static StringValue operator +(StringValue self, StringValue other) => new(self.RawValue + other.RawValue);
    public static StringValue operator +(StringValue self, string other) => new(self.RawValue + other);

    public IntValue GetIndexOf(StringValue value) => new IntValue(this.RawValue.IndexOf(value.RawValue, StringComparison.Ordinal));
    public BooleanValue Contains(StringValue value) => new BooleanValue(this.RawValue.Contains(value.RawValue, StringComparison.Ordinal));

    public string ToCSharpString() => this.RawValue;

    public static explicit operator StringValue (string value) => new (value);

    public static RuntimeObject CreateObject(string value) => new RuntimeObject(new StringValue(value), Builtins.String);

    public override RuntimeObject GetAsRuntimeObject()
    {
        return new RuntimeObject(this, Builtins.String);
    }
}
