namespace Aurora.Internals;

internal class Type : RuntimeObject
{
    public string Name { get; }

    public readonly Dictionary<string, Method> InstanceMethods = [];
    public readonly Dictionary<string, Method> StaticMethods = [];

    public readonly Dictionary<string, RuntimeObject> InstanceAttributes = [];
    public readonly Dictionary<string, RuntimeObject> StaticAttributes = [];

    public Type(string name, Type type)
    {
        this.Name = name;
        this.Type = type;
    }

    public Type(string name)
    {
        this.Name = name;
    }

    public bool IsSubclassOf(Type type)
    {
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

    public void AddStaticAttribute(string name, RuntimeObject value)
    {
        this.StaticAttributes.Add(name, value);
    }

    public void AddInstanceAttribute(string name, RuntimeObject value)
    {
        this.InstanceAttributes.Add(name, value);
    }

    public Method GetStaticMethod(string name, int? position = null)
    {
        Method? method = this.GetStaticMethodOrDefault(name);

        if (method is null)
            Errors.AlwaysThrow(new InvalidMethodError($"Object {this.Name} has no static method {name}"),
                position: position);

        return method;
    }

    public Method GetInstanceMethod(string name, int? position = null)
    {
        Method? method = this.GetInstanceMethodOrDefault(name);

        if (method is null)
            Errors.AlwaysThrow(new InvalidMethodError($"Object {this.Name} has no instance method {name}"),
                position: position);

        return method;
    }

    public RuntimeObject GetStaticAttribute(string name, int? position = null)
    {
        RuntimeObject? attribute = this.GetStaticAttributeOrDefault(name);

        if (attribute is null)
            Errors.AlwaysThrow(new InvalidAttributeError($"Object {this.Name} has no static attribute {name}"),
                position: position);

        return attribute;
    }

    public RuntimeObject GetInstanceAttribute(string name, int? position = null)
    {
        RuntimeObject? attribute = this.GetInstanceAttributeOrDefault(name);

        if (attribute is null)
            Errors.AlwaysThrow(new InvalidAttributeError($"Object {this.Name} has no instance attribute {name}"),
                position: position);

        return attribute;
    }

    private Method? GetStaticMethodOrDefault(string name)
    {
        Method? method = this.StaticMethods.GetValueOrDefault(name);

        if (this == this.Type) return method;

        return method ?? this.Type.GetStaticMethodOrDefault(name);
    }

    private Method? GetInstanceMethodOrDefault(string name)
    {
        Method? method = this.InstanceMethods.GetValueOrDefault(name);

        if (this == this.Type) return method;

        return method ?? this.Type.GetInstanceMethodOrDefault(name);
    }

    private RuntimeObject? GetStaticAttributeOrDefault(string name)
    {
        RuntimeObject? attribute = this.StaticAttributes.GetValueOrDefault(name);

        if (this == this.Type) return attribute;

        return attribute ?? this.Type.GetStaticAttributeOrDefault(name);
    }

    private RuntimeObject? GetInstanceAttributeOrDefault(string name)
    {
        RuntimeObject? attribute = this.InstanceAttributes.GetValueOrDefault(name);

        if (this == this.Type) return attribute;

        return attribute ?? this.Type.GetInstanceAttributeOrDefault(name);
    }
}