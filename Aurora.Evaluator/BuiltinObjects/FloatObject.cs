using Aurora.Core;
using Aurora.Evaluator.Internals;

namespace Aurora.Evaluator.BuiltinObjects;

internal class FloatObject : RuntimeObject
{
    public decimal Value { get; }

    public FloatObject(decimal value)
    {
        this.Value = value;
        this.Type = Builtins.Float;
    }

    public FloatObject(float value)
    {
        this.Value = (decimal)value;
        this.Type = Builtins.Float;
    }

    public FloatObject(string value)
    {
        bool isAFloatValue = decimal.TryParse(value, out decimal floatValue);

        if (!isAFloatValue)
            Errors.AlwaysThrow(new SystemError($"SE_002 `{value}` is not a valid float."),
                null); // Todo: Try add a source value

        this.Value = floatValue;

        this.Type = Builtins.Float;
    }

    public override bool Equals(RuntimeObject other)
    {
        if (other is FloatObject floatObject)
            return this.Value == floatObject.Value;

        return false;
    }
}