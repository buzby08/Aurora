using System.Globalization;
using Aurora.BuiltinMethods;

namespace Aurora.Internals;

internal static class Builtins
{
    public static Type Type;
    public static Type Int;
    public static Type Float;
    public static Type String;
    public static Type Boolean;
    public static Type Null;
    public static Type Unit;
    public static Type Terminal;

    public static RuntimeContext GlobalContext = new(null);

    public static void InitialiseTypes()
    {
        Type = new Type("Type");
        Type.Type = Type;

        InitialiseTypeType();

        Unit = new Type("Unit", type: Type);

        Int = new Type("Int", type: Type);

        InitialiseIntType();

        Float = new Type("Float", type: Type);

        InitialiseFloatType();

        String = new Type("String", type: Type);

        InitialiseStringType();

        Boolean = new Type("Boolean", type: Type);

        InitialiseBooleanType();

        Null = new Type("Null", type: Type);

        InitialiseNullType();

        Terminal = new Type("Terminal", type: Type);

        InitialiseTerminalType();

        // Todo: Initialise all types
    }

    public static void InitialiseTypeType()
    {
        Method typeCreateMethod = new(
            name: "create",
            returnType: Unit,
            parameters: null,
            body: (self, args, context) =>
            {
                var targetType = (Type)self;

                foreach (var (key, variable) in args)
                {
                    RuntimeObject variableObject = Evaluator.EvaluateAstList(variable, context.Parent!);

                    if (variableObject.Type != targetType)
                        Errors.AlwaysThrow(
                            new TypeMismatchError(
                                $"{targetType.Name}.create requires `{targetType.Name}`, not `{variableObject.Type.Name}`"));

                    context.Parent!.Create(key, variableObject);
                }

                return new UnitObject();
            });

        Type.AddStaticMethod(typeCreateMethod);

        Method typeSetMethod = new(
            name: "set",
            returnType: Unit,
            parameters: null,
            body: (self, args, context) =>
            {
                var targetType = (Type)self;

                foreach (var (key, variable) in args)
                {
                    RuntimeObject variableObject = Evaluator.EvaluateAstList(variable, context.Parent!);
                    if (variableObject.Type != targetType)
                        Errors.AlwaysThrow(
                            new TypeMismatchError(
                                $"{targetType.Name}.set requires `{targetType.Name}`, not `{variableObject.Type.Name}`"));

                    context.Set(key, variableObject);
                }

                return new UnitObject();
            });

        Type.AddStaticMethod(typeSetMethod);

        Method toString = new(
            name: "toString",
            returnType: String,
            parameters: [],
            body: (self, args, context) => new StringObject($"{self.Type.Name}"));

        Type.AddInstanceMethod(toString);
        Type.AddStaticMethod(toString);
    }

    public static void InitialiseIntType()
    {
        Method addMethod = new(
            name: "add",
            returnType: Int,
            parameters: [new ParameterDefinition(name: "other", type: Int)],
            body: (self, args, context) =>
            {
                IntObject left = (IntObject)self;
                IntObject right = (IntObject)context.Get("other");

                return new IntObject(
                    left.Value + right.Value);
            });

        Int.AddInstanceMethod(addMethod);

        Method subtractMethod = new(
            name: "subtract",
            returnType: Int,
            parameters: [new ParameterDefinition(name: "other", type: Int)],
            body: (self, args, context) =>
            {
                IntObject left = (IntObject)self;
                IntObject right = (IntObject)context.Get("other");

                return new IntObject(
                    left.Value - right.Value);
            });

        Int.AddInstanceMethod(subtractMethod);

        Method multiplyByMethod = new(
            name: "multiplyBy",
            returnType: Int,
            parameters: [new ParameterDefinition(name: "other", type: Int)],
            body: (self, args, context) =>
            {
                IntObject left = (IntObject)self;
                IntObject right = (IntObject)context.Get("other");

                return new IntObject(
                    left.Value * right.Value);
            });

        Int.AddInstanceMethod(multiplyByMethod);

        Method divideByMethod = new(
            name: "divideBy",
            returnType: Float,
            parameters: [new ParameterDefinition(name: "other", type: Int)],
            body: (self, args, context) =>
            {
                IntObject left = (IntObject)self;
                IntObject right = (IntObject)context.Get("other");

                return new FloatObject(
                    (decimal)left.Value / right.Value);
            });

        Int.AddInstanceMethod(divideByMethod);

        Method toString = new(
            name: "toString",
            returnType: String,
            parameters: [],
            body: (self, args, context) =>
            {
                IntObject selfAsInt = (IntObject)self;

                return new StringObject(selfAsInt.Value.ToString());
            });

        Int.AddInstanceMethod(toString);

        // Todo: Add other IntType methods
    }

