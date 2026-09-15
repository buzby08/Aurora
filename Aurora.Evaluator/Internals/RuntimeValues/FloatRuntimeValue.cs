using System.Globalization;
using Aurora.Core;

namespace Aurora.Evaluator.Internals.RuntimeValues;

public class FloatRuntimeValue(decimal value) : BaseRuntimeValue(value)
{
    public override RuntimeObject GetAsRuntimeObject()
    {
        return new RuntimeObject(this, Builtins.Float);
    }

    public decimal GetValue(SourceLocation? location) => base.GetValue<decimal>(location);
    public decimal RawValue => (decimal)this.Value!;


    public static explicit operator FloatRuntimeValue(decimal value) => new(value);
    public static explicit operator FloatRuntimeValue(int value) => new(value);

    public static FloatRuntimeValue CreateFromString(string value) => new(decimal.Parse(value, CultureInfo.InvariantCulture));

    public StringRuntimeValue ToStringValue()
    {
        string value = this.RawValue.ToString(CultureInfo.InvariantCulture);

        if (value.EndsWith(".0"))
            value = value[..^2];

        return new StringRuntimeValue(value);
    }


    public static RuntimeObject CreateObject(decimal value) => new RuntimeObject(new FloatRuntimeValue(value), Builtins.Float);

    public decimal ToCSharpDecimal() => this.RawValue;
}
