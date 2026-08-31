using System.Globalization;
using Aurora.Core;
using Aurora.Evaluator.BuiltinObjects;

namespace Aurora.Evaluator.Internals;

public static class Builtins
{
    public static Type Type = null!;
    public static Type Int = null!;
    public static Type Float = null!;
    public static Type String = null!;
    public static Type Boolean = null!;
    public static Type Null = null!;
    public static Type Unit = null!;
    public static Type Callable = null!;
    public static Type Terminal = null!;
    public static Type BooleanOutputStyles = null!;
    public static Type Optional = null!;
    public static Type Math = null!;
    public static Type Block = null!;
    public static Type Logic = null!;

    public static void InitialiseTypes()
    {
        Type = new Type(nameof(Type));
        Type.Type = Type;

        Callable = new Type(nameof(Callable), type: Type);

        Unit = new Type(nameof(Unit), type: Type);

        Optional = new Type(nameof(Optional), type: Type);

        Int = new Type(nameof(Int), type: Type);

        Float = new Type(nameof(Float), type: Type);

        String = new Type(nameof(String), type: Type);

        Boolean = new Type(nameof(Boolean), type: Type);

        Null = new Type(nameof(Null), type: Type);

        Terminal = new Type(nameof(Terminal), type: Type);

        BooleanOutputStyles = new Type(nameof(BooleanOutputStyles), type: Type);

        Math = new Type(nameof(Math), type: Type);

        Block = new Type(nameof(Block), type: Type);

        Logic = new Type(nameof(Logic), type: Type);

        InitialiseTypeType();
        InitialiseOptionalType();
        InitialiseIntType();
        InitialiseFloatType();
        InitialiseStringType();
        InitialiseBooleanType();
        InitialiseNullType();
        InitialiseTerminalType();
        InitialiseBooleanOutputStylesType();
        InitialiseMathType();
        InitialiseLogicType();

    }

    private static void InitialiseLogicType()
    {
        Method ifMethod = new(
            name: "if",
            returnType: Unit,
            parameters:
            [
                new ParameterDefinition(name: "condition", type: Boolean),
                new ParameterDefinition(name: "block", type: Block),
            ],
            body: (_, _, context) => InternalMethods.Logic.If(context));

        Logic.AddStaticMethod(ifMethod);
    }

    private static void InitialiseTypeType()
    {
        Method typeCreateMethod = new(
            name: "create",
            returnType: Unit,
            parameters: null,
            body: InternalMethods.Type.Create);

        Type.AddStaticMethod(typeCreateMethod);

        Method typeSetMethod = new(
            name: "set",
            returnType: Unit,
            parameters: null,
            body: InternalMethods.Type.Set);

        Type.AddStaticMethod(typeSetMethod);

        Method toString = new(
            name: "toString",
            returnType: String,
            parameters: [],
            body: (self, _, _) => InternalMethods.Type.ToString(self));

        Type.AddInstanceMethod(toString);
        Type.AddStaticMethod(toString);

        Method equals = new(
            name: "equals",
            returnType: Boolean,
            parameters: [new ParameterDefinition(name: "other", type: Type),],
            body: (self, _, context) => InternalMethods.Type.Equals(self, context));
        Type.AddInstanceMethod(equals);
        Type.AddStaticMethod(equals);
    }

