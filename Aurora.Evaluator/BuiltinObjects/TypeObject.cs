using System.Diagnostics;
using Aurora.Core;
using Aurora.Evaluator.Internals;
using Aurora.Evaluator.Internals.RuntimeValues;
using Attribute = Aurora.Evaluator.Internals.Attribute;

namespace Aurora.Evaluator.BuiltinObjects;

public class TypeObject : RuntimeObject
{
    public TypeObject? SuperType;
    public RuntimeType ActualValue => ((TypeValue)this.Value).GetValue(null);

    public TypeObject(RuntimeType value) : base(new TypeValue(value))
    {
        this.InstanceOf = Builtins.Type;
        this.SuperType = Builtins.Object;
    }

    public TypeObject(RuntimeType value, TypeObject superType) : base(new TypeValue(value))
    {
        this.InstanceOf = this;
        this.SuperType = superType;
    }

    public void MarkFinal(SourceLocation? location)
    {
        this.ActualValue.MarkFinal(location);
    }

    public void AddInterface(RuntimeObject @interface, SourceLocation? location)
    {
        if (!@interface.IsInstanceOf(Builtins.Interface))
            Errors.AlwaysThrow(
                new TypeMismatchError(
                    $"Cannot add object of type {@interface.GetInstanceName()} to the interface field " +
                    $"for type {this.ActualValue.Name}", user: location is not null),
                location);

        this.ActualValue.AddInterface(@interface.GetInterfaceValue().RawValue, location);
    }

    public bool IsSubclassOf(TypeObject other)
    {
        if (this.ActualValue.Equals(other.ActualValue)) return true;

        if (this.SuperType is null) return false;

        return this.SuperType.IsSubclassOf(other);
    }

    public void AddStaticMethod(Method method, SourceLocation? location)
    {
        if (method.Name == "new")
        {
            this.ActualValue.AddNewMethod(method, this, location);
            return;
        }


        this.ActualValue.AddStaticMethod(method, location);
    }

    public void AddInstanceMethod(Method method, SourceLocation? location)
    {
        this.ActualValue.AddInstanceMethod(method, location);
    }

    public void AddStaticAttribute(Attribute attribute, SourceLocation? location)
    {
        this.ActualValue.AddStaticAttribute(attribute, location);
    }

    public void AddInstanceAttribute(Attribute attribute, SourceLocation? location)
    {
        this.ActualValue.AddInstanceAttribute(attribute, location);
    }

    public override Method GetStaticMethod(string name, SourceLocation location)
    {
        Method? method = this.GetStaticMethodOrDefault(name, location);

        if (method is not null) return method;

        if (this.SuperType is not null)
            return this.SuperType.GetStaticMethod(name, location);

        Errors.AlwaysThrow(new InvalidMethodError($"Type {this.ActualValue.Name} has no static method {name}"), location);
        throw new UnreachableException();
    }

    public override Method GetInstanceMethod(string name, SourceLocation location)
    {
        Method? method = this.GetInstanceMethodOrDefault(name, location);

        if (method is not null) return method;

        if (this.SuperType is not null)
            return this.SuperType.GetInstanceMethod(name, location);

        Errors.AlwaysThrow(new InvalidMethodError($"Type {this.ActualValue.Name} has no static method {name}"), location);
        throw new UnreachableException();
    }

    public override Attribute GetStaticAttribute(string name, SourceLocation location)
    {
        Attribute? attribute = this.GetStaticAttributeOrDefault(name, location);
        if (attribute is null)
            return this.InstanceOf.GetStaticAttribute(name, location);

        return attribute;
    }

    public override Attribute GetInstanceAttribute(string name, SourceLocation location)
    {
        Attribute? attribute = this.GetInstanceAttributeOrDefault(name, location);
        if (attribute is null)
            return this.InstanceOf.GetInstanceAttribute(name, location);

        return attribute;
    }

    public Method? GetStaticMethodOrDefault(string name, SourceLocation location)
    {
        return this.ActualValue.GetStaticMethodOrDefault(name, location);
    }

    public Method? GetInstanceMethodOrDefault(string name, SourceLocation location)
    {
        return this.ActualValue.GetInstanceMethodOrDefault(name, location);
    }

    public Attribute? GetStaticAttributeOrDefault(string name, SourceLocation location)
    {
        return this.ActualValue.GetStaticAttributeOrDefault(name, location);
    }

    public Attribute? GetInstanceAttributeOrDefault(string name, SourceLocation location)
    {
        return this.ActualValue.GetInstanceAttributeOrDefault(name, location);
    }

    public string Name => this.ActualValue.Name;
    public bool IsStatic => this.ActualValue.IsStatic;


    public override bool Equals(RuntimeObject other)
    {
        if (other is not TypeObject type) return false;
        return this.ActualValue.Equals(type.ActualValue);
    }

    public override string ToString()
    {
        return $"Type<{this.ActualValue.Name}>";
    }
}
