namespace Aurora.Core;

internal static class CodeReader
{
    public static string GetCodeAtLine(int line)
    {
        string allCode = InternalVariables.Code;
        string[] codeByLines = allCode.Split('\n');

        if (line > codeByLines.Length)
            Errors.AlwaysThrow(
                new SystemError($"Code line number {line} is greater than the number of lines in the source file"), null);

        return codeByLines[line - 1];
    }

    public static string GetMessagePointingAtPosition(SourceLocation location, string prefix = "")
    {
        string line = GetCodeAtLine(location.LineNumber);

        string message = prefix + line + "\n" + prefix;
        message += new string(' ', location.ColumnNumber - 1) + '^';
        return message;
    }
}
