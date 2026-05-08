using Aurora.Internals;

namespace Aurora;

/// <summary>
/// Allows easy logs to a log file
/// </summary>
internal static class Logs
{
    public static bool AllowDebug = false;
    public static bool AllowVerbose = false;
    public static bool AllowWarning = true;
    public static bool NoConsole = false;
    public static bool ShowTimestamp = true;
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

    private static LinkedList<string> GenerateStackTrace(RuntimeContext? context)
    {
        LinkedList<string> stackTrace = new();

        if (context is null)
            return stackTrace;

        stackTrace.AddFirst(CreateMessage(context.FileName, context.LineNumber));

        RuntimeContext currentContext = context;

        while (currentContext.Parent is not null)
        {
            bool locationIsSame = currentContext.FileName == currentContext.Parent.FileName &&
                                  currentContext.LineNumber == currentContext.Parent.LineNumber;
            if (locationIsSame)
            {
                currentContext = currentContext.Parent;
                continue;
            }

            currentContext = currentContext.Parent;
            stackTrace.AddFirst(CreateMessage(currentContext.FileName, currentContext.LineNumber));
        }
        
        return stackTrace;

        string CreateMessage(string fileName, int lineNum) => $"at {{{fileName} : Line {lineNum}}}";
    }

    private static void LogOutput(string message, RuntimeContext? context = null)
    {
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string fullMessage = ShowTimestamp ? $"{timestamp}: {message}" : message;

        if (!NoConsole)
        {
            Console.WriteLine(message);
            foreach (string stackTraceItem in GenerateStackTrace(context))
                Console.WriteLine(stackTraceItem);
        }

        fullMessage = fullMessage
            .Replace("\\", @"\\")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t");

        if (!File.Exists(LogFilePath))
            File.Create(LogFilePath).Dispose();

        using StreamWriter writer = File.AppendText(LogFilePath);
        writer.WriteLine(fullMessage);
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
        if (!AllowDebug)
        {
            return;
        }

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
        if (!AllowVerbose)
        {
            return;
        }

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
        if (!AllowWarning)
        {
            return;
        }

        Console.ForegroundColor = ConsoleColor.Yellow;
        LogOutput($"[WARNING] {message}");
        Console.ResetColor();
    }

    public static void Error(string message, RuntimeContext context)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        LogOutput($"[ERROR] {message}", context);
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