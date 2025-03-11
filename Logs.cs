using System;

namespace Aurora 
{
    class Logs
    {
        public bool allowDebug = false;
        public bool allowVerbose = false;
        public bool allowWarning = false;
        public bool noConsole = false;
        public string logFilePath = "aurora.LOG";

        private void LogOutput(string message)
        {
            using StreamWriter writer = File.AppendText(logFilePath);
            writer.WriteLine(message);

            if (noConsole) { return; }

            Console.WriteLine(message);
        }

        public void Debug(string message)
        {
            if (!allowDebug) { return; }

            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            LogOutput($"[DEBUG] {message}");
            Console.ResetColor();
        }

        public void Verbose(string message)
        {
            if (!allowVerbose) { return; }

            Console.ForegroundColor = ConsoleColor.Cyan;
            LogOutput($"[VERBOSE] {message}");
            Console.ResetColor();
        }
        
        public void Warning(string message)
        {
            if (!allowWarning) { return; }

            Console.ForegroundColor = ConsoleColor.Yellow;
            LogOutput($"[WARNING] {message}");
            Console.ResetColor();
        }

        public void ForceLog(string message)
        {
            using StreamWriter writer = File.AppendText(logFilePath);
            writer.WriteLine(message);
        }
    }
}