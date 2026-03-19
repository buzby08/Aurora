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

        this.InstanceMethods = type.InstanceMethods.ToDictionary();
        this.StaticMethods = type.StaticMethods.ToDictionary();
        this.InstanceAttributes = type.InstanceAttributes.ToDictionary();
        this.StaticAttributes = type.StaticAttributes.ToDictionary();
    }

    public Type(string name)
    {
        this.Name = name;
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

    public Method GetStaticMethod(string name)
    {
        Method? method = this.StaticMethods.GetValueOrDefault(name);

        if (method is null)
            Errors.AlwaysThrow(new InvalidMethodError($"{this.Name} has no static method {name}"));

        return method;
    }

    public Method GetInstanceMethod(string name)
    {
        Method? method = this.InstanceMethods.GetValueOrDefault(name);

        if (method is null)
            Errors.AlwaysThrow(new InvalidMethodError($"{this.Name} has no instance method {name}"));

        return method;
    }

    public RuntimeObject GetStaticAttribute(string name)
    {
        RuntimeObject? attribute = this.StaticAttributes.GetValueOrDefault(name);

        if (attribute is null)
            Errors.AlwaysThrow(new InvalidAttributeError($"{this.Name} has no static attribute {name}"));

        return attribute;
    }

    public RuntimeObject GetInstanceAttribute(string name)
    {
        RuntimeObject? attribute = this.InstanceAttributes.GetValueOrDefault(name);

        if (attribute is null)
            Errors.AlwaysThrow(new InvalidAttributeError($"{this.Name} has no instance attribute {name}"));

        return attribute;
    }
}