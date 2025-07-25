using System.Diagnostics;
using System.Globalization;
using System.Numerics;

namespace Aurora;

internal class CustomInt
{
    private object _value;

    public object Value
    {
        get => this._value;
        set => this._value = Convert((string)value);
    }

    public CustomFloat AsFloat => new CustomFloat(this._value.ToString());

    public static bool TryParse(string input, out CustomInt? result)
    {
        if (int.TryParse(input, out int intResult))
        {
            result = new CustomInt(intResult, dontConvert: true);
            return true;
        }

        if (long.TryParse(input, out long longResult))
        {
            result = new CustomInt(longResult, dontConvert: true);
            return true;
        }

        if (BigInteger.TryParse(input, out BigInteger bigResult))
        {
            result = new CustomInt(bigResult, dontConvert: true);
            return true;
        }

        result = null;
        return false;
    }

    public static object Convert(string value)
    {
        if (TryParse(value, out var result))
        {
            return result!.Value;
        }

        Errors.AlwaysThrow(new OutOfRangeError("The provided integer value is too big or too small to support"));
        throw new UnreachableException();
    }

    public static CustomInt operator +(CustomInt a, CustomInt b)
    {
        if (a.Value is not int or long or BigInteger || b.Value is not int or long or BigInteger)
        {
            Errors.RaiseError(new SystemError("Attempting to add 2 ints together that do not support addition"));
            throw new UnreachableException();
        }

        return (a.Value, b.Value) switch
        {
            (int ia, int ib) => new CustomInt(ia + ib),
            (int ia, long ib) => new CustomInt(ia + ib),
            (long ia, int ib) => new CustomInt(ia + ib),
            (long ia, long ib) => new CustomInt(ia + ib),
            (int ia, BigInteger ib) => new CustomInt(ia + ib),
            (BigInteger ia, int ib) => new CustomInt(ia + ib),
            (long ia, BigInteger ib) => new CustomInt(ia + ib),
            (BigInteger ia, long ib) => new CustomInt(ia + ib),
            (BigInteger ia, BigInteger ib) => new CustomInt(ia + ib),

            _ => throw new UnreachableException()
        };
    }

    public static CustomFloat operator +(CustomInt a, CustomFloat b)
    {
        if (a.Value is not int or long or BigInteger || b.Value is not float or double or decimal)
        {
            Errors.RaiseError(
                new SystemError("Attempting to add int and float together that do not support addition"));
            throw new UnreachableException();
        }

        return (a.Value, b.Value) switch
        {
            (int ia, float fa) => new CustomFloat(ia + fa),
            (int ia, double da) => new CustomFloat(ia + da),
            (int ia, decimal ma) => new CustomFloat(ia + (double)ma),

            (long la, float fa) => new CustomFloat(la + fa),
            (long la, double da) => new CustomFloat(la + da),
            (long la, decimal ma) => new CustomFloat(la + (double)ma),

            (BigInteger bi, float fa) => new CustomFloat((double)bi + fa),
            (BigInteger bi, double da) => new CustomFloat((double)bi + da),
            (BigInteger bi, decimal ma) => new CustomFloat((double)bi + (double)ma),

            _ => throw new UnreachableException()
        };
    }

    public static CustomFloat operator +(CustomFloat a, CustomInt b)
    {
        if (a.Value is not float or double or decimal || b.Value is not int or long or BigInteger)
        {
            Errors.RaiseError(
                new SystemError("Attempting to add int and float together that do not support addition"));
            throw new UnreachableException();
        }

        return (a.Value, b.Value) switch
        {
            (float fa, int ia) => new CustomFloat(fa + ia),
            (double da, int ia) => new CustomFloat(da + ia),
            (decimal ma, int ia) => new CustomFloat((double)ma + ia),

            (float fa, long la) => new CustomFloat(fa + la),
            (double da, long la) => new CustomFloat(da + la),
            (decimal ma, long la) => new CustomFloat((double)ma + la),

            (float fa, BigInteger bi) => new CustomFloat(fa + (double)bi),
            (double da, BigInteger bi) => new CustomFloat(da + (double)bi),
            (decimal ma, BigInteger bi) => new CustomFloat((double)ma + (double)bi),

            _ => throw new UnreachableException()
        };
    }

