using System.Globalization;
using Aurora.Core;
using Aurora.Evaluator.BuiltinObjects;

namespace Aurora.Evaluator.Internals;

public static class Builtins
{
    public static TypeObject Type = null!;
    public static TypeObject Int = null!;
    public static TypeObject Float = null!;
    public static TypeObject String = null!;
    public static TypeObject Boolean = null!;
    public static TypeObject Null = null!;
    public static TypeObject Unit = null!;
    public static TypeObject Callable = null!;
    public static TypeObject Terminal = null!;
    public static TypeObject BooleanOutputStyles = null!;
    public static TypeObject Optional = null!;
    public static TypeObject Math = null!;
    public static TypeObject Block = null!;
    public static TypeObject Logic = null!;
    public static TypeObject LogicIfReturn = null!;
    public static TypeObject Loop = null!;
    public static TypeObject Array = null!;
    public static TypeObject Interface = null!;
    public static InterfaceObject ICollection = null!;

    public static void InitialiseTypes()
    {

        RuntimeType typeType = new RuntimeType(nameof(Type));
        Type = new TypeObject(typeType, typeType);

        Callable = new TypeObject(new RuntimeType(nameof(Callable), type: Type));

        Unit = new TypeObject(new RuntimeType(nameof(Unit), type: Type, isStatic: true));

        Optional = new TypeObject(new RuntimeType(nameof(Optional), type: Type));

        Int = new TypeObject(new RuntimeType(nameof(Int), type: Type));

        Float = new TypeObject(new RuntimeType(nameof(Float), type: Type));

        String = new TypeObject(new RuntimeType(nameof(String), type: Type));

        Boolean = new TypeObject(new RuntimeType(nameof(Boolean), type: Type));

        Null = new TypeObject(new RuntimeType(nameof(Null), type: Type));

        Terminal = new TypeObject(new RuntimeType(nameof(Terminal), type: Type, isStatic: true));

        BooleanOutputStyles = new TypeObject(new RuntimeType(nameof(BooleanOutputStyles), type: Type, isStatic: true));

        Math = new TypeObject(new RuntimeType(nameof(Math), type: Type, isStatic: true));

        Block = new TypeObject(new RuntimeType(nameof(Block), type: Type));

        Logic = new TypeObject(new RuntimeType(nameof(Logic), type: Type, isStatic: true));

        LogicIfReturn = new TypeObject(new RuntimeType(nameof(LogicIfReturn), type: Type));

        Loop = new TypeObject(new RuntimeType(nameof(Loop), type: Type, isStatic: true));

        Array = new TypeObject(new RuntimeType(nameof(Array), type: Type));

        Interface = new TypeObject(new RuntimeType(nameof(Interface), type: Type));

        ICollection = new InterfaceObject(new RuntimeInterface(nameof(ICollection)));

        // Todo: Add tests for interfaces.

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
        InitialiseLogicIfReturnType();
        InitialiseLoopType();
        InitialiseICollectionInterface();
        InitialiseArrayType();
        InitialiseInterfaceType();

        Type.MarkFinal(null);
        Interface.MarkFinal(null);
        Optional.MarkFinal(null);
        Int.MarkFinal(null);
        Unit.MarkFinal(null);
        Float.MarkFinal(null);
        String.MarkFinal(null);
        Boolean.MarkFinal(null);
        Null.MarkFinal(null);
        Terminal.MarkFinal(null);
        BooleanOutputStyles.MarkFinal(null);
        Math.MarkFinal(null);
        Logic.MarkFinal(null);
        LogicIfReturn.MarkFinal(null);
        Loop.MarkFinal(null);
        Array.MarkFinal(null);
    }

    private static void InitialiseICollectionInterface()
    {
        ICollection.Value.AddMethod(name: "at", returnType: Type, [new ParameterDefinition(name: "index", type: Int),]);
        ICollection.Value.AddMethod(name: "length", returnType: Int, []);
    }

    private static void InitialiseInterfaceType()
    {
        // Todo: Not implemented yet
    }

