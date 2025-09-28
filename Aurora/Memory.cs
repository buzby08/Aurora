using System.Diagnostics;

namespace Aurora;

internal struct MemoryItem
{
    public MemoryItem()
    {
    }

    public MemoryItem(string name, string owner, dynamic? value, bool constant = false)
    {
        Name = name;
        Owner = owner;
        Value = value;
        Constant = constant;
    }

    public required string Owner { get; set; }
    public required string Name { get; set; }
    public required dynamic? Value { get; set; }
    public bool Constant { get; set; } = false;
}

internal static class Memory
{
    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    private static Dictionary<string, Dictionary<string, MemoryItem>> _data = new();

    public static void Save(MemoryItem item)
    {
        if (!_data.ContainsKey(item.Owner))
            _data.Add(item.Owner, new Dictionary<string, MemoryItem>());

        _data[item.Owner][item.Name] = item;
    }

    public static MemoryItem? Get(string name, string owner)
    {
        if (!_data.TryGetValue(owner, out Dictionary<string, MemoryItem>? ownedMemory))
            return null;

        if (!ownedMemory.TryGetValue(name, out MemoryItem value))
            return null;

        return value;
    }

    public static void Update(string name, string owner, dynamic? value)
    {
        if (!_data.ContainsKey(owner))
            Errors.AlwaysThrow(new MemoryError());

        if (!_data[owner].ContainsKey(name))
            Errors.AlwaysThrow(new MemoryError());

        MemoryItem item = _data[owner][name];
        if (item.Constant)
            Errors.AlwaysThrow(new ConstantRedefinitionError("The system tried to modify constant memory",
                user: false));

        item.Value = value;
        _data[owner][name] = item;
    }

    public static void Clear()
    {
        _data.Clear();
    }
}