    public static void InitialiseStringType()
    {
        Method stringAddMethod = new(
            name: "add",
            returnType: String,
            parameters: [new ParameterDefinition(name: "other", type: String)],
            body: (self, args, context) =>
            {
                StringObject left = (StringObject)self;
                StringObject right = (StringObject)context.Get("other");

                string combinedObject = left.Value + right.Value;

                return new StringObject(
                    combinedObject);
            });

        String.AddInstanceMethod(stringAddMethod);

        Method toString = new(
            name: "toString",
            returnType: String,
            parameters: [],
            body: (self, args, context) =>
            {
                StringObject selfAsString = (StringObject)self;

                return selfAsString;
            });

        String.AddInstanceMethod(toString);

        Method staticConcatMethod = new(
            name: "concat",
            returnType: String,
            unlimitedPositionalArgumentsType: Type,
            unlimitedKeywordArgumentsType: null,
            body: (self, args, context) =>
            {
                // Todo: Change how positional args stored in context (naming)
                // Todo: Get all positional args from context, and add to full string
                string fullString = string.Empty;

                foreach ((string key, List<Ast> value) in args)
                {
                    RuntimeObject valueAsObject = Evaluator.EvaluateAstList(value, context.Parent!);
                    StringObject valueAsStringObject = valueAsObject.ConvertToStringObject(context);

                    if (fullString != string.Empty)
                        fullString += ' ';

                    fullString += valueAsStringObject.Value;
                }

                return new StringObject(fullString);
            });

        String.AddStaticMethod(staticConcatMethod);

        Method instanceConcatMethod = new(
            name: "concat",
            returnType: String,
            parameters: [new ParameterDefinition(name: "other", type: Type)],
            body: (self, args, context) =>
            {
                StringObject left = (StringObject)self;

                RuntimeObject right = context.Get("other");
                StringObject rightAsStringObject = right.ConvertToStringObject(context);

                return new StringObject(left.Value + rightAsStringObject.Value);
            });

        String.AddInstanceMethod(instanceConcatMethod);

        // Todo: Add other StringType methods
    }

