namespace Aurora;

/// <summary>
/// Allows easy logs to a log file
/// </summary>
internal static class Logs
{
    public static bool AllowDebug = false;
    public static bool AllowVerbose = false;
    public static bool AllowWarning = false;
    public static bool NoConsole = false;
    public static bool ShowTimestamp = false;
    public static string LogFilePath = "aurora.LOG";

    private static bool _clearFile = true;
    public static bool ClearFile
    {
        get => _clearFile;
        set
        {
            _clearFile = value;
            ClearLogFile();
        }
    }

    private static void LogOutput(string message)
    {
        message = message
            .Replace("\\", @"\\")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t");
        
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string fullMessage = ShowTimestamp ? $"{timestamp}: {message}" : message;
        using StreamWriter writer = File.AppendText(LogFilePath);
        writer.WriteLine(fullMessage);

        if (NoConsole) { return; }

        Console.WriteLine(message);
    }

    private static void ClearLogFile()
    {
        if (_clearFile)
            File.WriteAllText(LogFilePath, string.Empty);
    }

    /// <summary>
    /// Writes the debug message, if AllowDebug is true. Writes to the console if NoConsole is false, and writes to the
    /// log file specified in the LogFilePath attribute.
    /// </summary>
    /// <param name="message">The debug message to log.</param>
    public static void Debug(string message)
    {
        if (!AllowDebug) { return; }

        Console.ForegroundColor = ConsoleColor.DarkMagenta;
        LogOutput($"[DEBUG] {message}");
        Console.ResetColor();
    }

    /// <summary>
    /// Writes the verbose message, if AllowDebug is true. Writes to the console if NoConsole is false, and writes to
    /// the log file specified in the LogFilePath attribute.
    /// </summary>
    /// <param name="message">The verbose message to log.</param>
    public static void Verbose(string message)
    {
        if (!AllowVerbose) { return; }

        Console.ForegroundColor = ConsoleColor.Cyan;
        LogOutput($"[VERBOSE] {message}");
        Console.ResetColor();
    }
    
    /// <summary>
    /// Writes the warning message, if AllowDebug is true. Writes to the console if NoConsole is false, and writes to
    /// the log file specified in the LogFilePath attribute.
    /// </summary>
    /// <param name="message">The warning message to log.</param>
    public static void Warning(string message)
    {
        if (!AllowWarning) { return; }

        Console.ForegroundColor = ConsoleColor.Yellow;
        LogOutput($"[WARNING] {message}");
        Console.ResetColor();
    }

    /// <summary>
    /// Logs a message to the log file specified in the LogFilePath attribute, regardless of the AllowVerbose,
    /// AllowDebug, or AllowWarning attributes.
    /// </summary>
    /// <param name="message"></param>
    public static void ForceLog(string message)
    {
        using StreamWriter writer = File.AppendText(LogFilePath);
        writer.WriteLine(message);
    }

    public static void ForceConsoleLog(string message, bool addLineNumber = false)
    {
        // if (addLineNumber)
        //     message = $"[Line {GlobalVariables.LineNumber}] " + message;
        
        Console.WriteLine(message);
    }
}
