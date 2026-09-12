using System.Globalization;
using Aurora.Core;

namespace Aurora.Evaluator.Internals.RuntimeValues;

public class FloatValue(decimal value) : BaseValue(value)
{
    public override RuntimeObject GetAsRuntimeObject()
    {
        return new RuntimeObject(this, Builtins.Boolean);
    }

    public decimal GetValue(SourceLocation? location) => base.GetValue<decimal>(location);
    public decimal RawValue => (decimal)this.Value!;


    public static explicit operator FloatValue(decimal value) => new(value);
    public static explicit operator FloatValue(int value) => new(value);

    public static FloatValue CreateFromString(string value) => new(decimal.Parse(value, CultureInfo.InvariantCulture));

    public StringValue ToStringValue()
    {
        string value = this.RawValue.ToString(CultureInfo.InvariantCulture);

        if (value.EndsWith(".0"))
            value = value[..^2];

        return new StringValue(value);
    }


    public static RuntimeObject CreateObject(decimal value) => new RuntimeObject(new FloatValue(value), Builtins.Float);

    public decimal ToCSharpDecimal() => this.RawValue;
}
