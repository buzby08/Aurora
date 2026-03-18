using Aurora.Internals;

namespace Aurora.BuiltinMethods;

internal class FloatObject : RuntimeObject
{
    public decimal Value { get; }

    public FloatObject(decimal value)
    {
        Value = value;
        Type = Builtins.Float;
    }
    
    public FloatObject(float value)
    {
        Value = (decimal)value;
        Type = Builtins.Float;
    }

    public FloatObject(string value)
    {
        bool isAFloatValue = decimal.TryParse(value, out decimal floatValue);

        if (!isAFloatValue)
            Errors.AlwaysThrow(new SystemError($"`{value}` is not a valid float."));

        Value = floatValue;

        Type = Builtins.Float;
    }
}