    private static void InitialiseOptionalType()
    {
        Attribute isEmptyAttribute = new(
            name: "isEmpty",
            type: Boolean,
            valueGetter: (self, _) =>
            {
                OptionalObject selfAsOptional = (OptionalObject)self;

                return new BooleanObject(selfAsOptional.HasValue);
            });
        Optional.AddInstanceAttribute(isEmptyAttribute);

        Method fromMethod = new(
            name: "of",
            returnType: Optional,
            parameters: [new ParameterDefinition(name: "value", type: Type),],
            body: (_, _, context) =>
            {
                RuntimeObject valueObject = context.GetParam("value");
                return new OptionalObject(valueObject);
            });
        Optional.AddStaticMethod(fromMethod);

        Method emptyOptionalMethod = new(
            name: "empty",
            returnType: Optional,
            parameters: [],
            body: (_, _, _) => new OptionalObject(null));
        Optional.AddStaticMethod(emptyOptionalMethod);

        Attribute valueAttribute = new(
            name: "value",
            type: Type,
            valueGetter: (self, context) =>
            {
                OptionalObject selfAsOptional = (OptionalObject)self;

                if (!selfAsOptional.HasValue)
                    Errors.AlwaysThrow(new UnsupportedOperationError(
                            "Cannot access the value from an optional type where the object does not contain a value"),
                        null /* Todo: Add a better source location */);

                return selfAsOptional.Value!;
            });
        Optional.AddInstanceAttribute(valueAttribute);

        Method valueOrDefaultMethod = new(
            name: "valueOrDefault",
            returnType: Type,
            parameters: [new ParameterDefinition(name: "default", type: Type),],
            body: (self, _, context) =>
            {
                OptionalObject selfAsOptional = (OptionalObject)self;
                RuntimeObject defaultObject = context.GetParam("default");

                if (selfAsOptional.HasValue)
                    return selfAsOptional.Value!;

                return defaultObject;
            });
        Optional.AddInstanceMethod(valueOrDefaultMethod);

        Method toStringMethod = new(
            name: "toString",
            returnType: String,
            parameters: [],
            body: (self, _, context) =>
            {
                OptionalObject selfAsOptional = (OptionalObject)self;

                if (selfAsOptional.HasValue)
                    return new StringObject(
                        $"Optional({selfAsOptional.Value!.ConvertToCSharpString(context, context.CallSiteLocation)})");

                return new StringObject("Optional(Empty)");
            });
        Optional.AddInstanceMethod(toStringMethod);
    }

    private static void InitialiseIntType()
    {
        Method addMethod = new(
            name: "add",
            returnType: Int,
            parameters: [new ParameterDefinition(name: "other", type: Int),],
            body: (self, _, context) =>
            {
                IntObject left = (IntObject)self;
                IntObject right = (IntObject)context.GetParam("other");

                return new IntObject(
                    left.Value + right.Value);
            });

        Int.AddInstanceMethod(addMethod);

        Method subtractMethod = new(
            name: "subtract",
            returnType: Int,
            parameters: [new ParameterDefinition(name: "other", type: Int),],
            body: (self, _, context) =>
            {
                IntObject left = (IntObject)self;
                IntObject right = (IntObject)context.GetParam("other");

                return new IntObject(
                    left.Value - right.Value);
            });

        Int.AddInstanceMethod(subtractMethod);

        Method multiplyByMethod = new(
            name: "multiplyBy",
            returnType: Int,
            parameters: [new ParameterDefinition(name: "other", type: Int),],
            body: (self, _, context) =>
            {
                IntObject left = (IntObject)self;
                IntObject right = (IntObject)context.GetParam("other");

                return new IntObject(
                    left.Value * right.Value);
            });

        Int.AddInstanceMethod(multiplyByMethod);

        Method divideByMethod = new(
            name: "divideBy",
            returnType: Float,
            parameters: [new ParameterDefinition(name: "other", type: Int),],
            body: (self, _, context) =>
            {
                IntObject left = (IntObject)self;
                IntObject right = (IntObject)context.GetParam("other");

                return new FloatObject(
                    (decimal)left.Value / right.Value);
            });

        Int.AddInstanceMethod(divideByMethod);

        Method toString = new(
            name: "toString",
            returnType: String,
            parameters: [],
            body: (self, _, _) =>
            {
                IntObject selfAsInt = (IntObject)self;

                return new StringObject(selfAsInt.Value.ToString());
            });

        Int.AddInstanceMethod(toString);

        // Todo: Add other IntType methods
    }

