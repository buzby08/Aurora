using Aurora.Core;

namespace Aurora.Evaluator.Internals.RuntimeValues;

public class IntRuntimeValue(int value) : BaseRuntimeValue(value)
{
    public int GetValue(SourceLocation? location) => base.GetValue<int>(location);
    internal int RawValue => (int)this.Value!;

    public static IntRuntimeValue CreateFromString(string value) => new(int.Parse(value));

    public static BooleanRuntimeValue operator >(IntRuntimeValue self, IntRuntimeValue other)
    {
        return new BooleanRuntimeValue(self.RawValue > other.RawValue);
    }

    public static BooleanRuntimeValue operator <(IntRuntimeValue self, IntRuntimeValue other)
    {
        return new BooleanRuntimeValue(self.RawValue < other.RawValue);
    }

    public static BooleanRuntimeValue operator >=(IntRuntimeValue self, IntRuntimeValue other)
    {
        return new BooleanRuntimeValue(self.RawValue >= other.RawValue);
    }

    public static BooleanRuntimeValue operator <=(IntRuntimeValue self, IntRuntimeValue other)
    {
        return new BooleanRuntimeValue(self.RawValue <= other.RawValue);
    }

    public static BooleanRuntimeValue operator ==(IntRuntimeValue self, IntRuntimeValue other)
    {
        return new BooleanRuntimeValue(self.RawValue == other.RawValue);
    }

    public static BooleanRuntimeValue operator !=(IntRuntimeValue self, IntRuntimeValue other)
    {
        return new BooleanRuntimeValue(self.RawValue == other.RawValue);
    }

    public static IntRuntimeValue operator +(IntRuntimeValue self, IntRuntimeValue other)
    {
        return new IntRuntimeValue(self.RawValue + other.RawValue);
    }

    public static IntRuntimeValue operator -(IntRuntimeValue self, IntRuntimeValue other)
    {
        return new IntRuntimeValue(self.RawValue - other.RawValue);
    }

    public static IntRuntimeValue operator *(IntRuntimeValue self, IntRuntimeValue other)
    {
        return new IntRuntimeValue(self.RawValue * other.RawValue);
    }

    public static FloatRuntimeValue operator /(IntRuntimeValue self, IntRuntimeValue other)
    {
        return new FloatRuntimeValue((decimal)self.RawValue / other.RawValue);
    }

    public static explicit operator IntRuntimeValue(int value) => new(value);

    public override string ToString() => this.RawValue.ToString();
    public StringRuntimeValue ToStringValue() => new(this.RawValue.ToString());

    public static RuntimeObject CreateObject(int value) => new RuntimeObject(new IntRuntimeValue(value), Builtins.Int);

    public override RuntimeObject GetAsRuntimeObject()
    {
        return new RuntimeObject(this, Builtins.Int);
    }

    public int? ToCSharpInt() => this.RawValue;
}