    private static void InitialiseArrayType()
    {
        Array.AddInterface(ICollection, null);
        Method fromMethod = new(
            name: "from",
            returnType: Array,
            parameters: [new ParameterDefinition(name: "type", type: Type),],
            unlimitedPositionalArgumentsType: Type,
            unlimitedKeywordArgumentsType: null,
            body: (_, _, context) =>
            {
                TypeObject type = context.GetParam<TypeObject>("type");
                List<RuntimeObject> positionals = context.GetPositionalArgs();

                if (positionals.Count == 0)
                    return new ArrayObject(positionals.ToArray());

                if (!positionals.TrueForAll(x => x.Type.Name == type.Name))
                    Errors.AlwaysThrow(new TypeMismatchError($"An array can only store one type"), context.CallSiteLocation);

                return new ArrayObject(positionals.ToArray());
            });
        Array.AddStaticMethod(fromMethod, null);

        Method atMethod = ICollection.Value.GetFilledMethod("at", (self, _, context) =>
        {
            ArrayObject selfAsArray = (ArrayObject)self;
            IntObject index = context.GetParam<IntObject>("index");

            RuntimeObject[] value = selfAsArray.Value;
            int indexValue = index.Value;

            if (indexValue >= value.Length || indexValue < 0)
                Errors.AlwaysThrow(
                    new OutOfRangeError($"Index {indexValue} is out of bounds for array of length {value.Length}"),
                    context.CallSiteLocation);

            return value[indexValue];
        }, null);
        Array.AddInstanceMethod(atMethod, null);

        Method lengthMethod = ICollection.Value.GetFilledMethod("length", (self, _, _) =>
        {
            ArrayObject selfAsArray = (ArrayObject)self;
            return selfAsArray.Length;
        }, null);
        Array.AddInstanceMethod(lengthMethod, null);

        Method toString = new(
            name: "toString",
            returnType: String,
            parameters: [],
            body: (self, _, _) =>
            {
                ArrayObject selfAsArray = (ArrayObject)self;

                return new StringObject($"Array<type: {selfAsArray.Type.Name}, length: {selfAsArray.Value.Length}>");
            });

        Array.AddInstanceMethod(toString, null);

        Attribute lengthAttribute = new(
            name: "length",
            type: Int,
            valueGetter: ArrayObject.LengthGetter);
        Array.AddInstanceAttribute(lengthAttribute, null);
    }

    private static void InitialiseLoopType()
    {
        Method whileMethod = new(
            name: "while",
            returnType: Unit,
            parameters: null,
            body: (_, args, context) => InternalMethods.Loop.While(args, context));
        Loop.AddStaticMethod(whileMethod, null);

        Method forMethod = new(
            name: "for",
            returnType: Unit,
            parameters: null,
            body: (_, args, context) => InternalMethods.Loop.For(args, context));
        Loop.AddStaticMethod(forMethod, null);

        Method breakMethod = new(
            name: "break",
            returnType: Unit,
            parameters: [],
            body: (_, _, _) => InternalMethods.Loop.Break());
        Loop.AddStaticMethod(breakMethod, null);

        Method continueMethod = new(
            name: "continue",
            returnType: Unit,
            parameters: [],
            body: (_, _, _) => InternalMethods.Loop.Continue());
        Loop.AddStaticMethod(continueMethod, null);
    }

    private static void InitialiseLogicIfReturnType()
    {
        Method elseMethod = new(
            name: "else",
            returnType: LogicIfReturn,
            parameters: [new ParameterDefinition(name: "block", type: Block),],
            body: (self, _, context) => InternalMethods.Logic.Else(self, context));

        LogicIfReturn.AddInstanceMethod(elseMethod, null);
    }

    private static void InitialiseLogicType()
    {
        Method ifMethod = new(
            name: "if",
            returnType: LogicIfReturn,
            parameters:
            [
                new ParameterDefinition(name: "condition", type: Boolean),
                new ParameterDefinition(name: "block", type: Block),
            ],
            body: (_, _, context) => InternalMethods.Logic.If(context));

        Logic.AddStaticMethod(ifMethod, null);
    }

