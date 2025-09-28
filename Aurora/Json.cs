using System.Text.Json;
namespace Aurora;

internal static class Json
{
    public static List<T> ReadList<T>(string filePath)
    {
        if (!filePath.EndsWith(".json"))
            throw new ArgumentException($"JSON.Read requires a .json file, not {filePath}");

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"The provided file path - {filePath} - does not exist");

        using var r = new StreamReader(filePath);
        string json = r.ReadToEnd();

        var jsonObject = JsonSerializer.Deserialize<List<T>>(json);

        if (jsonObject is null) throw new JsonException("Deserialization failed: JSON content is invalid or empty");

        return jsonObject;
    }
    
    public static Dictionary<string, T> ReadDict<T>(string filePath)
    {
        if (!filePath.EndsWith(".json"))
            throw new ArgumentException($"JSON.Read requires a .json file, not {filePath}");

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"The provided file path - {filePath} - does not exist");

        using var r = new StreamReader(filePath);
        string json = r.ReadToEnd();

        var jsonObject = JsonSerializer.Deserialize<Dictionary<string, T>>(json);

        if (jsonObject is null) throw new JsonException("Deserialization failed: JSON content is invalid or empty");

        return jsonObject;
    }

    public static void Write<T>(Dictionary<string, T> data, string filePath)
    {
        string json = JsonSerializer.Serialize(data);
        using var r = new StreamWriter(filePath);
        r.Write(json);
    }

    public static void Write<T>(List<T> data, string filePath)
    {
        string json = JsonSerializer.Serialize(data);
        using var r = new StreamWriter(filePath);
        r.Write(json);
    }
}

