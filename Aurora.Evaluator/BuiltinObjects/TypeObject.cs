using System.Diagnostics;
using Aurora.Core;
using Aurora.Evaluator.Internals;
using Aurora.Evaluator.Internals.RuntimeValues;
using Attribute = Aurora.Evaluator.Internals.Attribute;

namespace Aurora.Evaluator.BuiltinObjects;

public class TypeObject : RuntimeObject
{
    public TypeObject? SuperType;
    public RuntimeType? RuntimeType => GetValueAsRuntimeType();
    public RuntimeInterface? Interface => GetValueAsRuntimeInterface();
    public RuntimeInterface? InterfaceContract;

    public bool IsInterface => this.InterfaceContract is not null;

    public TypeObject(RuntimeType value) : base(new TypeRuntimeValue(value))
    {
        this.InstanceOf = Builtins.Type;
        this.SuperType = Builtins.Object;
    }

    public TypeObject(RuntimeType value, TypeObject superType) : base(new TypeRuntimeValue(value))
    {
        this.InstanceOf = Builtins.Type;
        this.SuperType = superType;
    }

    public TypeObject(RuntimeInterface interfaceContract) : base(new InterfaceRuntimeValue(interfaceContract))
    {
        this.InstanceOf = Builtins.Type;
        this.InterfaceContract = interfaceContract;
        this.SuperType = Builtins.Object;
    }

    public RuntimeType? GetValueAsRuntimeType()
    {
        BaseRuntimeValue value = this.Value;

        if (value is TypeRuntimeValue typeRuntimeValue)
            return typeRuntimeValue.GetValue(null);

        return null;
    }

    public RuntimeInterface? GetValueAsRuntimeInterface()
    {
        BaseRuntimeValue value = this.Value;

        if (value is InterfaceRuntimeValue interfaceRuntimeValue)
            return interfaceRuntimeValue.GetValue(null);

        return null;
    }

    public void EnforceIsType(SourceLocation? location)
    {
        if (this.RuntimeType is null)
            Errors.AlwaysThrow(
                new TypeMismatchError(
                    $"Cannot complete action on type `{this.Name}` as it does not hold a " +
                    $"runtime type", user: false),
                location);
    }

    public void MarkFinal(SourceLocation? location)
    {
        this.EnforceIsType(location);
        this.RuntimeType!.MarkFinal(location);
    }

    public void AddInterface(TypeObject @interface, SourceLocation? location)
    {
        this.EnforceIsType(location);

        if (!@interface.IsInterface)
            Errors.AlwaysThrow(
                new TypeMismatchError(
                    $"Cannot add object of type {@interface.GetInstanceName()} to the interface field " +
                    $"for type {this.Name}", user: location is not null),
                location);

        this.RuntimeType!.AddInterface(@interface.GetInterfaceValue().RawValue, location);
        this.SuperType = @interface;
    }

    public bool IsSubclassOf(TypeObject other)
    {
        if (this.Equals(other)) return true;

        if (this.SuperType is null) return false;

        return this.SuperType.IsSubclassOf(other);
    }

    public void AddStaticMethod(Method method, SourceLocation? location)
    {
        this.EnforceIsType(location);
        if (method.Name == "new")
        {
            this.RuntimeType!.AddNewMethod(method, this, location);
            return;
        }


        this.RuntimeType!.AddStaticMethod(method, location);
    }

    public void AddInstanceMethod(Method method, SourceLocation? location)
    {
        this.EnforceIsType(location);
        this.RuntimeType!.AddInstanceMethod(method, location);
    }

    public void AddStaticAttribute(Attribute attribute, SourceLocation? location)
    {
        this.EnforceIsType(location);
        this.RuntimeType!.AddStaticAttribute(attribute, location);
    }

    public void AddInstanceAttribute(Attribute attribute, SourceLocation? location)
    {
        this.EnforceIsType(location);
        this.RuntimeType!.AddInstanceAttribute(attribute, location);
    }

    public override Method GetStaticMethod(string name, SourceLocation location)
    {
        Method? method = this.GetStaticMethodOrDefault(name, location);

        if (method is not null) return method;

        if (this.SuperType is not null)
            return this.SuperType.GetStaticMethod(name, location);

        Errors.AlwaysThrow(new InvalidMethodError($"Type {this.Name} has no static method {name}"), location);
        throw new UnreachableException();
    }

    public override Method GetInstanceMethod(string name, SourceLocation location)
    {
        Method? method = this.GetInstanceMethodOrDefault(name, location);

        if (method is not null) return method;

        if (this.SuperType is not null)
            return this.SuperType.GetInstanceMethod(name, location);

        Errors.AlwaysThrow(new InvalidMethodError($"Type {this.Name} has no static method {name}"), location);
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
        if (this.IsInterface)
            return this.SuperType!.GetStaticMethodOrDefault(name, location);

        this.EnforceIsType(location);
        return this.RuntimeType!.GetStaticMethodOrDefault(name, location);
    }

    public Method? GetInstanceMethodOrDefault(string name, SourceLocation location)
    {
        if (this.IsInterface)
            return this.SuperType!.GetInstanceMethodOrDefault(name, location);

        this.EnforceIsType(location);
        return this.RuntimeType!.GetInstanceMethodOrDefault(name, location);
    }

    public Attribute? GetStaticAttributeOrDefault(string name, SourceLocation location)
    {
        if (this.IsInterface)
            return this.SuperType!.GetStaticAttributeOrDefault(name, location);

        this.EnforceIsType(location);
        return this.RuntimeType!.GetStaticAttributeOrDefault(name, location);
    }

    public Attribute? GetInstanceAttributeOrDefault(string name, SourceLocation location)
    {
        if (this.IsInterface)
            return this.SuperType!.GetInstanceAttributeOrDefault(name, location);

        this.EnforceIsType(location);
        return this.RuntimeType!.GetInstanceAttributeOrDefault(name, location);
    }

    public string Name => this.RuntimeType?.Name ?? this.InterfaceContract?.Name ?? "Unknown";
    public bool IsStatic => this.RuntimeType?.IsStatic ?? this.InterfaceContract is not null;

    private bool RuntimeTypeEquals(RuntimeType? other)
    {
        if (this.RuntimeType is null ^ other is null) return false;

        return this.RuntimeType?.Equals(other!) ?? true;
    }

    private bool RuntimeInterfaceEquals(RuntimeInterface? other)
    {
        if (this.InterfaceContract is null ^ other is null) return false;

        return this.InterfaceContract?.Equals(other!) ?? true;
    }


    public override bool Equals(RuntimeObject other)
    {
        if (this == other) return true;

        if (other is not TypeObject type) return false;

        if (this.SuperType is null ^ type.SuperType is null) return false;

        if (!this.SuperType?.Equals(type.SuperType!) ?? false) return false;

        if (!this.RuntimeInterfaceEquals(type.InterfaceContract)) return false;

        return this.RuntimeTypeEquals(type.RuntimeType);
    }

    public override string ToString()
    {
        if (this.RuntimeType is not null)
            return $"Type<{this.RuntimeType.Name}, inherits from {this.SuperType?.Name ?? "N/A"}>";

        if (this.InterfaceContract is not null)
            return $"Type<(Interface){this.InterfaceContract.Name}>";

        return $"Type<?>";
    }
}
