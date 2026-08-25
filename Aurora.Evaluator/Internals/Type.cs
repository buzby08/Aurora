namespace Aurora.Internals;

internal class Type : RuntimeObject
{
    public string Name { get; }
    public bool CanAccessParentValues;

    public readonly Dictionary<string, Method> InstanceMethods = [];
    public readonly Dictionary<string, Method> StaticMethods = [];

    public readonly Dictionary<string, Attribute> InstanceAttributes = [];
    public readonly Dictionary<string, Attribute> StaticAttributes = [];

    public Type(string name, Type type, bool canAccessParentValues = true)
    {
        this.Name = name;
        this.Type = type;
        this.CanAccessParentValues = canAccessParentValues;
    }

    public Type(string name)
    {
        this.Name = name;
    }

    public bool IsSubclassOf(Type type)
    {
        if (this.Type == this && this != type) return false;
        return this == type || this.Type.IsSubclassOf(type);
    }

    public void AddStaticMethod(Method method)
    {
        this.StaticMethods.Add(method.Name, method);
    }

    public void AddInstanceMethod(Method method)
    {
        this.InstanceMethods.Add(method.Name, method);
    }

    public void AddStaticAttribute(Attribute value)
    {
        this.StaticAttributes.Add(value.Name, value);
    }

    public void AddInstanceAttribute(Attribute value)
    {
        this.InstanceAttributes.Add(value.Name, value);
    }

    public Method GetStaticMethod(string name, RuntimeContext context, int? position = null)
    {
        Method? method = this.GetStaticMethodOrDefault(name);

        if (method is null)
            Errors.AlwaysThrow(new InvalidMethodError($"Object {this.Name} has no static method {name}"),
                context, position: position);

        return method;
    }

    public Method GetInstanceMethod(string name, RuntimeContext context, int? position = null)
    {
        Method? method = this.GetInstanceMethodOrDefault(name);

        if (method is null)
            Errors.AlwaysThrow(new InvalidMethodError($"Object {this.Name} has no instance method {name}"),
                context, position: position);

        return method;
    }

    public Attribute GetStaticAttribute(string name, RuntimeContext context, int? position = null)
    {
        Attribute? attribute = this.GetStaticAttributeOrDefault(name);

        if (attribute is null)
            Errors.AlwaysThrow(new InvalidAttributeError($"Object {this.Name} has no static attribute {name}"),
                context, position: position);

        return attribute;
    }

    public Attribute GetInstanceAttribute(string name, RuntimeContext context, int? position = null)
    {
        Attribute? attribute = this.GetInstanceAttributeOrDefault(name);

        if (attribute is null)
            Errors.AlwaysThrow(new InvalidAttributeError($"Object {this.Name} has no instance attribute {name}"),
                context, position: position);

        return attribute;
    }

    private Method? GetStaticMethodOrDefault(string name)
    {
        Method? method = this.StaticMethods.GetValueOrDefault(name);

        if (this == this.Type) return method;

        if (!this.CanAccessParentValues) return method;

        return method ?? this.Type.GetStaticMethodOrDefault(name);
    }

    private Method? GetInstanceMethodOrDefault(string name)
    {
        Method? method = this.InstanceMethods.GetValueOrDefault(name);

        if (this == this.Type) return method;

        if (!this.CanAccessParentValues) return method;

        return method ?? this.Type.GetInstanceMethodOrDefault(name);
    }

    private Attribute? GetStaticAttributeOrDefault(string name)
    {
        Attribute? attribute = this.StaticAttributes.GetValueOrDefault(name);

        if (this == this.Type) return attribute;

        if (!this.CanAccessParentValues) return attribute;

        return attribute ?? this.Type.GetStaticAttributeOrDefault(name);
    }

    private Attribute? GetInstanceAttributeOrDefault(string name)
    {
        Attribute? attribute = this.InstanceAttributes.GetValueOrDefault(name);

        if (this == this.Type) return attribute;

        if (!this.CanAccessParentValues) return attribute;

        return attribute ?? this.Type.GetInstanceAttributeOrDefault(name);
    }

    public override bool Equals(RuntimeObject other)
    {
        if (other is not Type typeObject)
            return false;

        if (this.Name != typeObject.Name) return false;
        if (this.StaticAttributes != typeObject.StaticAttributes) return false;
        if (this.StaticMethods != typeObject.StaticMethods) return false;
        if (this.InstanceAttributes != typeObject.InstanceAttributes) return false;
        if (this.InstanceMethods != typeObject.InstanceMethods) return false;
        return true;
    }
}