using System.Diagnostics;
using System.Globalization;
using Aurora.Core;
using Aurora.Evaluator.BuiltinObjects;
using Aurora.Evaluator.Internals.RuntimeValues;

namespace Aurora.Evaluator.Internals;

public static class Builtins
{
    public static TypeObject Object = null!;
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
    public static RuntimeObject ICollection = null!;
    public static TypeObject Animal = null!;
    public static TypeObject Dog = null!;
    public static RuntimeObject dog = null!;

    public static void InitialiseTypes()
    {
        RuntimeType typeType = new(nameof(Type));
        Type = new TypeObject(typeType);

        Object = new TypeObject(typeType)
        {
            InstanceOf = Type,
            SuperType = null,
        };

        Type.SuperType = Object;
        Type.InstanceOf = Type;

        Callable = new TypeObject(new RuntimeType(nameof(Callable)));

        Unit = new TypeObject(new RuntimeType(nameof(Unit), isStatic: true));

        Optional = new TypeObject(new RuntimeType(nameof(Optional)));

        Int = new TypeObject(new RuntimeType(nameof(Int)));

        Float = new TypeObject(new RuntimeType(nameof(Float)));

        String = new TypeObject(new RuntimeType(nameof(String)));

        Boolean = new TypeObject(new RuntimeType(nameof(Boolean)));

        Null = new TypeObject(new RuntimeType(nameof(Null)));

        Terminal = new TypeObject(new RuntimeType(nameof(Terminal), isStatic: true));

        BooleanOutputStyles = new TypeObject(new RuntimeType(nameof(BooleanOutputStyles), isStatic: true));

        Math = new TypeObject(new RuntimeType(nameof(Math), isStatic: true));

        Block = new TypeObject(new RuntimeType(nameof(Block)));

        Logic = new TypeObject(new RuntimeType(nameof(Logic), isStatic: true));

        LogicIfReturn = new TypeObject(new RuntimeType(nameof(LogicIfReturn)));

        Loop = new TypeObject(new RuntimeType(nameof(Loop), isStatic: true));

        Array = new TypeObject(new RuntimeType(nameof(Array)));

        Interface = new TypeObject(new RuntimeType(nameof(Interface)));

        ICollection = InterfaceValue.GetAsObject(new RuntimeInterface(nameof(ICollection)));

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
        Animal.MarkFinal(null);
        Dog.MarkFinal(null);
    }

    private static void InitialiseICollectionInterface()
    {
        InterfaceValue rawInterface = ICollection.GetInterfaceValue();
        rawInterface.AddMethod(name: "at", returnType: Object, [new ParameterDefinition(name: "index", type: Int),]);
        rawInterface.AddMethod(name: "length", returnType: Int, []);
    }

    private static void InitialiseInterfaceType()
    {
        Method newMethod = new(
            name: "new",
            returnType: Interface,
            parameters: [],
            body: (self, args, context) =>
            {
                throw new NotImplementedException();
            });
        Interface.AddStaticMethod(newMethod, null);
        // Todo: Not implemented yet
    }

