using System.Diagnostics;
using Aurora.Core;

namespace Aurora.Evaluator.Internals.RuntimeValues;

public abstract class BaseRuntimeValue(object? value)
{
    public object? Value { get; set; } = value;

    public T? GetValue<T>(SourceLocation? location)
    {
        try
        {
            return (T?)this.Value;
        }
        catch
        {
            Errors.AlwaysThrow(
                new SystemError($"Could not convert object of type " +
                                $"`{this.Value?.GetType().FullName ?? "null"}` to type `{typeof(T)}`"), location);
            throw new UnreachableException();
        }
    }

    public abstract RuntimeObject GetAsRuntimeObject();

    public override string ToString() => $"{nameof(BaseRuntimeValue)}({this.Value ?? "null"})";
}
