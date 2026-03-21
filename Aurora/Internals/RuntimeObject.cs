using Aurora.BuiltinMethods;

namespace Aurora.Internals;

internal class RuntimeObject
{
    public Type Type;

    public StringObject ConvertToStringObject(RuntimeContext context)
    {
        RuntimeObject evaluatedValueAsObject =
            this.Type.GetInstanceMethod("toString")
                .Invoke(this, [], new RuntimeContext(context));
        StringObject valueAsString = (StringObject)evaluatedValueAsObject;
        return valueAsString;
    }
    
    public string ConvertToCSharpString(RuntimeContext context)
    {
        RuntimeObject evaluatedValueAsObject =
            this.Type.GetInstanceMethod("toString")
                .Invoke(this, [], new RuntimeContext(context));
        StringObject valueAsString = (StringObject)evaluatedValueAsObject;
        return valueAsString.Value;
    }

    public static RuntimeObject CreateFromToken(Token token, RuntimeContext context, int? position = null)
    {
        return token switch
        {
            StringToken s => new StringObject(s.ValueAsString),
            NumberToken n => CreateFromNumberToken(n),
            WordToken w => CreateFromWordToken(w, context),
            _ => Errors.AlwaysThrow<RuntimeObject>(
                new SystemError($"{token.Type} cannot be converted to a runtime object."), position: position)
        };
    }

    private static RuntimeObject CreateFromWordToken(WordToken token, RuntimeContext context)
    {
        if (token.ValueAsString == TrueValue)
            return new BooleanObject(true);
        if (token.ValueAsString == FalseValue)
            return new BooleanObject(false);
        if (token.ValueAsString == NullValue)
            return new NullObject();

        return context.Get(token.ValueAsString);
    }

    private static RuntimeObject CreateFromNumberToken(NumberToken token)
    {
        string value = token.ValueAsString;

        if (value.Contains('.'))
            return new FloatObject(value);

        return new IntObject(value);
    }

    private static string TrueValue = "true";
    private static string FalseValue = "false";
    private static string NullValue = "null";
}