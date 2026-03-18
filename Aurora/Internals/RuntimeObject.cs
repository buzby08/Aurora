using Aurora.BuiltinMethods;

namespace Aurora.Internals;

internal class RuntimeObject
{
    public Type Type;

    public static RuntimeObject CreateFromToken(Token token, RuntimeContext context)
    {
        return token switch
        {
            StringToken s => new StringObject(s.ValueAsString),
            NumberToken n => CreateFromNumberToken(n),
            WordToken w => CreateFromWordToken(w, context),
            _ => Errors.AlwaysThrow<RuntimeObject>(
                new SystemError($"{token.Type} cannot be converted to a runtime object."))
        };
    }

    private static RuntimeObject CreateFromWordToken(WordToken token, RuntimeContext context)
    {
        return context.Get(token.ValueAsString);
    }

    private static RuntimeObject CreateFromNumberToken(NumberToken token)
    {
        string value = token.ValueAsString;

        if (value.Contains('.'))
            return new FloatObject(value);

        return new IntObject(value);
    }
}