    public static CustomInt operator -(CustomInt a, CustomInt b)
    {
        if (a.Value is not int or long or BigInteger || b.Value is not int or long or BigInteger)
        {
            Errors.RaiseError(
                new SystemError("Attempting to subtract 2 ints together that do not support subtraction"));
            throw new UnreachableException();
        }

        return (a.Value, b.Value) switch
        {
            (int ia, int ib) => new CustomInt(ia - ib),
            (int ia, long ib) => new CustomInt(ia - ib),
            (long ia, int ib) => new CustomInt(ia - ib),
            (long ia, long ib) => new CustomInt(ia - ib),
            (int ia, BigInteger ib) => new CustomInt(ia - ib),
            (BigInteger ia, int ib) => new CustomInt(ia - ib),
            (long ia, BigInteger ib) => new CustomInt(ia - ib),
            (BigInteger ia, long ib) => new CustomInt(ia - ib),
            (BigInteger ia, BigInteger ib) => new CustomInt(ia - ib),

            _ => throw new UnreachableException()
        };
    }

    public static CustomFloat operator -(CustomInt a, CustomFloat b)
    {
        if (a.Value is not int or long or BigInteger || b.Value is not float or double or decimal)
        {
            Errors.RaiseError(
                new SystemError("Attempting to subtract int and float together that do not support subtraction"));
            throw new UnreachableException();
        }

        return (a.Value, b.Value) switch
        {
            (int ia, float fa) => new CustomFloat(ia - fa),
            (int ia, double da) => new CustomFloat(ia - da),
            (int ia, decimal ma) => new CustomFloat(ia - (double)ma),

            (long la, float fa) => new CustomFloat(la - fa),
            (long la, double da) => new CustomFloat(la - da),
            (long la, decimal ma) => new CustomFloat(la - (double)ma),

            (BigInteger bi, float fa) => new CustomFloat((double)bi - fa),
            (BigInteger bi, double da) => new CustomFloat((double)bi - da),
            (BigInteger bi, decimal ma) => new CustomFloat((double)bi - (double)ma),

            _ => throw new UnreachableException()
        };
    }

    public static CustomFloat operator -(CustomFloat a, CustomInt b)
    {
        if (a.Value is not float or double or decimal || b.Value is not int or long or BigInteger)
        {
            Errors.RaiseError(
                new SystemError("Attempting to subtract int and float together that do not support subtraction"));
            throw new UnreachableException();
        }

        return (a.Value, b.Value) switch
        {
            (float fa, int ia) => new CustomFloat(fa - ia),
            (double da, int ia) => new CustomFloat(da - ia),
            (decimal ma, int ia) => new CustomFloat((double)ma - ia),

            (float fa, long la) => new CustomFloat(fa - la),
            (double da, long la) => new CustomFloat(da - la),
            (decimal ma, long la) => new CustomFloat((double)ma - la),

            (float fa, BigInteger bi) => new CustomFloat(fa - (double)bi),
            (double da, BigInteger bi) => new CustomFloat(da - (double)bi),
            (decimal ma, BigInteger bi) => new CustomFloat((double)ma - (double)bi),

            _ => throw new UnreachableException()
        };
    }

    public static CustomInt operator *(CustomInt a, CustomInt b)
    {
        if (a.Value is not int or long or BigInteger || b.Value is not int or long or BigInteger)
        {
            Errors.RaiseError(
                new SystemError("Attempting to multiply 2 ints together that do not support multiplication"));
            throw new UnreachableException();
        }

        return (a.Value, b.Value) switch
        {
            (int ia, int ib) => new CustomInt(ia * ib),
            (int ia, long ib) => new CustomInt(ia * ib),
            (long ia, int ib) => new CustomInt(ia * ib),
            (long ia, long ib) => new CustomInt(ia * ib),
            (int ia, BigInteger ib) => new CustomInt(ia * ib),
            (BigInteger ia, int ib) => new CustomInt(ia * ib),
            (long ia, BigInteger ib) => new CustomInt(ia * ib),
            (BigInteger ia, long ib) => new CustomInt(ia * ib),
            (BigInteger ia, BigInteger ib) => new CustomInt(ia * ib),

            _ => throw new UnreachableException()
        };
    }

