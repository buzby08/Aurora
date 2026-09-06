using Aurora.Core;

namespace Aurora.Evaluator.Internals;

public class RuntimeType : RuntimeObject
{
    public string Name { get; }
    public bool CanAccessParentValues;

    public bool IsStatic { get; set; }
    public bool IsFinalized { get; set; }

    public List<RuntimeInterface>? Interface { get; set; }

    public readonly Dictionary<string, Method> InstanceMethods = [];
    public readonly Dictionary<string, Method> StaticMethods = [];

    public readonly Dictionary<string, Attribute> InstanceAttributes = [];
    public readonly Dictionary<string, Attribute> StaticAttributes = [];

    public RuntimeType(string name, RuntimeType type, bool canAccessParentValues = true, bool isStatic = false)
    {
        this.Name = name;
        this.Type = type;
        this.CanAccessParentValues = canAccessParentValues;
        this.IsStatic = isStatic;
    }

    public RuntimeType(string name)
    {
        this.Name = name;
    }

    public void MarkFinal(SourceLocation? location)
    {
        if (this.Interface is not null)
            foreach (RuntimeInterface @interface in this.Interface)
                @interface.EnsureTypeMeetsContract(this, location);

        this.IsFinalized = true;
    }

    public bool IsSubclassOf(RuntimeType type)
    {
        if (this.Type == this && this != type) return false;
        return this == type || this.Type.IsSubclassOf(type);
    }

    public void AddInterface(RuntimeInterface type, SourceLocation? location)
    {
        if (this.IsFinalized)
            Errors.AlwaysThrow(
                new UnsupportedOperationError($"Cannot modify type {this.Name} because it has been declared as final"),
                location);

        this.Interface ??= [];
        this.Interface.Add(type);
    }

    public void AddStaticMethod(Method method, SourceLocation? location)
    {
        if (this.IsFinalized)
            Errors.AlwaysThrow(
                new UnsupportedOperationError($"Cannot modify type {this.Name} because it has been declared as final"),
                location);

        this.StaticMethods.Add(method.Name, method);
    }

    public void AddInstanceMethod(Method method, SourceLocation? location)
    {
        if (this.IsFinalized)
            Errors.AlwaysThrow(
                new UnsupportedOperationError($"Cannot modify type {this.Name} because it has been declared as final"),
                location);

        if (this.IsStatic)
            Errors.AlwaysThrow(
                new InvalidMethodError($"Cannot add instance method {method.Name} to static type {this.Name}",
                    user: false), location);
        this.InstanceMethods.Add(method.Name, method);
    }

    public void AddStaticAttribute(Attribute value, SourceLocation? location)
    {
        if (this.IsFinalized)
            Errors.AlwaysThrow(
                new UnsupportedOperationError($"Cannot modify type {this.Name} because it has been declared as final"),
                location);

        this.StaticAttributes.Add(value.Name, value);
    }

    public void AddInstanceAttribute(Attribute value, SourceLocation? location)
    {
        if (this.IsFinalized)
            Errors.AlwaysThrow(
                new UnsupportedOperationError($"Cannot modify type {this.Name} because it has been declared as final"),
                location);

        if (this.IsStatic)
            Errors.AlwaysThrow(
                new InvalidMethodError($"Cannot add instance attribute {value.Name} to static type {this.Name}",
                    user: false), location);
        this.InstanceAttributes.Add(value.Name, value);
    }

    public Method GetStaticMethod(string name, SourceLocation location)
    {
        Method? method = this.GetStaticMethodOrDefault(name, location);

        if (method is null)
            Errors.AlwaysThrow(new InvalidMethodError($"Object {this.Name} has no static method {name}"),
                location);

        return method;
    }

    public Method GetInstanceMethod(string name, SourceLocation location)
    {
        if (this.IsStatic)
            Errors.AlwaysThrow(
                new InvalidMethodError($"Static object {this.Name} has no instance methods"), location);

        Method? method = this.GetInstanceMethodOrDefault(name, location);

        if (method is null)
            Errors.AlwaysThrow(new InvalidMethodError($"Object {this.Name} has no instance method {name}"),
                location);

        return method;
    }

