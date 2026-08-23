using Aurora.BuiltinMethods;
using Aurora.Internals;

namespace Aurora.InternalMethods;

internal static class Terminal
{
    public static UnitObject WriteLine(RuntimeContext context)
    {
        StringObject endObject = (StringObject)context.GetParam("end");
        StringObject separatorObject = (StringObject)context.GetParam("separator");

        string end = endObject.Value;
        string separator = separatorObject.Value;

        return WriteLine(context, context.GetPositionalArgs(), end, separator);
    }

    public static StringObject ReadLine(RuntimeContext context)
    {
        StringObject messageObject = (StringObject)context.GetParam("message");
        RuntimeObject defaultValueObject = context.GetParam("default");

        string message = messageObject.Value;
        string? defaultValue = defaultValueObject is NullObject ? null : ((StringObject)defaultValueObject).Value;

        return ReadLine(message, defaultValue);
    }

    public static IntObject ReadInteger(RuntimeContext context)
    {
        StringObject messageObject = (StringObject)context.GetParam("message");
        RuntimeObject minObject = context.GetParam("min");
        RuntimeObject maxObject = context.GetParam("max");

        string message = messageObject.Value;
        int? min = minObject is NullObject ? null : ((IntObject)minObject).Value;
        int? max = maxObject is NullObject ? null : ((IntObject)maxObject).Value;
        return ReadInteger(message, min, max);
    }

    public static FloatObject ReadFloat(RuntimeContext context)
    {
        StringObject messageObject = (StringObject)context.GetParam("message");
        RuntimeObject minObject = context.GetParam("min");
        RuntimeObject maxObject = context.GetParam("max");

        string message = messageObject.Value;
        decimal? min = minObject is NullObject ? null : ((FloatObject)minObject).Value;
        decimal? max = maxObject is NullObject ? null : ((FloatObject)maxObject).Value;
        return ReadFloat(message, min, max);
    }

    public static BooleanObject ReadBoolean(RuntimeContext context)
    {
        StringObject messageObject = context.GetParam<StringObject>("message");
        BooleanOutputStyleObject styleObject = context.GetParam<BooleanOutputStyleObject>("outputStyle");
        BooleanObject immediateObject = context.GetParam<BooleanObject>("immediate");

        string message = messageObject.Value;
        BooleanOutputStyleObject.Style style = styleObject.Value;
        bool immediate = immediateObject.Value;

        return ReadBoolean(context, message, style, immediate);
    }

    public static StringObject ReadKey(RuntimeContext context)
    {
        StringObject messageObject = context.GetParam<StringObject>("message");

        string message = messageObject.Value;

        return ReadKey(message);
    }

    public static UnitObject Clear()
    {
        Console.Clear();
        return new UnitObject();
    }

    private static UnitObject WriteLine(RuntimeContext context, List<RuntimeObject> positionalArgs, string end,
                                        string separator)
    {
        string valueToOutput = string.Empty;

        for (int index = 0; index < positionalArgs.Count; index++)
        {
            RuntimeObject value = positionalArgs[index];
            if (index > 0)
                valueToOutput += separator;
            valueToOutput += value.ConvertToCSharpString(context);
        }

        Console.Write(valueToOutput + end);

        return new UnitObject();
    }

    private static StringObject ReadLine(string message, string? defaultValue)
    {
        Console.Write(message);
        string? inputtedValue = Console.ReadLine();

        if (defaultValue is not null && string.IsNullOrWhiteSpace(inputtedValue))
            inputtedValue = defaultValue;

        return new StringObject(inputtedValue ?? string.Empty);
    }

    private static IntObject ReadInteger(string message, int? min, int? max)
    {
        while (true)
        {
            Console.Write(message);
            string? inputtedValue = Console.ReadLine();

            bool isAnInt = int.TryParse(inputtedValue, out int inputtedInt);

            if (!isAnInt)
            {
                Console.WriteLine("Please input an integer value");
                continue;
            }

            bool satisfiesMinRequirement = min is null || inputtedInt >= min;
            bool satisfiesMaxRequirement = max is null || inputtedInt <= max;

            if (!satisfiesMaxRequirement && !satisfiesMinRequirement)
            {
                Console.WriteLine(
                    $"Please input a value greater than or equal to {min} and less than or equal to {max}");
                continue;
            }

            if (!satisfiesMinRequirement)
            {
                Console.WriteLine($"Please enter a value greater than or equal to {min}");
                continue;
            }

            if (!satisfiesMaxRequirement)
            {
                Console.WriteLine($"Please enter a value less than or equal to {max}");
                continue;
            }


            return new IntObject(inputtedInt);
        }
    }

    private static FloatObject ReadFloat(string message, decimal? min, decimal? max)
    {
        while (true)
        {
            Console.Write(message);
            string? inputtedValue = Console.ReadLine();

            bool isAFloat = decimal.TryParse(inputtedValue, out decimal inputtedFloat);

            if (!isAFloat)
            {
                Console.WriteLine("Please input a floating point (decimal) value");
                continue;
            }

            bool satisfiesMinRequirement = min is null || inputtedFloat >= min;
            bool satisfiesMaxRequirement = max is null || inputtedFloat <= max;

            if (!satisfiesMaxRequirement && !satisfiesMinRequirement)
            {
                Console.WriteLine(
                    $"Please input a value greater than or equal to {min} and less than or equal to {max}");
                continue;
            }

            if (!satisfiesMinRequirement)
            {
                Console.WriteLine($"Please enter a value greater than or equal to {min}");
                continue;
            }

            if (!satisfiesMaxRequirement)
            {
                Console.WriteLine($"Please enter a value less than or equal to {max}");
                continue;
            }


            return new FloatObject(inputtedFloat);
        }
    }

    private static BooleanObject ReadBoolean(RuntimeContext context, string message,
                                             BooleanOutputStyleObject.Style style, bool immediate)
    {
        Console.Write(message);

        bool result = style switch
        {
            BooleanOutputStyleObject.Style.Word => BooleanOutputStyleObject.ReadWordOption(),
            BooleanOutputStyleObject.Style.YesNo => BooleanOutputStyleObject.ReadYesNo(),
            BooleanOutputStyleObject.Style.Char => BooleanOutputStyleObject.ReadChar(immediate),
            BooleanOutputStyleObject.Style.Binary => BooleanOutputStyleObject.ReadBinary(immediate),
            BooleanOutputStyleObject.Style.OnOff => BooleanOutputStyleObject.ReadOnOff(),
            _ => Errors.AlwaysThrow<bool>(
                new SystemError("A statement was reached that was deemed unreachable"),
                context),
        };
        return new BooleanObject(result);
    }

    private static StringObject ReadKey(string message)
    {
        Console.Write(message);

        ConsoleKeyInfo inputtedValue = Console.ReadKey();
        return new StringObject(inputtedValue.KeyChar.ToString());
    }
}