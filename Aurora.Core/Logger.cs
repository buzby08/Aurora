namespace Aurora.Core;

public class Logger(string caller)
{
    private static bool UseConsole = false;
    private static string? FilePath = "aurora.log";
    private static bool ClearFile = true;

    private static void Write(string message, ConsoleColor? color = null, bool useError = false)
    {
        if (color is not null)
            Console.ForegroundColor = color.Value;
        if (UseConsole && !useError)
            Console.WriteLine(message);
        if (useError)
            Console.Error.WriteLine(message);

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

    public void Error(string message, bool addCaller = true)
    {
        string outputtedMessage = addCaller ? FormatString(message, caller) : message;
        Write(outputtedMessage, ConsoleColor.Red, useError: true);
        Environment.Exit(1);
    }
}