    public static CustomFloat operator *(CustomInt a, CustomFloat b)
    {
        if (a.Value is not int or long or BigInteger || b.Value is not float or double or decimal)
        {
            Errors.RaiseError(
                new SystemError("Attempting to multiply int and float together that do not support multiplication"));
            throw new UnreachableException();
        }

        return (a.Value, b.Value) switch
        {
            (int ia, float fa) => new CustomFloat(ia * fa),
            (int ia, double da) => new CustomFloat(ia * da),
            (int ia, decimal ma) => new CustomFloat(ia * (double)ma),

            (long la, float fa) => new CustomFloat(la * fa),
            (long la, double da) => new CustomFloat(la * da),
            (long la, decimal ma) => new CustomFloat(la * (double)ma),

            (BigInteger bi, float fa) => new CustomFloat((double)bi * fa),
            (BigInteger bi, double da) => new CustomFloat((double)bi * da),
            (BigInteger bi, decimal ma) => new CustomFloat((double)bi * (double)ma),

            _ => throw new UnreachableException()
        };
    }

    public static CustomFloat operator *(CustomFloat a, CustomInt b)
    {
        if (a.Value is not float or double or decimal || b.Value is not int or long or BigInteger)
        {
            Errors.RaiseError(
                new SystemError("Attempting to multiply int and float together that do not support multiplication"));
            throw new UnreachableException();
        }

        return (a.Value, b.Value) switch
        {
            (float fa, int ia) => new CustomFloat(fa * ia),
            (double da, int ia) => new CustomFloat(da * ia),
            (decimal ma, int ia) => new CustomFloat((double)ma * ia),

            (float fa, long la) => new CustomFloat(fa * la),
            (double da, long la) => new CustomFloat(da * la),
            (decimal ma, long la) => new CustomFloat((double)ma * la),

            (float fa, BigInteger bi) => new CustomFloat(fa * (double)bi),
            (double da, BigInteger bi) => new CustomFloat(da * (double)bi),
            (decimal ma, BigInteger bi) => new CustomFloat((double)ma * (double)bi),

            _ => throw new UnreachableException()
        };
    }

    public static CustomInt operator /(CustomInt a, CustomInt b)
    {
        if (a.Value is not int or long or BigInteger || b.Value is not int or long or BigInteger)
        {
            Errors.RaiseError(new SystemError("Attempting to divide 2 ints together that do not support division"));
        }

        return (a.Value, b.Value) switch
        {
            (int ia, int ib) => new CustomInt(ia / ib),
            (int ia, long ib) => new CustomInt(ia / ib),
            (long ia, int ib) => new CustomInt(ia / ib),
            (long ia, long ib) => new CustomInt(ia / ib),
            (int ia, BigInteger ib) => new CustomInt(ia / ib),
            (BigInteger ia, int ib) => new CustomInt(ia / ib),
            (long ia, BigInteger ib) => new CustomInt(ia / ib),
            (BigInteger ia, long ib) => new CustomInt(ia / ib),
            (BigInteger ia, BigInteger ib) => new CustomInt(ia / ib),

            _ => throw new UnreachableException()
        };
    }

    public static CustomFloat operator /(CustomInt a, CustomFloat b)
    {
        if (a.Value is not int or long or BigInteger || b.Value is not float or double or decimal)
        {
            Errors.RaiseError(
                new SystemError("Attempting to divide int and float together that do not support division"));
            throw new UnreachableException();
        }

        return (a.Value, b.Value) switch
        {
            (int ia, float fa) => new CustomFloat(ia / fa),
            (int ia, double da) => new CustomFloat(ia / da),
            (int ia, decimal ma) => new CustomFloat(ia / (double)ma),

            (long la, float fa) => new CustomFloat(la / fa),
            (long la, double da) => new CustomFloat(la / da),
            (long la, decimal ma) => new CustomFloat(la / (double)ma),

            (BigInteger bi, float fa) => new CustomFloat((double)bi / fa),
            (BigInteger bi, double da) => new CustomFloat((double)bi / da),
            (BigInteger bi, decimal ma) => new CustomFloat((double)bi / (double)ma),

            _ => throw new UnreachableException()
        };
    }

