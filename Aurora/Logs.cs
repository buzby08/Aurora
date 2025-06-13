namespace Aurora;

/// <summary>
/// Allows easy logs to a log file
/// </summary>
internal class Logs
{
    public bool AllowDebug = false;
    public bool AllowVerbose = false;
    public bool AllowWarning = false;
    public bool NoConsole = false;
    public bool ShowTimestamp = false;
    public string LogFilePath = "aurora.LOG";

    private bool _clearFile = true;
    public bool ClearFile
    {
        get => _clearFile;
        set
        {
            this._clearFile = value;
            this.ClearLogFile();
        }
    }

    private void LogOutput(string message)
    {
        message = message
            .Replace("\\", @"\\")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t");
        
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string fullMessage = this.ShowTimestamp ? $"{timestamp}: {message}" : message;
        using StreamWriter writer = File.AppendText(this.LogFilePath);
        writer.WriteLine(fullMessage);

        if (this.NoConsole) { return; }

        Console.WriteLine(message);
    }

    private void ClearLogFile()
    {
        if (this._clearFile)
            File.WriteAllText(this.LogFilePath, string.Empty);
    }

    /// <summary>
    /// Writes the debug message, if AllowDebug is true. Writes to the console if NoConsole is false, and writes to the
    /// log file specified in the LogFilePath attribute.
    /// </summary>
    /// <param name="message">The debug message to log.</param>
    public void Debug(string message)
    {
        if (!this.AllowDebug) { return; }

        Console.ForegroundColor = ConsoleColor.DarkMagenta;
        LogOutput($"[DEBUG] {message}");
        Console.ResetColor();
    }

    /// <summary>
    /// Writes the verbose message, if AllowDebug is true. Writes to the console if NoConsole is false, and writes to
    /// the log file specified in the LogFilePath attribute.
    /// </summary>
    /// <param name="message">The verbose message to log.</param>
    public void Verbose(string message)
    {
        if (!this.AllowVerbose) { return; }

        Console.ForegroundColor = ConsoleColor.Cyan;
        LogOutput($"[VERBOSE] {message}");
        Console.ResetColor();
    }
    
    /// <summary>
    /// Writes the warning message, if AllowDebug is true. Writes to the console if NoConsole is false, and writes to
    /// the log file specified in the LogFilePath attribute.
    /// </summary>
    /// <param name="message">The warning message to log.</param>
    public void Warning(string message)
    {
        if (!this.AllowWarning) { return; }

        Console.ForegroundColor = ConsoleColor.Yellow;
        LogOutput($"[WARNING] {message}");
        Console.ResetColor();
    }

    /// <summary>
    /// Logs a message to the log file specified in the LogFilePath attribute, regardless of the AllowVerbose,
    /// AllowDebug, or AllowWarning attributes.
    /// </summary>
    /// <param name="message"></param>
    public void ForceLog(string message)
    {
        using StreamWriter writer = File.AppendText(this.LogFilePath);
        writer.WriteLine(message);
    }

    public void ForceConsoleLog(string message, bool addLineNumber = false)
    {
        if (addLineNumber)
            message = $"[Line {GlobalVariables.LineNumber}] " + message;
        
        Console.WriteLine(message);
    }
}
