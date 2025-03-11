using System;
using System.Diagnostics.CodeAnalysis;

namespace Aurora
{
    class Errors
    {
        [DoesNotReturn]
        public static void RaiseError(string title, string message)
        {
            string outputMessage;

            if (GlobalVariables.lineNumber is not null)
            {
                outputMessage = $"[ERROR] {{Line {GlobalVariables.lineNumber}}} {title} - {message}";
            }
            else
            {
                outputMessage = $"[ERROR] {{Unknown line}} {title} - {message}";
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(outputMessage);
            Console.ResetColor();

            using StreamWriter writer = File.AppendText(GlobalVariables.logger.logFilePath);
            writer.WriteLine(outputMessage);

            Environment.Exit(1);
        }
    }
}