    public static void InitialiseTerminalType()
    {
        Method writeMethod = new(
            name: "writeLine",
            returnType: Unit,
            unlimitedPositionalArgumentsType: Type,
            parameters:
            [
                // Todo: Make use of new unlimitedPositionalArgs
                // Todo: Change all SystemError calls to have a unique identifier, to find their location in the code.
                new ParameterDefinition(name: "end", type: String, defaultValue: new StringObject("\n"))
            ],
            unlimitedKeywordArgumentsType: null,
            body: (self, args, context) =>
            {
                List<RuntimeObject> values = context.GetPositionalArgs();
                string valueToOutput = string.Empty;

                foreach (RuntimeObject value in values)
                {
                    valueToOutput += value.ConvertToCSharpString(context);
                }

                StringObject defaultParam = (StringObject)context.GetParam("end");

                Console.Write(valueToOutput + defaultParam.Value);

                return new UnitObject();
            });

        Terminal.AddStaticMethod(writeMethod);

        Method readMethod = new(
            name: "readLine",
            returnType: String,
            parameters:
            [
                new ParameterDefinition(name: "message", type: String, defaultValue: new StringObject("")),
                new ParameterDefinition(name: "default", type: String, nullable: true, defaultValue: new NullObject())
            ],
            body: (self, args, context) =>
            {
                StringObject message = (StringObject)context.GetParam("message");
                RuntimeObject defaultValueObject = context.GetParam("default");

                Console.Write(message.Value);
                string? inputtedValue = Console.ReadLine();

                if (defaultValueObject is not NullObject && string.IsNullOrWhiteSpace(inputtedValue))
                    return (StringObject)defaultValueObject;

                return new StringObject(inputtedValue ?? string.Empty);
            });

        Terminal.AddStaticMethod(readMethod);

        Method readIntMethod = new(
            name: "readInt",
            returnType: Int,
            parameters:
            [
                new ParameterDefinition(name: "message", type: String, defaultValue: new StringObject("")),
                new ParameterDefinition(name: "min", type: Int, nullable: true, defaultValue: new NullObject()),
                new ParameterDefinition(name: "max", type: Int, nullable: true, defaultValue: new NullObject())
            ],
            body: (self, args, context) =>
            {
                StringObject message = (StringObject)context.GetParam("message");
                RuntimeObject minObject = context.GetParam("min");
                RuntimeObject maxObject = context.GetParam("max");

                int? minValue = minObject is NullObject ? null : ((IntObject)minObject!).Value;
                int? maxValue = maxObject is NullObject ? null : ((IntObject)maxObject!).Value;

                while (true)
                {
                    Console.Write(message.Value);
                    string? inputtedValue = Console.ReadLine();
                    bool isAnInt = int.TryParse(inputtedValue, out int inputtedInt);
                    bool satisfiesMinRequirement = minValue is null || inputtedInt >= minValue;
                    bool satisfiesMaxRequirement = maxValue is null || inputtedInt <= maxValue;

                    if (isAnInt && satisfiesMaxRequirement && satisfiesMinRequirement)
                        return new IntObject(inputtedInt);

                    if (!isAnInt)
                    {
                        Console.WriteLine("Please input an integer value");
                        continue;
                    }

                    if (!satisfiesMaxRequirement && !satisfiesMinRequirement)
                    {
                        Console.WriteLine(
                            $"Please input a value greater than or equal to {minValue} and less than or equal to {maxValue}");
                        continue;
                    }

                    if (!satisfiesMaxRequirement)
                    {
                        Console.WriteLine($"Please enter a value less than or equal to {maxValue}");
                        continue;
                    }

                    if (!satisfiesMinRequirement)
                    {
                        Console.WriteLine($"Please enter a value greater than or equal to {minValue}");
                        continue;
                    }
                }
            });

        Terminal.AddStaticMethod(readIntMethod);

        // Todo: Add other TerminalType methods
    }

    public static void InitialiseFloatType()
    {
        Method toString = new(
            name: "toString",
            returnType: String,
            parameters: [],
            body: (self, args, context) =>
            {
                FloatObject selfAsFloat = (FloatObject)self;

                string valueAsString = selfAsFloat.Value.ToString(CultureInfo.InvariantCulture);

                if (valueAsString.EndsWith(".0"))
                    valueAsString = valueAsString[..^2];

                return new StringObject(valueAsString);
            });

        Float.AddInstanceMethod(toString);
    }

    public static void InitialiseBooleanType()
    {
        Method toString = new(
            name: "toString",
            returnType: String,
            parameters: [],
            body: (self, args, context) =>
            {
                BooleanObject selfAsBoolean = (BooleanObject)self;

                return new StringObject(selfAsBoolean.Value ? "true" : "false");
            });

        Boolean.AddInstanceMethod(toString);

        // Todo: Add more BooleanType methods
    }

    public static void InitialiseNullType()
    {
        Method toString = new(
            name: "toString",
            returnType: String,
            parameters: [],
            body: (self, args, context) => new StringObject("null"));

        Null.AddInstanceMethod(toString);

        // Todo: Add more NullType methods
    }
}