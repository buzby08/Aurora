using Aurora.Core;
using Aurora.Evaluator.BuiltinObjects;
using Aurora.Evaluator.Internals.RuntimeValues;

namespace Aurora.Evaluator.Internals.InternalMethods;

internal static class Terminal
{
    public static RuntimeObject WriteLine(RuntimeContext context)
    {
        StringValue end = context.GetParam("end").GetStringValue();
        StringValue separator = context.GetParam("separator").GetStringValue();

        return WriteLine(context, context.GetPositionalArgs(), end, separator);
    }

    public static RuntimeObject ReadLine(RuntimeContext context)
    {
        StringValue messageObject = context.GetParam("message").GetStringValue();
        RuntimeObject defaultValueObject = context.GetParam("default");

        StringValue defaultValue = defaultValueObject.IsInstanceOf(Builtins.Null)
            ? (StringValue)""
            : defaultValueObject.GetStringValue();

        return ReadLine(messageObject, defaultValue);
    }

    public static RuntimeObject ReadInteger(RuntimeContext context)
    {
        StringValue messageObject = context.GetParam("message").GetStringValue();
        RuntimeObject minObject = context.GetParam("min");
        RuntimeObject maxObject = context.GetParam("max");

        string message = messageObject.ToCSharpString();
        int? min = minObject.IsInstanceOf(Builtins.Null) ? null : minObject.GetIntValue().ToCSharpInt();
        int? max = maxObject.IsInstanceOf(Builtins.Null) ? null : maxObject.GetIntValue().ToCSharpInt();
        return ReadInteger(message, min, max);
    }

    public static RuntimeObject ReadFloat(RuntimeContext context)
    {
        StringValue messageObject = context.GetParam("message").GetStringValue();
        RuntimeObject minObject = context.GetParam("min");
        RuntimeObject maxObject = context.GetParam("max");

        string message = messageObject.ToCSharpString();
        decimal? min = minObject.IsInstanceOf(Builtins.Null) ? null : minObject.GetFloatValue().ToCSharpDecimal();
        decimal? max = maxObject.IsInstanceOf(Builtins.Null) ? null : maxObject.GetFloatValue().ToCSharpDecimal();
        return ReadFloat(message, min, max);
    }

    public static RuntimeObject ReadBoolean(RuntimeContext context)
    {
        StringValue messageObject = context.GetParam("message").GetStringValue();
        BooleanOutputStyleValue styleObject = context.GetParam("outputStyle").GetBooleanOutputStyleValue();
        BooleanValue immediateObject = context.GetParam("immediate").GetBooleanValue();

        string message = messageObject.ToCSharpString();
        BooleanOutputStyleValue.Style style = styleObject.AsCSharpStyle;
        bool immediate = immediateObject.AsCSharpBool;

        return ReadBoolean(message, style, immediate);
    }

    public static RuntimeObject ReadKey(RuntimeContext context)
    {
        StringValue messageObject = context.GetParam("message").GetStringValue();

        string message = messageObject.ToCSharpString();

        return ReadKey(message);
    }

    public static RuntimeObject Clear()
    {
        Console.Clear();
        return UnitValue.CreateObject();
    }

    private static RuntimeObject WriteLine(RuntimeContext context, List<RuntimeObject> positionalArgs, StringValue end,
                                        StringValue separator)
    {
        StringValue valueToOutput = new(string.Empty);

        for (int index = 0; index < positionalArgs.Count; index++)
        {
            RuntimeObject value = positionalArgs[index];
            if (index > 0)
                valueToOutput += separator;
            valueToOutput += value.ConvertToCSharpString(context, context.CallSiteLocation);
        }

        valueToOutput += end;

        Console.Write(valueToOutput.ToCSharpString());

        return UnitValue.CreateObject();
    }

    private static RuntimeObject ReadLine(StringValue message, StringValue defaultValue)
    {
        Console.Write(message);
        string? inputtedValue = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(inputtedValue))
            inputtedValue = null;

        if (inputtedValue is null)
            return defaultValue.GetAsRuntimeObject();

        return StringValue.CreateObject(inputtedValue);
    }

    private static RuntimeObject ReadInteger(string message, int? min, int? max)
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


            return IntValue.CreateObject(inputtedInt);
        }
    }

    private static RuntimeObject ReadFloat(string message, decimal? min, decimal? max)
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


            return FloatValue.CreateObject(inputtedFloat);
        }
    }

    private static RuntimeObject ReadBoolean(string message,
                                             BooleanOutputStyleValue.Style style, bool immediate)
    {
        Console.Write(message);

        BooleanValue result = style switch
        {
            BooleanOutputStyleValue.Style.Word => BooleanOutputStyleValue.ReadWordOption(),
            BooleanOutputStyleValue.Style.YesNo => BooleanOutputStyleValue.ReadYesNo(),
            BooleanOutputStyleValue.Style.Char => BooleanOutputStyleValue.ReadChar(immediate),
            BooleanOutputStyleValue.Style.Binary => BooleanOutputStyleValue.ReadBinary(immediate),
            BooleanOutputStyleValue.Style.OnOff => BooleanOutputStyleValue.ReadOnOff(),
            _ => Errors.AlwaysThrow<BooleanValue>(
                new SystemError("A statement was reached that was deemed unreachable"),
                null),
        };
        return result.GetAsRuntimeObject();
    }

    private static RuntimeObject ReadKey(string message)
    {
        Console.Write(message);

        ConsoleKeyInfo inputtedValue = Console.ReadKey();
        return StringValue.CreateObject(inputtedValue.KeyChar.ToString());
    }
}