    public Attribute GetStaticAttribute(string name, SourceLocation location)
    {
        // Todo: Make it so a method cannot have the same name as an attribute and vice versa.

        // Todo: Make it so that if an attribute is not found, the method is returned, somehow.
        //  So, `Terminal.writeLine` would return `Method: writeLine (class Terminal)` or something similar, that can be
        //  stored as a variable and then invoked later.
        Attribute? attribute = this.GetStaticAttributeOrDefault(name, location);

        if (attribute is null)
            Errors.AlwaysThrow(new InvalidAttributeError($"Object {this.Name} has no static attribute {name}"),
                location);

        return attribute;
    }

    public Attribute GetInstanceAttribute(string name, SourceLocation location)
    {
        if (this.IsStatic)
            Errors.AlwaysThrow(
                new InvalidMethodError($"Static object {this.Name} has no instance attributes"), location);

        Attribute? attribute = this.GetInstanceAttributeOrDefault(name, location);

        if (attribute is null)
            Errors.AlwaysThrow(new InvalidAttributeError($"Object {this.Name} has no instance attribute {name}"),
                location);

        return attribute;
    }

    private Method? GetStaticMethodOrDefault(string name, SourceLocation? location)
    {
        if (!this.IsFinalized)
            Errors.AlwaysThrow(
                new UnsupportedOperationError(
                    $"Cannot use type {this.Name} because it has not yet been declared as final"), location);

        Method? method = this.StaticMethods.GetValueOrDefault(name);

        if (this == this.Type) return method;

        if (!this.CanAccessParentValues) return method;

        return method ?? this.Type.GetStaticMethodOrDefault(name, location);
    }

    internal Method? GetInstanceMethodOrDefault(string name, SourceLocation? location, bool throwNotFinalError = true)
    {
        if (!this.IsFinalized && throwNotFinalError)
            Errors.AlwaysThrow(
                new UnsupportedOperationError(
                    $"Cannot use type {this.Name} because it has not yet been declared as final"), location);

        Method? method = this.InstanceMethods.GetValueOrDefault(name);

        if (this == this.Type) return method;

        if (!this.CanAccessParentValues) return method;

        return method ?? this.Type.GetInstanceMethodOrDefault(name, location);
    }

    private Attribute? GetStaticAttributeOrDefault(string name, SourceLocation? location)
    {
        if (!this.IsFinalized)
            Errors.AlwaysThrow(
                new UnsupportedOperationError(
                    $"Cannot use type {this.Name} because it has not yet been declared as final"), location);

        Attribute? attribute = this.StaticAttributes.GetValueOrDefault(name);

        if (this == this.Type) return attribute;

        if (!this.CanAccessParentValues) return attribute;

        return attribute ?? this.Type.GetStaticAttributeOrDefault(name, location);
    }

    private Attribute? GetInstanceAttributeOrDefault(string name, SourceLocation? location)
    {
        if (!this.IsFinalized)
            Errors.AlwaysThrow(
                new UnsupportedOperationError(
                    $"Cannot use type {this.Name} because it has not yet been declared as final"), location);

        Attribute? attribute = this.InstanceAttributes.GetValueOrDefault(name);

        if (this == this.Type) return attribute;

        if (!this.CanAccessParentValues) return attribute;

        return attribute ?? this.Type.GetInstanceAttributeOrDefault(name, location);
    }

    public override bool Equals(RuntimeObject other)
    {
        if (other is not RuntimeType typeObject)
            return false;

        if (this.Name != typeObject.Name) return false;
        if (this.StaticAttributes != typeObject.StaticAttributes) return false;
        if (this.StaticMethods != typeObject.StaticMethods) return false;
        if (this.InstanceAttributes != typeObject.InstanceAttributes) return false;
        if (this.InstanceMethods != typeObject.InstanceMethods) return false;
        return true;
    }

    public override string ToString() => $"{nameof(RuntimeObject)} {nameof(RuntimeType)}({this.Name})";
}
