using Aurora.Core;
using Aurora.Evaluator.Internals;
using Attribute = Aurora.Evaluator.Internals.Attribute;

namespace Aurora.Evaluator.BuiltinObjects;

public class TypeObject : RuntimeObject
{
    public RuntimeType Value;

    public TypeObject(RuntimeType value)
    {
        this.Value = value;
        this.Type = Builtins.Type.Value;
    }

    public TypeObject(RuntimeType value, RuntimeType type)
    {
        this.Value = value;
        this.Type = type;
    }

    public void MarkFinal(SourceLocation? location)
    {
        this.Value.MarkFinal(location);
    }

    public void AddInterface(InterfaceObject @interface, SourceLocation? location)
    {
        this.Value.AddInterface(@interface.Value, location);
    }

    public bool IsSubclassOf(RuntimeObject other)
    {
        return this.Value.IsSubclassOf(other);
    }

    public void AddStaticMethod(Method method, SourceLocation? location)
    {
        if (method.Name == "new")
        {
            this.Value.AddNewMethod(method, this, location);
            return;
        }


        this.Value.AddStaticMethod(method, location);
    }

    public void AddInstanceMethod(Method method, SourceLocation? location)
    {
        this.Value.AddInstanceMethod(method, location);
    }

    public void AddStaticAttribute(Attribute attribute, SourceLocation? location)
    {
        this.Value.AddStaticAttribute(attribute, location);
    }

    public void AddInstanceAttribute(Attribute attribute, SourceLocation? location)
    {
        this.Value.AddInstanceAttribute(attribute, location);
    }

    public Method GetStaticMethod(string name, SourceLocation location)
    {
        return this.Value.GetStaticMethod(name, location);
    }

    public Method GetInstanceMethod(string name, SourceLocation location)
    {
        return this.Value.GetInstanceMethod(name, location);
    }

    public Attribute GetStaticAttribute(string name, SourceLocation location)
    {
        return this.Value.GetStaticAttribute(name, location);
    }

    public Attribute GetInstanceAttribute(string name, SourceLocation location)
    {
        return this.Value.GetInstanceAttribute(name, location);
    }

    public string Name => this.Value.Name;
    public bool IsStatic => this.Value.IsStatic;


    public override bool Equals(RuntimeObject other)
    {
        if (other is not TypeObject type) return false;
        return this.Value.Equals(type.Value);
    }

    public override string ToString()
    {
        return $"Type<{this.Value.Name}>";
    }
}
