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
    public static TypeObject ICollection = null!;

    public static void InitialiseTypes()
    {
        Type = new TypeObject(new RuntimeType(nameof(Type)));

        Object = new TypeObject(new RuntimeType(nameof(Object)))
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

        ICollection = new TypeObject(new RuntimeInterface(nameof(ICollection)));

        // Todo: Add tests for interfaces.

        InitialiseObjectType();
        Object.MarkFinal(null);

        InitialiseTypeType();
        Type.MarkFinal(null);

        InitialiseInterfaceType();
        Interface.MarkFinal(null);

        InitialiseBlockType();
        Block.MarkFinal(null);

        InitialiseOptionalType();
        Optional.MarkFinal(null);

        InitialiseIntType();
        Int.MarkFinal(null);

        Unit.MarkFinal(null);

        InitialiseFloatType();
        Float.MarkFinal(null);

        InitialiseStringType();
        String.MarkFinal(null);

        InitialiseBooleanType();
        Boolean.MarkFinal(null);

        InitialiseNullType();
        Null.MarkFinal(null);

        InitialiseTerminalType();
        Terminal.MarkFinal(null);

        InitialiseBooleanOutputStylesType();
        BooleanOutputStyles.MarkFinal(null);

        InitialiseMathType();
        Math.MarkFinal(null);

        InitialiseLogicType();
        Logic.MarkFinal(null);

        InitialiseLogicIfReturnType();
        LogicIfReturn.MarkFinal(null);

        InitialiseLoopType();
        Loop.MarkFinal(null);

        InitialiseICollectionInterface();

        InitialiseArrayType();
        Array.MarkFinal(null);
    }

    private static void InitialiseBlockType()
    {
        Method newMethod = new(
            name: "new",
            returnType: Block,
            parameters: [],
            body: (self, args, context) => { throw new NotImplementedException(); });
        Block.AddStaticMethod(newMethod, null);

        Method toStringMethod = new(
            name: "toString",
            returnType: String,
            parameters: [],
            body: (self, args, context) =>
            {
                BlockRuntimeValue selfAsBlock = self.GetBlockValue();

                return selfAsBlock.ToStringValue().GetAsRuntimeObject();
            });
        Block.AddInstanceMethod(toStringMethod, null);
    }

    private static void InitialiseObjectType()
    {
        Method newMethod = new(
            name: "new",
            returnType: Object,
            parameters: [],
            body: (self, args, context) => { throw new NotImplementedException(); });
        Object.AddStaticMethod(newMethod, null);

        Method typeCreateMethod = new(
            name: "create",
            returnType: Unit,
            parameters: null,
            body: InternalMethods.Type.Create);

        Object.AddStaticMethod(typeCreateMethod, null);

        Method typeSetMethod = new(
            name: "set",
            returnType: Unit,
            parameters: null,
            body: InternalMethods.Type.Set);

        Object.AddStaticMethod(typeSetMethod, null);

        Method toString = new(
            name: "toString",
            returnType: String,
            parameters: [],
            body: (self, _, _) => InternalMethods.Type.ToString(self));

        Object.AddInstanceMethod(toString, null);
        Object.AddStaticMethod(toString, null);

        Method equals = new(
            name: "equals",
            returnType: Boolean,
            parameters: [new ParameterDefinition(name: "other", type: Object),],
            body: (self, _, context) => InternalMethods.Type.Equals(self, context));
        Object.AddInstanceMethod(equals, null);
        Object.AddStaticMethod(equals, null);
    }

    private static void InitialiseICollectionInterface()
    {
        ICollection.AddInterfaceMethod(name: "at", returnType: Object,
            [new ParameterDefinition(name: "index", type: Int),], location: null);
        ICollection.AddInterfaceMethod(name: "length", returnType: Int, [], location: null);
    }

    private static void InitialiseInterfaceType()
    {
        Method newMethod = new(
            name: "new",
            returnType: Interface,
            parameters: [],
            body: (self, args, context) => { throw new NotImplementedException(); });
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
                    Errors.AlwaysThrow(new TypeMismatchError($"An array can only store one type"),
                        context.CallSiteLocation);

                return ArrayValue.CreateObject(positionals.ToArray());
            });
        Array.AddStaticMethod(newMethod, null);

        Method atMethod = ICollection.GetInterfaceValue().GetFilledMethod("at", (self, _, context) =>
        {
            ArrayValue selfAsArray = self.GetArrayValue();
            IntRuntimeValue index = context.GetParam("index").GetIntValue();

            if (index >= selfAsArray.Length || index < (IntRuntimeValue)0)
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

        // Method test = new(
        //     name: "test",
        //     returnType: Unit,
        //     parameters: [new ParameterDefinition("a", ICollection),],
        //     body: (self, args, context) =>
        //     {
        //
        //         Console.WriteLine($"Worked - {context.GetParam("a").ToString()}");
        //         return UnitRuntimeValue.CreateObject();
        //     });
        // Array.AddStaticMethod(test, null);
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
                Errors.AlwaysThrow(new UnsupportedOperationError("Cannot create a LogicIfReturn object"),
                    context.CallSiteLocation);
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
            body: (self, args, context) => { throw new NotImplementedException(); });
        Type.AddStaticMethod(newMethod, null);
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
                return OptionalRuntimeValue.CreateObject(valueObject);
            });
        Optional.AddStaticMethod(newMethod, null);

        Attribute isEmptyAttribute = new(
            name: "isEmpty",
            type: Boolean,
            valueGetter: (self, context) =>
            {
                RuntimeObject? value = ((OptionalRuntimeValue)self.Value).GetValue(context.CallSiteLocation);

                return BooleanRuntimeValue.CreateObject(value is null);
            });
        Optional.AddInstanceAttribute(isEmptyAttribute, null);

        Method emptyOptionalMethod = new(
            name: "empty",
            returnType: Optional,
            parameters: [],
            body: (_, _, _) => OptionalRuntimeValue.CreateObject(null));
        Optional.AddStaticMethod(emptyOptionalMethod, null);

        Attribute valueAttribute = new(
            name: "value",
            type: Type,
            valueGetter: (self, context) =>
            {
                OptionalRuntimeValue value = (OptionalRuntimeValue)self.Value;

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
                OptionalRuntimeValue selfAsOptional = (OptionalRuntimeValue)self.Value;
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
                OptionalRuntimeValue selfValue = (OptionalRuntimeValue)self.Value;

                if (selfValue.HasValue)
                    return StringRuntimeValue.CreateObject(
                        $"Optional({selfValue.RawValue!.ConvertToCSharpString(context, context.CallSiteLocation)})");

                return StringRuntimeValue.CreateObject("Optional(Empty)");
            });
        Optional.AddInstanceMethod(toStringMethod, null);
    }

    private static void InitialiseIntType()
    {
        Method newMethod = new(
            name: "new",
            returnType: Int,
            parameters: [],
            body: (self, args, context) => IntRuntimeValue.CreateObject(0));
        Int.AddStaticMethod(newMethod, null);

        Method addMethod = new(
            name: "add",
            returnType: Int,
            parameters: [new ParameterDefinition(name: "other", type: Int),],
            body: (self, _, context) =>
            {
                IntRuntimeValue left = self.GetIntValue();
                IntRuntimeValue right = context.GetParam("other").GetIntValue();

                return (left + right).GetAsRuntimeObject();
            });

        Int.AddInstanceMethod(addMethod, null);

        Method subtractMethod = new(
            name: "subtract",
            returnType: Int,
            parameters: [new ParameterDefinition(name: "other", type: Int),],
            body: (self, _, context) =>
            {
                IntRuntimeValue left = self.GetIntValue();
                IntRuntimeValue right = context.GetParam("other").GetIntValue();

                return (left - right).GetAsRuntimeObject();
            });

        Int.AddInstanceMethod(subtractMethod, null);

        Method multiplyByMethod = new(
            name: "multiplyBy",
            returnType: Int,
            parameters: [new ParameterDefinition(name: "other", type: Int),],
            body: (self, _, context) =>
            {
                IntRuntimeValue left = self.GetIntValue();
                IntRuntimeValue right = context.GetParam("other").GetIntValue();

                return (left * right).GetAsRuntimeObject();
            });

        Int.AddInstanceMethod(multiplyByMethod, null);

        Method divideByMethod = new(
            name: "divideBy",
            returnType: Float,
            parameters: [new ParameterDefinition(name: "other", type: Int),],
            body: (self, _, context) =>
            {
                IntRuntimeValue left = self.GetIntValue();
                IntRuntimeValue right = context.GetParam("other").GetIntValue();

                return (left / right).GetAsRuntimeObject();
            });

        Int.AddInstanceMethod(divideByMethod, null);

        Method toString = new(
            name: "toString",
            returnType: String,
            parameters: [],
            body: (self, _, _) =>
            {
                IntRuntimeValue selfAsInt = self.GetIntValue();

                return selfAsInt.ToStringValue().GetAsRuntimeObject();
            });

        Int.AddInstanceMethod(toString, null);

        Method lessThan = new(
            name: "lessThan",
            returnType: Boolean,
            parameters: [new ParameterDefinition(name: "other", type: Int),],
            body: (self, _, context) =>
            {
                IntRuntimeValue left = self.GetIntValue();
                IntRuntimeValue right = context.GetParam("other").GetIntValue();

                return (left < right).GetAsRuntimeObject();
            });
        Int.AddInstanceMethod(lessThan, null);

        Method lessThanOrEqual = new(
            name: "lessThanOrEqual",
            returnType: Boolean,
            parameters: [new ParameterDefinition(name: "other", type: Int),],
            body: (self, _, context) =>
            {
                IntRuntimeValue left = self.GetIntValue();
                IntRuntimeValue right = context.GetParam("other").GetIntValue();

                return (left <= right).GetAsRuntimeObject();
            });
        Int.AddInstanceMethod(lessThanOrEqual, null);

        Method greaterThan = new(
            name: "greaterThan",
            returnType: Boolean,
            parameters: [new ParameterDefinition(name: "other", type: Int),],
            body: (self, _, context) =>
            {
                IntRuntimeValue left = self.GetIntValue();
                IntRuntimeValue right = context.GetParam("other").GetIntValue();

                return (left > right).GetAsRuntimeObject();
            });
        Int.AddInstanceMethod(greaterThan, null);

        Method greaterThanOrEqual = new(
            name: "greaterThanOrEqual",
            returnType: Boolean,
            parameters: [new ParameterDefinition(name: "other", type: Int),],
            body: (self, _, context) =>
            {
                IntRuntimeValue left = self.GetIntValue();
                IntRuntimeValue right = context.GetParam("other").GetIntValue();

                return (left >= right).GetAsRuntimeObject();
            });
        Int.AddInstanceMethod(greaterThanOrEqual, null);

        Method incrementInstance = new(
            name: "increment",
            returnType: Unit,
            parameters:
            [
                new ParameterDefinition(name: "amount", type: Int, defaultValue: IntRuntimeValue.CreateObject(1)),
            ],
            body: (self, _, context) =>
            {
                IntRuntimeValue selfAsInt = self.GetIntValue();
                IntRuntimeValue amount = context.GetParam("amount").GetIntValue();

                context.UpdateThis((selfAsInt + amount).GetAsRuntimeObject());
                return UnitRuntimeValue.CreateObject();
            });
        Int.AddInstanceMethod(incrementInstance, null);

        Method decrementInstance = new(
            name: "decrement",
            returnType: Unit,
            parameters:
            [
                new ParameterDefinition(name: "amount", type: Int, defaultValue: IntRuntimeValue.CreateObject(1)),
            ],
            body: (self, _, context) =>
            {
                IntRuntimeValue selfAsInt = self.GetIntValue();
                IntRuntimeValue amount = context.GetParam("amount").GetIntValue();

                context.UpdateThis((selfAsInt - amount).GetAsRuntimeObject());
                return UnitRuntimeValue.CreateObject();
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
                StringRuntimeValue left = self.GetStringValue();
                StringRuntimeValue right = context.GetParam("other").GetStringValue();

                StringRuntimeValue combinedObject = left + right;

                return new RuntimeObject(combinedObject, String);
            });

        String.AddInstanceMethod(stringAddMethod, null);

        Method toString = new(
            name: "toString",
            returnType: String,
            parameters: [],
            body: (self, _, _) => { return self; });

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
                    StringRuntimeValue valueAsStringObject =
                        item.ConvertToStringValue(context, context.CallSiteLocation);

                    if (fullString != string.Empty)
                        fullString += ' ';

                    fullString += valueAsStringObject.Value;
                }

                return StringRuntimeValue.CreateObject(fullString);
            });

        String.AddStaticMethod(staticConcatMethod, null);

        Method instanceConcatMethod = new(
            name: "concat",
            returnType: String,
            parameters: [new ParameterDefinition(name: "other", type: Object),],
            body: (self, _, context) =>
            {
                StringRuntimeValue left = self.GetStringValue();

                RuntimeObject right = context.GetParam("other");
                StringRuntimeValue rightAsStringObject = right.ConvertToStringValue(context, context.CallSiteLocation);

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
                StringRuntimeValue selfAsString = self.GetStringValue();
                IntRuntimeValue start = context.GetParam("start").GetIntValue();
                IntRuntimeValue end = context.GetParam("end").GetIntValue();

                int selfLength = selfAsString.Length;

                if (start > end)
                    Errors.AlwaysThrow(
                        new InvalidRangeError(
                            $"Start cannot be greater than end value ({start} > {end})"),
                        null /* Todo: Add a better source location */);

                if (start < (IntRuntimeValue)0)
                    Errors.AlwaysThrow(new InvalidRangeError($"Start cannot be less than zero ({start} < 0)"),
                        null /* Todo: Add a better source location */);

                if (end > (IntRuntimeValue)selfLength)
                    Errors.AlwaysThrow(
                        new InvalidRangeError(
                            $"End cannot be greater than the string length ({end} > {selfLength})"),
                        null /* Todo: Add a better source location */);

                string substring = selfAsString[start.RawValue, end.RawValue];
                return StringRuntimeValue.CreateObject(substring);
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
                StringRuntimeValue selfAsString = self.GetStringValue();

                IntRuntimeValue index = context.GetParam("index").GetIntValue();

                if (index > (IntRuntimeValue)selfAsString.Length)
                    Errors.AlwaysThrow(
                        new InvalidRangeError(
                            $"Index cannot be greater than the string length ({index.Value} > {selfAsString.Length})"),
                        null /* Todo: Add a better source location */);

                if (index < (IntRuntimeValue)0)
                    Errors.AlwaysThrow(new InvalidRangeError(
                            $"Index cannot be less than zero ({index.Value} < 0)"),
                        null /* Todo: Add a better source location */);

                return StringRuntimeValue.CreateObject(selfAsString[index.RawValue]);
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
                StringRuntimeValue selfAsString = self.GetStringValue();
                StringRuntimeValue findValue = context.GetParam("value").GetStringValue();

                IntRuntimeValue index = selfAsString.GetIndexOf(findValue);

                if (selfAsString.Length == 0)
                    index = (IntRuntimeValue)(-1);

                if (index == (IntRuntimeValue)(-1))
                    return OptionalRuntimeValue.CreateObject(null);

                return OptionalRuntimeValue.CreateObject(index.GetAsRuntimeObject());
            });
        String.AddInstanceMethod(findMethod, null);

        Method containsMethod = new(
            name: "contains",
            returnType: Boolean,
            parameters: [new ParameterDefinition(name: "substring", type: String),],
            body: (self, _, context) =>
            {
                StringRuntimeValue selfAsString = self.GetStringValue();
                StringRuntimeValue containsValue = context.GetParam("substring").GetStringValue();

                return selfAsString.Contains(containsValue).GetAsRuntimeObject();
            });
        String.AddInstanceMethod(containsMethod, null);

        Attribute lengthAttribute = new(
            name: "length",
            type: Int,
            valueGetter: (self, _) =>
            {
                StringRuntimeValue selfValue = self.GetStringValue();
                return IntRuntimeValue.CreateObject(selfValue.Length);
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
                new ParameterDefinition(name: "separator", type: String,
                    defaultValue: StringRuntimeValue.CreateObject(" ")),
                // Todo: Change all SystemError calls to have a unique identifier, to find their location in the code.
                new ParameterDefinition(name: "end", type: String, defaultValue: StringRuntimeValue.CreateObject("\n")),
            ],
            unlimitedKeywordArgumentsType: null,
            body: (_, _, context) => InternalMethods.Terminal.WriteLine(context));

        Terminal.AddStaticMethod(writeMethod, null);

        Method readMethod = new(
            name: "readLine",
            returnType: String,
            parameters:
            [
                new ParameterDefinition(name: "message", type: String,
                    defaultValue: StringRuntimeValue.CreateObject("")),
                new ParameterDefinition(name: "default", type: String, nullable: true,
                    defaultValue: NullRuntimeValue.CreateObject()),
            ],
            body: (_, _, context) => InternalMethods.Terminal.ReadLine(context));

        Terminal.AddStaticMethod(readMethod, null);

        Method readIntMethod = new(
            name: "readInt",
            returnType: Int,
            parameters:
            [
                new ParameterDefinition(name: "message", type: String,
                    defaultValue: StringRuntimeValue.CreateObject("")),
                new ParameterDefinition(name: "min", type: Int, nullable: true,
                    defaultValue: NullRuntimeValue.CreateObject()),
                new ParameterDefinition(name: "max", type: Int, nullable: true,
                    defaultValue: NullRuntimeValue.CreateObject()),
            ],
            body: (_, _, context) => InternalMethods.Terminal.ReadInteger(context));

        Terminal.AddStaticMethod(readIntMethod, null);

        Method readFloatMethod = new(
            name: "readFloat",
            returnType: Int,
            parameters:
            [
                new ParameterDefinition(name: "message", type: String,
                    defaultValue: StringRuntimeValue.CreateObject("")),
                new ParameterDefinition(name: "min", type: Float, nullable: true,
                    defaultValue: NullRuntimeValue.CreateObject()),
                new ParameterDefinition(name: "max", type: Float, nullable: true,
                    defaultValue: NullRuntimeValue.CreateObject()),
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
                    defaultValue: BooleanOutputStyleRuntimeValue.CreateObject(BooleanOutputStyleRuntimeValue.Style
                        .Word)),
                new ParameterDefinition(name: "immediate", type: Boolean,
                    defaultValue: BooleanRuntimeValue.CreateObject(false)),
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
            body: (self, args, context) => FloatRuntimeValue.CreateObject(0));
        Float.AddStaticMethod(newMethod, null);

        Method toString = new(
            name: "toString",
            returnType: String,
            parameters: [],
            body: (self, _, _) =>
            {
                FloatRuntimeValue selfAsFloat = self.GetFloatValue();

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
            body: (self, args, context) => BooleanRuntimeValue.CreateObject(false));
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
        BooleanOutputStyleRuntimeValue wordStyle = new(BooleanOutputStyleRuntimeValue.Style.Word);
        BooleanOutputStyleRuntimeValue yesNoStyle = new(BooleanOutputStyleRuntimeValue.Style.YesNo);
        BooleanOutputStyleRuntimeValue charStyle = new(BooleanOutputStyleRuntimeValue.Style.Char);
        BooleanOutputStyleRuntimeValue onOffStyle = new(BooleanOutputStyleRuntimeValue.Style.OnOff);
        BooleanOutputStyleRuntimeValue binaryStyle = new(BooleanOutputStyleRuntimeValue.Style.Binary);
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
            body: (self, args, context) => NullRuntimeValue.CreateObject());
        Null.AddStaticMethod(newMethod, null);

        Method toString = new(
            name: "toString",
            returnType: String,
            parameters: [],
            body: (_, _, _) => StringRuntimeValue.CreateObject("null"));

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
                new ParameterDefinition(name: "places", type: Int, defaultValue: IntRuntimeValue.CreateObject(0)),
            ],
            body: (_, _, context) => MathFunctions.Truncate(context));
        Math.AddStaticMethod(truncateMethod, null);
    }
}