    private static void InitialiseArrayType()
    {
        Array.AddInterface(ICollection, null);

        Method newMethod = new(
            name: "new",
            returnType: Array,
            parameters: [new ParameterDefinition(name: "type", type: Type),],
            unlimitedPositionalArgumentsType: Object,
            unlimitedKeywordArgumentsType: null,
            body: (_, _, context) =>
            {
                TypeObject type = context.GetParam<TypeObject>("type");
                List<RuntimeObject> positionals = context.GetPositionalArgs();

                if (positionals.Count == 0)
                    return ArrayValue.CreateObject(positionals.ToArray());

                if (!positionals.TrueForAll(x => x.InstanceOf.IsSubclassOf(type)))
                    Errors.AlwaysThrow(new TypeMismatchError($"An array can only store one type"), context.CallSiteLocation);

                return ArrayValue.CreateObject(positionals.ToArray());
            });
        Array.AddStaticMethod(newMethod, null);

        Method atMethod = ICollection.GetInterfaceValue().GetFilledMethod("at", (self, _, context) =>
        {
            ArrayValue selfAsArray = self.GetArrayValue();
            IntValue index = context.GetParam("index").GetIntValue();

            if (index >= selfAsArray.Length || index < (IntValue)0)
                Errors.AlwaysThrow(
                    new OutOfRangeError($"Index {index} is out of bounds for array of length {selfAsArray.Length}"),
                    context.CallSiteLocation);

            return selfAsArray[index.RawValue];
        }, null);
        Array.AddInstanceMethod(atMethod, null);

        Method lengthMethod = ICollection.GetInterfaceValue().GetFilledMethod("length", (self, _, _) =>
        {
            ArrayValue selfAsArray = self.GetArrayValue();
            return selfAsArray.Length.GetAsRuntimeObject();
        }, null);
        Array.AddInstanceMethod(lengthMethod, null);

        Method toString = new(
            name: "toString",
            returnType: String,
            parameters: [],
            body: (self, _, _) =>
            {
                ArrayValue selfAsArray = self.GetArrayValue();

                return selfAsArray.ToStringValue().GetAsRuntimeObject();
            });

        Array.AddInstanceMethod(toString, null);

        Attribute lengthAttribute = new(
            name: "length",
            type: Int,
            valueGetter: (self, _) => self.GetArrayValue().Length.GetAsRuntimeObject());
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
        Method newMethod = new(
            name: "new",
            returnType: LogicIfReturn,
            parameters: [],
            body: (self, args, context) =>
            {
                // Todo: Turn into a private constructor, or internal, so not accessible from user code
                Errors.AlwaysThrow(new UnsupportedOperationError("Cannot create a LogicIfReturn object"), context.CallSiteLocation);
                throw new UnreachableException();
            });
        LogicIfReturn.AddStaticMethod(newMethod, null);

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
        Method newMethod = new(
            name: "new",
            returnType: Type,
            parameters: [],
            body: (self, args, context) =>
            {
                throw new NotImplementedException();
            });
        Type.AddStaticMethod(newMethod, null);

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
            parameters: [new ParameterDefinition(name: "other", type: Object),],
            body: (self, _, context) => InternalMethods.Type.Equals(self, context));
        Type.AddInstanceMethod(equals, null);
        Type.AddStaticMethod(equals, null);
    }

    private static void InitialiseOptionalType()
    {
        Method newMethod = new(
            name: "new",
            returnType: Optional,
            parameters: [new ParameterDefinition(name: "value", type: Object),],
            body: (_, _, context) =>
            {
                RuntimeObject valueObject = context.GetParam("value");
                return OptionalValue.CreateObject(valueObject);
            });
        Optional.AddStaticMethod(newMethod, null);

        Attribute isEmptyAttribute = new(
            name: "isEmpty",
            type: Boolean,
            valueGetter: (self, context) =>
            {
                RuntimeObject? value = ((OptionalValue)self.Value).GetValue(context.CallSiteLocation);

                return BooleanValue.CreateObject(value is null);
            });
        Optional.AddInstanceAttribute(isEmptyAttribute, null);

        Method emptyOptionalMethod = new(
            name: "empty",
            returnType: Optional,
            parameters: [],
            body: (_, _, _) => OptionalValue.CreateObject(null));
        Optional.AddStaticMethod(emptyOptionalMethod, null);

        Attribute valueAttribute = new(
            name: "value",
            type: Type,
            valueGetter: (self, context) =>
            {
                OptionalValue value = (OptionalValue)self.Value;

                if (!value.HasValue)
                    Errors.AlwaysThrow(new UnsupportedOperationError(
                            "Cannot access the value from an optional type where the object does not contain a value"),
                        context.CallSiteLocation);

                return value.GetValue(context.CallSiteLocation)!;
            });
        Optional.AddInstanceAttribute(valueAttribute, null);

        Method valueOrDefaultMethod = new(
            name: "valueOrDefault",
            returnType: Type,
            parameters: [new ParameterDefinition(name: "default", type: Object),],
            body: (self, _, context) =>
            {
                OptionalValue selfAsOptional = (OptionalValue)self.Value;
                RuntimeObject defaultObject = context.GetParam("default");

                if (selfAsOptional.HasValue)
                    return selfAsOptional.GetValue(context.CallSiteLocation)!;

                return defaultObject;
            });
        Optional.AddInstanceMethod(valueOrDefaultMethod, null);

        Method toStringMethod = new(
            name: "toString",
            returnType: String,
            parameters: [],
            body: (self, _, context) =>
            {
                OptionalValue selfValue = (OptionalValue)self.Value;

                if (selfValue.HasValue)
                    return StringValue.CreateObject(
                        $"Optional({self.ConvertToCSharpString(context, context.CallSiteLocation)})");

                return StringValue.CreateObject("Optional(Empty)");
            });
        Optional.AddInstanceMethod(toStringMethod, null);
    }

    private static void InitialiseIntType()
    {
        Method newMethod = new(
            name: "new",
            returnType: Int,
            parameters: [],
            body: (self, args, context) => IntValue.CreateObject(0));
        Int.AddStaticMethod(newMethod, null);

        Method addMethod = new(
            name: "add",
            returnType: Int,
            parameters: [new ParameterDefinition(name: "other", type: Int),],
            body: (self, _, context) =>
            {
                IntValue left = self.GetIntValue();
                IntValue right = context.GetParam("other").GetIntValue();

                return (left + right).GetAsRuntimeObject();
            });

        Int.AddInstanceMethod(addMethod, null);

        Method subtractMethod = new(
            name: "subtract",
            returnType: Int,
            parameters: [new ParameterDefinition(name: "other", type: Int),],
            body: (self, _, context) =>
            {
                IntValue left = self.GetIntValue();
                IntValue right = context.GetParam("other").GetIntValue();

                return (left - right).GetAsRuntimeObject();
            });

        Int.AddInstanceMethod(subtractMethod, null);

        Method multiplyByMethod = new(
            name: "multiplyBy",
            returnType: Int,
            parameters: [new ParameterDefinition(name: "other", type: Int),],
            body: (self, _, context) =>
            {
                IntValue left = self.GetIntValue();
                IntValue right = context.GetParam("other").GetIntValue();

                return (left * right).GetAsRuntimeObject();
            });

        Int.AddInstanceMethod(multiplyByMethod, null);

        Method divideByMethod = new(
            name: "divideBy",
            returnType: Float,
            parameters: [new ParameterDefinition(name: "other", type: Int),],
            body: (self, _, context) =>
            {
                IntValue left = self.GetIntValue();
                IntValue right = context.GetParam("other").GetIntValue();

                return (left / right).GetAsRuntimeObject();
            });

        Int.AddInstanceMethod(divideByMethod, null);

        Method toString = new(
            name: "toString",
            returnType: String,
            parameters: [],
            body: (self, _, _) =>
            {
                IntValue selfAsInt = self.GetIntValue();

                return selfAsInt.ToStringValue().GetAsRuntimeObject();
            });

        Int.AddInstanceMethod(toString, null);

        Method lessThan = new(
            name: "lessThan",
            returnType: Boolean,
            parameters: [new ParameterDefinition(name: "other", type: Int),],
            body: (self, _, context) =>
            {
                IntValue left = self.GetIntValue();
                IntValue right = context.GetParam("other").GetIntValue();

                return (left < right).GetAsRuntimeObject();
            });
        Int.AddInstanceMethod(lessThan, null);

        Method lessThanOrEqual = new(
            name: "lessThanOrEqual",
            returnType: Boolean,
            parameters: [new ParameterDefinition(name: "other", type: Int),],
            body: (self, _, context) =>
            {
                IntValue left = self.GetIntValue();
                IntValue right = context.GetParam("other").GetIntValue();

                return (left <= right).GetAsRuntimeObject();
            });
        Int.AddInstanceMethod(lessThanOrEqual, null);

        Method greaterThan = new(
            name: "greaterThan",
            returnType: Boolean,
            parameters: [new ParameterDefinition(name: "other", type: Int),],
            body: (self, _, context) =>
            {
                IntValue left = self.GetIntValue();
                IntValue right = context.GetParam("other").GetIntValue();

                return (left > right).GetAsRuntimeObject();
            });
        Int.AddInstanceMethod(greaterThan, null);

        Method greaterThanOrEqual = new(
            name: "greaterThanOrEqual",
            returnType: Boolean,
            parameters: [new ParameterDefinition(name: "other", type: Int),],
            body: (self, _, context) =>
            {
                IntValue left = self.GetIntValue();
                IntValue right = context.GetParam("other").GetIntValue();

                return (left >= right).GetAsRuntimeObject();
            });
        Int.AddInstanceMethod(greaterThanOrEqual, null);

        Method incrementInstance = new(
            name: "increment",
            returnType: Unit,
            parameters: [new ParameterDefinition(name: "amount", type: Int, defaultValue: IntValue.CreateObject(1)),],
            body: (self, _, context) =>
            {
                IntValue selfAsInt = self.GetIntValue();
                IntValue amount = context.GetParam("amount").GetIntValue();

                context.UpdateThis((selfAsInt + amount).GetAsRuntimeObject());
                return UnitValue.CreateObject();
            });
        Int.AddInstanceMethod(incrementInstance, null);

        Method decrementInstance = new(
            name: "decrement",
            returnType: Unit,
            parameters: [new ParameterDefinition(name: "amount", type: Int, defaultValue: IntValue.CreateObject(1)),],
            body: (self, _, context) =>
            {
                IntValue selfAsInt = self.GetIntValue();
                IntValue amount = context.GetParam("amount").GetIntValue();

                context.UpdateThis((selfAsInt - amount).GetAsRuntimeObject());
                return UnitValue.CreateObject();
            });
        Int.AddInstanceMethod(decrementInstance, null);

        // Todo: Add other IntType methods
    }

    private static void InitialiseStringType()
    {
        Method newMethod = new(
            name: "new",
            returnType: String,
            parameters: [],
            body: (self, args, context) =>
            {
                throw new NotImplementedException();
                // Todo: Add
            });
        String.AddStaticMethod(newMethod, null);

        Method stringAddMethod = new(
            name: "add",
            returnType: String,
            parameters: [new ParameterDefinition(name: "other", type: String),],
            body: (self, _, context) =>
            {
                StringValue left = self.GetStringValue();
                StringValue right = context.GetParam("other").GetStringValue();

                StringValue combinedObject = left + right;

                return new RuntimeObject(combinedObject, String);
            });

        String.AddInstanceMethod(stringAddMethod, null);

        Method toString = new(
            name: "toString",
            returnType: String,
            parameters: [],
            body: (self, _, _) =>
            {
                return self;
            });

        String.AddInstanceMethod(toString, null);

        Method staticConcatMethod = new(
            name: "concat",
            returnType: String,
            unlimitedPositionalArgumentsType: Object,
            unlimitedKeywordArgumentsType: null,
            body: (_, args, context) =>
            {
                // Todo: Change how positional args stored in context (naming)
                // Todo: Get all positional args from context, and add to full string
                string fullString = string.Empty;

                foreach (RuntimeObject item in context.GetPositionalArgs())
                {
                    StringValue valueAsStringObject =
                        item.ConvertToStringValue(context, context.CallSiteLocation);

                    if (fullString != string.Empty)
                        fullString += ' ';

                    fullString += valueAsStringObject.Value;
                }

                return StringValue.CreateObject(fullString);
            });

        String.AddStaticMethod(staticConcatMethod, null);

        Method instanceConcatMethod = new(
            name: "concat",
            returnType: String,
            parameters: [new ParameterDefinition(name: "other", type: Object),],
            body: (self, _, context) =>
            {
                StringValue left = self.GetStringValue();

                RuntimeObject right = context.GetParam("other");
                StringValue rightAsStringObject = right.ConvertToStringValue(context, context.CallSiteLocation);

                return new RuntimeObject(left + " " + rightAsStringObject, String);
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
                StringValue selfAsString = self.GetStringValue();
                IntValue start = context.GetParam("start").GetIntValue();
                IntValue end = context.GetParam("end").GetIntValue();

                int selfLength = selfAsString.Length;

                if (start > end)
                    Errors.AlwaysThrow(
                        new InvalidRangeError(
                            $"Start cannot be greater than end value ({start} > {end})"),
                        null /* Todo: Add a better source location */);

                if (start < (IntValue)0)
                    Errors.AlwaysThrow(new InvalidRangeError($"Start cannot be less than zero ({start} < 0)"),
                        null /* Todo: Add a better source location */);

                if (end > (IntValue)selfLength)
                    Errors.AlwaysThrow(
                        new InvalidRangeError(
                            $"End cannot be greater than the string length ({end} > {selfLength})"),
                        null /* Todo: Add a better source location */);

                string substring = selfAsString[start.RawValue, end.RawValue];
                return StringValue.CreateObject(substring);
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
                StringValue selfAsString = self.GetStringValue();

                IntValue index = context.GetParam("index").GetIntValue();

                if (index > (IntValue)selfAsString.Length)
                    Errors.AlwaysThrow(
                        new InvalidRangeError(
                            $"Index cannot be greater than the string length ({index.Value} > {selfAsString.Length})"),
                        null /* Todo: Add a better source location */);

                if (index < (IntValue)0)
                    Errors.AlwaysThrow(new InvalidRangeError(
                            $"Index cannot be less than zero ({index.Value} < 0)"),
                        null /* Todo: Add a better source location */);

                return StringValue.CreateObject(selfAsString[index.RawValue]);
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
                StringValue selfAsString = self.GetStringValue();
                StringValue findValue = context.GetParam("value").GetStringValue();

                IntValue index = selfAsString.GetIndexOf(findValue);

                if (selfAsString.Length == 0)
                    index = (IntValue)(-1);

                if (index == (IntValue)(-1))
                    return OptionalValue.CreateObject(null);

                return OptionalValue.CreateObject(index.GetAsRuntimeObject());
            });
        String.AddInstanceMethod(findMethod, null);

        Method containsMethod = new(
            name: "contains",
            returnType: Boolean,
            parameters: [new ParameterDefinition(name: "substring", type: String),],
            body: (self, _, context) =>
            {
                StringValue selfAsString = self.GetStringValue();
                StringValue containsValue = context.GetParam("substring").GetStringValue();

                return selfAsString.Contains(containsValue).GetAsRuntimeObject();
            });
        String.AddInstanceMethod(containsMethod, null);

        Attribute lengthAttribute = new(
            name: "length",
            type: Int,
            valueGetter: (self, _) =>
            {
                StringValue selfValue = self.GetStringValue();
                return IntValue.CreateObject(selfValue.Length);
            });
        String.AddInstanceAttribute(lengthAttribute, null);

        // Todo: Add other StringType methods
    }

    private static void InitialiseTerminalType()
    {
        Method writeMethod = new(
            name: "writeLine",
            returnType: Unit,
            unlimitedPositionalArgumentsType: Object,
            parameters:
            [
                new ParameterDefinition(name: "separator", type: String, defaultValue: StringValue.CreateObject(" ")),
                // Todo: Change all SystemError calls to have a unique identifier, to find their location in the code.
                new ParameterDefinition(name: "end", type: String, defaultValue: StringValue.CreateObject("\n")),
            ],
            unlimitedKeywordArgumentsType: null,
            body: (_, _, context) => InternalMethods.Terminal.WriteLine(context));

        Terminal.AddStaticMethod(writeMethod, null);

        Method readMethod = new(
            name: "readLine",
            returnType: String,
            parameters:
            [
                new ParameterDefinition(name: "message", type: String, defaultValue: StringValue.CreateObject("")),
                new ParameterDefinition(name: "default", type: String, nullable: true, defaultValue: NullValue.CreateObject()),
            ],
            body: (_, _, context) => InternalMethods.Terminal.ReadLine(context));

        Terminal.AddStaticMethod(readMethod, null);

        Method readIntMethod = new(
            name: "readInt",
            returnType: Int,
            parameters:
            [
                new ParameterDefinition(name: "message", type: String, defaultValue: StringValue.CreateObject("")),
                new ParameterDefinition(name: "min", type: Int, nullable: true, defaultValue: NullValue.CreateObject()),
                new ParameterDefinition(name: "max", type: Int, nullable: true, defaultValue: NullValue.CreateObject()),
            ],
            body: (_, _, context) => InternalMethods.Terminal.ReadInteger(context));

        Terminal.AddStaticMethod(readIntMethod, null);

        Method readFloatMethod = new(
            name: "readFloat",
            returnType: Int,
            parameters:
            [
                new ParameterDefinition(name: "message", type: String, defaultValue: StringValue.CreateObject("")),
                new ParameterDefinition(name: "min", type: Float, nullable: true, defaultValue: NullValue.CreateObject()),
                new ParameterDefinition(name: "max", type: Float, nullable: true, defaultValue: NullValue.CreateObject()),
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
                    defaultValue: BooleanOutputStyleValue.CreateObject(BooleanOutputStyleValue.Style.Word)),
                new ParameterDefinition(name: "immediate", type: Boolean, defaultValue: BooleanValue.CreateObject(false)),
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
        Method newMethod = new(
            name: "new",
            returnType: Float,
            parameters: [],
            body: (self, args, context) => FloatValue.CreateObject(0));
        Float.AddStaticMethod(newMethod, null);

        Method toString = new(
            name: "toString",
            returnType: String,
            parameters: [],
            body: (self, _, _) =>
            {
                FloatValue selfAsFloat = self.GetFloatValue();

                return selfAsFloat.ToStringValue().GetAsRuntimeObject();
            });

        Float.AddInstanceMethod(toString, null);
    }

    private static void InitialiseBooleanType()
    {
        Method newMethod = new(
            name: "new",
            returnType: Boolean,
            parameters: [],
            body: (self, args, context) => BooleanValue.CreateObject(false));
        Boolean.AddStaticMethod(newMethod, null);

        Method toString = new(
            name: "toString",
            returnType: String,
            parameters: [],
            body: (self, _, _) => self.GetBooleanValue().ToStringValue().GetAsRuntimeObject());

        Boolean.AddInstanceMethod(toString, null);

        // Todo: Add more BooleanType methods
    }

    private static void InitialiseBooleanOutputStylesType()
    {
        BooleanOutputStyleValue wordStyle = new(BooleanOutputStyleValue.Style.Word);
        BooleanOutputStyleValue yesNoStyle = new(BooleanOutputStyleValue.Style.YesNo);
        BooleanOutputStyleValue charStyle = new(BooleanOutputStyleValue.Style.Char);
        BooleanOutputStyleValue onOffStyle = new(BooleanOutputStyleValue.Style.OnOff);
        BooleanOutputStyleValue binaryStyle = new(BooleanOutputStyleValue.Style.Binary);
        BooleanOutputStyles.AddStaticAttribute(new Attribute("word", BooleanOutputStyles,
            (_, _) => wordStyle.GetAsRuntimeObject()), null);
        BooleanOutputStyles.AddStaticAttribute(new Attribute("yesNo", BooleanOutputStyles,
            (_, _) => yesNoStyle.GetAsRuntimeObject()), null);
        BooleanOutputStyles.AddStaticAttribute(new Attribute("char", BooleanOutputStyles,
            (_, _) => charStyle.GetAsRuntimeObject()), null);
        BooleanOutputStyles.AddStaticAttribute(new Attribute("onOff", BooleanOutputStyles,
            (_, _) => onOffStyle.GetAsRuntimeObject()), null);
        BooleanOutputStyles.AddStaticAttribute(new Attribute("binary", BooleanOutputStyles,
            (_, _) => binaryStyle.GetAsRuntimeObject()), null);
    }

    private static void InitialiseNullType()
    {
        Method newMethod = new(
            name: "new",
            returnType: Null,
            parameters: [],
            body: (self, args, context) => NullValue.CreateObject());
        Null.AddStaticMethod(newMethod, null);

        Method toString = new(
            name: "toString",
            returnType: String,
            parameters: [],
            body: (_, _, _) => StringValue.CreateObject("null"));

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
                new ParameterDefinition(name: "places", type: Int, defaultValue: IntValue.CreateObject(0)),
            ],
            body: (_, _, context) => MathFunctions.Truncate(context));
        Math.AddStaticMethod(truncateMethod, null);
    }
}
