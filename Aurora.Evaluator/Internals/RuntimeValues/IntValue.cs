using Aurora.Core;

namespace Aurora.Evaluator.Internals.RuntimeValues;

public class IntValue(int value) : BaseValue(value)
{
    public int GetValue(SourceLocation? location) => base.GetValue<int>(location);
    internal int RawValue => (int)this.Value!;

    public static IntValue CreateFromString(string value) => new(int.Parse(value));

    public static BooleanValue operator >(IntValue self, IntValue other)
    {
        return new BooleanValue(self.RawValue > other.RawValue);
    }

    public static BooleanValue operator <(IntValue self, IntValue other)
    {
        return new BooleanValue(self.RawValue < other.RawValue);
    }

    public static BooleanValue operator >=(IntValue self, IntValue other)
    {
        return new BooleanValue(self.RawValue >= other.RawValue);
    }

    public static BooleanValue operator <=(IntValue self, IntValue other)
    {
        return new BooleanValue(self.RawValue <= other.RawValue);
    }

    public static BooleanValue operator ==(IntValue self, IntValue other)
    {
        return new BooleanValue(self.RawValue == other.RawValue);
    }

    public static BooleanValue operator !=(IntValue self, IntValue other)
    {
        return new BooleanValue(self.RawValue == other.RawValue);
    }

    public static IntValue operator +(IntValue self, IntValue other)
    {
        return new IntValue(self.RawValue + other.RawValue);
    }

    public static IntValue operator -(IntValue self, IntValue other)
    {
        return new IntValue(self.RawValue - other.RawValue);
    }

    public static IntValue operator *(IntValue self, IntValue other)
    {
        return new IntValue(self.RawValue * other.RawValue);
    }

    public static FloatValue operator /(IntValue self, IntValue other)
    {
        return new FloatValue((decimal)self.RawValue / other.RawValue);
    }

    public static explicit operator IntValue(int value) => new(value);

    public override string ToString() => this.RawValue.ToString();
    public StringValue ToStringValue() => new(this.RawValue.ToString());

    public static RuntimeObject CreateObject(int value) => new RuntimeObject(new IntValue(value), Builtins.Int);

    public override RuntimeObject GetAsRuntimeObject()
    {
        return new RuntimeObject(this, Builtins.Int);
    }

    public int? ToCSharpInt() => this.RawValue;
}
