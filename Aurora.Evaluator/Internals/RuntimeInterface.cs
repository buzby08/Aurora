using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Aurora.Core;

namespace Aurora.Evaluator.Internals;

public class RuntimeInterface : RuntimeObject
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

    public void EnsureTypeMeetsContract(RuntimeType type, SourceLocation? location)
    {
        List<string> invalidMethods = [];

        foreach (string methodName in this._methods.Keys)
        {
            Method method = this._methods[methodName];
            Method? methodFromType = type.GetInstanceMethodOrDefault(methodName, location);

            if (methodFromType is null || !methodFromType.Equals(method)) invalidMethods.Add(methodName);
        }

        if (invalidMethods.Count > 0) ThrowContractViolation(invalidMethods.ToArray(), location);
    }

    [DoesNotReturn]
    private void ThrowContractViolation(string[] invalidMembers, SourceLocation? location)
    {
        Errors.AlwaysThrow(new ContractError(
            contractProvider: "interface",
            className: this.Name,
            invalidMembers: invalidMembers
        ), location);
        throw new UnreachableException();
    }

    public override string ToString() => $"{nameof(RuntimeObject)} {nameof(RuntimeInterface)}({this.Name})";

    public override bool Equals(RuntimeObject other)
    {
        throw new NotImplementedException();
    }
}
