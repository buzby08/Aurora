using System.Diagnostics;
using Aurora.Core;
using Aurora.Evaluator.BuiltinObjects;

namespace Aurora.Evaluator.Internals;

public abstract class RuntimeObject
{
    public Type Type;

    public StringObject ConvertToStringObject(RuntimeContext context, SourceLocation sourceLocation)
    {
        RuntimeObject evaluatedValueAsObject =
            this.Type.GetInstanceMethod("toString", sourceLocation)
                .Invoke(this, null, [], context, sourceLocation);
        StringObject valueAsString = (StringObject)evaluatedValueAsObject;
        return valueAsString;
    }

    public string ConvertToCSharpString(RuntimeContext context, SourceLocation location)
    {
        RuntimeObject evaluatedValueAsObject =
            this.Type.GetInstanceMethod("toString", location)
                .Invoke(this, null, [], context, location);
        StringObject valueAsString = (StringObject)evaluatedValueAsObject;
        return valueAsString.Value;
    }

    public static RuntimeObject CreateFromToken(Token token, RuntimeContext context, out string? variableName)
    {
        variableName = null;
        return token switch
        {
            StringToken s => new StringObject(s.ValueAsString),
            NumberToken n => CreateFromNumberToken(n),
            WordToken w => CreateFromWordToken(w, context, out variableName),
            _ => Errors.AlwaysThrow<RuntimeObject>(
                new SystemError($"{token.Type} cannot be converted to a runtime object."), token.StartLocation),
        };
    }

    public abstract bool Equals(RuntimeObject other);

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
            return new BooleanObject(true);
        if (token.ValueAsString == FalseValue)
            return new BooleanObject(false);
        if (token.ValueAsString == NullValue)
            return new NullObject();

        variableName = token.ValueAsString;

        return context.Get(token.ValueAsString, token.StartLocation);
    }

    private static RuntimeObject CreateFromNumberToken(NumberToken token)
    {
        string value = token.ValueAsString;

        if (value.Contains('.'))
            return new FloatObject(value);

        return new IntObject(value);
    }

    public override string ToString()
    {
        return $"Token: {this.Type}";
    }

    private static string TrueValue = "true";
    private static string FalseValue = "false";
    private static string NullValue = "null";
}