    public static CustomFloat operator /(CustomFloat a, CustomInt b)
    {
        if (a.Value is not float or double or decimal || b.Value is not int or long or BigInteger)
        {
            Errors.RaiseError(
                new SystemError("Attempting to divide int and float together that do not support division"));
            throw new UnreachableException();
        }

        return (a.Value, b.Value) switch
        {
            (float fa, int ia) => new CustomFloat(fa / ia),
            (double da, int ia) => new CustomFloat(da / ia),
            (decimal ma, int ia) => new CustomFloat((double)ma / ia),

            (float fa, long la) => new CustomFloat(fa / la),
            (double da, long la) => new CustomFloat(da / la),
            (decimal ma, long la) => new CustomFloat((double)ma / la),

            (float fa, BigInteger bi) => new CustomFloat(fa / (double)bi),
            (double da, BigInteger bi) => new CustomFloat(da / (double)bi),
            (decimal ma, BigInteger bi) => new CustomFloat((double)ma / (double)bi),

            _ => throw new UnreachableException()
        };
    }

    public static bool operator >(CustomFloat a, CustomInt b)
    {
        if (a.Value is not float or double or decimal || b.Value is not int or long or BigInteger)
            Errors.AlwaysThrow(
                new SystemError("Attempting to compare int and float together that do not support comparison"));

        return (dynamic)a.Value > (dynamic)b.Value;
    }

    public static bool operator >(CustomInt a, CustomFloat b)
    {
        if (b.Value is not float or double or decimal || a.Value is not int or long or BigInteger)
            Errors.AlwaysThrow(
                new SystemError("Attempting to compare float and int together that do not support comparison"));

        return (dynamic)a.Value > (dynamic)b.Value;
    }

    public static bool operator >(CustomInt a, CustomInt b)
    {
        if (a.Value is not int or long or BigInteger || b.Value is not int or long or BigInteger)
            Errors.AlwaysThrow(
                new SystemError("Attempting to compare int and int together that do not support comparison"));

        return (dynamic)a.Value > (dynamic)b.Value;
    }

    public static bool operator <(CustomFloat a, CustomInt b)
    {
        if (a.Value is not float or double or decimal || b.Value is not int or long or BigInteger)
            Errors.AlwaysThrow(
                new SystemError("Attempting to compare int and float together that do not support comparison"));

        return (dynamic)a.Value < (dynamic)b.Value;
    }

    public static bool operator <(CustomInt a, CustomFloat b)
    {
        if (b.Value is not float or double or decimal || a.Value is not int or long or BigInteger)
            Errors.AlwaysThrow(
                new SystemError("Attempting to compare float and int together that do not support comparison"));

        return (dynamic)a.Value < (dynamic)b.Value;
    }

    public static bool operator <(CustomInt a, CustomInt b)
    {
        if (a.Value is not int or long or BigInteger || b.Value is not int or long or BigInteger)
            Errors.AlwaysThrow(
                new SystemError("Attempting to compare int and int together that do not support comparison"));

        return (dynamic)a.Value < (dynamic)b.Value;
    }

    public override string ToString()
    {
        return this.Value?.ToString() ?? "null";
    }


    public CustomInt(object? value)
    {
        string stringValue = (value ?? string.Empty).ToString() ?? string.Empty;
        this._value = Convert(stringValue);
    }

    private CustomInt(int value, bool dontConvert = true)
    {
        if (!dontConvert)
        {
            string stringValue = value.ToString() ?? string.Empty;
            this._value = Convert(stringValue);
            return;
        }

        this._value = value;
    }

    private CustomInt(long value, bool dontConvert = true)
    {
        if (!dontConvert)
        {
            string stringValue = value.ToString() ?? string.Empty;
            this._value = Convert(stringValue);
            return;
        }

        this._value = value;
    }

    private CustomInt(BigInteger value, bool dontConvert = true)
    {
        if (!dontConvert)
        {
            string stringValue = value.ToString() ?? string.Empty;
            this._value = Convert(stringValue);
            return;
        }

        this._value = value;
    }