    private static void InitialiseStringType()
    {
        Method stringAddMethod = new(
            name: "add",
            returnType: String,
            parameters: [new ParameterDefinition(name: "other", type: String),],
            body: (self, _, context) =>
            {
                StringObject left = (StringObject)self;
                StringObject right = (StringObject)context.GetParam("other");

                string combinedObject = left.Value + right.Value;

                return new StringObject(
                    combinedObject);
            });

        String.AddInstanceMethod(stringAddMethod);

        Method toString = new(
            name: "toString",
            returnType: String,
            parameters: [],
            body: (self, _, _) =>
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
            body: (_, args, context) =>
            {
                // Todo: Change how positional args stored in context (naming)
                // Todo: Get all positional args from context, and add to full string
                string fullString = string.Empty;

                foreach ((string _, RawMethodArgument rawArg) in args)
                {
                    Evaluator evaluator = new(context.Parent!);
                    RuntimeObject valueAsObject = evaluator.EvaluateExpressionForValue(rawArg.Value);
                    StringObject valueAsStringObject =
                        valueAsObject.ConvertToStringObject(context, context.CallSiteLocation);

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
            parameters: [new ParameterDefinition(name: "other", type: Type),],
            body: (self, _, context) =>
            {
                StringObject left = (StringObject)self;

                RuntimeObject right = context.GetParam("other");
                StringObject rightAsStringObject = right.ConvertToStringObject(context, context.CallSiteLocation);

                return new StringObject(left.Value + ' ' + rightAsStringObject.Value);
            });

        String.AddInstanceMethod(instanceConcatMethod);

        Method substringMethod = new(
            name: "substring",
            returnType: String,
            parameters:
            [
                new ParameterDefinition(name: "start", type: Int),
                new ParameterDefinition(name: "end", type: Int),
            ],
            body: (self, _, context) =>
            {
                StringObject selfAsString = (StringObject)self;
                IntObject start = (IntObject)context.GetParam("start");
                IntObject end = (IntObject)context.GetParam("end");

                int selfLength = selfAsString.Value.Length;

                if (start.Value > end.Value)
                    Errors.AlwaysThrow(
                        new InvalidRangeError(
                            $"Start cannot be greater than end value ({start.Value} > {end.Value})"),
                        null /* Todo: Add a better source location */);

                if (start.Value < 0)
                    Errors.AlwaysThrow(new InvalidRangeError($"Start cannot be less than zero ({start.Value} < 0)"),
                        null /* Todo: Add a better source location */);

                if (end.Value > selfLength)
                    Errors.AlwaysThrow(
                        new InvalidRangeError(
                            $"End cannot be greater than the string length ({end.Value} > {selfLength})"),
                        null /* Todo: Add a better source location */);

                string substring = selfAsString.Value[start.Value..end.Value];
                return new StringObject(substring);
            });
        String.AddInstanceMethod(substringMethod);

        Method elementAtMethod = new(
            name: "elementAt",
            returnType: String,
            parameters:
            [
                new ParameterDefinition(name: "index", type: Int),
            ],
            body: (self, _, context) =>
            {
                StringObject selfAsString = (StringObject)self;
                int length = selfAsString.Value.Length;

                IntObject index = (IntObject)context.GetParam("index");

                if (index.Value > length)
                    Errors.AlwaysThrow(
                        new InvalidRangeError(
                            $"Index cannot be greater than the string length ({index.Value} > {length})"),
                        null /* Todo: Add a better source location */);

                if (index.Value < 0)
                    Errors.AlwaysThrow(new InvalidRangeError(
                            $"Index cannot be less than zero ({index.Value} < 0)"),
                        null /* Todo: Add a better source location */);

                return new StringObject(selfAsString.Value[index.Value].ToString());
            });
        String.AddInstanceMethod(elementAtMethod);

        Method findMethod = new(
            name: "find",
            returnType: Optional,
            parameters:
            [
                new ParameterDefinition(name: "value", type: String),
            ],
            body: (self, _, context) =>
            {
                StringObject selfAsString = (StringObject)self;
                StringObject findValue = (StringObject)context.GetParam("value");

                int index = selfAsString.Value.IndexOf(findValue.Value, StringComparison.Ordinal);

                if (selfAsString.Value.Length == 0)
                    index = -1;

                if (index == -1)
                    return new OptionalObject(null);

                return new OptionalObject(new IntObject(index));
            });
        String.AddInstanceMethod(findMethod);

        Method containsMethod = new(
            name: "contains",
            returnType: Boolean,
            parameters: [new ParameterDefinition(name: "substring", type: String),],
            body: (self, _, context) =>
            {
                StringObject selfAsString = (StringObject)self;
                StringObject containsValue = (StringObject)context.GetParam("substring");

                return new BooleanObject(selfAsString.Value.Contains(containsValue.Value, StringComparison.Ordinal));
            });
        String.AddInstanceMethod(containsMethod);

        Attribute lengthAttribute = new(
            name: "length",
            type: Int,
            valueGetter: (self, _) =>
            {
                StringObject selfAsString = (StringObject)self;
                return new IntObject(selfAsString.Value.Length);
            });
        String.AddInstanceAttribute(lengthAttribute);

        // Todo: Add other StringType methods
    }

    private static void InitialiseTerminalType()
    {
        Method writeMethod = new(
            name: "writeLine",
            returnType: Unit,
            unlimitedPositionalArgumentsType: Type,
            parameters:
            [
                new ParameterDefinition(name: "separator", type: String, defaultValue: new StringObject(" ")),
                // Todo: Change all SystemError calls to have a unique identifier, to find their location in the code.
                new ParameterDefinition(name: "end", type: String, defaultValue: new StringObject("\n")),
            ],
            unlimitedKeywordArgumentsType: null,
            body: (_, _, context) => InternalMethods.Terminal.WriteLine(context));

        Terminal.AddStaticMethod(writeMethod);

        Method readMethod = new(
            name: "readLine",
            returnType: String,
            parameters:
            [
                new ParameterDefinition(name: "message", type: String, defaultValue: new StringObject("")),
                new ParameterDefinition(name: "default", type: String, nullable: true, defaultValue: new NullObject()),
            ],
            body: (_, _, context) => InternalMethods.Terminal.ReadLine(context));

        Terminal.AddStaticMethod(readMethod);

        Method readIntMethod = new(
            name: "readInt",
            returnType: Int,
            parameters:
            [
                new ParameterDefinition(name: "message", type: String, defaultValue: new StringObject("")),
                new ParameterDefinition(name: "min", type: Int, nullable: true, defaultValue: new NullObject()),
                new ParameterDefinition(name: "max", type: Int, nullable: true, defaultValue: new NullObject()),
            ],
            body: (_, _, context) => InternalMethods.Terminal.ReadInteger(context));

        Terminal.AddStaticMethod(readIntMethod);

        Method readFloatMethod = new(
            name: "readFloat",
            returnType: Int,
            parameters:
            [
                new ParameterDefinition(name: "message", type: String, defaultValue: new StringObject("")),
                new ParameterDefinition(name: "min", type: Float, nullable: true, defaultValue: new NullObject()),
                new ParameterDefinition(name: "max", type: Float, nullable: true, defaultValue: new NullObject()),
            ],
            body: (_, _, context) => InternalMethods.Terminal.ReadFloat(context));

        Terminal.AddStaticMethod(readFloatMethod);

        Method readBooleanMethod = new(
            name: "readBoolean",
            returnType: Boolean,
            parameters:
            [
                new ParameterDefinition(name: "message", type: String),
                new ParameterDefinition(
                    name: "outputStyle",
                    type: BooleanOutputStyles,
                    defaultValue: new BooleanOutputStyleObject(BooleanOutputStyleObject.Style.Word)),
                new ParameterDefinition(name: "immediate", type: Boolean, defaultValue: new BooleanObject(false)),
            ],
            body: (_, _, context) => InternalMethods.Terminal.ReadBoolean(context));

        Terminal.AddStaticMethod(readBooleanMethod);

        Method readKeyMethod = new(
            name: "readKey",
            returnType: String,
            parameters: [new ParameterDefinition(name: "message", type: String),],
            body: (_, _, context) => InternalMethods.Terminal.ReadKey(context));
        Terminal.AddStaticMethod(readKeyMethod);

        Method clearMethod = new(
            name: "clear",
            returnType: Unit,
            parameters: [],
            body: (_, _, _) => InternalMethods.Terminal.Clear());
        Terminal.AddStaticMethod(clearMethod);
    }

    private static void InitialiseFloatType()
    {
        Method toString = new(
            name: "toString",
            returnType: String,
            parameters: [],
            body: (self, _, _) =>
            {
                FloatObject selfAsFloat = (FloatObject)self;

                string valueAsString = selfAsFloat.Value.ToString(CultureInfo.InvariantCulture);

                if (valueAsString.EndsWith(".0"))
                    valueAsString = valueAsString[..^2];

                return new StringObject(valueAsString);
            });

        Float.AddInstanceMethod(toString);
    }

    private static void InitialiseBooleanType()
    {
        Method toString = new(
            name: "toString",
            returnType: String,
            parameters: [],
            body: (self, _, _) =>
            {
                BooleanObject selfAsBoolean = (BooleanObject)self;

                return new StringObject(selfAsBoolean.Value ? "true" : "false");
            });

        Boolean.AddInstanceMethod(toString);

        // Todo: Add more BooleanType methods
    }

    private static void InitialiseBooleanOutputStylesType()
    {
        BooleanOutputStyleObject wordStyle = new(BooleanOutputStyleObject.Style.Word);
        BooleanOutputStyleObject yesNoStyle = new(BooleanOutputStyleObject.Style.YesNo);
        BooleanOutputStyleObject charStyle = new(BooleanOutputStyleObject.Style.Char);
        BooleanOutputStyleObject onOffStyle = new(BooleanOutputStyleObject.Style.OnOff);
        BooleanOutputStyleObject binaryStyle = new(BooleanOutputStyleObject.Style.Binary);
        BooleanOutputStyles.AddStaticAttribute(new Attribute("word", BooleanOutputStyles,
            (_, _) => wordStyle));
        BooleanOutputStyles.AddStaticAttribute(new Attribute("yesNo", BooleanOutputStyles,
            (_, _) => yesNoStyle));
        BooleanOutputStyles.AddStaticAttribute(new Attribute("char", BooleanOutputStyles,
            (_, _) => charStyle));
        BooleanOutputStyles.AddStaticAttribute(new Attribute("onOff", BooleanOutputStyles,
            (_, _) => onOffStyle));
        BooleanOutputStyles.AddStaticAttribute(new Attribute("binary", BooleanOutputStyles,
            (_, _) => binaryStyle));
    }

    private static void InitialiseNullType()
    {
        Method toString = new(
            name: "toString",
            returnType: String,
            parameters: [],
            body: (_, _, _) => new StringObject("null"));

        Null.AddInstanceMethod(toString);

        // Todo: Add more NullType methods
    }

    private static void InitialiseMathType()
    {
        Method truncateMethod = new(
            name: "truncate",
            returnType: Float,
            parameters:
            [
                new ParameterDefinition(name: "value", type: Float),
                new ParameterDefinition(name: "places", type: Int, defaultValue: new IntObject(0)),
            ],
            body: (_, _, context) => MathFunctions.Truncate(context));
        Math.AddStaticMethod(truncateMethod);
    }
}
