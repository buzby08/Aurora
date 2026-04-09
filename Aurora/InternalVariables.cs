using Aurora.Internals;
using CommandLine;

namespace Aurora;

/// <summary>
/// Stores all internal variables for the interpreter.
/// </summary>
internal static class InternalVariables
{
    public static string CodeFilePath { get; set; } = string.Empty;

    public static RuntimeContext GlobalContext = new(null);

    public static string[] Code { get; set; } = [];

    public static int? LineNumber { get; set; } = null;

    public static int ExpressionDepth = 0;

    public static int RecursionDepth = 0;

    public const string ConfigFilePath = "auroraConfig.json";

    public static bool StrictFlagMode = false;

    public static bool InlineStackTrace = true;

    public static bool? PreviousIfIsTrue = null;

    public static bool EasterEggs = true;

    public static bool DisableErrors = false;

    public static int[] LinesToDebug = [4];

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

    public static void IncrementRecursionDepth()
    {
        RecursionDepth++;
        if (RecursionDepth > /*UserConfiguration.MaxExpressionDepth*/ 256)
            Errors.AlwaysThrow(new MaxRecursionDepthExceededError());
    }
}