    public static explicit operator CustomFloat(CustomInt customInt)
    {
        return new CustomFloat(customInt.Value.ToString());
    }
}

internal class CustomFloat
{
    private object _value;

    public object Value
    {
        get => this._value;
        set => this._value = Convert((string)value);
    }

    public static bool TryParse(string input, out CustomFloat? result)
    {
        if (float.TryParse(input, out float floatResult))
        {
            result = new CustomFloat(floatResult, dontConvert: true);
            return true;
        }

        if (double.TryParse(input, out double doubleResult))
        {
            result = new CustomFloat(doubleResult, dontConvert: true);
            return true;
        }

        if (decimal.TryParse(input, out decimal decimalResult))
        {
            result = new CustomFloat(decimalResult, dontConvert: true);
            return true;
        }

        result = null;
        return false;
    }

    public static object Convert(string value)
    {
        if (TryParse(value, out var result))
        {
            return result!.Value;
        }

        Errors.AlwaysThrow(new OutOfRangeError("The floating point number provided is too big or small to support"));
        throw new UnreachableException();
    }

    public static CustomFloat operator +(CustomFloat a, CustomFloat b)
    {
        if (a.Value is not float or double or decimal || b.Value is not float or double or decimal)
        {
            Errors.AlwaysThrow(new SystemError("Attempting to add 2 floats together that do not support addition"));
            throw new UnreachableException();
        }

        return (a.Value, b.Value) switch
        {
            (float fa, float fb) => new CustomFloat(fa + fb),
            (double da, double db) => new CustomFloat(da + db),
            (decimal ma, decimal mb) => new CustomFloat(ma + mb),

            (float f1, double d1) => new CustomFloat(f1 + d1),
            (double d2, float f2) => new CustomFloat(d2 + f2),

            (float f3, decimal m3) => new CustomFloat(f3 + (double)m3),
            (decimal m4, float f4) => new CustomFloat((double)m4 + f4),

            (double d5, decimal m5) => new CustomFloat(d5 + (double)m5),
            (decimal m6, double d6) => new CustomFloat((double)m6 + d6),

            _ => throw new UnreachableException()
        };
    }

    public static CustomFloat operator -(CustomFloat a, CustomFloat b)
    {
        if (a.Value is not float or double or decimal || b.Value is not float or double or decimal)
        {
            Errors.AlwaysThrow(
                new SystemError("Attempting to subtract 2 floats together that do not support subtraction"));
            throw new UnreachableException();
        }

        return (a.Value, b.Value) switch
        {
            (float fa, float fb) => new CustomFloat(fa - fb),
            (double da, double db) => new CustomFloat(da - db),
            (decimal ma, decimal mb) => new CustomFloat(ma - mb),

            (float f1, double d1) => new CustomFloat(f1 - d1),
            (double d2, float f2) => new CustomFloat(d2 - f2),

            (float f3, decimal m3) => new CustomFloat(f3 - (double)m3),
            (decimal m4, float f4) => new CustomFloat((double)m4 - f4),

            (double d5, decimal m5) => new CustomFloat(d5 - (double)m5),
            (decimal m6, double d6) => new CustomFloat((double)m6 - d6),

            _ => throw new UnreachableException()
        };
    }

    public static CustomFloat operator *(CustomFloat a, CustomFloat b)
    {
        if (a.Value is not float or double or decimal || b.Value is not float or double or decimal)
        {
            Errors.AlwaysThrow(
                new SystemError("Attempting to multiply 2 floats together that do not support multiplication"));
            throw new UnreachableException();
        }

        return (a.Value, b.Value) switch
        {
            (float fa, float fb) => new CustomFloat(fa * fb),
            (double da, double db) => new CustomFloat(da * db),
            (decimal ma, decimal mb) => new CustomFloat(ma * mb),

            (float f1, double d1) => new CustomFloat(f1 * d1),
            (double d2, float f2) => new CustomFloat(d2 * f2),

            (float f3, decimal m3) => new CustomFloat(f3 * (double)m3),
            (decimal m4, float f4) => new CustomFloat((double)m4 * f4),

            (double d5, decimal m5) => new CustomFloat(d5 * (double)m5),
            (decimal m6, double d6) => new CustomFloat((double)m6 * d6),

            _ => throw new UnreachableException()
        };
    }

