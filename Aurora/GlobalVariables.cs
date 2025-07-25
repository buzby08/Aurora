using CommandLine;

namespace Aurora;

/// <summary>
/// Stores all global variables in the program as a singular static class
/// </summary>
internal static class GlobalVariables
{
    public static readonly Logs LOGGER = new();
    public static Errors Errors = new();
    public static string CodeFilePath = "";
    public static int? LineNumber;
    public static int ExpressionDepth = 0;
    public const string ConfigFilePath = @"\mnt\CTRL-S\Aurora\auroraConfig.json";
    public static bool StrictFlagMode = false;
    public static ParserResult<Options>? ParserResult;

    public static readonly Tuple<char, char> STRING_START_CHARS = new('"', '\'');

    /// <summary>
    /// Returns a version of the string with its surrounding quotes, not interfering with the internal string
    /// content
    /// </summary>
    /// <param name="value">The string value to convert</param>
    /// <returns>The converted string</returns>
    public static string? ReprString(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        char quote = value.Contains('\'') ? '"' : '\'';
        return $"{quote}{value}{quote}";
    }
}