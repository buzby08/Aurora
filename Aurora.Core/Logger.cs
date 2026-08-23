namespace Aurora;

internal class Logger(string caller)
{
    private static bool UseConsole = true;
    private static string? FilePath = "aurora.log";
    private static bool ClearFile = true;

    private static void Write(string message, ConsoleColor? color = null)
    {
        if (color is not null)
            Console.ForegroundColor = color.Value;
        if (UseConsole)
            Console.WriteLine(message);

        Console.ResetColor();

        if (FilePath is null)
            return;

        if (ClearFile)
            File.WriteAllText(FilePath, string.Empty);

        File.AppendAllText(FilePath, message + Environment.NewLine);
    }

    private static string FormatString(string message, string caller) => $"[{caller}] {message}";

    public void Debug(string message)
    {
        Write(FormatString(message, caller), ConsoleColor.DarkGray);
    }

    public void Info(string message)
    {
        Write(FormatString(message, caller));
    }

    public void Warning(string message)
    {
        Write(FormatString(message, caller), ConsoleColor.Yellow);
    }

    public void Error(string message)
    {
        Write(FormatString(message, caller), ConsoleColor.Red);
        Environment.Exit(1);
    }
}
