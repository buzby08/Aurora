// using CommandLine;
//
// namespace Aurora;
//
// /// <summary>
// /// Stores all global variables in the program as a singular static class
// /// </summary>
// internal static class GlobalVariables
// {
//     private static string _owner = "Aurora.GlobalVariables";
//
//     public static readonly Logs LOGGER = new();
//
//     public static string CodeFilePath
//     {
//         get => Memory.Get("CodeFilePath", _owner)?.Value ?? "";
//         set => Memory.Update("CodeFilePath", _owner, value);
//     }
//
//     public static string[] Code
//     {
//         get => Memory.Get("Code", _owner)?.Value ?? Array.Empty<string>();
//         set => Memory.Update("Code", _owner, value);
//     }
//
//     public static int? LineNumber
//     {
//         get => (int)Variables.GetVariable("__LINE_NUMBER__", new IntegerToken().Initialise(0)).ValueAsInt.Value;
//         set => Variables.ModifySystemVariable("__LINE_NUMBER__", new IntegerToken().Initialise(value!));
//     }
//
//     public static int ExpressionDepth = 0;
//
//     public static int RecursionDepth = 0;
//
//     public const string ConfigFilePath = "auroraConfig.json";
//
//     public static bool StrictFlagMode
//     {
//         get => Memory.Get("StrictFlagMode", _owner)?.Value ?? false;
//         set => Memory.Update("StrictFlagMode", _owner, value);
//     }
//
//     public static bool InlineStackTrace
//     {
//         get => Memory.Get("InlineStackTrace", _owner)?.Value ?? false;
//         set => Memory.Update("InlineStackTrace", _owner, value);
//     }
//
//     public static bool? PreviousIfIsTrue
//     {
//         get => Memory.Get("PreviousIfIsTrue", _owner)?.Value ?? null;
//         set => Memory.Update("PreviousIfIsTrue", _owner, value);
//     }
//
//     public static bool EasterEggs
//     {
//         get => Memory.Get("EasterEggs", _owner)?.Value ?? true;
//         set => Memory.Update("EasterEggs", _owner, value);
//     }
//
//     public static int[] LinesToDebug = [5];
//
//     public static Evaluate Evaluator = new();
//
//     public static readonly Tuple<char, char> STRING_START_CHARS = new('"', '\'');
//
//     /// <summary>
//     /// Returns a version of the string with its surrounding quotes, not interfering with the internal string
//     /// content
//     /// </summary>
//     /// <param name="value">The string value to convert</param>
//     /// <returns>The converted string</returns>
//     public static string? ReprString(string? value)
//     {
//         if (string.IsNullOrEmpty(value))
//         {
//             return value;
//         }
//
//         char quote = value.Contains('\'') ? '"' : '\'';
//         return $"{quote}{value}{quote}";
//     }
//
//     public static void IncrementRecursionDepth()
//     {
//         RecursionDepth++;
//         if (RecursionDepth > UserConfiguration.MaxExpressionDepth)
//             Errors.AlwaysThrow(new MaxRecursionDepthExceededError());
//     }
//
//     public static void InititaliseMembers()
//     {
//         MemoryItem codeFilePath = new MemoryItem
//         {
//             Owner = _owner,
//             Name = "CodeFilePath",
//             Value = ""
//         };
//         MemoryItem code = new MemoryItem
//         {
//             Owner = _owner,
//             Name = "Code",
//             Value = Array.Empty<string>()
//         };
//         MemoryItem strictFlagMode = new MemoryItem
//         {
//             Owner = _owner,
//             Name = "StrictFlagMode",
//             Value = false
//         };
//         MemoryItem inlineStackTrace = new MemoryItem
//         {
//             Owner = _owner,
//             Name = "InlineStackTrace",
//             Value = false
//         };
//         MemoryItem previousIfIsTrue = new MemoryItem
//         {
//             Owner = _owner,
//             Name = "PreviousIfIsTrue",
//             Value = null
//         };
//         MemoryItem easterEggs = new MemoryItem
//         {
//             Owner = _owner,
//             Name = "EasterEggs",
//             Value = true
//         };
//
//         Memory.Save(codeFilePath);
//         Memory.Save(code);
//         Memory.Save(strictFlagMode);
//         Memory.Save(inlineStackTrace);
//         Memory.Save(previousIfIsTrue);
//         Memory.Save(easterEggs);
//
//         Variables.ModifySystemVariable("__LINE_NUMBER__", new IntegerToken().Initialise(0));
//     }
// }