    public static CustomFloat operator /(CustomFloat a, CustomFloat b)
    {
        if (a.Value is not float or double or decimal || b.Value is not float or double or decimal)
        {
            Errors.AlwaysThrow(
                new SystemError("Attempting to divide 2 floats together that do not support division"));
            throw new UnreachableException();
        }

        return (a.Value, b.Value) switch
        {
            (float fa, float fb) => new CustomFloat(fa / fb),
            (double da, double db) => new CustomFloat(da / db),
            (decimal ma, decimal mb) => new CustomFloat(ma / mb),

            (float f1, double d1) => new CustomFloat(f1 / d1),
            (double d2, float f2) => new CustomFloat(d2 / f2),

            (float f3, decimal m3) => new CustomFloat(f3 / (double)m3),
            (decimal m4, float f4) => new CustomFloat((double)m4 / f4),

            (double d5, decimal m5) => new CustomFloat(d5 / (double)m5),
            (decimal m6, double d6) => new CustomFloat((double)m6 / d6),

            _ => throw new UnreachableException()
        };
    }

    public static explicit operator CustomFloat(CustomInt value)
    {
        return new CustomFloat((string)value.Value);
    }

    public static bool operator >(CustomFloat a, CustomInt b)
    {
        if (a.Value is not float or double or decimal || b.Value is not int or long or BigInteger)
            Errors.AlwaysThrow(
                new SystemError("Attempting to compare int and float together that do not support comparison"));

        return (dynamic)a.Value > (dynamic)b.Value;
    }

    public static bool operator >(CustomInt a, CustomFloat b)
    {
        if (b.Value is not float or double or decimal || a.Value is not int or long or BigInteger)
            Errors.AlwaysThrow(
                new SystemError("Attempting to compare float and int together that do not support comparison"));

        return (dynamic)a.Value > (dynamic)b.Value;
    }

    public static bool operator >(CustomFloat a, CustomFloat b)
    {
        if (a.Value is not float or double or decimal || b.Value is not float or double or decimal)
            Errors.AlwaysThrow(
                new SystemError("Attempting to compare int and int together that do not support comparison"));

        return (dynamic)a.Value > (dynamic)b.Value;
    }

    public static bool operator <(CustomFloat a, CustomInt b)
    {
        if (a.Value is not float or double or decimal || b.Value is not int or long or BigInteger)
            Errors.AlwaysThrow(
                new SystemError("Attempting to compare int and float together that do not support comparison"));

        return (dynamic)a.Value < (dynamic)b.Value;
    }

    public static bool operator <(CustomInt a, CustomFloat b)
    {
        if (b.Value is not float or double or decimal || a.Value is not int or long or BigInteger)
            Errors.AlwaysThrow(
                new SystemError("Attempting to compare float and int together that do not support comparison"));

        return (dynamic)a.Value < (dynamic)b.Value;
    }

    public static bool operator <(CustomFloat a, CustomFloat b)
    {
        if (a.Value is not float or double or decimal || b.Value is not float or double or decimal)
            Errors.AlwaysThrow(
                new SystemError("Attempting to compare int and int together that do not support comparison"));

        return (dynamic)a.Value < (dynamic)b.Value;
    }


    public override string ToString()
    {
        return this.Value?.ToString() ?? "null";
    }

    public CustomFloat(object? value)
    {
        string stringValue = (value ?? string.Empty).ToString() ?? string.Empty;
        this._value = Convert(stringValue);
    }

    private CustomFloat(float value, bool dontConvert = true)
    {
        if (!dontConvert)
        {
            string stringValue = value.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
            this._value = Convert(stringValue);
            return;
        }

        this._value = value;
    }

    private CustomFloat(double value, bool dontConvert = true)
    {
        if (!dontConvert)
        {
            string stringValue = value.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
            this._value = Convert(stringValue);
            return;
        }

        this._value = value;
    }

    private CustomFloat(decimal value, bool dontConvert = true)
    {
        if (!dontConvert)
        {
            string stringValue = value.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
            this._value = Convert(stringValue);
            return;
        }

        this._value = value;
    }
}