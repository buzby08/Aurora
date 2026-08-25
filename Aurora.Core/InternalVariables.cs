namespace Aurora.Core;

/// <summary>
/// Stores all internal variables for the interpreter.
/// </summary>
public static class InternalVariables
{
    public static string CodeFilePath { get; set; } = string.Empty;

    public static Logger GlobalLogger = new("MainProcess");

    public static string Code { get; set; } = "";

    public const string ConfigFilePath = "auroraConfig.json";

    public static bool StrictFlagMode = false;

    public static bool InlineStackTrace = true;

    public static bool? PreviousIfIsTrue = null;

    public static bool EasterEggs = true;

    public static bool DisableErrors = false;

    public static int[] LinesToDebug = [4,];

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

    public static SourceLocation GetEmptySourceLocation()
    {
        return new SourceLocation
        {
            FilePath = CodeFilePath,
            LineNumber = 0,
            ColumnNumber = 0,
            Offset = 0,
        };
    }
}
