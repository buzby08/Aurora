using System.Diagnostics;
using Aurora.Core;
using Aurora.Evaluator.BuiltinObjects;
using Aurora.Evaluator.Internals.RuntimeValues;

namespace Aurora.Evaluator.Internals;

public class RuntimeObject
{
    public TypeObject InstanceOf = Builtins.Type;

    public RuntimeObject(BaseValue value, TypeObject instanceOf)
    {
        this.InstanceOf = instanceOf;
        this.Value = value;
    }

    public RuntimeObject(BaseValue value)
    {
        this.Value = value;
    }

    public BaseValue Value { get; set; }

    public RuntimeType GetRuntimeType() => this.InstanceOf.ActualValue;
    public string GetInstanceName() => this.InstanceOf.Name;

    public StringValue ConvertToStringValue(RuntimeContext context, SourceLocation sourceLocation)
    {
        RuntimeObject evaluatedValueAsObject =
            this.InstanceOf.GetInstanceMethod("toString", sourceLocation)
                .Invoke(this, null, [], context, sourceLocation);
        StringValue valueAsString = evaluatedValueAsObject.GetStringValue();
        return valueAsString;
    }

    public string ConvertToCSharpString(RuntimeContext context, SourceLocation location)
    {
        RuntimeObject evaluatedValueAsObject =
            this.InstanceOf.GetInstanceMethod("toString", location)
                .Invoke(this, null, [], context, location);
        StringValue valueAsString = evaluatedValueAsObject.GetStringValue();
        return valueAsString.RawValue;
    }

    public static RuntimeObject CreateFromToken(Token token, RuntimeContext context, out string? variableName)
    {
        variableName = null;
        return token switch
        {
            StringToken s => StringValue.CreateObject(s.ValueAsString),
            NumberToken n => CreateFromNumberToken(n),
            WordToken w => CreateFromWordToken(w, context, out variableName),
            _ => Errors.AlwaysThrow<RuntimeObject>(
                new SystemError($"{token.Type} cannot be converted to a runtime object."), token.StartLocation),
        };
    }

    public virtual bool Equals(RuntimeObject other)
    {
        if (!this.InstanceOf.Equals(other.InstanceOf)) return false;

        if (this.Value.Value is null ^ other.Value.Value is null) return false;

        return this.Value.Value?.Equals(other.Value.Value) ?? true;
    }

    public virtual RuntimeObject Invoke(List<Argument> arguments, RuntimeContext parentContext, SourceLocation callSiteLocation)
    {
        Errors.AlwaysThrow(new UnsupportedOperationError("Object is not invokable"),
            null /* Todo: Try add a better source value*/);
        throw new UnreachableException();
    }

    private static RuntimeObject CreateFromWordToken(WordToken token, RuntimeContext context, out string? variableName)
    {
        variableName = null;

        if (token.ValueAsString == TrueValue)
            return BooleanValue.CreateObject(true);
        if (token.ValueAsString == FalseValue)
            return BooleanValue.CreateObject(false);
        if (token.ValueAsString == NullValue)
            return RuntimeValues.NullValue.CreateObject();

        variableName = token.ValueAsString;

        return context.Get(token.ValueAsString, token.StartLocation);
    }

    private static RuntimeObject CreateFromNumberToken(NumberToken token)
    {
        string value = token.ValueAsString;

        if (value.Contains('.'))
            return FloatValue.CreateFromString(value).GetAsRuntimeObject();

        return IntValue.CreateFromString(value).GetAsRuntimeObject();
    }

    public StringValue GetStringValue() => (StringValue)this.Value;
    public IntValue GetIntValue() => (IntValue)this.Value;
    public OptionalValue GetOptionalValue() => (OptionalValue)this.Value;
    public BooleanValue GetBooleanValue() => (BooleanValue)this.Value;
    public FloatValue GetFloatValue() => (FloatValue)this.Value;
    public LogicIfReturnValue GetLogicIfReturnValue() => (LogicIfReturnValue)this.Value;
    public ArrayValue GetArrayValue() => (ArrayValue)this.Value;
    public BlockValue GetBlockValue() => (BlockValue)this.Value;
    public InterfaceValue GetInterfaceValue() => (InterfaceValue)this.Value;
    public BooleanOutputStyleValue GetBooleanOutputStyleValue() => (BooleanOutputStyleValue)this.Value;
    public TypeValue GetTypeValue() => (TypeValue)this.Value;

    public T GetValueAsBase<T>() where T : BaseValue => (T)this.Value;
    public T? GetRawValue<T>(SourceLocation? location) => this.Value.GetValue<T>(location);

    public bool IsInstanceOf(TypeObject other)
    {
        return this.InstanceOf.Equals(other);
    }

    public virtual Method GetStaticMethod(string name, SourceLocation location)
    {
        return this.InstanceOf.GetStaticMethod(name, location);
    }

    public virtual Method GetInstanceMethod(string name, SourceLocation location)
    {
        return this.InstanceOf.GetInstanceMethod(name, location);
    }

    public virtual Attribute GetStaticAttribute(string name, SourceLocation location)
    {
        return this.InstanceOf.GetStaticAttribute(name, location);
    }

    public virtual Attribute GetInstanceAttribute(string name, SourceLocation location)
    {
        return this.InstanceOf.GetInstanceAttribute(name, location);
    }

    public override string ToString()
    {
        return $"RuntimeObject<Instance of {this.InstanceOf.Name}, value: {this.Value}>";
    }

    private const string TrueValue = "true";
    private const string FalseValue = "false";
    private const string NullValue = "null";
}