    private static void InitialiseTypeType()
    {
        Method typeCreateMethod = new(
            name: "create",
            returnType: Unit,
            parameters: null,
            body: InternalMethods.Type.Create);

        Type.AddStaticMethod(typeCreateMethod, null);

        Method typeSetMethod = new(
            name: "set",
            returnType: Unit,
            parameters: null,
            body: InternalMethods.Type.Set);

        Type.AddStaticMethod(typeSetMethod, null);

        Method toString = new(
            name: "toString",
            returnType: String,
            parameters: [],
            body: (self, _, _) => InternalMethods.Type.ToString(self));

        Type.AddInstanceMethod(toString, null);
        Type.AddStaticMethod(toString, null);

        Method equals = new(
            name: "equals",
            returnType: Boolean,
            parameters: [new ParameterDefinition(name: "other", type: Type),],
            body: (self, _, context) => InternalMethods.Type.Equals(self, context));
        Type.AddInstanceMethod(equals, null);
        Type.AddStaticMethod(equals, null);
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
        Optional.AddInstanceAttribute(isEmptyAttribute, null);

        Method fromMethod = new(
            name: "of",
            returnType: Optional,
            parameters: [new ParameterDefinition(name: "value", type: Type),],
            body: (_, _, context) =>
            {
                RuntimeObject valueObject = context.GetParam("value");
                return new OptionalObject(valueObject);
            });
        Optional.AddStaticMethod(fromMethod, null);

        Method emptyOptionalMethod = new(
            name: "empty",
            returnType: Optional,
            parameters: [],
            body: (_, _, _) => new OptionalObject(null));
        Optional.AddStaticMethod(emptyOptionalMethod, null);

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
        Optional.AddInstanceAttribute(valueAttribute, null);

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
        Optional.AddInstanceMethod(valueOrDefaultMethod, null);

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
        Optional.AddInstanceMethod(toStringMethod, null);
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

        Int.AddInstanceMethod(addMethod, null);

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

        Int.AddInstanceMethod(subtractMethod, null);

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

        Int.AddInstanceMethod(multiplyByMethod, null);

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

        Int.AddInstanceMethod(divideByMethod, null);

        Method toString = new(
            name: "toString",
            returnType: String,
            parameters: [],
            body: (self, _, _) =>
            {
                IntObject selfAsInt = (IntObject)self;

                return new StringObject(selfAsInt.Value.ToString());
            });

        Int.AddInstanceMethod(toString, null);

        Method lessThan = new(
            name: "lessThan",
            returnType: Boolean,
            parameters: [new ParameterDefinition(name: "other", type: Int),],
            body: (self, _, context) =>
            {
                IntObject left = (IntObject)self;
                IntObject right = context.GetParam<IntObject>("other");

                return new BooleanObject(left.Value < right.Value);
            });
        Int.AddInstanceMethod(lessThan, null);

        Method lessThanOrEqual = new(
            name: "lessThanOrEqual",
            returnType: Boolean,
            parameters: [new ParameterDefinition(name: "other", type: Int),],
            body: (self, _, context) =>
            {
                IntObject left = (IntObject)self;
                IntObject right = context.GetParam<IntObject>("other");

                return new BooleanObject(left.Value <= right.Value);
            });
        Int.AddInstanceMethod(lessThanOrEqual, null);

        Method greaterThan = new(
            name: "greaterThan",
            returnType: Boolean,
            parameters: [new ParameterDefinition(name: "other", type: Int),],
            body: (self, _, context) =>
            {
                IntObject left = (IntObject)self;
                IntObject right = context.GetParam<IntObject>("other");

                return new BooleanObject(left.Value > right.Value);
            });
        Int.AddInstanceMethod(greaterThan, null);

        Method greaterThanOrEqual = new(
            name: "greaterThanOrEqual",
            returnType: Boolean,
            parameters: [new ParameterDefinition(name: "other", type: Int),],
            body: (self, _, context) =>
            {
                IntObject left = (IntObject)self;
                IntObject right = context.GetParam<IntObject>("other");

                return new BooleanObject(left.Value >= right.Value);
            });
        Int.AddInstanceMethod(greaterThanOrEqual, null);

        Method incrementInstance = new(
            name: "increment",
            returnType: Unit,
            parameters: [new ParameterDefinition(name: "amount", type: Int, defaultValue: new IntObject(1)),],
            body: (self, _, context) =>
            {
                IntObject selfAsInt = (IntObject)self;
                IntObject amount = context.GetParam<IntObject>("amount");

                context.UpdateThis(new IntObject(selfAsInt.Value + amount.Value));
                return new UnitObject();
            });
        Int.AddInstanceMethod(incrementInstance, null);

        Method decrementInstance = new(
            name: "decrement",
            returnType: Unit,
            parameters: [new ParameterDefinition(name: "amount", type: Int, defaultValue: new IntObject(1)),],
            body: (self, _, context) =>
            {
                IntObject selfAsInt = (IntObject)self;
                IntObject amount = context.GetParam<IntObject>("amount");

                context.UpdateThis(new IntObject(selfAsInt.Value - amount.Value));
                return new UnitObject();
            });
        Int.AddInstanceMethod(decrementInstance, null);

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

        String.AddInstanceMethod(stringAddMethod, null);

        Method toString = new(
            name: "toString",
            returnType: String,
            parameters: [],
            body: (self, _, _) =>
            {
                StringObject selfAsString = (StringObject)self;

                return selfAsString;
            });

        String.AddInstanceMethod(toString, null);

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
                    using Evaluator evaluator = Evaluator.CreateChild(context.Parent!);
                    RuntimeObject valueAsObject = evaluator.EvaluateExpressionForValue(rawArg.Value);
                    StringObject valueAsStringObject =
                        valueAsObject.ConvertToStringObject(context, context.CallSiteLocation);

                    if (fullString != string.Empty)
                        fullString += ' ';

                    fullString += valueAsStringObject.Value;
                }

