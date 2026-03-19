using System.Globalization;
using Aurora.BuiltinMethods;

namespace Aurora.Internals;

internal class Builtins
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

        Null = new Type("Null", type: Type);

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

        // Todo: Add other StringType methods
    }

    public static void InitialiseTerminalType()
    {
        Method writeMethod = new(
            name: "writeLine",
            returnType: Unit,
            parameters:
            [
                new ParameterDefinition(name: "value", type: String),
                new ParameterDefinition(name: "end", type: String, defaultValue: new StringObject("\n"))
            ],
            body: (self, args, context) =>
            {
                StringObject value = (StringObject)context.GetParam("value");
                StringObject defaultParam = (StringObject)context.GetParam("end");

                Console.Write(value.Value + defaultParam.Value);

                return new UnitObject();
            });

        Terminal.AddStaticMethod(writeMethod);

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

    // Todo: Add BooleanType methods

    // Todo: Add NullType methods
}