using System;
using System.Diagnostics;

namespace Aurora;

internal class CustomClass(string name)
{
    public delegate Token CustomMethod(List<Token> positionals, Dictionary<string, Token> keywords);

    public string Name { get; } = name;
    public Dictionary<string, Func<Token>> Attributes { get; } = new();
    public Dictionary<string, CustomMethod> Methods { get; } = new();

    public void AddAttribute(string name, Func<Token> getter)
    {
        Attributes[name] = getter;
    }

    public void AddMethod(string name, CustomMethod method)
    {
        Methods[name] = method;
    }
    
    public bool HasAttribute(string name) => Attributes.ContainsKey(name);
    public bool HasMethod(string name) => Methods.ContainsKey(name);

    public Func<Token> GetAttribute(string name)
    {
        if (HasAttribute(name))
            return Attributes[name];

        return Errors.AlwaysThrow<Func<Token>>(
            new InvalidAttributeError($"Class '{this.Name}' has no attribute '{name}'"));
    }
    
    public CustomMethod GetMethod(string name)
    {
        return this.HasMethod(name)
            ? this.Methods[name]
            : Errors.AlwaysThrow<CustomMethod>(new InvalidMethodError($"Class '{this.Name}' has no method '{name}'"));
    }
}