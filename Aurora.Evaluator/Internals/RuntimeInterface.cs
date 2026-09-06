using Aurora.Core;

namespace Aurora.Evaluator.Internals;

internal class RuntimeInterface : RuntimeObject
{
    public required string Name;

    private Dictionary<string, Method> _methods = new();

    public void AddMethod(string name, RuntimeType returnType, ParameterDefinition[] parameters)
    {
        Method method = new(
            name: name,
            returnType: returnType,
            parameters: parameters,
            body: (_, _, _) => throw new NotImplementedException());

        this._methods.Add(name, method);
    }

    public Method? GetMethodOrDefault(string name) => this._methods.GetValueOrDefault(name);
    public Method GetMethod(string name, SourceLocation? location)
    {
        Method? method = this.GetMethodOrDefault(name);

        if (method is null)
            Errors.AlwaysThrow(new InvalidMethodError($"Interface {this.Name} has no method {name}"), location);

        return method;
    }

    public Method GetFilledMethod(string name, MethodBody body, SourceLocation? location)
    {
        Method method = GetMethod(name, location);

        Method newMethod = new(
            name: method.Name,
            returnType: method.DeclaringType,
            parameters: method.Parameters,
            body: body);

        return newMethod;
    }

    public override string ToString() => $"{nameof(RuntimeObject)} {nameof(RuntimeInterface)}({this.Name})";

    public override bool Equals(RuntimeObject other)
    {
        throw new NotImplementedException();
    }
}