                return new StringObject(fullString);
            });

        String.AddStaticMethod(staticConcatMethod, null);

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

        String.AddInstanceMethod(instanceConcatMethod, null);

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
        String.AddInstanceMethod(substringMethod, null);

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
        String.AddInstanceMethod(elementAtMethod, null);

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
        String.AddInstanceMethod(findMethod, null);

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
        String.AddInstanceMethod(containsMethod, null);

        Attribute lengthAttribute = new(
            name: "length",
            type: Int,
            valueGetter: (self, _) =>
            {
                StringObject selfAsString = (StringObject)self;
                return new IntObject(selfAsString.Value.Length);
            });
        String.AddInstanceAttribute(lengthAttribute, null);

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

        Terminal.AddStaticMethod(writeMethod, null);

        Method readMethod = new(
            name: "readLine",
            returnType: String,
            parameters:
            [
                new ParameterDefinition(name: "message", type: String, defaultValue: new StringObject("")),
                new ParameterDefinition(name: "default", type: String, nullable: true, defaultValue: new NullObject()),
            ],
            body: (_, _, context) => InternalMethods.Terminal.ReadLine(context));

        Terminal.AddStaticMethod(readMethod, null);

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

        Terminal.AddStaticMethod(readIntMethod, null);

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

        Terminal.AddStaticMethod(readFloatMethod, null);

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

        Terminal.AddStaticMethod(readBooleanMethod, null);

        Method readKeyMethod = new(
            name: "readKey",
            returnType: String,
            parameters: [new ParameterDefinition(name: "message", type: String),],
            body: (_, _, context) => InternalMethods.Terminal.ReadKey(context));
        Terminal.AddStaticMethod(readKeyMethod, null);

        Method clearMethod = new(
            name: "clear",
            returnType: Unit,
            parameters: [],
            body: (_, _, _) => InternalMethods.Terminal.Clear());
        Terminal.AddStaticMethod(clearMethod, null);
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

        Float.AddInstanceMethod(toString, null);
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

        Boolean.AddInstanceMethod(toString, null);

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
            (_, _) => wordStyle), null);
        BooleanOutputStyles.AddStaticAttribute(new Attribute("yesNo", BooleanOutputStyles,
            (_, _) => yesNoStyle), null);
        BooleanOutputStyles.AddStaticAttribute(new Attribute("char", BooleanOutputStyles,
            (_, _) => charStyle), null);
        BooleanOutputStyles.AddStaticAttribute(new Attribute("onOff", BooleanOutputStyles,
            (_, _) => onOffStyle), null);
        BooleanOutputStyles.AddStaticAttribute(new Attribute("binary", BooleanOutputStyles,
            (_, _) => binaryStyle), null);
    }

    private static void InitialiseNullType()
    {
        Method toString = new(
            name: "toString",
            returnType: String,
            parameters: [],
            body: (_, _, _) => new StringObject("null"));

        Null.AddInstanceMethod(toString, null);

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
        Math.AddStaticMethod(truncateMethod, null);
    }
}
