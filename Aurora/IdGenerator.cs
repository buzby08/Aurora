namespace Aurora;

internal static class IdGenerator
{
    private static Dictionary<string, int> _ids = new();

    public static bool RegisterCategory(string category)
    {
        return _ids.TryAdd(category, 0);
    }

    public static int GenerateId(string category)
    {
        if (!_ids.ContainsKey(category)) RegisterCategory(category);
        return ++_ids[category];
    }

    public static string ToString()
    {
        return $"{nameof(IdGenerator)}(" + string.Join(", ", _ids.Select(x => $"{x.Key} = {x.Value}")) + ")";
    }
}
