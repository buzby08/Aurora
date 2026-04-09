using System.Diagnostics;
using System.Globalization;
using Aurora.BuiltinMethods;
using Aurora.Internals;

namespace Aurora;

internal static class MathFunctions
{
    public static FloatObject Truncate(RuntimeContext context)
    {
        IntObject placesObject = (IntObject)context.GetParam("places");
        FloatObject valueObject = (FloatObject)context.GetParam("value");
        int places = placesObject.Value;
        decimal value = valueObject.Value;

        return new FloatObject(Truncate(places, value));
    }

    public static string Truncate(int places, decimal value)
    {
        string valueAsString = value.ToString(CultureInfo.InvariantCulture);

        if (!valueAsString.Contains('.'))
            valueAsString += ".0";

        string[] parts = valueAsString.Split('.');
        string valueBeforeDecimalString = parts[0];
        string valueAfterDecimalString = parts[1];

        bool isPositivePlaces = places > 0;
        bool valueAfterDecimalLengthValid = valueAfterDecimalString.Length > places;
        bool valueBeforeDecimalLengthValid = valueBeforeDecimalString.Length > places*-1;

        if (isPositivePlaces && valueAfterDecimalLengthValid)
        {
            string right = valueAfterDecimalString[..places];

            return valueBeforeDecimalString + '.' + right;
        }

        if (!isPositivePlaces &&valueBeforeDecimalLengthValid)
        {
            char[] left = valueBeforeDecimalString.ToArray();
            int indexToKeep = valueBeforeDecimalString.Length - 1 + places;

            for (int i = indexToKeep + 1; i < left.Length; i++)
            {
                left[i] = '0';
            }

            string leftAsString = new string(left);
            return leftAsString;
        }

        if (!isPositivePlaces && !valueBeforeDecimalLengthValid)
            return "0";

        if (isPositivePlaces && !valueAfterDecimalLengthValid)
            return value.ToString(CultureInfo.InvariantCulture);
        
        throw new UnreachableException();
    }

    public static FloatObject Round(RuntimeContext context)
    {
        IntObject placesObject = (IntObject)context.GetParam("places");
        FloatObject valueObject = (FloatObject)context.GetParam("value");
        
        return new FloatObject(Round(placesObject.Value, valueObject.Value));
    }

    public static string Round(int places, decimal value)
    {
        string valueAsString = value.ToString(CultureInfo.InvariantCulture);

        if (!valueAsString.Contains('.'))
            valueAsString += ".0";

        string[] parts = valueAsString.Split('.');
        string valueBeforeDecimalString = parts[0];
        char[] left = valueBeforeDecimalString.ToCharArray();
        string valueAfterDecimalString = parts[1];
        char[] right = valueAfterDecimalString.ToCharArray();

        bool isPositivePlaces = places > 0;
        bool placesTooLarge = places >= right.Length;
        bool placesTooSmall = places * -1 > left.Length;

        if (placesTooLarge) return valueAsString;
        if (placesTooSmall) return "0";

        int placesFromStart = left.Length - 1 + places;

        int decimalLocation = left.Length;
    }
}